namespace Invi.Models
{
    public class ClientModel
    {
    }

    public class PartyViewModel
    {
        public long? PartyId { get; set; } // Nullable long
        public long? TenantId { get; set; } // Nullable long
        public long? OrganizationId { get; set; } // Nullable long

        public string? NamePrefix { get; set; } // Nullable string
        public string? ContactName { get; set; } // Nullable string
        public string? CompanyName { get; set; } // Nullable string
        public Guid? PartyCode { get; set; } // Nullable Guid

        public int? PartyTypeId { get; set; } // Nullable int
        public int? CustomerTypeId { get; set; } // Nullable int
        public int? BillingModeId { get; set; } // Nullable int

        public string? GSTIN { get; set; } // Nullable string
        public string? PAN { get; set; } // Nullable string

        public string? Phone { get; set; } // Nullable string
        public string? Email { get; set; } // Nullable string
        public string? Address { get; set; } // Nullable string
        public string? City { get; set; } // Nullable string
        public int? StateId { get; set; } // Nullable int
        public string? PinCode { get; set; } // Nullable string
        public string? CreditDays { get; set; } // Nullable string
        public string? Creditamount { get; set; } // Nullable string
        public bool? IsActive { get; set; } = null; // Nullable bool
        public bool? EnablePortal { get; set; } = null; // Nullable bool
        public DateTime? CreatedOn { get; set; } = null; // Nullable DateTime
        public long? CreatedBy { get; set; } // Nullable long
        public int? BusinessTypeId { get; set; }
        public List<StateList>? stateLists { get; set; } // Nullable List
        public List<PartyType>? PartyTypes { get; set; } // Nullable List
        public List<CustomerType>? CustomerTypes { get; set; } // Nullable List
        public List<BillingMode>? BillingModes { get; set; } // Nullable List
        public List<PaymentTerm>? PaymentTerms { get; set; } // Nullable List
        public List<DiscountModeMaster>? DiscountModes { get; set; } // Nullable List
        public List<AddressType>? AddressTypes { get; set; } // Nullable List

        public ContactPersonViewModel? NewContactPerson { get; set; } // Nullable object
        public List<ContactPersonViewModel>? ContactPersons { get; set; } // Nullable List
        public PartyAddressViewModel? Partyaddress { get; set; } // Nullable object
        public PartyOtherDetailsViewModel? OtherDetails { get; set; } // Nullable object
        public List<BusinessType>? BusinessTypes { get; set; } // Nullable List
    }

    public class PartyType
    {
        public int? PartyTypeId { get; set; } // Nullable int
        public string? TypeCode { get; set; } // Nullable string
        public string? TypeName { get; set; } // Nullable string
    }
    public class BusinessType
    {
        public int? BusinessTypeId { get; set; } // Nullable int
        public string? BusinessTypeName { get; set; } // Nullable string
    }

    public class CustomerType
    {
        public int? CustomerTypeId { get; set; } // Nullable int
        public string? TypeCode { get; set; } // Nullable string
        public string? TypeName { get; set; } // Nullable string
    }

    public class BillingMode
    {
        public int? BillingModeId { get; set; } // Nullable int
        public string? ModeCode { get; set; } // Nullable string
        public string? ModeName { get; set; } // Nullable string
    }

    public class AddressType
    {
        public int? AddressTypeId { get; set; } // Nullable int
        public string? TypeCode { get; set; } // Nullable string
        public string? TypeName { get; set; } // Nullable string
    }

    public class StateList
    {
        public int? StateId { get; set; } // Nullable int
        public string? StateName { get; set; } // Nullable string
    }

    public class PaymentTerm
    {
        public int? PaymentTermsId { get; set; } // Nullable int
        public string? TermCode { get; set; } // Nullable string
        public string? TermName { get; set; } // Nullable string
        public int? CreditDays { get; set; } // Nullable int
        public string? Description { get; set; } // Nullable string
    }

    public class DiscountModeMaster
    {
        public int? DiscountModeId { get; set; } // Nullable int
        public string? ModeCode { get; set; } // Nullable string
        public string? ModeName { get; set; } // Nullable string
    }

    public class ContactPersonViewModel
    {
        public long? ContactPersonId { get; set; } // Nullable long
        public long? PartyId { get; set; } // Nullable long
        public string? Prefix { get; set; } // Nullable string
        public string? FirstName { get; set; } // Nullable string
        public string? MiddleName { get; set; } // Nullable string
        public string? LastName { get; set; } // Nullable string
        public string? Department { get; set; } // Nullable string
        public string? Designation { get; set; } // Nullable string
        public string? Email { get; set; } // Nullable string
        public string? Phone { get; set; } // Nullable string
        public bool? IsPrimary { get; set; } = null; // Nullable bool
    }

    public class PartyAddressViewModel
    {
        public long? PartyId { get; set; } // Nullable long
        public DateTime? CreatedOn { get; set; } = null; // Nullable DateTime
        public bool? IsDefault { get; set; } = null; // Nullable bool
        public List<AddressType>? AddressTypes { get; set; } // Nullable List

        public int? BillingAddressTypeId { get; set; } = null; // Nullable int
        public string? BillingAddressLineOne { get; set; } // Nullable string
        public string? BillingAddressLineTwo { get; set; } = null; // Nullable string
        public string? BillingCity { get; set; } = null; // Nullable string
        public int? BillingStateId { get; set; } = null; // Nullable int
        public string? BillingPINCode { get; set; } = null; // Nullable string
        public string? BillingPhone { get; set; } = null; // Nullable string
        public string? BillingEmail { get; set; } = null; // Nullable string

        public int? ShippingAddressTypeId { get; set; } = null; // Nullable int
        public string? ShippingAddressLineOne { get; set; } = null; // Nullable string
        public string? ShippingAddressLineTwo { get; set; } = null; // Nullable string
        public string? ShippingCity { get; set; } = null; // Nullable string
        public int? ShippingStateId { get; set; } = null; // Nullable int
        public string? ShippingPINCode { get; set; } = null; // Nullable string
        public string? ShippingPhone { get; set; } = null; // Nullable string
        public string? ShippingEmail { get; set; } = null; // Nullable string
    }

    public class PartyOtherDetailsViewModel
    {
        public long? PartyOtherDetailsId { get; set; } // Nullable long
        public long? PartyId { get; set; } // Nullable long
        public int? PaymentTermsId { get; set; } = null; // Nullable int
        public decimal? CreditLimit { get; set; } = null; // Nullable decimal
        public string? BillingInstructions { get; set; } = null; // Nullable string
        public string? DeliveryInstructions { get; set; } = null; // Nullable string
        public string? GSTTreatment { get; set; } = null; // Nullable string
        public bool? IsTDSApplicable { get; set; } = null; // Nullable bool
        public string? OtherNotes { get; set; } = null; // Nullable string
        public DateTime? CreatedOn { get; set; } = null; // Nullable DateTime
        public List<PaymentTerm>? PaymentTermsList { get; set; } = null; // Nullable List
    }
}
