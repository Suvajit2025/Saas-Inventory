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

                return BadRequest(string.Join(" | ", errors));
            }
            string encryptedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password); 
            // Prepare SP parameters
            var parameters = new Dictionary<string, object>
            {
                { "@BusinessName", model.Company },
                { "@Phone", model.Mobile },
                { "@Email", model.Email },
                { "@Password", encryptedPassword },
                { "@Role", "Admin" }
            };

            // Call the SP using DataService
            var resultTable = await _dataService.GetDataAsync("SP_Save_TenantRegister", parameters);

            string existsType = "NONE";
            if (resultTable != null && resultTable.Rows.Count > 0)
            {
                existsType = resultTable.Rows[0]["ExistsType"]?.ToString();
            }

            // Handle result
            if (existsType == "EMAIL")
                return BadRequest("Email already registered.");
            if (existsType == "MOBILE")
                return BadRequest("Mobile number already registered.");

            // TODO: Proceed to save the user
            return Ok("User registered successfully.");
        }

       
        public IActionResult Login()
        {
            return View();
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

    }
}
