function PageViewModel(dataVm) {  
    debugger;
    var self = this;
    for (var key in dataVm) {
        self[key] = dataVm[key];
    }
    if (ko.isObservable(dataVm.Company)) {
        self.Company = null;
    }
    // For managing photos
    if (self.Company != null) {
        self.Company.ProfilePhoto = ko.observable(dataVm.Company.ProfilePhoto);
        self.CompImgData = ko.observable(self.Company.ProfilePhoto().Data());
    }
    self.Profile.ProfilePhotoModel = ko.observable(dataVm.Profile.ProfilePhotoModel);
    self.validFileTypes = KeysFiles.validImgFiles;
    self.ImgData = ko.observable(self.Profile.ProfilePhotoModel().Data());
    self.FileWarning = ko.observable(KeysInstrucTion.fileUpLoad);
    //Remove photo on company tab
    self.RemoveCompanyLogo = function (file) {
        self.Company.RemoveProfilePhoto(true);
        self.Company.NewCompPhoto(null);
        $('#editcompanyimage').attr('src', '/images/icon-user-default.png');
    };
    //Remove photo on profile tab
    self.RemoveUserPhoto = function (file) {
        self.Profile.RemoveUserPhoto(true);
        self.Profile.NewPhoto(null);
        $('#edituserimage').attr('src', '/images/icon-user-default.png');
    }
    //Validation
    if (self.Company != null) {
        KeysUtils.injectExtends(self.Company, KeysExtendsDic.CompanyOverView)
        self.CompanyErrors = ko.validation.group(self.Company, { deep: true });
        self.IsCompanyValid = ko.computed(function () {
            return self.CompanyErrors().length == 0;
        });
    }

    

    KeysUtils.injectExtends(self.Profile, KeysExtendsDic.ProfileOverView)

    //Validation for Edit user as solution to KEYS-943
    self.Profile.MiddleName = ko.observable().extend({ maxLength: 12 });
 
    
    //var allowedInput = function (length) {
    //    return {
    //        init: function (element, valueAccessor, allBindingsAccessor, bindingContext) {
    //            ko.bindingHandlers.textInput.init(element, valueAccessor, allBindingsAccessor, bindingContext);
    //        },

    //        update: function (element, valueAccessor) {
    //            var value = ko.unwrap(valueAccessor());
    //            if (value.length >= length) {
    //                valueAccessor()(value.slice(0, -1));
    //            }
    //        }
    //    }
    //};

    //ko.bindingHandlers.limitInput = allowedInput(10);
   
    //var Input = function (length) {
    //    return {
    //        init: function (element, valueAccessor, allBindingsAccessor, bindingContext) {
    //            ko.bindingHandlers.textInput.init(element, valueAccessor, allBindingsAccessor, bindingContext);
    //        },

    //        update: function (element, valueAccessor) {
    //            var value = ko.unwrap(valueAccessor());                
    //            if (value.length >= length || isNaN(value)) {
    //                valueAccessor()(value.slice(0, -1));
    //            }
    //        }
    //    }
    //};

    //ko.bindingHandlers.numericInput = Input(10);
    //ko.bindingHandlers.Input = Input(5);
    
    //End of solution for KEYS 943

    self.PersonErrors = ko.validation.group(self.Profile, { deep: true });
    self.IsProfileValid = ko.computed(function () {
        return self.PersonErrors().length == 0;
    });   
    // View when page is loaded
    self.MainView = ko.observable(true);
    self.EditUser = ko.observable(false);
    self.EditCompanyProfile = ko.observable(false);
    
    // return to main page
    self.backToMain = function () {
        window.location.href = "../Account/AccountOverview";
    }
    // Go to edit person tab
    self.editPerson = function () {
        self.MainView(false);
        self.EditCompanyProfile(false);
        self.EditUser(true);

    }
    // Go to edit company tab
    self.editCompany = function () {
        self.MainView(false);
        self.EditUser(false);
        self.EditCompanyProfile(true);
    }
    // Copy address if it is the same functions
    self.checkCompShipSame = function () {
        if (self.Company.IsCompShipSame()) {
            UpdateAddress(ko.mapping.toJS(self.Company.PhysicalAddress), self.Company.BillingAddress);

        }
        else {
            self.Company.BillingAddress.Street();
            self.Company.BillingAddress.Suburb();
            self.Company.BillingAddress.City();
            self.Company.BillingAddress.PostCode();

        }
    };
    self.checkShipSame = function () {
        if (self.Profile.IsShipSame()) {
            UpdateAddress(ko.mapping.toJS(self.Profile.PhysicalAddress), self.Profile.BillingAddress);
        }
        else {
            self.Profile.BillingAddress.Street();
            self.Profile.BillingAddress.Suburb();
            self.Profile.BillingAddress.City();
            self.Profile.BillingAddress.PostCode();
        }
    };
    function UpdateAddress(data, address) {
        data.Number ? address.Number(data.Number) : 1;
        data.Street ? address.Street(data.Street) : 1;
        data.Suburb ? address.Suburb(data.Suburb) : 1;
        data.City ? address.City(data.City) : 1;
        data.PostCode ? address.PostCode(data.PostCode) : 1;
        data.Latitude ? address.Latitude(data.Latitude) : 1;
        data.Longitude ? address.Longitude(data.Longitude) : 1;
    }
    // Save for edit person
    self.savePerson = function (data) {
        var middleName = data.MiddleName();
        var occupation = data.Occupation();
        var linkedInUrl = data.LinkedinUrl();
        var newPhoto = data.NewPhoto();
        var formData = KeysUtils.toData(data);
        formData.delete("ProfilePhotoModel");
        debugger;
        if (occupation === null) {
            formData.delete("Occupation");
        }
        if (linkedInUrl === null) {
            formData.delete("LinkedinUrl");
        }
        if (middleName === null) {
            formData.delete("MiddleName")
        }
        if (newPhoto === null) {
            formData.delete("NewPhoto");
        }
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }
        if (self.IsProfileValid) {
            var url = "/Account/Edit/";
            $.ajax({
                type: "Post",
                url: url,
                data: formData,
                contentType: false,
                processData: false,
                success: function (data) {
                    if (data.success) {
                        KeysUtils.notification.show('<p>User edited successfully!</p>', 'success', KeysUtils.reload);
                    }
                },
                error: function (error) {

                    alert(error.status + "<!----!>" + error.statusText);
                },
                fail: function () { }

            });
        }

    };
    //Save for edit company
    self.saveCompany = function (data) {
        var newCompPhoto = data.NewCompPhoto();
        var formData = KeysUtils.toData(data);
        formData.delete("ProfilePhoto");
        // Had to do this to get it to pass the modelstate is valid in the controller. toData puts a string type in NewCompPhoto which does match the wrapper in the model
        if (newCompPhoto === null) {
            formData.delete("NewCompPhoto");
        }
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }
        if (self.IsCompanyValid()) {
            var url = "/Account/EditCompany/";
            $.ajax({
                type: "Post", // type:"PUT"
                url: url,
                data: formData,//ko.toJSON(newjob), //
                // dataType: 'json',
                contentType: false,
                processData: false,
                success: function (data) {
                    if (data.success) {
                        KeysUtils.notification.show('<p>Company edited successfully!</p>', 'success', KeysUtils.reload);
                    }
                },
                error: function (error) {
                    alert(error.status + "<!----!>" + error.statusText);
                },
                fail: function () { }
            });
        }
    };
}
