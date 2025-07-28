using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
            var Loginuser = userRow["User_Name"].ToString();
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
                new Claim(ClaimTypes.Name, Loginuser ?? ""),
                new Claim(ClaimTypes.Role, role ?? ""),
                new Claim("TenantId", tenantId ?? ""),
                new Claim("UserId", userId ?? ""), 
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

            var sessionModel = new TenantSessionModel
            {
                TenantId = Convert.ToInt32(tenantId),
                TenantCode = Guid.Parse(tenantKey),
                IsActive = true,
                OrgExists = hasOrganization,
                LoginUser = Loginuser,
                Role = role
            };

            if (hasOrganization)
            {
                var row = orgTable.Rows[0];
                sessionModel.TenantName = row["TenantName"]?.ToString();
                sessionModel.Email = row["Email"]?.ToString();
                sessionModel.Phone = row["Phone"]?.ToString();
                sessionModel.CreatedOn = Convert.ToDateTime(row["CreatedOn"]);

                sessionModel.OrganizationKey = row.Table.Columns.Contains("OrganizationKey") &&
                                               Guid.TryParse(row["OrganizationKey"]?.ToString(), out Guid orgKey)
                                               ? orgKey : (Guid?)null;

                sessionModel.OrganizationName = row.Table.Columns.Contains("OrganizationName")
                                               ? row["OrganizationName"]?.ToString() : null;
            }

            // ✅ Store session
            HttpContext.Session.SetString("TenantSession", JsonConvert.SerializeObject(sessionModel));

            // ✅ Return redirect info for SPA or JS handling
            return Ok(new
            {
                success = true,
                message = "Login successful.",
                Role = role,
                TenantId = tenantId,
                UserId = userId,
                redirectUrl = hasOrganization ? "/TenantTransaction/Dashboard" : "/User/Organization"
            });
        }


        public IActionResult Login()
        {
            return View();
        }

        public async Task<ActionResult> Organization()
        {
            // Step 1: Get session value and deserialize
            string? tenantSessionJson = HttpContext.Session.GetString("TenantSession");

            string tenantName = "Unknown Tenant";
            int tenantid = 0;

            if (!string.IsNullOrEmpty(tenantSessionJson))
            {
                var tenantSession = JsonConvert.DeserializeObject<TenantSessionModel>(tenantSessionJson);
                if (tenantSession != null)
                {
                    tenantName = tenantSession.TenantName;
                    tenantid = tenantSession.TenantId;
                }
            }

            ViewBag.TenantName = tenantName;
            
            // Step 2: Load Master data
            var ds = await _dataService.GetAllDatasetAsync("SP_Get_MasterData", new Dictionary<string, object>());

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
                }).ToList(),

                TenantId= tenantid
            };

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Organization([FromBody] OrganizationModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid input.");
            }
            try 
            {
                model.OrganizationCode = Guid.NewGuid(); // ✅ generate valid guid
                // Prepare parameters for SP_Insert_Organization
                var parameters = new Dictionary<string, object>
                {
                    { "@TenantId", model.TenantId },
                    { "@OrganizationName", model.OrganizationName },
                    { "@OrganizationCode", model.OrganizationCode }, // should be a Guid
                    { "@BusinessTypeId", model.BusinessTypeId },
                    { "@GSTIN", model.GSTIN ?? (object)DBNull.Value },
                    { "@Address", model.Address ?? (object)DBNull.Value },
                    { "@City", model.City ?? (object)DBNull.Value },
                    { "@StateId", model.StateId ?? (object)DBNull.Value },
                    { "@PINCode", model.PINCode ?? (object)DBNull.Value }
                };

                // Call stored procedure
                var resultTable = await _dataService.GetDataAsync("SP_Insert_Organization", parameters);

                if (resultTable == null || resultTable.Rows.Count == 0)
                    return BadRequest(new { success = false, message = "Unknown error occurred while inserting organization." });

                // Read result from stored procedure
                string existsType = resultTable.Rows[0]["ExistsType"]?.ToString() ?? "NONE";
                string organizationKeyStr = resultTable.Rows[0]["OrganizationKey"]?.ToString();
                int organizationId= Convert.ToInt32(resultTable.Rows[0]["OrganizationId"]?.ToString());
                if (existsType == "EXISTS")
                    return BadRequest(new { success = false, message = "Organization already exists for this tenant." });

                if (!Guid.TryParse(organizationKeyStr, out Guid organizationKey))
                    return StatusCode(500, new { success = false, message = "Invalid organization key returned." });

                 
                // ✅ Build session model
                var sessionModel = new TenantSessionModel
                {
                    TenantId = model.TenantId, 
                    OrganizationId = organizationId,
                    OrganizationKey = organizationKey,
                    OrganizationName = model.OrganizationName
                };

                // ✅ Set session using extension
                HttpContext.Session.SetObject("TenantSession", sessionModel);

                // ✅ Save cookies
                Response.Cookies.Append("OrgKey", organizationKey.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

                // ✅ Return success + redirect URL
                return Ok(new
                {
                    success = true,
                    message = "Organization created successfully.",
                    organizationKey = organizationKey,
                    redirectUrl = $"/{organizationKey}/Dashboard"
                });
            }
            catch(Exception ex)
            {
                // ✅ Proper logging
                return StatusCode(500, new { success = false, message = "Exception: " + ex.Message });
            }
 
        }

        public async Task<IActionResult> Logout()
        {
            // 1. Sign out of cookie authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 2. Clear the session
            HttpContext.Session.Clear();

            // 3. Remove all cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            // 4. Redirect to login page
            return Redirect("/User/Login");
        }

    }
}
