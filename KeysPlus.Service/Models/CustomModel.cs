using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Routing;

namespace KeysPlus.Service.Models
{
    public class ChangePasswordModel
    {
        [Required(ErrorMessage = "* Current password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        [Remote("IsPassValid", "Account", ErrorMessage = "* Please enter correct current password.")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "* New password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        [MinLength(7, ErrorMessage = "* The new password must be at least 7 characters.")]
        [Remote("IsPassNew", "Account", ErrorMessage = "* New Password must differ from old password.")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "* Confirm password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [MinLength(7, ErrorMessage = "* The confirm password must be at least 7 characters."),]
        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "* The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public bool HasError { get; set; }

        public List<ErrorModel> ErrorList { get; set; }
    }

    public class ErrorModel
    {
        public string PropertyName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class SendGridEmailModel
    {
        public string RecipentName { get; set; }
        public string OwnerName { get; set; }
        public string TenantName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ButtonText { get; set; }
        public string ButtonUrl { get; set; }
        public string RecipentEmail { get; set; }
        public string NewUserName { get; set; }
        public string NewPassWord { get; set; }
        public string Address { get; set; }
        public string JobTitle { get; set; }
        public string PersonType { get; set; }
        public string Date { get; set; }
    }

    public class Recipient
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PersonType { get; set; }
    }

    public enum EmailType
    {
        ActivationEmail,
        ForGetPasswordEmail,
        ResetPasswordEmail,
        OwnerCreatNewTenantEmail,
        OwnerAddTenantEmail,
        NewApplicationEmail,
        ApplicationUpdatedEmail,
        NewRequestEmail,
        AcceptRequestEmail,
        UpdateRequestEmail,
        InspectionUpdatedEmail,
        NewQuoteEmail,
        QuoteUpdatedEmail,
        AcceptQuote,
        AcceptRentalApplication,
        DeletePropertyRentApplicationDeclined,
        DeletePropertyRentalCanceled,
        DeletePropertyJobCanceled,
        DeleteRentalListingRentApplicationDeclined,
        TenantPaymentReminder,
        OwnerUpcomingRentalPayment,
        DeclineQuote,
        DeclineRequest
    }

    public enum AllowedFileType
    {
        AllFiles,
        Images,
        Document
    }

    public enum MediaType
    {
        Image = 1,
        Document = 2
    }
    public class GetFinanceReportModel
    {
        public int PropertyId { get; set; }
    }
    public class FindTransportModel
    {
        public string Mode { get; set; }
        public string LocationType { get; set; }
        public decimal DestLat { get; set; }
        public decimal DestLon { get; set; }
        public decimal DistanceBounding { get; set; }
    }
    public class FindSchoolsModel
    {
        public string SchoolTypeCommon { get; set; }
        public decimal DestLat { get; set; }
        public decimal DestLon { get; set; }
        public decimal DistanceBounding { get; set; }
    }

    public class FindSchoolsResultModel
    {
        public FindSchoolsResultModel()
        {

        }
        public string SchoolName { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string SchoolTypeCommon { get; set; }
        public string SchoolType { get; set; }
        public string RegionalCouncil { get; set; }
        public decimal SchoolLat { get; set; }
        public decimal SchoolLon { get; set; }
        public double DistanceBoundingInKM { get; set; }
        public string CountryName { get; set; }
    }

    public class FindTransportsResultModel
    {
        public FindTransportsResultModel()
        {

        }
        public int StopId { get; set; }
        public string StopName { get; set; }
        public decimal StopLat { get; set; }
        public decimal StopLon { get; set; }
        public string LocationType { get; set; }
        public string CountryName { get; set; }
        public double DistanceBoundingInKM { get; set; }
    }
    public class  PropertyFinanceResultModel{
        public decimal AnnualRentAtCurrentYear { get; set; }
        public decimal RentAmountPerWeek { get; set; }
        public decimal NetYieldPercent { get; set; }
        public decimal GrossYieldPercent { get; set; }
        public decimal AverageSuburbAnnualGrowth { get; set; }
        public decimal TotalExpenseAtCurrentYear { get; set; }
        public decimal IndicatedHomeValue { get; set; }
        public decimal CashflowPreTaxAtCurrentYear {get; set;}
        public decimal CapitalGainAtCurrentYear { get; set; }
        public decimal EstimatedSalesPriceAtYear5 { get; set; }
        public decimal AnnualRentAtYear5 { get; set; }
        public decimal TotalExpenseAtYear5 { get; set; }
        public decimal CashflowPreTaxAtYear5 { get; set; }
        public decimal CapitalGainAtYear5 {get ;set;}
        public decimal EstimatedSalesPriceAtYear10 { get; set; }
        public decimal AnnualRentAtYear10 { get; set; }
        public decimal TotalExpenseAtYear10 {get; set;}
        public decimal CashflowPreTaxAtYear10 { get; set; }
        public decimal CapitalGainAtYear10 { get; set; }
        public decimal MonthlyRentalIncome { get; set; }
        public decimal MonthlyExpense { get; set; }
        public decimal MonthlyMoneyInPocket { get; set; }
    }
    public abstract class ItemModel
    {
        public int Id { get; set; }
        public AddressViewModel Address { get; set; }
        public List<MediaModel> MediaFiles { get; set; }
    }
    public class PagedInput
    {
        public String ActionName { get; set; }
        public String ControllerName { get; set; }
        public RouteValueDictionary PagedLinkValues { get; set; }
    }
    public class SortOrderModel
    {
        public string SortOrder { get; set; }
        public string ActionName { get; set; }
        public RouteValueDictionary RouteValues { get; set; }
    }

    public class SearchInput
    {
        public String Name { get; set; }
        public String Value { get; set; }
    }

    public class WatchListItem<T> : ItemModel
    {
        public T Model  { get; set; }
    }

    public class WatchListDisplayModel : SearchInputModel
    {
        public string EditUrl { get; set; }
        public string DeleteUrl { get; set; }

        public string Title { get; set; }
    }
}
