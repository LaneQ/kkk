using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KeysPlus.Website.Models;
using KeysPlus.Website.Areas.Admin.Models;
using PagedList;
using Microsoft.AspNet.Identity;
using KeysPlus.Data;
using KeysPlus.Website.Areas.Tools;
using KeysPlus.Service.Models;

namespace KeysPlus.Website.Areas.Admin.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        KeysEntities db = new KeysEntities();
        // GET: Users/Home
        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            // var userContext = db.Person;
            var loginContext = db.Login;
            var personContext = db.Person;
            var listofRoles = db.Role;
            var listofRoleList = db.LoginRole;
            ViewBag.SearchCount = 1;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;
            ViewBag.FirstNameSortParm = sortOrder == "Fname_asc" ? "Fname" : "Fname_asc";
            ViewBag.MiddleNameSortParm = sortOrder == "Mname_asc" ? "Mname" : "Mname_asc";
            ViewBag.LastNameSortParm = sortOrder == "Lname_asc" ? "Lname" : "Lname_asc";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            //ViewBag.CurrentFilter = searchString;
            //ViewBag.CurrentFilter2 = searchString;

        // login table join with person table
        var userContextFull = from login in loginContext
                                  join p in personContext
                                  on login.Id equals p.Id
                                  where p.IsActive == true
                                  select new
                                  {
                                      Id = p.Id,
                                      //  Loginrole = login.LoginRole,
                                      UserName = login.UserName,
                                      LoginId = p.Id,
                                      FirstName = p.FirstName,
                                      MiddleName = p.MiddleName ?? "",
                                      LastName = p.LastName,
                                      Language = p.Language,
                                      PlaceOfBirth = p.PlaceOfBirth,
                                      Occupation=p.Occupation,
                                      LinkedinUrl=p.LinkedinUrl,
                                      IsActive = p.IsActive,
                                      CreatedOn = p.CreatedOn.ToString(),
                                      ProfilePhoto = p.ProfilePhoto,
                                      //ProfilePhoto = new MediaModel
                                      //{
                                      //    OldFileName = p.ProfilePhoto

                                      //},
                                      Address1 = new AddressViewModel
                                      {
                                          AddressId = p.PhysicalAddressId,
                                          CountryId = p.Address1.CountryId,
                                          Number = p.Address1.Number,
                                          Street = p.Address1.Street,
                                          City = p.Address1.City,
                                          Suburb = p.Address1.Suburb,
                                          PostCode = p.Address1.PostCode,
                                      },
                                      Address = new AddressViewModel
                                      {
                                          AddressId = p.BillingAddressId,
                                          CountryId = p.Address.CountryId,
                                          Number = p.Address.Number,
                                          Street = p.Address.Street,
                                          City = p.Address.City,
                                          Suburb = p.Address.Suburb,
                                          PostCode = p.Address.PostCode,
                                      }

                                  };

            var userContext = userContextFull;

            switch (sortOrder)
            {
                
                case "Fname":
                    userContext = userContext.OrderByDescending(s => s.FirstName);
                    break;
                case "Fname_asc":
                    userContext = userContext.OrderBy(s => s.FirstName);
                    break;
               
                case "Mname":
                    userContext = userContext.OrderByDescending(s => s.MiddleName);
                    break;
                case "Mname_asc":
                    userContext = userContext.OrderBy(s => s.MiddleName);
                    break;
              
                case "Lname":
                    userContext = userContext.OrderByDescending(s => s.LastName);
                    break;
                case "Lname_asc":
                    userContext = userContext.OrderBy(s => s.LastName);
                    break;
                default:
                    userContext = userContext.OrderBy(s => s.FirstName);
                    sortOrder = "Fname_asc";
                    break;
            }

            if (!String.IsNullOrWhiteSpace(searchString))
            {
                SearchTool searchTool = new SearchTool();
                int searchType = searchTool.CheckDisplayType(searchString);
                string formatString = searchTool.ConvertString(searchString);
                switch (searchType)
                {
                    case 1:
                        userContext = userContext.Where(userlist => userlist.FirstName.EndsWith(formatString)
                                                         || userlist.MiddleName.EndsWith(formatString)
                                                         || userlist.LastName.EndsWith(formatString)
                                                         || userlist.Language.EndsWith(formatString)
                                                         || userlist.Address.City.EndsWith(formatString)
                                                         || userlist.Address.Suburb.EndsWith(formatString)
                                                         || userlist.Address1.City.EndsWith(formatString)
                                                         || userlist.Address1.Suburb.EndsWith(formatString)
                                                         );
                        break;
                    case 2:
                        userContext = userContext.Where(userlist => userlist.FirstName.StartsWith(formatString)
                                                         || userlist.MiddleName.StartsWith(formatString)
                                                         || userlist.LastName.StartsWith(formatString)
                                                         || userlist.Language.StartsWith(formatString)
                                                         || userlist.Address.City.StartsWith(formatString)
                                                         || userlist.Address.Suburb.StartsWith(formatString)
                                                         || userlist.Address1.City.StartsWith(formatString)
                                                         || userlist.Address1.Suburb.StartsWith(formatString)
                                                         );
                        break;
                    case 3:
                        userContext = userContext.Where(userlist => userlist.FirstName.Contains(formatString)
                                                         || userlist.MiddleName.Contains(formatString)
                                                         || userlist.LastName.Contains(formatString)
                                                         || userlist.Language.Contains(formatString)
                                                         || userlist.Address.City.Contains(formatString)
                                                         || userlist.Address.Suburb.Contains(formatString)
                                                         || userlist.Address1.City.Contains(formatString)
                                                         || userlist.Address1.Suburb.Contains(formatString)
                                                          );
                        break;
                }
            }

            if (userContext.Count() == 0)
            {
                ViewBag.SearchCount = 0;
                TempData["search"] = searchString ?? "";
                ViewBag.CurrentFilter = "";
                //ViewBag.CurrentFilter = searchString ?? "";
                userContext = userContextFull.OrderBy(q => q.FirstName);
            }

            int pageNumber = (page ?? 1);

            var users = userContext
                .Where(q => q.IsActive == true)
                .Select(q => new UserViewModel
                {
                    Id = q.Id,
                    LoginId = q.LoginId,
                    UserName = q.UserName,
                    FirstName = q.FirstName,
                    MiddleName = q.MiddleName ?? "",
                    LastName = q.LastName,
                    Language = q.Language,
                    PlaceOfBirth = q.PlaceOfBirth,
                    Occupation=q.Occupation,
                    Linkedin=q.LinkedinUrl,
                    IsActive = q.IsActive,
                    CreatedOn = q.CreatedOn.ToString(),
                    ProfilePhoto = q.ProfilePhoto,

                    PhysicalAddress = new AddressViewModel
                    {
                        AddressId = q.Address1.AddressId,
                        CountryId = q.Address1.CountryId,
                        Number = q.Address1.Number,
                        Street = q.Address1.Street,
                        City = q.Address1.City,
                        Suburb = q.Address1.Suburb,
                        PostCode = q.Address1.PostCode,
                    },
                    BillingAddress = new AddressViewModel
                    {
                        AddressId = q.Address.AddressId,
                        CountryId = q.Address.CountryId,
                        Number = q.Address.Number,
                        Street = q.Address.Street,
                        City = q.Address.City,
                        Suburb = q.Address.Suburb,
                        PostCode = q.Address.PostCode,
                    }



                }).ToList();   //.ToPagedList(pageNumber, 10);

            users.ForEach(x =>
            {

                x.ProfilePhoto = Url.Content("~/images/" + x.ProfilePhoto);
            });




            foreach (var item in users)
            {
                //users.ForEach(x =>
                //{
                //    x.MediaFiles.ForEach(y => {
                //        y.Data = Url.Content("~/images/" + y.NewFileName);
                //        y.MediaType = MediaService.GetMediaType(y.NewFileName);
                //    });
           
              //  item.ProfilePhoto = new MediaModel { Data = Url.Content("~/images/" + item.ProfilePhoto) };
                //});

                

                var singleUserRoles = from role in listofRoles
                                      from loginrole in listofRoleList.Where(q => q.PersonId == item.LoginId && (role.Id == q.RoleId)).DefaultIfEmpty()
                                      select new
                                      {
                                          Id = role.Id,     //id from the role tabe
                                          RoleName = role.Name,
                                          IsActive = (bool?)loginrole.IsActive ?? false,
                                          PendingApproval = (bool?)loginrole.PendingApproval ?? false,
                                          LoginId = item.LoginId
                                      };

                foreach (var userRole in singleUserRoles)
                {
                    item.UserRoleList.Add(new RoleViewModel()
                    {
                        Id = userRole.Id,
                        Name = userRole.RoleName,
                        loginId = userRole.LoginId,
                        IsActive = userRole.IsActive,
                        PendingApproval = userRole.PendingApproval

                    });
                }
              
            }



    

            if (Request.IsAjaxRequest())
            {
                return PartialView("_PagedUsers", users.ToPagedList(pageNumber, 10));
            }
            TempData["CurrentLink"] = "Admin";
            return View(users.ToPagedList(pageNumber, 10));
        }


        public ActionResult Create(UserViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return Json(new { response = "Something went wrong.Please make sure all data passed is valid", model });
            }

            else
            {
                var newUser = new Person();
                var newModel = new UserViewModel();

                if (model.IsShipSame)
                {
                    var physicalAddress = new Address
                    {
                        Number = model.PhysicalAddress.Number,
                        CountryId = 1,
                        Street = model.PhysicalAddress.Street,
                        City = model.PhysicalAddress.City,
                        Suburb = model.PhysicalAddress.Suburb,
                        PostCode = model.PhysicalAddress.PostCode,
                        Lat = model.PhysicalAddress.Latitude,
                        Lng = model.PhysicalAddress.Longitude,
                        CreatedBy = HttpContext.User.Identity.GetUserName(),
                        CreatedOn = DateTime.Now,
                        UpdatedBy = HttpContext.User.Identity.GetUserName(),
                        UpdatedOn = DateTime.Now
                    };
                    db.Address.Add(physicalAddress);
                    db.SaveChanges();

                    newUser.FirstName = model.FirstName;
                    newUser.MiddleName = model.MiddleName;
                    newUser.LastName = model.LastName;
                    newUser.IsActive = false;
                    newUser.PhysicalAddressId = physicalAddress.AddressId;
                    newUser.BillingAddressId = physicalAddress.AddressId;
                    newUser.Language = model.Language;
                    newUser.CreatedOn = DateTime.Now;
                    newUser.CreatedBy = HttpContext.User.Identity.GetUserName();
                    newUser.Id = int.Parse(HttpContext.User.Identity.GetUserId());
                    newUser.UID = Guid.NewGuid();

                    newModel.Id = newUser.Id;
                    newModel.FirstName = newUser.FirstName;
                    newModel.MiddleName = newUser.MiddleName;
                    newModel.LastName = newUser.LastName;
                    newModel.Language = newUser.Language;
                    newModel.PlaceOfBirth = newUser.PlaceOfBirth;
                    newModel.IsActive = newUser.IsActive;
                    newModel.IsShipSame = true;
                    newModel.PhysicalAddress = new AddressViewModel
                    {
                        AddressId = physicalAddress.AddressId,
                        CountryId = physicalAddress.CountryId,
                        Number = physicalAddress.Number,
                        Street = physicalAddress.Street,
                        City = physicalAddress.City,
                        Suburb = physicalAddress.Suburb,
                        PostCode = physicalAddress.PostCode
                    };

                }
                else if (!model.IsShipSame)
                {
                    Address physicalAddress = new Address
                    {
                        Number = model.PhysicalAddress.Number,
                        CountryId = 1,
                        Street = model.PhysicalAddress.Street,
                        City = model.PhysicalAddress.City,
                        Suburb = model.PhysicalAddress.Suburb,
                        PostCode = model.PhysicalAddress.PostCode,
                        Lat = model.PhysicalAddress.Latitude,
                        Lng = model.PhysicalAddress.Longitude,
                        CreatedBy = HttpContext.User.Identity.GetUserName(),
                        CreatedOn = DateTime.Now,
                        UpdatedBy = HttpContext.User.Identity.GetUserName(),
                        UpdatedOn = DateTime.Now
                    };
                    Address billingAddress = new Address
                    {
                        Number = model.BillingAddress.Number,
                        CountryId = 1,
                        Street = model.BillingAddress.Street,
                        City = model.BillingAddress.City,
                        Suburb = model.BillingAddress.Suburb,
                        PostCode = model.BillingAddress.PostCode,
                        Lat = model.BillingAddress.Latitude,
                        Lng = model.BillingAddress.Longitude,
                        CreatedBy = HttpContext.User.Identity.GetUserName(),
                        CreatedOn = DateTime.Now,
                        UpdatedBy = HttpContext.User.Identity.GetUserName(),
                        UpdatedOn = DateTime.Now
                    };

                    db.Address.Add(physicalAddress);
                    db.Address.Add(billingAddress);
                    db.SaveChanges();

                    newUser.FirstName = model.FirstName;
                    newUser.MiddleName = model.MiddleName;
                    newUser.LastName = model.LastName;
                    newUser.IsActive = false;
                    newUser.PhysicalAddressId = physicalAddress.AddressId;
                    newUser.BillingAddressId = billingAddress.AddressId;
                    newUser.Language = model.Language;
                    newUser.CreatedOn = DateTime.Now;
                    newUser.CreatedBy = HttpContext.User.Identity.GetUserName();
                    newUser.Id = int.Parse(HttpContext.User.Identity.GetUserId());
                    newUser.UID = Guid.NewGuid();


                    newModel.Id = newUser.Id;
                    newModel.FirstName = newUser.FirstName;
                    newModel.MiddleName = newUser.MiddleName;
                    newModel.LastName = newUser.LastName;
                    newModel.Language = newUser.Language;
                    newModel.PlaceOfBirth = newUser.PlaceOfBirth;
                    newModel.IsActive = newUser.IsActive;
                    newModel.IsShipSame = true;
                    newModel.PhysicalAddress = new AddressViewModel
                    {
                        AddressId = physicalAddress.AddressId,
                        CountryId = physicalAddress.CountryId,
                        Number = physicalAddress.Number,
                        Street = physicalAddress.Street,
                        City = physicalAddress.City,
                        Suburb = physicalAddress.Suburb,
                        PostCode = physicalAddress.PostCode
                    };
                    newModel.BillingAddress = new AddressViewModel
                    {
                        AddressId = billingAddress.AddressId,
                        CountryId = billingAddress.CountryId,
                        Number = billingAddress.Number,
                        Street = billingAddress.Street,
                        City = billingAddress.City,
                        Suburb = billingAddress.Suburb,
                        PostCode = billingAddress.PostCode
                    };
                }

                db.Person.Add(newUser);
                db.SaveChanges();
                return Json(newModel);
            }
        }
        [HttpPost]
        [Authorize]
        public ActionResult Edit(UserViewModel model)
        {
            //var person = db.Person.Where(p => p.LoginId == model.LoginId).First();
            var person = db.Person.Where(p => p.Id == model.Id).FirstOrDefault();
            var status = true;
            var message = "";

            if (!ModelState.IsValid)
            {
                status = false;
                message = "Something went wrong, a field must have been invalid.";
            }
            else
            {

                person.FirstName = model.FirstName;
                person.MiddleName = model.MiddleName;
                person.LastName = model.LastName;
                person.Language = model.Language;
                person.PlaceOfBirth = model.PlaceOfBirth;
                person.UpdatedBy = HttpContext.User.Identity.GetUserName();
                person.UpdatedOn = DateTime.Now;


                if (person.PhysicalAddressId == person.BillingAddressId)
                {
                    if (model.IsShipSame)
                    {
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;
                    }
                    else
                    {
                        Address billingAddress = new Address
                        {
                            Number = model.BillingAddress.Number,
                            CountryId = 1,
                            Street = model.BillingAddress.Street,
                            City = model.BillingAddress.City,
                            Suburb = model.BillingAddress.Suburb,
                            PostCode = model.BillingAddress.PostCode,
                            Lat = model.BillingAddress.Latitude,
                            Lng = model.BillingAddress.Longitude,
                            CreatedBy = HttpContext.User.Identity.GetUserName(),
                            CreatedOn = DateTime.Now,
                            UpdatedBy = HttpContext.User.Identity.GetUserName(),
                            UpdatedOn = DateTime.Now
                        };
                        db.Address.Add(billingAddress);
                        person.BillingAddressId = billingAddress.AddressId;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (!model.IsShipSame)
                    {
                        //physical address
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;

                        //billing address
                        person.Address.Number = model.BillingAddress.Number;
                        person.Address.Street = model.BillingAddress.Street;
                        person.Address.PostCode = model.BillingAddress.PostCode;
                        person.Address.City = model.BillingAddress.City;
                        person.Address.Suburb = model.BillingAddress.Suburb;


                    }
                    else
                    {
                        person.Address1.Number = model.PhysicalAddress.Number;
                        person.Address1.Street = model.PhysicalAddress.Street;
                        person.Address1.PostCode = model.PhysicalAddress.PostCode;
                        person.Address1.City = model.PhysicalAddress.City;
                        person.Address1.Suburb = model.PhysicalAddress.Suburb;

                        var bill = db.Address.Where(p => p.AddressId == model.BillingAddress.AddressId).First();
                     
                      
                        person.BillingAddressId = person.PhysicalAddressId;
                     //   db.Address.Remove(bill);

                        db.SaveChanges();
                    }

                }
                db.SaveChanges();
                message = "User edited successfully.";
            }
            return Json(new
            {
                success = status,
                message = message,
                data = model
            });
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var db = new KeysEntities())
            {
                var delPerson = db.Person.Find(id);
                // Moved this into the Else statement. Dmitry
                //var delPersonLogin = db.Login.Find(delPerson.LoginId);
                
                // Bug #1185
                // Changed AND to an OR. Dmitry
                if (delPerson == null || delPerson.IsActive == false)
                {
                    return Json(new { Result = "norecord", Message = "Cannot find this record !" });
                }

                else
                {
                    var delPersonLogin = db.Login.Find(delPerson.Id);

                    if (delPersonLogin.UserName == User.Identity.Name)
                    {
                        return Json(new { Result = "ownrecord", Message = "Admin can not delete their record !" });
                    }
                    else
                    {
                        delPerson.IsActive = false;
                        delPersonLogin.IsActive = false;
                        db.SaveChanges();
                        return Json(new { Result = "success", Message = "Record deleted successfully" });
                    }
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


