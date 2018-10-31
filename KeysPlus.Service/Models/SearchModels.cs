using KeysPlus.Data;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KeysPlus.Service.Models
{
    public class SearchInputModel
    {
        public SearchInputModel()
        {
            Page = 1;
            InputValues = new List<SearchInput>();
        }

        public int Page { get; set; }
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public int SearchCount { get; set; }
        public bool? NoResultFound { get; set; }
        public int PageCount { get; set; }
        public string ReturnUrl { get; set; }
        public IEnumerable<SortOrderModel> SortOrders { get; set; }
        public IPagedList Items { get; set; }
        public IEnumerable<SearchInput> InputValues { get; set; }
        public PagedInput PagedInput { get; set; }
    }

    public class AdvanceSearchInput : SearchInputModel
    {
        public string Region { get; set; }
        public string City { get; set; }
        public IList<string> Suburbs { get; set; }
    }

    public class AdvancedMarketJobSearchModel : AdvanceSearchInput
    {
        public string Title { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public string Suburb { get; set; }
        public List<string> SuburbList { get; set; }
        public bool IsUserServiceSupply { get; set; }
        public bool IsProfileComplete {get; set;}
        public bool IfNotNull()
        {
            if(MaxBudget!=0)
            {
                return true;
            }
            else if(MinBudget!=0)
            {
                return true;
            }
            else if(Suburb!="")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class RentalAdvancedSearchViewModel : SearchInputModel
    {
        public string Title { get; set; }
        public AddressViewModel Address { get; set; }
        public string PropertyDescription { get; set; }
        public string RentalDescription { get; set; }
        public decimal? MovingCost { get; set; }
        public decimal? RentMin { get; set; }
        public decimal? RentMax { get; set; }
        public DateTime? AvailableDate { get; set; }
        public string Furnishing { get; set; }
        public string IdealTenant { get; set; }
        public int? OccupantCount { get; set; }
        public string PetsAllowed { get; set; }
        public string PropertyName { get; set; }
        public string PropertyAddress { get; set; }
        public List<String> PropertyType { get; set; }
        public int PropertyTypeId { get; set; }
        public List<PropertyType> PropType { get; set; }
        public string RentalPaymentType { get; set; }
        public int? BedroomMin { get; set; }
        public int? BedroomMax { get; set; }
        public int? BathroomMin { get; set; }
        public int? BathroomMax { get; set; }
        public double? LandSqmMin { get; set; }
        public double? LandSqmMax { get; set; }
        public int? ParkingSpaces { get; set; }
        public double? FloorArea { get; set; }
        public bool IsUserTenant { get; set; }
        public bool IsTenantProfileComplete { get; set; }       

        public bool CheckForNull(object myObject)
        {
            string stringValue = "";
            int intValue;
            decimal decValue;
            foreach (PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if (pi.PropertyType == typeof(string))
                {
                    stringValue = (string)pi.GetValue(myObject);
                    if (!(string.IsNullOrEmpty(stringValue)))
                    {
                        return true;
                    }

                }
                else if (pi.PropertyType == typeof(AddressViewModel))
                {
                    if (Address.SuburbList.Count > 0)
                    {
                        return true;
                    }

                }
                else if (pi.PropertyType == typeof(List<String>))
                {
                    if (PropertyType.Count > 0)
                    {
                        return true;
                    }
                }
                else if (Nullable.GetUnderlyingType(pi.PropertyType) != null)
                {
                    if (pi.GetValue(this, null) != null)
                    {
                        if (pi.PropertyType == typeof(decimal))
                        {
                            decValue = (decimal)pi.GetValue(myObject);
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            intValue = (int)pi.GetValue(myObject);
                        }
                        return true;
                    }

                }
            }
            return false;
        }
    }
    public class SearchResult
    {
        public int SearchCount { get; set; }
        public IPagedList Items { get; set; }
        public bool? NoResultFound { get; set; }
    }

}
