function Property(data) {
    var self = this;
    self.Id = data.Id;
    self.AddressString = data.AddressString;
    self.YearBuilt = data.YearBuilt;
  
}

//var returnToUrl = "";
function PropertyTenants(data, propData) {
    var self = this;
    self.DeleteLiabilities = ko.observableArray();
    if (propData) {
        self.SelectedProp = ko.observable();
        self.Properties = ko.observableArray();
        propData.forEach(function (item) {
            self.Properties.push(new Property(item));
        });
        self.SelectedProp(self.Properties()[0]);
    }
    self.YearBuilt = ko.computed(function () {
        if (data != null) {
            data.YearBuilt
        } else {
            self.SelectedProp().YearBuilt
        }
    });
    //self.returnUrl = ko.observable(data.ReturnUrl);
    //returnToUrl = data.ReturnUrl; 

    self.TenantExist = ko.observable(false);
    self.CanBeAdded = ko.observable(true);
    self.CanBeAddedMsg = ko.observable();
    self.TenantEmail = ko.observable().extend({
        required: {
            params: true,
            message: "Please enter the email"
        },
        pattern: {
            params: /^[a-z0-9][a-zA-Z0-9_]*(\.[a-zA-Z][a-zA-Z0-9_]*)?@[a-z][a-zA-Z-0-9]*\.[a-z]+(\.[a-z]+)?$/,
            message: "Please enter a valid email."
        },
        maxLength: { params: 50, message: "Maximum 50 characters only" },
        canBeAddedAsTenant: { otherVal: self.CanBeAdded, msg: self.CanBeAddedMsg }
               
    });

    self.FirstName = ko.observable().extend(Extender.firstName);
    self.LastName = ko.observable().extend(Extender.lastName);
    self.RentAmount = ko.observable().extend(Extender.decimalNumeric());
    self.RentFrequency = ko.observable(1);
    self.StartDate = ko.observable().extend({
        required: {
            params: true,
            message: "Please enter a date."
        },
        datePickerAfterYear: self.YearBuilt()
    });

    self.EndDate = ko.observable().extend({
        datePickerAfter: self.StartDate
    });
    self.PaymentStartDate = ko.observable().extend({
        required: {
            params: true, message: "Please enter a date."

        },
        datePickerPaymentDate: self.StartDate
    });
    self.PaymentDueDate = ko.observable(1);
    self.DueDateOptions = ko.computed(function () {
        var freq = self.RentFrequency();
        if (freq == 1) {
            return [1, 2, 3, 4, 5, 6, 7];
        }
        else if (freq == 2) {
            return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];
        }
        else return [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30];
    });
    self.IsMainTenant = ko.observable(true);
    self.PropertyId = ko.observable();
    self.LiabilityValues = ko.observableArray();
    self.SelectedLiabilityValue = ko.observable({});
    self.originalData = ko.observable();

    self.CheckTenant = function () {
        $.ajax({
            type: "GET",
            url: '/Account/CheckIfCanBeTenant',
            data: { tenantEmail: self.TenantEmail(), propertyId: self.SelectedProp().Id },
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    debugger;
                    self.CanBeAdded(true);
                }
                else {
                    self.CanBeAdded(false);
                    self.CanBeAddedMsg(result.Msg);
                }
                if (result.Exist) {
                    self.TenantExist(true);
                    self.FirstName(result.FirstName);
                    self.LastName(result.LastName);
                }
                else {
                    self.TenantExist(false);
                    if (self.FirstName()) self.FirstName('');
                    if (self.LastName()) self.LastName('');
                }
                //if (result.Exist) {
                //    self.TenantExist(true);
                //    self.FirstName(result.FirstName);
                //    self.LastName(result.LastName);
                //    self.TenantInProperty(result.TenantInProperty);
                //    self.SupplierOrOwner(result.SupplierOrOwner);
                                   
                //}
                //else {
                //    self.TenantExist(false);
                //    self.FirstName(null);
                //    self.LastName(null);
                //    self.TenantInProperty(result.TenantInProperty);
                //    self.SupplierOrOwner(result.SupplierOrOwner);
                                   
                //}
            },
        });
        
    }
    
    self.AddLiabilityValues = function () {
        var newValue = new LiabilityValueViewModel({});
        newValue.IsEdit(true);
        newValue.IsNew(true);
        newValue.Status("Add");
        self.LiabilityValues.push(newValue);
    }
    self.showSummaryTable = function () {
        return 'displaySummaryTable';
    }
    self.LiabilityValueDisplay = function (data) {
        //debugger;
        if (data.IsNew()) return 'newLiabilityValue';
        if (!self.SelectedLiabilityValue().Id) return 'displayLiabilityValue';
        var result = data.Id() == self.SelectedLiabilityValue().Id() ? 'newLiabilityValue' : 'displayLiabilityValue';
        return result;
    }
    self.EditLiabilityValue = function (data) {
        self.originalData(ko.mapping.fromJS(ko.mapping.toJS(data)));
        self.SelectedLiabilityValue(ko.mapping.fromJS(ko.mapping.toJS(data)));
    }
    self.AddtoLList = function (data) {
        //console.log(">> AddtoLList >> data :", data);
        data.IsNew(false);
        if (ko.mapping.toJS(data).Status == "Load") {
            data.Status("Update");
        }
        //console.log("L List", self.LiabilityValues());
        self.SelectedLiabilityValue({});
    }
    self.CancelEditLiabilityValue = function (data) {
        if (data.IsNew()) {
            self.LiabilityValues.remove(data);
        }
        else {
            self.SelectedLiabilityValue({});
            var newVM = new LiabilityValueViewModel({});
            newVM.TypeId(self.originalData().TypeId());
            newVM.Value(self.originalData().Value());
            newVM.Status("Load");
            self.LiabilityValues.replace(data, newVM);
        }
    }
    if (data != null) {
        self.FirstName(data.FirstName);
        self.LastName(data.LastName);
        self.TenantEmail(data.TenantEmail);
        self.IsMainTenant(data.IsMainTenant);
        console.log("self.IsMainTenant", self.IsMainTenant());
        if (self.IsMainTenant()) {
            $("#isMainTenantDropdown").val("true");
        } else {
            $("#isMainTenantDropdown").val("false");
        }
        self.RentAmount(data.PaymentAmount);
        self.RentFrequency(data.PaymentFrequencyId);
        self.StartDate(data.StartDate);
        self.EndDate(data.EndDate);
        self.PropertyId(data.PropertyId);
        self.PaymentStartDate(data.PaymentStartDate);
        self.PaymentDueDate(data.PaymentDueDate);
        console.log(data);
        data.Liabilities.forEach(function (item) {
            console.log(item);
            var liabilityData = new LiabilityValueViewModel({});
            liabilityData.IsEdit(false);
            liabilityData.IsNew(false);
            liabilityData.Id(item.Id);
            liabilityData.TypeId(item.Name);
            liabilityData.Value(item.Amount);
            liabilityData.Status(item.Status);
            self.LiabilityValues.push(liabilityData);
        });
    }//end of if(data != null)

    self.basicError = ko.validation.group([
        self.TenantEmail,
        //self.FirstName,
        //self.LastName,
        //self.StartDate,
        //self.EndDate,
        //self.RentAmount,
        //self.RentFrequency,
        //self.PaymentStartDate,
    ]);
    self.basicValid = ko.computed(function () {
        //console.log(self.basicError());
        debugger;
        return self.basicError().length == 0;
    });

    self.AddTenantToProperty = function (data) {
        debugger;
        var liabilities = [];
        data.LiabilityValues().forEach(function (element) {
            liabilities.push({
                Name: element.TypeId(),
                Amount: element.Value()
            })
        });
        var tenantData = {
            TenantEmail: data.TenantEmail(),
            FirstName: data.FirstName(),
            LastName: data.LastName(),
            StartDate: KeysUtils.toDotnetDate(data.StartDate()),
            EndDate: KeysUtils.toDotnetDate(data.EndDate()),
            PaymentFrequencyId: data.RentFrequency(),
            PaymentAmount: data.RentAmount(),
            PropertyId: data.SelectedProp().Id,
            PaymentStartDate: KeysUtils.toDotnetDate(self.PaymentStartDate()),
            PaymentDueDate: self.PaymentDueDate(),
            IsMainTenant: self.IsMainTenant(),
            Liabilities: liabilities
            //Liabilities: liabilities,
            //ReturnUrl: data.returnUrl()
        };
        //debugger;
        $.ajax({
            type: "POST",
            url: '/PropertyOwners/Property/AddTenantToProperty',
            data: tenantData,
            async: false,

        }).done(function (response) {
            debugger;
            if (response.Success) {
                KeysUtils.notification.show('<p>Tenant Added Successfully</p>', 'notice', reload);
            }
            else {
                KeysUtils.notification.show(KeysUtils.notification.errorMsg, 'error');
                //KeysUtils.notification.show(response.Msg , 'error');
            }
        });

    };

    self.EditTenantInProperty = function (data) {
        var liabilities = [];
        data.LiabilityValues().forEach(function (element) {
            liabilities.push({
                Id: element.Id(),
                Name: element.TypeId(),
                Amount: element.Value(),
                Status: element.Status()
            })
        });
        var tenantData = {
            TenantEmail: data.TenantEmail(),
            FirstName: data.FirstName(),
            LastName: data.LastName(),
            StartDate: KeysUtils.toDotnetDate(data.StartDate()),
            EndDate: KeysUtils.toDotnetDate(data.EndDate()),
            PaymentFrequencyId: data.RentFrequency(),
            PaymentAmount: data.RentAmount(),
            PropertyId: data.PropertyId(),
            PaymentStartDate: KeysUtils.toDotnetDate(self.PaymentStartDate()),
            PaymentDueDate: self.PaymentDueDate(),
            IsMainTenant: self.IsMainTenant(),
            Liabilities: liabilities,
            DeleteLiabilities: self.DeleteLiabilities()
            //DeleteLiabilities: self.DeleteLiabilities(),
            //ReturnUrl: data.returnUrl()
        };

        $.ajax({
            type: "POST",
            url: '/PropertyOwners/Property/EditPropertyTenant',
            data: tenantData,
            async: false,
            success: function (response) {
                if (response.Success) {
                    KeysUtils.notification.show('<p>Edit Successfull</p>', 'success', reload);
                }
            },
            error: function () {
                KeysUtils.notification.show('<p>Something went wrong, please try again later</p>', 'error');
                return;
            }
        });

        
    };
    
    self.DeleteLiabilityValue = function (data) {
        self.LiabilityValues.remove(data);
        var dataJS = ko.mapping.toJS(data);
        if (dataJS.Status != "Add") {
            self.DeleteLiabilities.push(dataJS.Id);
        }
    }
    self.gotoBasic = function () {
        $('#BasicDetail').css('display', 'block');
        $('#LiabilityDetail').css('display', 'none');
        $('#liabilities').removeClass("active step");
    }
    self.gotoSummary = function () {
        $('#SummaryDetail').css('display', 'block');
        $('#LiabilityDetail').css('display', 'none');
        $('#summary').removeClass("disabled step");
        $('#summary').addClass("active step");

    }
    self.backtoLiability = function () {
        $('#SummaryDetail').css('display', 'none');
        $('#LiabilityDetail').css('display', 'block');
        $('#summary').removeClass("active step");
    }
    self.gotoLiability = function () {
        //KeysUtils.moveFormNext($('#LiabilityDetail'));
        $('#BasicDetail').css('display', 'none');
        $('#LiabilityDetail').css('display', 'block');
        $('#liabilities').addClass("active step");
    }


}

function LiabilityValueViewModel(data) {
    var self = this;
    //self.Status = ko.observable(data.Status) || ko.observable();
    //self.Id = ko.observable(data ? data.Id ? data.Id : 0 : 0);
    //self.PropertyId = ko.observable(data ? data.PropertyId ? data.PropertyId : 0 : 0);
    self.Status = ko.observable(data ? data.Status : "Load");
    self.Id = ko.observable(data ? data.Id : 0);
    self.PropertyId = ko.observable(data ? data.PropertyId : 0);
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.Value = ko.observable(data.Value) || ko.observable();
    self.Value.extend(Extender.decimalNumeric());
    self.TypeId = ko.observable(data.TypeId) || ko.observable(1);
    self.Errors = ko.validation.group([
        self.Value
    ]);
    self.IsValid = ko.computed(function () {
        return self.Errors().length == 0;
    });
    self.TypeOptions = { 1: 'Bond', 2: 'Insurance', 3: 'Letting', 4: 'Body Corp' };
    self.ValueType = ko.computed(function () {
        return self.TypeOptions[self.TypeId() + ''];
    });
}


function reload() {
    window.location.replace("/PropertyOwners/Home/Index");
    //window.location.replace(returnToUrl);
}