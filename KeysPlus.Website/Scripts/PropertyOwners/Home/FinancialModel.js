function RepaymentViewModel(yearBuilt, data) {
    var self = this;
    self.PureData = data;
    self.Id = ko.observable(data ? data.Id ? data.Id : 0 : 0);
    self.PropertyId = ko.observable(data ? data.PropertyId ? data.PropertyId : 0 : 0);
    self.YearBuilt = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.YearBuilt(year);
    }
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.Amount = ko.observable(data.Amount)|| ko.observable("");
    self.Amount.extend(Extender.decimalNumeric());
    self.FrequencyType = ko.observable(data.FrequencyType) || ko.observable(1);

    self.StartDate = ko.observable(data.StartDate) || ko.observable();
    self.StartDate.extend({
        required: { params: true, message: "Please enter date." },
        datePickerAfterYear: self.YearBuilt(),
    });
    self.EndDate = ko.observable(data.EndDate) || ko.observable();
    self.EndDate.extend({ dateAfterStart: self.StartDate});
    self.Errors = ko.validation.group([
        self.Amount,
        self.StartDate,
        self.EndDate,
    ]);
    self.IsValid = ko.computed(function () {
        return self.Errors().length == 0;
    });
    self.FrequencyOptions = { 1: 'Weekly', 2: 'Fortnightly', 3: 'Monthly' };
    self.FrequencyName = ko.computed(function () {
        return self.FrequencyOptions[self.FrequencyType() + ''];
    });
}
function ExpenseViewModel(yearBuilt, data) {
    var self = this;
    self.Dis = ko.observable(false);
    self.PureData = data;
    self.Id = ko.observable(data ? data.Id ? data.Id : 0 : 0);
    self.PropertyId = ko.observable(data ? data.PropertyId ? data.PropertyId : 0 : 0);
    self.YearBuilt = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.YearBuilt(year);
    }
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.Amount = ko.observable(data.Amount) || ko.observable("");
    self.Amount.extend(Extender.decimalNumeric());
    self.Description = ko.observable(data.Description) || ko.observable("");
    self.Description.extend(Extender.description);
    self.ExpenseDate = ko.observable(data.ExpenseDate) || ko.observable();
    self.ExpenseDate.extend({
        required: { params: true, message: "Please enter date." },
        datePickerAfterYear: self.YearBuilt(),
    });
    self.Errors = ko.validation.group([
        self.Amount,
        self.ExpenseDate,
        self.Description,
    ]);
    self.IsValid = ko.computed(function () {
        var val = self.Errors().length == 0;
        return val;
    });
}
function HomeValueViewModel(yearBuilt, data) {
    var self = this;
    self.PureData = data;
    self.Id = ko.observable(data ? data.Id ? data.Id : 0 : 0);
    self.PropertyId = ko.observable(data ? data.PropertyId ? data.PropertyId : 0 : 0);
    self.YearBuilt = ko.observable(yearBuilt);
    self.IsActive = ko.observable(data.IsActive) || ko.observable(false);
    self.Types = [{ Name:'Current', Value : 1}, { Name:'Estimated', Value : 2}, {Name:'Registered', Value : 3}];
    self.UpdateYear = function (year) {
        self.YearBuilt(year);
    }
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.Value = ko.observable(data.Value) || ko.observable("");
    
    self.Value.extend(Extender.decimalNumeric());
    self.TypeId = ko.observable(data.TypeId) || ko.observable("1");
    self.Date = ko.observable(data.Date) || ko.observable();
    self.Date.extend({
        required: { params: true, message: "Please enter date." },
        datePickerAfterYear: self.YearBuilt(),
    });
    self.Errors = ko.validation.group([
        self.Value,
        self.Date,
    ]);
    self.IsValid = ko.computed(function () {
        return self.Errors().length == 0;
    });

    self.TypeOptions = { 1: 'Current', 2: 'Estimated', 3: 'Registered' };
    self.ValueType = ko.computed(function () {
        return self.TypeOptions[self.TypeId() + ''];
    });
}
function RentalPaymentViewModel(yearBuilt, data) {
    var self = this;
    self.PureData = data;
    self.Id = ko.observable(data ? data.Id ? data.Id : 0 : 0);
    self.PropertyId = ko.observable(data ? data.PropertyId ? data.PropertyId : 0 : 0);
    self.YearBuilt = ko.observable(yearBuilt);
    self.UpdateYear = function (year) {
        self.YearBuilt(year);
    }
    self.IsEdit = ko.observable(false);
    self.IsNew = ko.observable(false);
    self.PropertyId = data ? data.PropertyId : 0;
    self.Amount = ko.observable(data.Amount) || ko.observable("");
    self.Amount.extend(Extender.decimalNumeric());
    self.FrequencyTypeId = ko.observable(data.FrequencyTypeId) || ko.observable(1);
    self.FrequencyOptions = { 1: 'Weekly', 2: 'Fortnightly', 3: 'Monthly' };
    self.FrequencyType = ko.computed(function () {
        return self.FrequencyOptions[self.FrequencyTypeId() + ''];
    });
    self.Date = ko.observable(data.Date) || ko.observable();
    self.Date.extend({
        required: { params: true, message: "Please enter date." },
        datePickerAfterYear: self.YearBuilt(),
    });
    self.Errors = ko.validation.group([
       self.Amount,
       self.Date,
    ]);
    self.IsValid = ko.computed(function () {
        return self.Errors().length == 0;
    });
}
function PropertyFinancialModel(property) {
    var self = this;
    self.YearBuilt = ko.observable(property.Model.YearBuilt());
    if (property.PurchasePrice) {
        self.PurchasePrice = ko.observable(property.PurchasePrice());
    }
    if (property.CurrentHomeValue) {
        self.CurrentHomeValue = ko.observable(property.CurrentHomeValue());
    }
    if (property.Mortgage) {
        self.Mortgage = ko.observable(property.Mortgage);
    }
    self.Repayments = ko.observableArray();
    self.IsRepayment = ko.computed(function () {
        if (self.Repayments().length > 0) {
            return true;
        }
        else {
            return false;
        }
    });
    self.Expenses = ko.observableArray();
    self.IsExpense = ko.computed(function () {
        if (self.Expenses().length > 0) {
            return true;
        }
        else {
            return false;
        }
    });
    self.HomeValues = ko.observableArray();
    self.RentalPayments = ko.observableArray();
    self.IsRentalPayment = ko.computed(function () {
        if (self.RentalPayments().length > 0) {
            return true;
        }
        else {
            return false;
        }
    });
    self.Report = ko.observable();
    self.UpdateDetails = function (data) {
        self.CurrentHomeValue(data.CurrentHomeValue);
        self.Mortgage(data.Mortgage);
        self.Report(data.FinanceReport);
        self.PropId = data.PropId;
        self.HomeValues([]);
        data.HomeValues.forEach(function (element) {
            self.HomeValues.push(new HomeValueViewModel(self.YearBuilt(), element));
        });
        self.Repayments([]);
        data.Repayments.forEach(function (element) {
            self.Repayments.push(new RepaymentViewModel(self.YearBuilt(), element));
        });
        self.Expenses([]);
        data.Expenses.forEach(function (element) {
            self.Expenses.push(new ExpenseViewModel(self.YearBuilt(), element));
        });
        self.RentalPayments([]);
        data.RentalPayments.forEach(function (element) {
            self.RentalPayments.push(new RentalPaymentViewModel(self.YearBuilt(), element));
        });
    }
    self.SelectedHomeValue = ko.observable({});
    self.SelectedRepayment = ko.observable({});
    self.SelectedExpense = ko.observable({});
    self.SelectedRentalPayment = ko.observable({});
    
    self.HomeValueDisplay = function (data) {
        if (data.IsNew()) return 'newPropValue';
        if (!self.SelectedHomeValue().Id) return 'displayPropValue';
        var result = data.Id() == self.SelectedHomeValue().Id() ? 'editPropValue' : 'displayPropValue';
        return result;
    }
    self.RepaymentDisplay = function (data) {
        if (data.IsNew()) return 'newRepayment';
        if (!self.SelectedRepayment().Id) return 'displayRepayment';
        var result = data.Id() == self.SelectedRepayment().Id() ? 'newRepayment' : 'displayRepayment';
        return result;
    }
    self.ExpenseDisplay = function (data) {
        if (data.IsNew()) return 'newExpense';
        if (!self.SelectedExpense().Id) return 'displayExpense';
        var result = data.Id() == self.SelectedExpense().Id() ? 'newExpense' : 'displayExpense';
        return result;
    }
    self.RentalPaymentDisplay = function (data) {
        if (data.IsNew()) return 'newRentalPayment';
        if (!self.SelectedRentalPayment().Id) return 'displayRentalPayment';
        var result = data.Id() == self.SelectedRentalPayment().Id() ? 'newRentalPayment' : 'displayRentalPayment';
        return result;
    }

    self.EditRentalPayment = function (data) {
        self.SelectedRentalPayment(ko.mapping.fromJS(ko.mapping.toJS(data)));
    }
    self.CancelEditRentalPayment = function (data) {
       
        if (data.IsNew()) {
            self.RentalPayments.remove(data);
        }
        else {
            self.SelectedRentalPayment({});
            self.RentalPayments.replace(data, new RentalPaymentViewModel(data.YearBuilt(), data.PureData));
        }
    }
    self.AddRentalPayment = function () {
        var newRentalPayment = new RentalPaymentViewModel(self.YearBuilt(), {});
        newRentalPayment.IsEdit(true);
        newRentalPayment.IsNew(true);
        self.RentalPayments.push(newRentalPayment);
    }

    self.EditRepayment = function (data) {
        self.SelectedRepayment(ko.mapping.fromJS(ko.mapping.toJS(data)));
    }
    self.CancelEditRepayment = function (data) {
        if (data.IsNew()) {
            self.Repayments.remove(data);
        }
        else {
            self.SelectedRepayment({});
            self.Repayments.replace(data, new RepaymentViewModel(data.YearBuilt(), data.PureData));
        }
    }
    self.AddRepayment = function () {
        var newRepayment = new RepaymentViewModel(self.YearBuilt(), {});
        newRepayment.IsEdit(true);
        newRepayment.IsNew(true);
        self.Repayments.push(newRepayment);
    }

    self.EditExpense = function (data) {
        self.SelectedExpense(ko.mapping.fromJS(ko.mapping.toJS(data)));
    }
    self.CancelEditExpense = function (data) {
        if (data.IsNew()) {
            self.Expenses.remove(data);
        }
        else {
            self.SelectedExpense({});
            self.Expenses.replace(data, new ExpenseViewModel(data.YearBuilt(), data.PureData));
        }

    }
    self.AddExpense = function () {
        var newExpense = new ExpenseViewModel(self.YearBuilt(), {});
        newExpense.IsEdit(true);
        newExpense.IsNew(true);
        self.Expenses.push(newExpense);
    }

    self.EditHomeValue = function (data) {
        self.SelectedHomeValue(ko.mapping.fromJS(ko.mapping.toJS(data)));
    }
    self.CancelEditHomeValue = function (data) {
        if (data.IsNew()) {
            self.HomeValues.remove(data);
        }
        else {
            self.SelectedHomeValue({});
            self.HomeValues.replace(data, new HomeValueViewModel(data.YearBuilt(), data.PureData));
        }
    }
    self.AddHomeValue = function () {
        var newHomeValue = new HomeValueViewModel(self.YearBuilt(), {});
        newHomeValue.IsEdit(true);
        newHomeValue.IsNew(true);
        self.HomeValues.push(newHomeValue);
    }

    self.SaveHomeValue = function (data, event) {
        var context = ko.contextFor(event.target);
        var index = context.$index();
        var valid = data.IsValid();
        if (!valid) {
            data.Errors.showAllMessages(true);
            return;
        }
        var date = moment(data.Date(), 'DD/MM/YYYY h:mm A');
        date = moment(date).format('DD/MM/YYYY h:mm A');
        var formData = {
            Id: data.Id(),
            PropertyId: self.PropId,
            Value: data.Value(),
            TypeId: data.TypeId(),
            Date: date,
            IsActive : data.IsActive()
        }
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/SavePropertyHomeValue',
            data: formData,
            success: function (result) {
                if (result.Success) {
                    if (result.IsActive) {
                        updateActiveValue(data);
                    }
                    if (result.NewId) {
                        data.IsNew(false);
                        data.Id(result.NewId);
                        return;
                    }
                    self.SelectedHomeValue({});
                }
                else {
                }
            },
            error: function () { },
            fail: function () { }
        });
    }

    function updateActiveValue(item) {
        for (var i = 0; i < self.HomeValues().length; i++) {
            if (self.HomeValues()[i].Id() != item.Id() && self.HomeValues()[i].IsActive()) {
                self.HomeValues()[i].IsActive(false);
                break;
            }
        }
    }
    self.DeleteHomeValue = function (data) {
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/DeleteHomeValue',
            data: { homeValueId: data.Id()},
            success: function (result) {
                if (result.Success) {
                    self.HomeValues.remove(data);
                        return;
                }
                else {
                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.SaveRepayment = function (data) {
        var valid = data.IsValid();
        if (!valid) {
            data.Errors.showAllMessages(true);
            return;
        }
        var startDate = moment(data.StartDate(), 'DD/MM/YYYY h:mm A');
        startDate = moment(startDate).format('DD/MM/YYYY h:mm A');
        var endDate = moment(data.EndDate(), 'DD/MM/YYYY h:mm A');
        endDate = endDate.isValid() ? moment(endDate).format('DD/MM/YYYY h:mm A') : null;
        var formData = {
            Id: data.Id,
            PropertyId: self.PropId,
            Amount: data.Amount(),
            FrequencyType: data.FrequencyType(),
            StartDate : startDate,
            EndDate: endDate
        }
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/SaveRepayment',
            data: formData,
            success: function (result) {
                if (result.Success) {

                    if (result.NewId) {
                        data.IsNew(false);
                        data.Id(result.NewId);
                        return;
                    }
                    self.SelectedRepayment({});
                }
                else {

                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.DeleteRepayment = function (data) {
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/DeleteRepayment',
            data: { repaymentId: data.Id() },
            success: function (result) {
                if (result.Success) {
                    self.Repayments.remove(data);
                    return;
                }
                else {
                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.SaveExpense = function (data) {
        var valid = data.IsValid();
        if (!valid) {
            data.Errors.showAllMessages(true);
            return;
        }
        var date = moment(data.ExpenseDate(), 'DD/MM/YYYY h:mm A');
        date = moment(date).format('DD/MM/YYYY h:mm A');

        var formData = {
            Id : data.Id,
            PropertyId: self.PropId,
            Amount: data.Amount(),
            Description: data.Description(),
            ExpenseDate : date,
        };

        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/SaveExspense',
            data: formData,
            success: function (result) {
                if (result.Success) {
                    if (result.NewId) {
                        data.IsNew(false);
                        data.Id(result.NewId);
                        return;
                    }
                    
                    self.SelectedExpense({});
                }
                else {
                }
            },
            error: function () { },
            fail: function () { }
        });

    }
    self.DeleteExpense = function (data) {
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/DeleteExpense',
            data: { expenseId: data.Id() },
            success: function (result) {
                if (result.Success) {
                    self.Expenses.remove(data);
                    return;
                }
                else {
                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.SaveRentalPayment = function (data) {
        var valid = data.IsValid();
        if (!valid) {
            data.Errors.showAllMessages(true);
            return;
        }
        var date = moment(data.Date(), 'DD/MM/YYYY h:mm A');
        date = moment(date).format('DD/MM/YYYY h:mm A');
        var formData = {
            Id: data.Id,
            PropertyId: self.PropId,
            Amount: data.Amount(),
            FrequencyTypeId: data.FrequencyTypeId(),
            Date : date
        }
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/SaveRentalPayment',
            data: formData,
            success: function (result) {
                if (result.Success) {
                    if (result.NewId) {
                        data.IsNew(false);
                        data.Id(result.NewId);

                        return;
                    }
                    
                    self.SelectedRentalPayment({});
                }
                else {
                    debugger;
                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.DeleteRentalPayment = function (data) {
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/DeleteRentalPayment',
            data: { rentalPaymentId: data.Id() },
            success: function (result) {
                if (result.Success) {
                    self.RentalPayments.remove(data);
                    return;
                }
                else {
                    debugger;
                }
            },
            error: function () { },
            fail: function () { }
        });
    }

}