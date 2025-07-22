namespace Invi.Models
{
    public class ProductModel
    {
        public long ProductId { get; set; }
        public long TenantId { get; set; }
        public long? OrganizationId { get; set; }
        public Guid ProductCode { get; set; }

        public int? ProductTypeId { get; set; }
        public string ProductName { get; set; }
        public string HSNSACCode { get; set; }
        public string Description { get; set; }

        public int? PurchaseUnitId { get; set; }
        public int? SalesUnitId { get; set; }

        public decimal? PurchasePrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? CostPrice { get; set; }

        public int? GstRateId { get; set; }
        public bool IsTaxInclusive { get; set; }
        public int? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }
        public long? CreatedBy { get; set; }

        // Dropdown Lists
        public List<GstMaster> GstList { get; set; }=new List<GstMaster>();
        public List<UnitMaster> UnitList { get; set; } = new List<UnitMaster>();
        public List<ProductType> ProductTypeList { get; set; } = new List<ProductType>();
    }
    public class GstMaster
    {
        public int GstId { get; set; }
        public string GstRate { get; set; } // e.g., "5%"
    }

    public class UnitMaster
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; } // e.g., "Box", "Piece"
    }

    public class ProductType
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; } // e.g., "Goods", "Services"
    }

}
