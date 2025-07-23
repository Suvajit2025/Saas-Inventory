using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Mvc;
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

            // ✅ Get the TenantKey (GUID)
            var tenantKey = resultTable.Rows[0]["TenanatKey"]?.ToString();

            // ✅ Store cookie to redirect to Organization page
            Response.Cookies.Append("TenantKey", tenantKey.ToString(), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                IsEssential = true,
                HttpOnly = true,
                Secure = true
            });

            Response.Cookies.Append("NeedOrganization", "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(15), // Grace period
                IsEssential = true,
                HttpOnly = true,
                Secure = true
            });


            // ✅ Return success response with TenantKey (GUID)
            return Ok(new
            {
                success = true,
                message = "Registration successful.", 
            });
        }





        [HttpPost]
        public async Task<IActionResult> SignIn([FromBody] SignInViewModel model)
        {
            if (string.IsNullOrEmpty(model.EmailOrMobile))
                return BadRequest("Please enter either Email or Mobile Number.");

            if (string.IsNullOrEmpty(model.Password))
                return BadRequest("Password is required.");


            if (!_generalService.IsValidEmail(model.EmailOrMobile) && !_generalService.IsValidMobile(model.EmailOrMobile))
                return BadRequest("Enter a valid Email address or Mobile number.");

            var parameters = new Dictionary<string, object>
            {
                { "@UserName", string.IsNullOrEmpty(model.EmailOrMobile) ? DBNull.Value : model.EmailOrMobile }
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

            //compare password
            var isMatch = BCrypt.Net.BCrypt.Verify(model.Password, dbPassword);
            if (!isMatch)
                return Unauthorized("Invalid credentials.");

            // Role-based validation
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (userRow["OrganizationId"] == DBNull.Value)
                    return BadRequest("Organization ID is required for non-admin users.");
            }

            // Success response — you can return a token or user info
            return Ok(new
            {
                Message = "Login successful.",
                Role = role,
                TenantId = userRow["TenantId"],
                UserId = userRow["UserId"]
            });
        }
        public IActionResult Login()
        {
            return View();
        }

        public ActionResult Organization()
        {
            return View();
        }
    }
}
