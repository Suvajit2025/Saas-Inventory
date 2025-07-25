﻿using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Invi.Controllers
{
    public class ClientController : Controller
    {
        private readonly DataService _dataService;
        private readonly GeneralSevice _generalService;

        public ClientController(DataService dataSevice, EncryptDecryptService encryptDecryptService, GeneralSevice generalSevice)
        {
            _generalService = generalSevice;
            _dataService = dataSevice;
        }

        public async Task<IActionResult> Index()
        {
            var ds = await _dataService.GetAllDatasetAsync("SP_Get_PartyMasterData", new Dictionary<string, object>());
            var viewModel = new PartyViewModel
            {
                AddressTypes = ds.Tables[0].AsEnumerable().Select(row => new AddressType
                {
                    AddressTypeId = Convert.ToInt32(row["AddressTypeId"]),
                    TypeCode= row["TypeCode"].ToString(),
                    TypeName = row["TypeName"].ToString()
                }).ToList(),

                BillingModes = ds.Tables[1].AsEnumerable().Select(row => new BillingMode
                {
                    BillingModeId = Convert.ToInt32(row["BillingModeId"]),
                    ModeCode = row["ModeCode"].ToString(),
                    ModeName = row["ModeName"].ToString()
                }).ToList(),

                CustomerTypes = ds.Tables[2].AsEnumerable().Select(row => new CustomerType
                {
                    CustomerTypeId = Convert.ToInt32(row["CustomerTypeId"]),
                    TypeCode = row["TypeCode"].ToString(),
                    TypeName = row["TypeName"].ToString()
                }).ToList(),

                DiscountModes = ds.Tables[3].AsEnumerable().Select(r => new DiscountModeMaster
                {
                    DiscountModeId = Convert.ToInt32(r["DiscountModeId"]),
                    ModeCode = r["ModeCode"].ToString(),
                    ModeName = r["ModeName"].ToString()
                }).ToList(),

                PartyTypes = ds.Tables[4].AsEnumerable().Select(r => new PartyType
                {
                    PartyTypeId = Convert.ToInt32(r["PartyTypeId"]),
                    TypeCode = r["TypeCode"].ToString(),
                    TypeName = r["TypeName"].ToString()
                }).ToList(),

                PaymentTerms = ds.Tables[5].AsEnumerable().Select(r => new PaymentTerm
                {
                    PaymentTermsId = Convert.ToInt32(r["PaymentTermsId"]),
                    TermCode = r["TermCode"].ToString(),
                    TermName = r["TermName"].ToString(),
                    CreditDays = Convert.ToInt32(r["CreditDays"]),
                    Description = r["Description"].ToString()
                }).ToList(),
                Partyaddress = new PartyAddressViewModel
                {
                    BillingAddressTypeId = 1, // Default to Billing
                    ShippingAddressTypeId = 2  // Default to Shipping
                },
                stateLists = ds.Tables[6].AsEnumerable().Select(r => new StateList
                {
                    StateId = Convert.ToInt32(r["StateId"]),
                    StateName = r["StateName"].ToString()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> SavePartyDetails(PartyViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Process the data (e.g., save to database)
                    // Example: Save the party data (you can replace this with your actual database save logic)
                    // _partyService.SaveParty(model);

                    // If the data was saved successfully, return a success response with a message
                    return Json(new { success = true, message = "Party details saved successfully." });
                }
                catch (Exception ex)
                {
                    // Handle any errors that occur during the process
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
            else
            {
                // If the model validation fails, return the validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage).ToList();
                return Json(new { success = false, message = string.Join(", ", errors) });
            }
        }

    }
}
