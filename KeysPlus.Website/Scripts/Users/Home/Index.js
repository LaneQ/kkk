ko.bindingHandlers.country = {
    update: function (element, valueAccessor, allBindingsAccessor, viewModel, bindingContext) {
        var $element = $(element);
        var valueUnwrapped = ko.utils.unwrapObservable(valueAccessor());
        // show only when value is provided
        if (valueUnwrapped && valueUnwrapped.length > 0) {
            $element.css('display', 'block');
            $element.data('country', valueUnwrapped);
            $element.val(valueUnwrapped);

            var data = $element.data();
            delete data.bfhcountries;
            data.country = valueUnwrapped;
            $element.bfhcountries(data);
        } else {
            $element.css('display', 'none');
        }
    }
};

function PersonFields(item) {
    var self = this;
    var copy = item;
   // debugger;
    self.ValidationModel = ko.validatedObservable({
       
        UserName: self.UserName = ko.observable(item.UserName).extend({ required: true }),
        FirstName: self.FirstName = ko.observable(item.FirstName).extend({ required: { params: true, message: "Please enter your first name" }, pattern: { params: "^[A-Za-z][A-Za-z\\s\~\\`\\'\\,\\-\]{1,50}$", message: "Please enter a valid First Name" } }),
        LastName: self.LastName = ko.observable(item.LastName).extend({ required: { params: true, message: "Please enter your last name" }, pattern: { params: "^[A-Za-z][A-Za-z\\s\~\\`\\'\\,\\-\]{1,50}$", message: "Please enter a valid Last Name" } }),
        MiddleName: self.MiddleName = ko.observable(item.MiddleName).extend({ required: { params: false, message: "Please enter your middle name" }, pattern: { params: "^[A-Za-z][A-Za-z\\s\~\\`\\'\\,\\-\]{1,50}$", message: "Please enter a valid Middle Name" } }),
        LoginId: self.LoginId = ko.observable(item.LoginId),
        FullName: self.FullName = ko.observable(),
        Language: self.Language = ko.observable(item.Language).extend({ required: { params: false, message: "Please enter language"}, pattern: { params: "^[A-Za-z][A-Za-z\\s\,\]{1,19}$", message: "Please enter a valid data must be less than 20 characters" } }),
        PlaceOfBirth: self.PlaceOfBirth = ko.observable(item.PlaceOfBirth).extend({ required: { params: false, message: "Please enter your birth place" }, pattern: { params: "^[A-Za-z][A-Za-z\\s\,\\-\.\/\]{1,50}$", message: "Please enter a valid Place of Birth" } }),
        Occupation: self.Occupation = ko.observable(item.Occupation).extend({ required: { params: false, message: "Please enter Occupation", pattern: { params: "^[A-Za-z][A-Za-z\\s\~\\`\\'\\,\\-\]{1,50}$", message: "Please enter a valid Occupation" } } }),
        LinkedinUrl: self.LinkedinUrl = ko.observable(item.LinkedinUrl).extend({ required: { params: false, message: "Please enter Linkedin", pattern: { params: "^[A-Za-z][A-Za-z\\s\~\\`\\'\\,\\-\]{1,50}$", message: "Please enter a valid LinkedinUrl" } } }),
        IsDeleted: self.IsDeleted = ko.observable(item.IsDeleted),
        PhysicalAddress: self.PhysicalAddress = ko.observable(ProjectKeys.Address.EditAddress(item.PhysicalAddress)),
        BillingAddress: self.BillingAddress = ko.observable(ProjectKeys.Address.EditAddress(item.BillingAddress))


    });
    self.FullEmployeeName = ko.pureComputed(function () {
        //debugger;
        return self.FirstName() + " " + self.MiddleName() + " " + self.LastName();

    }
   );

    self.UserName = ko.observable(item.UserName);
    self.LoginId = ko.observable(item.LoginId);
    self.Id = ko.observable(item.Id);
    self.UserRoleList = ko.observableArray(item.UserRoleList);
    self.DisableItem = ko.observable(true);
    self.IsShipSame = ko.observable(item.IsShipSame);
    self.ProfilePhoto = ko.observable(item.ProfilePhoto);
    console.log(self.ProfilePhoto());
    self.GetFullName = ko.computed({
        read: function () {
            //debugger;
            self.FullName(self.FirstName() + " " + self.LastName());
            return self.FullName();
        },
        write: function () {
            // debugger;
            self.FullName(self.FirstName() + " " + self.LastName());
        }
    });

    self.ToggleDisable = ko.computed({
        read: function () {

            if (!self.ValidationModel.isValid()) {
                self.ValidationModel.errors.showAllMessages();
                self.DisableItem(true);
            } else {
                self.DisableItem(false);
            }
            return self.DisableItem();
        }
    });

    self.ShowAddress = ko.computed({
        read: function () {
            return self.BillingAddress();
        },
        write: function () {
            self.IsShipSame(!self.IsShipSame());
        }
    });

    self.ResetAddress = ko.computed({
        read: function () {
            if (self.IsShipSame()) {
                $.each(self.BillingAddress(), function (key, value) {

                    if (key === "AddressId") {
                        return true;
                    }
                    self.BillingAddress()[key](self.PhysicalAddress()[key]());

                    if (self.BillingAddress()[key].hasOwnProperty("isModified")) {
                        self.BillingAddress()[key].isModified(false);
                    }
                });
            }
            return self.BillingAddress();
        },
        write: function () {
            self.IsShipSame(!self.IsShipSame());
        }
    });

    self.ResetAll = function () {

        $.each(self.ValidationModel(), function (key, value) {
            //debugger;

            if (~["PhysicalAddress", "BillingAddress"].indexOf(key)) {
                $.each(value(), function (childKey, childValue) {

                    if (childKey === "AddressId") {
                        return true;
                    }
                    value()[childKey](copy[key][childKey]);

                    if (value()[childKey].hasOwnProperty("isModified")) {
                        value()[childKey].isModified(false);
                     
                    }
                });
            } else {
                self.ValidationModel()[key](copy[key]);
                
            }
            self.ValidationModel()[key].isModified(false);
          
        });
      
    };
};

function UserViewModel(data) {
    ko.validation.init({
        grouping: {
            deep: true,
            observable: true,
            live: true
        }
    });
    var self = this;
    self.users = ko.observableArray();
    self.user = ko.observable();
    self.currentUser = ko.observable();
    //main page
    self.showCurrentUser = ko.observable(false);
    self.showMainPage = ko.observable(true);
    self.CurrentUserDetails = function (data) {
        
        self.currentUser(data);
        self.showCurrentUser(true);
        self.showMainPage(false);
        console.log(self.showMainPage());
   }
    self.goToMainPage = function (data) {
        self.showCurrentUser(false);
        self.showMainPage(true);
    }
  
    //self.showCurrentUser = function (user) {
    //    console.log(user);
    //    self.currentUser(user);
    //    //debugger;
    //    $("#userDetailsModal").modal("show");
    //};

  
    self.editCurrentUser = function (user) {
        var userCopy= ko.mapping.fromJS(ko.toJS(user)); //creating copy of user object
        self.currentUser(userCopy);
        $("#editUserModal").modal("show");
    };
    self.removeCurrentUser = function (user) {

        self.currentUser(user);
        $("#removeUserModal").modal("show");
    }
    data.forEach(function (user) {
        self.users.push(new PersonFields(user));
    });

    ko.validation.init({
        grouping: {
            deep: true,
            observable: true,
            live: true
        }
    });

    ko.bindingHandlers.AddCss = {
        init: function (elem) {
            $(elem).on("click", function () {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            });
        },
        update: function (elem, value) {
            var val = ko.unwrap(value());

            if (!val.IsShipSame()) {
                $(".form-group label.keys").addClass("col-sm-3");
                $(".form-group div.keys").addClass("col-sm-9");
                $(".validationMessage").addClass("col-sm-12");
            }
        }
    }

    self.AdminTemplate = ko.observable("listUserTable");

    self.cancelDataId = function () {
      
        $("#editUserModal").modal("hide");
      
    };

    self.AddUser = (function () {
        var user = {
            Id: 0,
            FirstName: "",
            MiddleName: "",
            LastName: "",
            FullName: "",
            IsDeleted: false,
            IsShipSame: true,
            PlaceOfBirth: "",
            Language: "",
            PhysicalAddress: {
                Address: "",
                CountryId: 1,
                AddressId: 0,
                AddressCont: "",
                Suburb: "",
                City: "",
                PostCode: "",
                Latitude: "",
                Longitude: ""
            },
            BillingAddress: {
                Address: "",
                CountryId: 1,
                AddressId: 0,
                AddressCont: "",
                Suburb: "",
                City: "",
                PostCode: "",
                Latitude: "",
                Longitude: ""
            }
        };
        self.user(new PersonFields(user));
    })();

    self.SaveUser = function (user) {
        console.log("At Save User");
       
        console.log(user);
        var userId = user.Id().toString();

        if (user.Id() === 0) {
            console.log("At Save Create");

            $.ajax("/Admin/Home/Create", {
                data: ko.toJSON(user),
                type: "post",
                contentType: "application/json",
                success: function (result) {
                    console.log("User Created");
                    $("#addUser").modal("hide");
                    self.users.push(new PersonFields(result));
                    $("#editUserModal").modal("hide");
                    $('#SaveModal').appendTo("body").modal('show');
                }
            });
        } else {
            console.log("At Save Edit");
           // debugger;
            $.ajax("/Admin/Home/Edit", {
                data: ko.toJSON(user),
                type: "post",
                contentType: "application/json",
                success: function (result) {
                    console.log("User Edited");
                    
                    self.users().forEach(function (element) {
                        if (element.Id() == user.Id()) {
                            debugger;
                            element.FirstName(user.FirstName());
                            element.LastName(user.LastName());
                            element.MiddleName(user.MiddleName());
                            element.Language(user.Language());
                            element.PlaceOfBirth(user.PlaceOfBirth());
                           
                         
                        }
                        
                        $("#editUser" + userId).modal("hide");
                        $("#editUserModal").modal("hide");
                        $('#SaveModal').appendTo("body").modal('show');
                      
                    });

                   
                }
            });
        }
    };

    self.CancelUserDetail = function (user) {

        //debugger;
        $('#CancelModal').appendTo("body").modal('show');

    };

    //function reloadPage() {
    //    window.location.reload();
    //}

    self.DeleteUser = function (user) {
        console.log("At Delete User");
        console.log(user);
        user.IsDeleted(true);

        //var userId = user.Id().toString();
        var userLoginId = user.LoginId().toString();

        $.ajax("/Admin/Home/Delete", {
            data: ko.toJSON(user),
            type: "post",
            contentType: "application/json",
            success: function (response) {
                debugger;
                if (response.Result == "success") {

                    $("#deleteuser").modal("hide");
                    $("#UserRecord").modal("show");
                    self.users.remove(user);
                    //location.reload();
                    //reloadPage();

                } else if (response.Result == "ownrecord") {
                    $("#deleteuser").modal("hide");
                    $("#OwnRecord").modal("show");
                    //location.reload();

                } else if (response.Result == "norecord") {
                    $("#deleteuser").modal("hide");
                    $("#NoRecord").modal("show");

                    // Bug #1185
                    // Added this field so when the user does not exists, 
                    // the user is removed from the form. Dmitry
                    self.users.remove(user);
                }
                //console.log("User Deleted");
                //$("#deleteUser" +userId).modal("hide");
                // self.users.remove(user);
                //reloadPage();
            }
        });
    };
};


