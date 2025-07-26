using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace Invi.Controllers
{
    
    public class UserController: Controller
    {
        private readonly DataService _dataService;
        private readonly GeneralSevice _generalService;
        public UserController(DataService dataSevice, EncryptDecryptService encryptDecryptService,GeneralSevice generalSevice)
        {
           _generalService = generalSevice;
           _dataService = dataSevice;
        }

        public IActionResult Index()
        {
            return View();
        }
         
        public IActionResult SignUp()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SignUp([FromBody] SignUpViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return BadRequest(new { success = false, message = string.Join(" | ", errors) });
            }

            string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var parameters = new Dictionary<string, object>
            {
                { "@BusinessName", model.Company },
                { "@Phone", model.Mobile },
                { "@Email", model.Email },
                { "@Password", encryptedPassword },
                { "@Role", "Admin" }
            };

            var resultTable = await _dataService.GetDataAsync("SP_Save_TenantRegister", parameters);

            if (resultTable == null || resultTable.Rows.Count == 0)
                return BadRequest(new { success = false, message = "Unknown error occurred." });

            string existsType = resultTable.Rows[0]["ExistsType"]?.ToString() ?? "NONE";

            if (existsType == "EMAIL")
                return BadRequest(new { success = false, message = "Email already registered." });

            if (existsType == "MOBILE")
                return BadRequest(new { success = false, message = "Mobile number already registered." });

            // ✅ Extract and validate TenantKey
            string tenantKeyStr = resultTable.Rows[0]["TenantKey"]?.ToString(); // Corrected spelling
            if (!Guid.TryParse(tenantKeyStr, out Guid tenantKey))
            {
                return StatusCode(500, new { success = false, message = "Invalid tenant key returned." });
            }

            // ✅ Set secure cookies
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax // Improve CSRF protection
            };

            Response.Cookies.Append("TenantKey", tenantKey.ToString(), cookieOptions);

            Response.Cookies.Append("NeedOrganization", "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(15),
                HttpOnly = true,
                Secure = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            });
            // ✅ Automatically log in the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.Email),
                new Claim(ClaimTypes.Email, model.Email),
                new Claim("TenantKey", tenantKey.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


            // ✅ Return success with optional TenantKey
            return Ok(new
            {
                success = true,
                message = "Registration successful.",
                tenantKey = tenantKey
            });
        }

        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] SignInViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailOrMobile))
                return BadRequest("Please enter either Email or Mobile Number.");

            if (string.IsNullOrWhiteSpace(model.Password))
                return BadRequest("Password is required.");

            if (!_generalService.IsValidEmail(model.EmailOrMobile) && !_generalService.IsValidMobile(model.EmailOrMobile))
                return BadRequest("Enter a valid Email address or Mobile number.");

            var parameters = new Dictionary<string, object>
            {
                { "@UserName", model.EmailOrMobile }
            };

            var userTable = await _dataService.GetDataAsync("SP_Validate_UserLogin", parameters);
            if (userTable == null || userTable.Rows.Count == 0)
                return Unauthorized("Invalid credentials.");

            var userRow = userTable.Rows[0];

            var dbPassword = userRow["Password"]?.ToString();
            var role = userRow["Role"]?.ToString();
            var isActive = Convert.ToBoolean(userRow["IsActive"]);

            if (!isActive)
                return Unauthorized("Account is inactive.");

            if (!BCrypt.Net.BCrypt.Verify(model.Password, dbPassword))
                return Unauthorized("Invalid credentials.");

            var tenantId = userRow["TenantId"]?.ToString();
            var tenantKey = userRow["TenantKey"]?.ToString();
            var userId = userRow["UserId"]?.ToString();

            // ✅ Create Claims for authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, model.EmailOrMobile),
                new Claim(ClaimTypes.Role, role ?? ""),
                new Claim("TenantId", tenantId ?? ""),
                new Claim("UserId", userId ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ✅ Set custom cookies (TenantKey, UserId, Role)
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                IsEssential = true
            };

            Response.Cookies.Append("TenantKey", tenantKey ?? "", cookieOptions);
            Response.Cookies.Append("UserId", userId ?? "", cookieOptions);
            Response.Cookies.Append("Role", role ?? "", cookieOptions);

            // ✅ Check if organization exists
            var orgParams = new Dictionary<string, object>
            {
                { "@tenantKey", tenantKey }
            };

            var orgTable = await _dataService.GetDataAsync("SP_Check_OrganizationExists", orgParams);
            bool hasOrganization = orgTable != null && orgTable.Rows.Count > 0;

            // ✅ Return redirect info for SPA or JS handling
            return Ok(new
            {
                success = true,
                message = "Login successful.",
                Role = role,
                TenantId = tenantId,
                UserId = userId,
                redirectUrl = hasOrganization ? "/Dashboard/Index" : "/User/Organization"
            });
        }


        public IActionResult Login()
        {
            return View();
        }

        public async Task<ActionResult> Organization()
        {
            var ds = await _dataService.GetAllDatasetAsync("SP_Get_PartyMasterData", new Dictionary<string, object>());

            var model = new PartyViewModel
            {
                stateLists = ds.Tables[6].AsEnumerable().Select(r => new StateList
                {
                    StateId = Convert.ToInt32(r["StateId"]),
                    StateName = r["StateName"].ToString()
                }).ToList(),

                BusinessTypes = ds.Tables[10].AsEnumerable().Select(r => new BusinessType
                {
                    BusinessTypeId = Convert.ToInt32(r["BusinessTypeId"]),
                    BusinessTypeName = r["BusinessTypeName"].ToString()
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> OrganizationSave([FromBody] OrganizationModel model)
        {
            return Ok(Ok(model));
        }
    }
}
