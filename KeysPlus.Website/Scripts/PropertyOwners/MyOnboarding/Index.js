function RepaymentModel(yearBuilt) {
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    } 
    self.Amount = ko.observable("").extend(Extender.commonDecimal);
    self.FrequencyType = ko.observable("").extend({ required: { params: false, message: "" } });

    self.startDate = ko.observable("").extend({
        required: { params: true, message: "Please enter start date." },
        datePickerAfterYear: self.Year,
    });
    self.endDate = ko.observable().extend({
        datePickerAfter: self.startDate
    });
}
function ExpenseModel(yearBuilt) {      //  we are no longer having expense in the onboarding process
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    }
    self.Amount = ko.observable("").extend(Extender.commonDecimal);
    self.Description = ko.observable().extend(Extender.description);
    self.ExpenseDate = ko.observable("").extend({
        //date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        datePickerAfterYear: self.Year
    });
}
function TenantModel(yearBuilt) {
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    }
    self.TenantEmail = ko.observable().extend(Extender.email);
    self.FirstName = ko.observable().extend(Extender.firstName);
    self.LastName = ko.observable().extend(Extender.lastName);
    //self.RentAmount = ko.observable().extend(Extender.decimalNumeric());
    self.IsMainTenant = ko.observable(true);
    self.StartDate = ko.observable("").extend({
        //date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        //dateAfterYearBuilt: self.Year,
        datePickerAfterYear: self.Year
    });
    self.EndDate = ko.observable("").extend({
        //date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        datePickerAfter: self.StartDate
    });
    self.PaymentFrequencyId = ko.observable(1);
    self.RentFrequency = ko.observable();
    self.PaymentAmount = ko.observable().extend(Extender.commonDecimal);
    self.PaymentDueDate = ko.observable(1);
    self.TenantExist = ko.observable(false);
    self.CheckTenant = function () {
        $.ajax({
            type: "GET",
            url: '/Account/CheckAccountExist',
            data: { userName: self.TenantEmail() },
            dataType: "json",
            success: function (result) {
                if (result.Exist) {
                    self.TenantExist(true);
                    self.FirstName(result.FirstName);
                    self.LastName(result.LastName);
                }
                else {
                    self.TenantExist(false);
                    self.FirstName(null);
                    self.LastName(null);
                }
            },
        });
    }
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

    self.PaymentStartDate = ko.observable().extend({
        required: {
            params: true, message: "Please enter a date."
        },
        datePickerAfter: self.StartDate,
        datePickerAfterYear: self.Year,
    });
    
}

function propertyViewModel() {

    $('#property-details').css("display", "block");
    $('#financeSection').css('display', "none");
    $('#tenantSection').css('display', "none");

    $('#withoutTenant').hide();
    $('#withTenant').show();

    $('#financePrevious').click(function () {
        $('#financeStep').removeClass("active step");
        $('#financeStep').addClass("step");
        $('#propertyStep').removeClass("step");
        $('#propertyStep').addClass("active step");

    });
    $('#tenantPrevious').click(function () {
        $('#tenantStep').removeClass("active step");
        $('#tenantStep').addClass("step");
        $('#financeStep').removeClass("step");
        $('#financeStep').addClass("active step");

    });
    var current_fs, next_fs, previous_fs; //fieldsets
    var left, opacity, scale; //fieldset properties which we will animate
    var animating; //flag to prevent quick multi-click glitches
    function moveNext(element) {
        if (animating) return false;
        animating = true;

        current_fs = element.parent();
        next_fs = element.parent().next();

        //activate next step on progressbar using the index of next_fs
        //$("#progressbar li").eq($("fieldset").index(next_fs)).addClass("active");

        //show the next fieldset
        next_fs.show();
        //hide the current fieldset with style
        current_fs.animate({ opacity: 0 }, {
            step: function (now, mx) {
                //as the opacity of current_fs reduces to 0 - stored in "now"
                //1. scale current_fs down to 80%
                scale = 1 - (1 - now) * 0.2;
                //2. bring next_fs from the right(50%)
                left = (now * 50) + "%";
                //3. increase opacity of next_fs to 1 as it moves in
                opacity = 1 - now;
                current_fs.css({ 'transform': 'scale(' + scale + ')' });
                next_fs.css({ 'left': left, 'opacity': opacity });
            },
            duration: 800,
            complete: function () {
                current_fs.hide();
                animating = false;
            },
            //this comes from the custom easing plugin
            easing: 'easeInOutBack'
        });
    }
    var self = this;
    self.CanOperate = ko.observable(true);
    self.MediaFiles = ko.observableArray();
    self.FileWarning = ko.observable();
    self.ValidFiles = ["image/gif", "image/jpeg", "image/png", "image/jpg"];
    self.RemoveFile = function (file) {
        self.MediaFiles.remove(file);
    }

    self.goF = function () {
        $('#property-details').css("display", "none");
        $('#financeSection').css('display', 'block');
        $('#pd').removeClass("active");
        $("#fd").addClass("active");
        $('#propertyStep').removeClass("active step");
        $('#propertyStep').addClass("step");
        $('#financeStep').removeClass("step");
        $('#financeStep').addClass("active step");
        $('#propertyStepNT').removeClass("active step");
        $('#propertyStepNT').addClass("step");
        $('#financeStepNT').removeClass("step");
        $('#financeStepNT').addClass("active step");
        window.scrollTo(0, 0);
        self.Repayments().forEach(function (repayment) {
            repayment.UpdateYear(self.YearBuilt());
        });
        self.expenses().forEach(function (expense) {
            expense.UpdateYear(self.YearBuilt());
        });
        self.TenantToPropertyModel.UpdateYear(self.YearBuilt());
    }

    self.goP = function () {
        $('#property-details').css("display", "block");
        $('#financeSection').css('display', 'none');
        $('#fd').removeClass("active");
        $("#pd").addClass("active");
        $('#financeStep').removeClass("active step");
        $('#financeStep').addClass("step");
        $('#propertyStep').removeClass("step");
        $('#propertyStep').addClass("active step");
        $('#financeStepNT').removeClass("active step");
        $('#financeStepNT').addClass("step");
        $('#propertyStepNT').removeClass("step");
        $('#propertyStepNT').addClass("active step");
        window.scrollTo(0, 0);
    }
    self.goT = function () {
        $('#financeSection').css('display', 'none');
        $('#tenantSection').css("display", "block");
        $('#fd').removeClass("active");
        $("#td").addClass("active");
        $('#financeStep').removeClass("active step");
        $('#financeStep').addClass("step");
        $('#tenantStep').removeClass("step");
        $('#tenantStep').addClass("active step");
        window.scrollTo(0, 0);
    }
    self.TtoF = function () {
        $('#financeSection').css('display', 'block');
        $('#tenantSection').css("display", "none");
        $('#td').removeClass("active");
        $("#fd").addClass("active");
        $('#tenantStep').removeClass("active step");
        $('#tenantStep').addClass("step");
        $('#financeStep').removeClass("step");
        $('#financeStep').addClass("active step");
        window.scrollTo(0, 0);
    }

    self.PropertyName = ko.observable().extend(Extender.propertyName);
    self.AddressName = ko.observable().extend(Extender.addressName);
    self.PropId = ko.observable();
    self.Description = ko.observable().extend(Extender.description);
    self.Address = new AddressViewModel();
    KeysMap.findGeoCodeAddress(self.Address, geocoder);
    self.PropertyTypeId = ko.observable(1);
    self.LandArea = ko.observable().extend(Extender.landArea);
    self.FloorArea = ko.observable().extend(Extender.floorArea);
    self.TargetRent = ko.observable().extend(Extender.targetRent);
    self.TargetRentType = ko.observable(1);
    self.YearBuilt = ko.observable().extend(Extender.yearBuilt);
    self.Bathroom = ko.observable().extend(Extender.bathroom);
    self.Bedroom = ko.observable().extend(Extender.bedroom);
    self.ParkingSpace = ko.observable().extend(Extender.parkingSpace);
    self.IsOwnerOccupied = ko.observable(false);
    self.CheckOwnOcc = ko.computed(function () {
        if (self.IsOwnerOccupied()) {
            $('#withoutTenant').show();
            $('#withTenant').hide();
        }
        else {
            $('#withoutTenant').hide();
            $('#withTenant').show();
        }
    });
    self.PurchasePrice = ko.observable().extend(Extender.commonDecimal);
    self.Mortgage = ko.observable().extend(Extender.commonDecimal);
    self.CurrentHomeValue = ko.observable().extend(Extender.commonDecimal);
    self.HomeValueType = ko.observable(1);
    self.Repayments = ko.observableArray().extend({
        minArrayLength: {
            params: {
                minLength: 1,
                objectType: "RepaymentModel"
            },
            message: 'Must specify at least one repayment'
        }
    });
    self.basicError = ko.validation.group([
        self.PropertyName,
        self.Address.Number,
        self.Address.Street,
        self.Address.City,
        self.Address.PostCode,
        self.YearBuilt,
        self.TargetRent,
        self.Bathroom,
        self.Bedroom,
        self.ParkingSpace,
        self.Description,
    ]);
    self.basicValid = ko.computed(function () {
        //console.log(self.basicError());
        return self.basicError().length == 0;
    });
    self.addRepayment = function () {
        self.Repayments.push(new RepaymentModel(self.YearBuilt()));
    };
    self.removeRepayment = function (repayment) {
        self.Repayments.remove(repayment);
    };
    self.expenses = ko.observableArray();

    self.addExpense = function () {
        self.expenses.push(new ExpenseModel(self.YearBuilt()));
    };
    self.removeExpense = function (expense) {
        self.expenses.remove(expense);
    };
    self.LiabilityValues = ko.observableArray();

    self.GoToTenant = function () {
        
        self.validateFinancialModel.showAllMessages();
        self.validateRepaymentsModel.showAllMessages();


        if (self.validateFinancialModel() == false && self.validateRepaymentsModel() == false) {
            $('#financeStep').removeClass("active step");
            $('#financeStep').addClass("step");
            $('#tenantStep').removeClass("step");
            $('#tenantStep').addClass("active step");
            moveNext($('#moveToTenant'));
        }
        else {

        }
    }
    self.SkipToTenant = function () {
        moveNext($('#moveToTenant'));

    }
    //Created for property owners who rent out, to validate and save (includes tenant data)
    self.ValidateSaveRented = function (data) {
        self.TenantErrors.showAllMessages();
        if (self.IsTenantValid()) {
            self.SaveAllDetails(data);
            console.log(data);
        }
        else {
            console.log("error to fix");
            console.log(data);
            return;
        }
    }
    //Created for an owner occupier to validate and save property and financial data (there is no tenant data)
    self.ValidateSaveOwnerOcc = function (data) {

        self.validateFinancialModel.showAllMessages();
        self.validateRepaymentsModel.showAllMessages();
        if (self.validateFinancialModel() == false && self.validateRepaymentsModel() == false) {

            self.SaveAllDetails(data);
        }
        else {
            return;
        }
    }
    self.GoToFinance = function () {
        $('#propertyStep').removeClass("active step");
        $('#propertyStep').addClass("step");
        $('#financeStep').removeClass("step");
        $('#financeStep').addClass("active step");

        if (self.PropOnboardErrors().length == 0) {
            moveNext($('#addProperty'));
            self.Repayments().forEach(function (repayment) {
                repayment.UpdateYear(self.YearBuilt());
            });
            self.TenantToPropertyModel.UpdateYear(self.YearBuilt());
        }
        else {
            self.PropOnboardErrors.showAllMessages();
        }
    };
    self.TenantToPropertyModel = new TenantModel(self.YearBuilt());
    self.TenantErrors = ko.validation.group(self.TenantToPropertyModel, { deep: true, live: true });
    self.LiabilityErrors = ko.validation.group(self.LiabilityValues, { deep: true, live: true });
    self.IsTenantValid = ko.computed(function () {
        var a = self.TenantErrors().length == 0;
        var b = self.LiabilityErrors().length == 0;
        return a && b;
    });
    self.validateFinancialModel = ko.validation.group([
        self.PurchasePrice,
        self.Mortgage,
        self.CurrentHomeValue,
    ]);
    self.validateRepaymentsModel = ko.validation.group(
        self.Repayments, { deep: true, live: true }
    );
    self.validateExpensesModel = ko.validation.group(
       self.expenses, { deep: true, live: true }
    );
    self.IsFinancialValid = ko.computed(function () {
        return self.validateFinancialModel().length == 0 && self.validateRepaymentsModel().length == 0 && self.validateExpensesModel().length == 0;
    });
    self.PropOnboardErrors = ko.validation.group([self.PropertyName, self.AddressName, self.Description, self.YearBuilt, self.Bathroom, self.Bedroom, self.ParkingSpace, self.TargetRent]);
    self.PropertyErrors = ko.validation.group([self.PropertyName, self.Description, self.YearBuilt, self.Bathroom, self.Bedroom, self.ParkingSpace, self.FloorArea, self.TargetRent]);
    self.IsPropertyValid = ko.computed(function () {
        var a = self.Address.IsValid();
        var b = self.PropertyErrors().length == 0;
        return a && b;
    });
    self.AddLiabilityValues = function () {
        self.LiabilityValues.push(new LiabilityValueViewModel());
    }
    self.RemoveLiability = function (value) {
        self.LiabilityValues.remove(value);
    }
    self.SaveNewProperty = function (data) {
        self.CanOperate(false);
        var forSaving = ko.toJSON(data);
        var formData = new FormData();
        //for (var key in forSaving) {
        //    //formData.append(key, JSON.stringify(forSaving[key]));
        //    formData.append(key, forSaving[key]);
        //}
        //self.IsOwnerOccupied() ? formData.set('TenantToPropertyModel', ko.toJS(self.TenantToPropertyModel)) : formData.set('TenantToPropertyModel', null);
        //var ad = ko.toJS(self.Address);
        //formData.append('Address', ko.toJS(self.Address))
        self.MediaFiles().forEach(function (item) {
            formData.append("FileUpload", item.File);
        });
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }
        $.ajax({
            type: 'post',
            url: '/PropertyOwners/Property/AddNewProperty',
            data: forSaving,
            async: false,
            dataType: 'json',
            contentType: 'application/json;charset=utf-8',
        }).done(function (response) {
            if (!response.NewPropId) {
                KeysUtils.notification.show('<p>Something has gone wrong, please try again later!</p>', 'error');
                self.CanOperate(true);
                return;
            }
            if (self.MediaFiles().length != 0) {
                $.ajax({
                    type: 'post',
                    url: '/PropertyOwners/Home/UpdatePhotos/' + response.NewPropId,
                    data: formData,
                    async: false,
                    dataType: 'json',
                    contentType: false,
                    processData: false,
                    success: function (response) {
                        debugger;
                        KeysUtils.notification.show('<p>Property added successfully!</p>', 'success', reload);
                    },
                    error: function () {
                        KeysUtils.notification.show('<p>Something has gone wrong, please try again later!</p>', 'error');
                        self.CanOperate(true);
                        return;
                    },
                    fail: function () {
                    }
                });
            }
            KeysUtils.notification.show('<p>Property added successfully!</p>', 'success', reload);

        }).fail(function (data) {
            self.CanOperate(true);
            console.log(data);
        });


    }
    function reload() {
        window.setTimeout(function () {
            KeysUtils.goPage('/PropertyOwners/Home/Index');
        }, 1000);
    }
    self.SaveAllDetails = function (data) {
        var forSaving = ko.toJSON(data);
        console.log(forSaving);
        $.ajax({
            type: 'post',
            url: '/PropertyOwners/Onboarding/AddProperty',
            data: forSaving,
            dataType: 'json',
            contentType: 'application/json;charset=utf-8',
        }).done(function (response) {
            console.log(response);
            //if (!self.IsOwnerOccupied()) {

            //    if (response.Todo && response.Todo == 'Send email') {
            //        var result = confirm("Tenant does not exist in the system.Do you wish your tenant to be registered to the community?");
            //        if (result) {
            //            var tenantData = ko.toJS(self.TenantToPropertyModel);
            //            tenantData = {
            //                TenantEmail: self.TenantToPropertyModel.TenantEmail(),
            //                StartDate: self.TenantToPropertyModel.StartDate(),
            //                EndDate: self.TenantToPropertyModel.EndDate(),
            //                PaymentFrequencyId: self.TenantToPropertyModel.PaymentFrequencyId(),
            //                PaymentAmount: self.TenantToPropertyModel.PaymentAmount(),
            //                PropertyId: response.NewPropId
            //            };

            //            $.ajax({
            //                type: "POST",
            //                url: '/PropertyOwners/Onboarding/SendInvitationEmailToTenant',
            //                data: tenantData,
            //                success: function (data) {
            //                    debugger;
            //                    if (data.Success) {
            //                        window.location.replace("/PropertyOwners/Home/Index");
            //                    }
            //                },
            //            });
            //        }

            //    }
            //}
            window.location = '/PropertyOwners/Home/Index';
        }).fail(function (data) {
            console.log(data);
        });
    }

    ko.bindingHandlers.save = {
        init: function (elem, value, allComp, model, context) {
            $(elem).on("click", function () {
                var accessor = ko.unwrap(value());
                var forSaving = ko.toJSON(accessor);
                $.ajax({
                    type: 'post',
                    url: '/PropertyOwners/Onboarding/AddProperty',
                    data: forSaving,
                    dataType: 'json',
                    contentType: 'application/json;charset=utf-8',
                }).done(function (response) {
                    console.log(response);
                    if (!self.IsOwnerOccupied()) {
                        if (response.Todo && response.Todo == 'Send email') {
                            var result = confirm("Tenant does not exist in the system.Do you wish your tenant to be registered to the community?");
                            if (result) {
                                var tenantData = ko.toJS(self.TenantToPropertyModel);
                                tenantData = {
                                    TenantEmail: self.TenantToPropertyModel.TenantEmail(),
                                    StartDate: self.TenantToPropertyModel.StartDate(),
                                    EndDate: self.TenantToPropertyModel.EndDate(),
                                    PaymentFrequencyId: self.TenantToPropertyModel.PaymentFrequencyId(),
                                    PaymentAmount: self.TenantToPropertyModel.PaymentAmount(),
                                    PropertyId: response.NewPropId
                                };
                                debugger;
                                $.ajax({
                                    type: "POST",
                                    url: '/PropertyOwners/Onboarding/SendInvitationEmailToTenant',
                                    data: tenantData,
                                    success: function (data) {
                                        debugger;
                                        if (data.Success) {
                                            window.location.replace("/PropertyOwners/Home/Index");
                                        }
                                    },
                                });
                            }

                        }
                    }
                    window.location = '/PropertyOwners/Home/Index';

                }).fail(function (data) {
                    console.log(data);
                });

            });
        }
    };
}
function LiabilityValueViewModel() {
    var self = this;
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.Value = ko.observable();
    self.Value.extend(Extender.commonDecimal);
    self.TypeId = ko.observable(1);
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
ko.applyBindings(new propertyViewModel());

