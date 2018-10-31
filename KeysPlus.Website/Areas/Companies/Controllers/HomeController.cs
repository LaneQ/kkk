using KeysPlus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using KeysPlus.Service.Models;
using CompanyViewModel = KeysPlus.Service.Models.CompanyViewModel;
using KeysPlus.Service.Services;
using System.Web.Routing;

namespace KeysPlus.Website.Areas.Companies.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();
        public ActionResult MyJobs(SSMyJobsSearchModel model)
        {
            var user = User.Identity.Name;
            var login = AccountService.GetLoginByEmail(user);
            if (String.IsNullOrWhiteSpace(model.SortOrder))
            {
                model.SortOrder = "Latest Jobs";
            }
            var res = CompanyService.GetJobs(model, login);
            model.PagedInput = new PagedInput
            {
                ActionName = "MyJobs",
                ControllerName = "Home",
                PagedLinkValues = new RouteValueDictionary(new { SortOrder = model.SortOrder, SearchString = model.SearchString })
            };
            var rvr = new RouteValueDictionary(new { SearchString = model.SearchString });
            var sortOrders = new List<SortOrderModel>();
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Progress", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Progress", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Progress") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Highest Accepted Quote", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Highest Accepted Quote") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Lowest Accepted Quote", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Lowest Accepted Quote") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Latest Jobs", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Latest Jobs") });
            sortOrders.Add(new SortOrderModel { SortOrder = "Earliest Jobs", ActionName = "MyJobs", RouteValues = rvr.AddRouteValue("SortOrder", "Earliest Jobs") });
            model.SortOrders = sortOrders;
            model.SearchCount = res.SearchCount;
            if (String.IsNullOrWhiteSpace(model.SearchString)) model.Page = 1;
            model.PageCount = res.Items.PageCount;
            model.Items = res.Items;
            model.EditUrl = "/Jobs/Home/UpdateJobStatus";
            TempData["CurrentLink"] = "My Jobs";
            return View(model);
        }

        [HttpPost]
        public ActionResult AddNewCompany(CompanyViewModel model)
        {
            var status = true;
            var message = "Company added";
            var data = model;
            var newCompanyModel = new CompanyViewModel();
            if (!ModelState.IsValid)
            {
                return Json("Something went wrong");
            }
            else
            {
                if (model.IsShipSame)
                {
                    var physicalAddress = new Address
                    {
                        CountryId = 1,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        UpdatedBy = User.Identity.Name,
                        UpdatedOn = DateTime.Now,
                        Number = model.PhysicalAddress.Number,
                        Street = model.PhysicalAddress.Street,
                        City = model.PhysicalAddress.City,
                        Suburb = model.PhysicalAddress.Suburb,
                        PostCode = model.PhysicalAddress.PostCode
                    };
                    db.Address.Add(physicalAddress);
                    db.SaveChanges();
                    var company = new Company()
                    {
                        IsActive = true,
                        Name = model.Name,
                        Website = model.Website,
                        PhoneNumber = model.PhoneNumber,
                        UpdatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        UpdatedOn = DateTime.Now,
                        PhysicalAddressId = physicalAddress.AddressId,
                        BillingAddressId = physicalAddress.AddressId

                    };
                    newCompanyModel.PhysicalAddress = new AddressViewModel
                    {
                        AddressId = physicalAddress.AddressId,
                        CountryId = physicalAddress.CountryId,
                        Number = physicalAddress.Number,
                        Street = physicalAddress.Street,
                        City = physicalAddress.City,
                        Suburb = physicalAddress.Suburb,
                        PostCode = physicalAddress.PostCode
                    };
                    newCompanyModel.BillingAddress = new AddressViewModel
                    {
                        AddressId = physicalAddress.AddressId,
                        CountryId = physicalAddress.CountryId,
                        Number = physicalAddress.Number,
                        Street = physicalAddress.Street,
                        City = physicalAddress.City,
                        Suburb = physicalAddress.Suburb,
                        PostCode = physicalAddress.PostCode
                    };
                    db.Company.Add(company);
                    db.SaveChanges();
                    newCompanyModel.Id = company.Id;
                    newCompanyModel.IsActive = company.IsActive;
                    newCompanyModel.Name = company.Name;
                    newCompanyModel.Website = company.Website;
                    data = newCompanyModel;
                }
                else
                {
                    var physicalAddress = new Address
                    {
                        CountryId = 1,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        UpdatedBy = User.Identity.Name,
                        UpdatedOn = DateTime.Now,
                        Number = model.PhysicalAddress.Number,
                        Street = model.PhysicalAddress.Street,
                        City = model.PhysicalAddress.City,
                        Suburb = model.PhysicalAddress.Suburb,
                        PostCode = model.PhysicalAddress.PostCode

                    };
                    db.Address.Add(physicalAddress);
                    db.SaveChanges();

                    var billingAddress = new Address
                    {
                        CountryId = 1,
                        CreatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        UpdatedBy = User.Identity.Name,
                        UpdatedOn = DateTime.Now,
                        Number = model.BillingAddress.Number,
                        Street = model.BillingAddress.Street,
                        City = model.BillingAddress.City,
                        Suburb = model.BillingAddress.Suburb,
                        PostCode = model.BillingAddress.PostCode

                    };
                    db.Address.Add(billingAddress);
                    db.SaveChanges();
                    var company = new Company()
                    {
                        IsActive = true,
                        Name = model.Name,
                        Website = model.Website,
                        PhoneNumber = model.PhoneNumber,
                        UpdatedBy = User.Identity.Name,
                        CreatedOn = DateTime.Now,
                        CreatedBy = User.Identity.Name,
                        UpdatedOn = DateTime.Now,
                        PhysicalAddressId = physicalAddress.AddressId,
                        BillingAddressId = billingAddress.AddressId

                    };
                    newCompanyModel.PhysicalAddress = new AddressViewModel
                    {
                        AddressId = physicalAddress.AddressId,
                        CountryId = physicalAddress.CountryId,
                        Number = physicalAddress.Number,
                        Street = physicalAddress.Street,
                        City = physicalAddress.City,
                        Suburb = physicalAddress.Suburb,
                        PostCode = physicalAddress.PostCode
                    };
                    newCompanyModel.BillingAddress = new AddressViewModel
                    {
                        AddressId = billingAddress.AddressId,
                        CountryId = billingAddress.CountryId,
                        Number = billingAddress.Number,
                        Street = billingAddress.Street,
                        City = billingAddress.City,
                        Suburb = billingAddress.Suburb,
                        PostCode = billingAddress.PostCode
                    };
                    db.Company.Add(company);
                    db.SaveChanges();

                    newCompanyModel.Id = company.Id;
                    newCompanyModel.IsActive = company.IsActive;
                    newCompanyModel.Name = company.Name;
                    newCompanyModel.Website = company.Website;

                    data = newCompanyModel;
                }
            }
            return Json(new
            {
                success = status,
                message = message,
                data = data
            });
        }

        public JsonResult ImageUpload(HttpPostedFileBase Files, int id)
        {
            var message = "Photo added successfully";
            var status = true;
            var findCompany = db.Company.Find(id);

            List<string> acceptedExtensions = new List<string>
                {
                    ".jpg",
                    ".png",
                    ".gif",
                    ".jpeg"
                };

            var photo = Request.Files;
            for (int i = 0; i < photo.Count; i++)
            {
                var file = photo[i];
                var fileExtension = Path.GetExtension(file.FileName);

                if (fileExtension != null && !acceptedExtensions.Contains(fileExtension.ToLower()))
                {
                    message = "Supported file types are *.jpg, *.png, *.gif, *.jpeg";
                    status = false;
                    break;
                }
                else
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var index = fileName.LastIndexOf(".");
                    var newFileName = fileName.Insert(index, $"{Guid.NewGuid()}");
                    var physicalPath = Path.Combine(Server.MapPath("~/images"), newFileName);

                    file.SaveAs(physicalPath);
                    findCompany.ProfilePhoto = newFileName;
                    db.SaveChanges();
                }
            }
            return Json(new
            {
                success = status,
                message = message
            });
        }

        [HttpPost]
        public ActionResult EditCompany(CompanyViewModel model)
        {
            var foundCompany1 = db.Company.FirstOrDefault(x => x.Id == model.Id);
            if (foundCompany1 != null)
            {
                if (ModelState.IsValid)
                {

                    foundCompany1.Id = model.Id;
                    foundCompany1.Name = model.Name;
                    foundCompany1.Website = model.Website;
                    foundCompany1.PhoneNumber = model.PhoneNumber;
                    foundCompany1.UpdatedBy = User.Identity.Name;
                    foundCompany1.UpdatedOn = Convert.ToDateTime(DateTime.Now);

                    if (foundCompany1.PhysicalAddressId == foundCompany1.BillingAddressId)
                    {
                        if (model.IsShipSame) //check box is checked
                        { //updating physical address only if intially same and current physical addresses are same 
                            foundCompany1.Address1.Number = model.PhysicalAddress.Number;
                            foundCompany1.Address1.Street = model.PhysicalAddress.Street;
                            foundCompany1.Address1.City = model.PhysicalAddress.City;
                            foundCompany1.Address1.Suburb = model.PhysicalAddress.Suburb;
                            foundCompany1.Address1.PostCode = model.PhysicalAddress.PostCode;
                            
                            db.SaveChanges();

                        }
                        else //Not same
                        {
                            if (model.PhysicalAddress == model.BillingAddress)
                            {
                                model.IsShipSame = false;
                            }
                            else
                            {
                                // Physical address updating 
                                foundCompany1.Address1.Number = model.PhysicalAddress.Number;
                                foundCompany1.Address1.Street = model.PhysicalAddress.Street;
                                foundCompany1.Address1.City = model.PhysicalAddress.City;
                                foundCompany1.Address1.Suburb = model.PhysicalAddress.Suburb;
                                foundCompany1.Address1.PostCode = model.PhysicalAddress.PostCode;

                                //Generating new billing address
                                Address billingAddress = new Address
                                {
                                    CountryId = 1,
                                    Number = model.BillingAddress.Number,
                                    Street = model.BillingAddress.Street,
                                    Suburb = model.BillingAddress.Suburb,
                                    City = model.BillingAddress.City,
                                    PostCode = model.BillingAddress.PostCode,
                                };
                                db.Address.Add(billingAddress);
                                foundCompany1.BillingAddressId = billingAddress.AddressId;
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        if (!model.IsShipSame) //is not checked
                        {
                            //Physical address updated
                            foundCompany1.Address1.Number = model.PhysicalAddress.Number;
                            foundCompany1.Address1.Street = model.PhysicalAddress.Street;
                            foundCompany1.Address1.City = model.PhysicalAddress.City;
                            foundCompany1.Address1.Suburb = model.PhysicalAddress.Suburb;
                            foundCompany1.Address1.PostCode = model.PhysicalAddress.PostCode;

                            //Billing Address updated
                            foundCompany1.Address.Number = model.BillingAddress.Number;
                            foundCompany1.Address.Street = model.BillingAddress.Street;
                            foundCompany1.Address.Suburb = model.BillingAddress.Suburb;
                            foundCompany1.Address.City = model.BillingAddress.City;
                            foundCompany1.Address.PostCode = model.BillingAddress.PostCode;
                            db.SaveChanges();

                        }
                        else
                        {
                            //physical address updating
                            var bill = db.Address.Where(p => p.AddressId == foundCompany1.BillingAddressId).First();
                            foundCompany1.Address1.Number = model.PhysicalAddress.Number;
                            foundCompany1.Address1.Street = model.PhysicalAddress.Street;
                            foundCompany1.Address1.City = model.PhysicalAddress.City;
                            foundCompany1.Address1.Suburb = model.PhysicalAddress.Suburb;
                            foundCompany1.Address1.PostCode = model.PhysicalAddress.PostCode;
                            //billing address and physical adress are same
                            foundCompany1.BillingAddressId = foundCompany1.PhysicalAddressId;
                            db.Address.Remove(bill);
                            db.SaveChanges();

                        }
                    }
                    if (model.PhotoProfile == null)
                    {
                        foundCompany1.ProfilePhoto = model.PhotoProfile;
                        db.SaveChanges();
                    }
                }
                else
                {
                    return Json(new { Success = false, Message = "Model State isnt valid!" });
                }
            }
            else
            {
                return Json(new { Success = false, Message = "Company not found!" });
            }
            return Json(new { Success = true, Message = "Company details has been updated successfully!" });

        }

        [HttpPost]
        public ActionResult DeleteCompany(int id)
        {
            var foundCompany = db.Company.Find(id);
            if (foundCompany == null && foundCompany.IsActive == false)
            {
                return Json(new
                {
                    Success = false,
                    Message = "Cannot find this record !"
                });
            }
            else
            {
                foundCompany.IsActive = false;
                db.SaveChanges();

                return Json(new
                {
                    success = true,
                    Message = "SuccessFully Deleted!",
                    id = id
                });
            }
        }
        protected override void Dispose(bool disposing)
        {
            db?.Dispose();
            base.Dispose(disposing);
        }

    }
}

