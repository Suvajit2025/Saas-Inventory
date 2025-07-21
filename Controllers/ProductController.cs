using Invi.HelperClass;
using Invi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Invi.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataService _dataService;
        private readonly GeneralSevice _generalService;
        public ProductController(DataService dataSevice, EncryptDecryptService encryptDecryptService, GeneralSevice generalSevice)
        {
            _generalService = generalSevice;
            _dataService = dataSevice;
        }
        public async Task<IActionResult> Index()
        {
            var ds = await _dataService.GetAllDatasetAsync("SP_Get_PartyMasterData", new Dictionary<string, object>());

            var model = new ProductModel
            {
                GstList = ds.Tables[7].AsEnumerable().Select(row => new GstMaster
                {
                    GstId = Convert.ToInt32(row["GstId"]),
                    GstRate = row["GstRate"].ToString(),
                }).ToList(),

                ProductTypeList = ds.Tables[8].AsEnumerable().Select(row => new ProductType
                {
                    ProductTypeId = Convert.ToInt32(row["ProductTypeId"]),
                    ProductTypeName = row["ProductTypeName"].ToString(),
                }).ToList(),

                UnitList = ds.Tables[9].AsEnumerable().Select(row => new UnitMaster
                {
                    UnitId = Convert.ToInt32(row["UnitId"]),
                    UnitName = row["UnitName"].ToString(),
                }).ToList(),
            };

            return View(model);
        }
        

    }
}
