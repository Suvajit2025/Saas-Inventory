namespace Invi.Models
{
    public class ClientModel
    {
    }

    public class PartyViewModel
    {
        public long PartyId { get; set; }
        public long TenantId { get; set; }
        public long? OrganizationId { get; set; }

        public string NamePrefix { get; set; }
        public string ContactName { get; set; }
        public string CompanyName { get; set; }
        public Guid PartyCode { get; set; }

        public int PartyTypeId { get; set; }
        public int CustomerTypeId { get; set; }
        public int? BillingModeId { get; set; }

        public string GSTIN { get; set; }
        public string PAN { get; set; }

        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public int StateId { get; set; }
        public string PinCode { get; set; }
        public string CreditDays { get; set; }
        public string Creditamount { get; set; }
        public bool IsActive { get; set; } = true;
        public bool EnablePortal { get; set; } = false;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public long? CreatedBy { get; set; }

        public List<StateList> stateLists { get; set; } = new();
        public List<PartyType> PartyTypes { get; set; } = new();
        public List<CustomerType> CustomerTypes { get; set; } = new();
        public List<BillingMode> BillingModes { get; set; } = new();
        public List<PaymentTerm> PaymentTerms { get; set; } = new();  // ✅ Renamed to avoid class/property conflict
        public List<DiscountModeMaster> DiscountModes { get; set; } = new();
        public List<AddressType> AddressTypes { get; set; } = new();

        public ContactPersonViewModel NewContactPerson { get; set; } = new();
        public List<ContactPersonViewModel> ContactPersons { get; set; } = new();
        public PartyAddressViewModel Partyaddress { get; set; } = new();
        public PartyOtherDetailsViewModel OtherDetails { get; set; } = new();
    }

    public class PartyType
    {
        public int PartyTypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
    }

    public class CustomerType
    {
        public int CustomerTypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
    }

    public class BillingMode
    {
        public int BillingModeId { get; set; }
        public string ModeCode { get; set; }
        public string ModeName { get; set; }
    }

    public class AddressType
    {
        public int AddressTypeId { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
    }
    public class StateList
    {
        public int StateId { get; set; }
        public string StateName { get; set; }
    }

    public class PaymentTerm   // ✅ renamed from PaymentTerms to avoid collision
    {
        public int PaymentTermsId { get; set; }
        public string TermCode { get; set; }
        public string TermName { get; set; }
        public int CreditDays { get; set; }
        public string? Description { get; set; }
    }

    public class DiscountModeMaster
    {
        public int DiscountModeId { get; set; }
        public string ModeCode { get; set; }
        public string ModeName { get; set; }
    }

    public class ContactPersonViewModel
    {
        public long ContactPersonId { get; set; }
        public long PartyId { get; set; }
        public string? Prefix { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? Department { get; set; }
        public string? Designation { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public bool? IsPrimary { get; set; } = false;
    }

    public class PartyAddressViewModel
    {
        // Common Fields
        public long PartyId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public bool IsDefault { get; set; } = false;
        public List<AddressType>? AddressTypes { get; set; }

        // Billing Address
        public int BillingAddressTypeId { get; set; } = 1;
        public string BillingAddressLineOne { get; set; } 
        public string? BillingAddressLineTwo { get; set; } = string.Empty;
        public string BillingCity { get; set; } = string.Empty;
        public int BillingStateId { get; set; }
        public string BillingPINCode { get; set; } = string.Empty;
        public string? BillingPhone { get; set; }
        public string? BillingEmail { get; set; }

        // Shipping Address
        public int ShippingAddressTypeId { get; set; } = 2;
        public string ShippingAddressLineOne { get; set; } = string.Empty;
        public string? ShippingAddressLineTwo { get; set; }
        public string ShippingCity { get; set; } = string.Empty;
        public int ShippingStateId { get; set; }
        public string ShippingPINCode { get; set; } = string.Empty;
        public string? ShippingPhone { get; set; }
        public string? ShippingEmail { get; set; }
    }


    public class PartyOtherDetailsViewModel
    {
        public long PartyOtherDetailsId { get; set; }
        public long PartyId { get; set; }
        public int PaymentTermsId { get; set; }
        public decimal? CreditLimit { get; set; }
        public string? BillingInstructions { get; set; }
        public string? DeliveryInstructions { get; set; }
        public string? GSTTreatment { get; set; }
        public bool IsTDSApplicable { get; set; } = false;
        public string? OtherNotes { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public List<PaymentTerm>? PaymentTermsList { get; set; }
    }
}
