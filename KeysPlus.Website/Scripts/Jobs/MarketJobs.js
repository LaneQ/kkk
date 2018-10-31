function PageViewModel(dataVm) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectViewProps(item);
    });
    self.SelectedItem = ko.observable();
    self.MainView = ko.observable(true);
    self.DetailView = ko.observable(false);
    self.ApplyView = ko.observable(false);
    self.SupplierInfoView = ko.observable(false);
    self.JobQuote = ko.observable();
    self.SupplierInfo = new SupplierInfo(KeysExtendsDic.SupplierInfo);
    self.ShowMain = function () {
        self.DetailView(false);
        self.MainView(true);
        self.ApplyView(false);
        
    }
    self.ShowDetail = function (data) {
        self.SelectedItem(data);
        self.DetailView(true);
        self.MainView(false);
        self.ApplyView(false);
    }
    self.ShowApply = function (data) {
        if (self.IsProfileComplete()) {
            self.MainView(false);
            self.DetailView(false);
            self.SupplierInfoView(false);
            self.ApplyView(true);
        }
        else {
            self.MainView(false);
            self.DetailView(false);
            self.ApplyView(false);
            self.SupplierInfoView(true);
        }
        self.SelectedItem(data);
        var quote = new EntityViewModel(KeysExtendsDic.JobQuote(data.Model.MaxBudget()));
        quote.Model.JobRequestId = data.Model.Id();
        quote.Model.PropertyId = data.Model.PropertyId();
        quote.Model.Id = -1;
        self.JobQuote(quote)
    }
    self.HasMapData = ko.computed(function () {
        var item = self.SelectedItem();
        if (!item) return false;
        return item.Address.Latitude && item.Address.Longitude;
    });
    self.SubmitSupplierInfo = function (data) {
        var formData = KeysUtils.toData(data.Model);
        formData.append("__RequestVerificationToken", token);
        $.ajax({
            type: 'POST',
            url: '/Companies/Onboarding/AddNewServiceProvider',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    self.IsProfileComplete(true);
                    self.MainView(false);
                    self.DetailView(false);
                    self.SupplierInfoView(false);
                    self.ApplyView(true);
                }
                else {
                    KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                }
            }
        });
    }
    self.SubmitQuote = function (data) {
        var formData = KeysUtils.toData(data.Model);
        console.log(data);
        formData.append("__RequestVerificationToken", token);
        $.ajax({
            url: "/Jobs/Home/SaveJobQuote",
            data: formData,
            method: "POST",
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Quote submitted successfully!</p>', 'error', KeysUtils.reload);
                }
                else {
                    KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                }
            }
        });
    }
    self.RemoveFile = function (data) {
        self.JobQuote().RemoveFile(data);
    }

    self.AddOrRemoveWatchList = function (data) {
        var Id = data.Model.Id();
        if (data.IsInWatchlist()) {
            $.ajax({
                type: "POST",
                url: '/Personal/Watchlist/RemoveFromWatchlist',
                data: { Id: Id, Type: 2 },
                success: function (response) {
                    if (response.Success) {
                        data.IsInWatchlist(false);
                    }
                }
            });
        }
        else {
            $.ajax({
                type: "POST",
                url: '/Personal/Watchlist/SaveToWatchlist',
                data: { Id: Id, ItemType: 2 },
                success: function (response) {
                    if (response.Success) {
                        data.IsInWatchlist(true);
                    }
                }
            });
        }
    }
}
function PersonAddressModel() {
    var self = this;
    self.Number = ko.observable().extend({
        required: {
            params: true,
            message: "Please include your property's number."
        },
        pattern: {
            params: "^[A-Za-z0-9][A-Za-z0-9\\d\\s\\-,_\\//]{0,50}$",
            message: "The Number field must be alphanumeric"
        }
    });
    self.Street = ko.observable().extend({
        required: {
            params: true,
            message: "Please include your Street."
        },
        pattern: {
            params: "^[A-Za-z0-9][A-Za-z0-9\\s\-\\,\\/\\&\]{1,100}$",
            message: "The Street field Must be between 1-100 characters and cannot start with number."
        }
    });

    self.Suburb = ko.observable().extend({
        required: {
            params: true,
            message: "Please include your suburb."
        },
        pattern: {
            params: "^[A-Za-z ]{1,50}$",
            message: "The suburb field can accept only 1-50 characters."
        }
    });
    self.City = ko.observable().extend({
        required: {
            params: true,
            message: "Please include your City."
        },
        pattern: {
            params: "^[A-Za-z ]{1,20}$",
            message: "The city field can accept only 1-20 characters."
        }
    }),
        self.PostCode = ko.observable().extend({
            required: {
                params: true,
                message: "Please include post code."
            },
            pattern: {
                params: "^[0-9 ]{0,4}$",
                message: "The post code field must be numeric between 1-4 characters."
            }
        });
    self.Region = ko.observable();
    self.Latitude = ko.observable();
    self.Longitude = ko.observable();
    self.Country = ko.observable();
    self.Errors = ko.validation.group(self);
    self.IsValid = ko.computed(function () {
        return self.Errors().length == 0;
    });

}
function SupplierInfo(dic) {
    var self = this;
    var interact = false;
    self.validFileTypes = ["image/jpeg", "image/png", "image/gif"];
    self.Model = new Entity(dic);
    self.Model.PhysicalAddress = new PersonAddressModel();
    self.Model.BillingAddress = new PersonAddressModel();
    self.Model.PhotoFile = ko.observable();
    self.ImgData = ko.observable();
    self.ShowFirst = ko.observable(true);
    self.ShowSecond = ko.observable(false);
    self.ToNext = function () {
        self.ShowFirst(false);
        self.ShowSecond(true);
        $('#cd').removeClass("active");
        $("#ca").addClass("active");
    }
    self.ToPrevious = function () {
        self.ShowFirst(true);
        self.ShowSecond(false);
        $('#ca').removeClass("active");
        $("#cd").addClass("active");
    }
    self.IsShipSame = ko.observable(false);
    self.CheckShip = ko.computed(function () {
        if (self.IsShipSame()) {
            interact = true;
            UpdateAddress(self.Model.PhysicalAddress, self.Model.BillingAddress);
        }
        else {
            if (interact) {
                clearAddress(self.Model.BillingAddress);
            }

        }
    });
    self.FirstPageErrors = ko.validation.group([self.Model.Name, self.Model.PhoneNumber, self.Model.Website]);
    self.FirstPageValid = ko.computed(function () {
        return self.FirstPageErrors().length == 0;
    });
    self.SecondPageErrors = ko.validation.group([self.Model.PhysicalAddress, self.Model.BillingAddress], { deep: true });
    self.SecondPageValid = ko.computed(function () {
        return self.SecondPageErrors().length == 0;
    });
    function clearAddress(address) {
        address.Number(null);
        address.Street(null);
        address.Suburb(null);
        address.City(null);
        address.PostCode(null);
        address.Latitude(null);
        address.Longitude(null);
    }
    function UpdateAddress(data, address) {
        data.Number ? address.Number(data.Number()) : 1;
        data.Street ? address.Street(data.Street()) : 1;
        data.Suburb ? address.Suburb(data.Suburb()) : 1;
        data.City ? address.City(data.City()) : 1;
        data.PostCode ? address.PostCode(data.PostCode()) : 1;
        data.Latitude ? address.Latitude(data.Latitude()) : 1;
        data.Longitude ? address.Longitude(data.Longitude()) : 1;
    }
}