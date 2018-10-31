function AddTenantViewModel(propId, yearBuilt) {
    debugger;
    var self = this;
    self.changeTitle = function () {
        $('#propertyTitle').html(" Add New Tenant Details ");
    }
    self.changeTitle();
    var token = $("input[name = '__RequestVerificationToken']").val();
    var tenantUrl = $("#addTenant").val();
    self.PropId = ko.observable(propId);
    self.YearBuilt = ko.observable(yearBuilt).extend({
        required: {
            params: true,
            message: "Please enter the Year Built."
        },

        pattern: {
            params: "^(19\\d\\d|200[0-9]|201[0-7]){0,4}$",
            message: "The Year Built field must be a number and is from 1900 to present."
        }
    });
    self.TenantToPropertyModel = new TenantModel(yearBuilt);
    self.tenantErrors = ko.validation.group(self.TenantToPropertyModel, { deep: true, live: true });
    self.isTenantValid = ko.computed(function () {
        return self.tenantErrors().length == 0;
    });
    ko.bindingHandlers.saveTenant = {
        init: function (elem, value, allProp, model, context) {
            var accessor = ko.unwrap(value());
            debugger
            //console.log(accesor);
            $(elem).on("click", function () {
                //console.log("Finance Url", financeUrl);
                var forSaving = ko.toJSON(accessor);
                $.ajax({
                    type: 'post',
                    url: tenantUrl,
                    headers: {
                        "__RequestVerificationToken": token
                    },
                    data: forSaving,
                    dataType: 'json',
                    contentType: 'application/json;charset=utf-8',
                    success: function (response) {
                        console.log(response);
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

                        $("#tenantSection").collapse('hide');
                        $('body').removeClass('modal-open');
                        $('.modal-backdrop').remove();

                        location.href = "/PropertyOwners/";

                    },
                    error: function (error) {
                        alert(error.statusText);
                    }
                });

                return false;
            }
            );
        }
    };
}
function AddFinancialViewModel(propId, yearBuilt, IsOwnerOccupied) {
    debugger;
    var self = this;
    self.changeTitle = function () {
        $('#propertyTitle').html(" Add New Financial Details ");
    }
    self.changeTitle();
    self.IsOwnerOccupied = ko.observable(IsOwnerOccupied);
    var token = $("input[name = '__RequestVerificationToken']").val();
    var financeUrl = $("#updateFinance").val();
    var tenantUrl = $("#addTenant").val();
    self.PropId = ko.observable(propId);
    self.YearBuilt = ko.observable(yearBuilt).extend({
        required: {
            params: true,
            message: "Please enter the Year Built."
        },

        pattern: {
            params: "^(19\\d\\d|200[0-9]|201[0-7]){0,4}$",
            message: "The Year Built field must be a number and is from 1900 to present."
        }
    });
    ko.bindingHandlers.stopBinding = {
        init: function () {
            return { controlsDescendantBindings: true };
        }
    };
    self.PurchasePrice = ko.observable('').extend(Extender.decimalNumeric());
    self.Mortgage = ko.observable().extend(Extender.decimalNumeric());
    self.CurrentHomeValue = ko.observable().extend(Extender.decimalNumeric());
    self.HomeValueType = ko.observable("");
    self.repayments = ko.observableArray().extend({
        minArrayLength: {
            params: {
                minLength: 1,
                objectType: "RepaymentModel"
            },
            message: 'Must specify at least one repayment'
        }
    });
    self.addRepayment = function () {
        self.repayments.push(new RepaymentModel(yearBuilt));
    };
    self.addRepayment();
    self.removeRepayment = function (repayment) {
        self.repayments.remove(repayment);
    };
    self.expenses = ko.observableArray();

    self.addExpense = function () {
        self.expenses.push(new ExpenseModel(yearBuilt));
    };
    self.addExpense();
    self.financeErrors = ko.validation.group(this, { deep: true, live: true });
    self.isFinanceValid = ko.computed(function () {
        return self.financeErrors().length == 0;
    });

    ko.bindingHandlers.saveFinancial = {
        init: function (elem, value, allProp, model, context) {
            var accessor = ko.unwrap(value());
            //console.log(accesor);
            $(elem).on("click", function () {
                console.log("OWNEROCCUPIED", self.IsOwnerOccupied());
                debugger;
                //console.log("Finance Url", financeUrl);
                var forSaving = ko.toJSON(accessor);
                console.log("Financial object", forSaving, "AncessorId", accessor.PropId());
                $.ajax({
                    type: 'post',
                    url: financeUrl,
                    headers: {
                        "__RequestVerificationToken": token
                    },
                    data: forSaving,
                    dataType: 'json',
                    contentType: 'application/json;charset=utf-8',
                    success: function (result) {

                        console.log(result);
                        //location.href="/PropertyOwners/";
                    },
                    error: function (error) {
                        alert(error.statusText);
                    }
                });
                if (self.IsOwnerOccupied()) {
                    location.href = "/PropertyOwners/";
                }
                else {
                    $('#propertyTitle').html(" Add New Tenant Details ");
                    $("#financeSection").collapse('hide');
                    $("#tenantSection").collapse('show');
                    $('body').removeClass('modal-open');
                    $('.modal-backdrop').remove();
                    ko.applyBindings(new AddTenantViewModel(accessor.PropId(), accessor.YearBuilt()), document.getElementById('addPropertyTenant'));
                }

                return false;
            }
            );
        }
    };
    self.removeExpense = function (expense) {
        self.expenses.remove(expense);
    };

}
function RepaymentModel(yearBuilt, data) {
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    }
    self.Amount = ko.observable("").extend(Extender.decimalNumeric());
    self.FrequencyType = ko.observable("").extend({ required: { params: false, message: "" } });
    self.startDate = ko.observable("").extend({
        date: true,
        required: { params: true, message: "Please enter start date." },
        dateAfterYearBuilt: self.Year
    });
    self.endDate = ko.observable("").extend({
        date: true,
        required: { params: true, message: " " },
        dateAfterStart: self.startDate
    });
}
function ExpenseModel(yearBuilt, data) {
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    }
    self.Amount = ko.observable("").extend(Extender.decimalNumeric());
    self.Description = ko.observable("").extend(Extender.description);
    self.ExpenseDate = ko.observable("").extend({
        date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        dateAfterYearBuilt: self.Year
    });
}
function TenantModel(yearBuilt) {
    var self = this;
    self.Year = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.Year(year);
    }
    self.TenantEmail = ko.observable().extend(Extender.email);
    self.StartDate = ko.observable("").extend({
        date: true,
        required: {
            params: true, message: "Please enter a date."
        },
        dateAfterYearBuilt: self.Year
    });
    self.EndDate = ko.observable("").extend({
        date: true,
        //required: { params: true, message: "Please enter end date." },
        dateAfterStart: self.StartDate
    });
    self.PaymentFrequencyId = ko.observable('1');
    self.PaymentAmount = ko.observable().extend(Extender.decimalNumeric());
}
function PageViewModel(dataVm, dic) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    self.ShowProps = ko.observable(true);
    self.ShowFinance = ko.observable(false);
    self.ValidFileTypes = KeysFiles.validImgFiles;
    self.ExtendDic = dic;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, self.ExtendDic);
        KeysUtils.injectViewProps(item);
        item.IsOwnerOccupiedDisplay = ko.computed(function () {
            return item.Model.IsOwnerOccupied() ? "Yes" : "No";
        });
        item.Finance = new PropertyFinancialModel(item);
        
    });
    KeysUtils.injectOpProps(self);
    self.ShowEditView = function (item) {
        viewModel.SelectedItem(item);
        var itemCopy = ko.mapping.fromJS(ko.toJS(item));
        KeysUtils.injectExtends(itemCopy.Model, viewModel.ExtendDic);
        KeysUtils.injectViewProps(itemCopy);
        viewModel.SelectedItemCopy(itemCopy);
        viewModel.EditView(true);
        viewModel.MainView(false);
        viewModel.DetailView(false);
        viewModel.DeleteView(false);
        KeysMap.findGeoCodeAddress(itemCopy.Model.Address, geocoder);
    }
    self.ShowFinanceDetail = function (data) {
        self.ShowProps(false);
        self.ShowFinance(true);
        self.SelectedItem(data);
        $('.menu .item').tab();
        GetFinaceDetails(data);
    }
    self.ShowPropertites = function () {
        self.ShowProps(true);
        self.ShowFinance(false);
    }
    function GetFinaceDetails(data) {
        $.ajax({
            type: "GET",
            url: "/PropertyOwners/Property/GetPropertyFinanceDetails/",
            data: { propId: data.Model.Id() },
            success: function (res) {
                self.SelectedItem().Finance.UpdateDetails(res);
            },
            error: function (error) {
                alert(error.status + " <--and--> " + error.statusText);
            }

        });
    }
}