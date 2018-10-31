// Scripts for PropertyWatchlist tab.
function RentalWatchlistViewModel(dataVm) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectViewProps(item);
    });
    self.MainView = ko.observable(true);
    self.DetailView = ko.observable(false);
    self.ShowDelete = ko.observable(false);
    self.SelectedItem = ko.observable();
    self.ApplyView = ko.observable(false);
    self.RentalApplication = ko.observable();
    self.TenantInfoView = ko.observable(false);
    self.TenantInfo = new TenantInfoVm(KeysExtendsDic.TenantInfo);
    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
        self.ShowDelete(false);
        self.ApplyView(false);
        self.TenantInfoView(false);
    }
    self.confirmationModal = function ()
    { }
    self.HasApplied = function () {

    }
    self.ShowDetails = function (data) {
        self.MainView(false);
        self.DetailView(true);
        self.ApplyView(false);
        self.ShowDelete(false);
        self.TenantInfoView(false);
        self.SelectedItem(data);
        $('.tabular.menu .item').click(function () {
            $('.active').removeClass('active');
            $(this).addClass('active');
        });
        window.scrollTo(0, 0);
    }
    self.DeleteWatchlist = function (data) {
        self.ShowDelete(true);
        self.MainView(false);
        self.DetailView(false);
        self.ApplyView(false);
        self.SelectedItem(data);
    }
    self.HasMapData = ko.computed(function () {
        var item = self.SelectedItem();
        if (!item) return false;
        return item.Address.Latitude && item.Address.Longitude;
    });
    self.confirmDelete = function (data) {
        var watchListId = self.SelectedItem().Model.WatchListId();
        $.ajax({
            type: 'POST',
            url: '/Personal/Watchlist/DeleteWatchlist',
            data: { Id: watchListId, Type: 1 },
            dataType: 'json',
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Watchlist Deleted successfully!</p>', 'notice', KeysUtils.reload);
                    self.ShowMain();
                }
                else {
                    KeysUtils.notification.show('<p>Something went dfd wrong please try again later!</p>', 'error');
                }
            }
        });
    }
    self.ApplyRental = function (data) {
        var application = new EntityViewModel(KeysExtendsDic.RentalApp);
        if (self.IsUserTenant(true)) {
            self.MainView(false);
            self.DetailView(false);
            self.TenantInfoView(false);
            self.ApplyView(true);
            self.SelectedItem(data);
        }
        else {
            self.MainView(true);
            self.DetailView(false);
            self.ApplyView(false);
            self.TenantInfoView(true);
        }
        self.SelectedItem(data);
        application.Model.RentalListingId = data.Model.Id();
        application.Model.PropertyId = data.Model.PropertyId();
        application.Model.ApplicationStatusId = 1;
        self.RentalApplication(application);
    }
    self.RemoveFile = function (data) {
        self.RentalApplication().RemoveFile(data);
    }
    self.SubmitApp = function (data) {
        var formData = KeysUtils.toData(data.Model);
        console.log(data);
        $.ajax({
            type: 'POST',
            url: '/Personal/Watchlist/AddRentalApplication',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Application submitted successfully!</p>', 'notice', KeysUtils.reload);
                    self.toMain();
                }
                else {
                    KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                }
            }
        });
    }
    function TenantInfoVm(dic) {
        var self = this;
        self.validFileTypes = ["image/jpeg", "image/png", "image/gif"];
        self.Model = new Entity(dic);
        self.Model.PhotoFile = ko.observable();
        self.ImgData = ko.observable();
        self.Model.DateOfBirth = ko.observable().extend({
            date: true,
            required: {
                params: true,
                message: "Please enter a date."
            }
        });
        self.Model.Address = new AddressViewModel();
        self.AddressField = ko.observable().extend({
            required: {
                params: true,
                message: "Please enter address."
            }
        });
        self.Errors = ko.validation.group([self.Model, self.AddressField], { deep: true });
        self.IsValid = ko.computed(function () {
            return self.Errors().length === 0;
        });
    }
}

// Scripts for JobWatchlist Tab
function SupplierInfo(dic) {
    var self = this;
    var interact = false;
    self.validFileTypes = ["image/jpeg", "image/png", "image/gif"];
    self.Model = new Entity(dic);
    self.Model.PhysicalAddress = new AddressViewModel();
    self.Model.BillingAddress = new AddressViewModel();
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
        return self.FirstPageErrors().length === 0;
    });
    self.SecondPageErrors = ko.validation.group([self.Model.PhysicalAddress, self.Model.BillingAddress, self.PhysicalAddressField, self.BillingAddressField], { deep: true });
    self.SecondPageValid = ko.computed(function () {
        return self.SecondPageErrors().length === 0;
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
// view model if item type is market job
function MaketJobWatchlistViewModel(dataVm) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectViewProps(item);
    });

    self.MainView = ko.observable(true);
    self.DetailView = ko.observable(false);
    self.ShowDelete = ko.observable(false);
    self.SelectedItem = ko.observable();
    self.ApplyView = ko.observable(false);
    self.SupplierInfoView = ko.observable(false);
    self.JobQuote = ko.observable();
    self.SupplierInfo = new SupplierInfo(KeysExtendsDic.SupplierInfo);


    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
        self.ShowDelete(false);
        self.ApplyView(false);
    }
    self.ShowDetails = function (data) {
        self.MainView(false);
        self.DetailView(true);
        self.ApplyView(false);
        self.ShowDelete(false);
        self.SelectedItem(data);
    }
    self.DeleteWatchlist = function (data) {
        self.ShowDelete(true);
        self.MainView(false);
        self.DetailView(false);
        self.ApplyView(false);
        self.SelectedItem(data);
    }
    self.HasMapData = ko.computed(function () {
        var item = self.SelectedItem();
        if (!item) return false;
        //debugger;
        return item.Address.Latitude && item.Address.Longitude;
    });
    self.confirmDelete = function (data) {
        var watchListId = self.SelectedItem().Model.WatchListId();
        // alert(watchListId);
        $.ajax({
            type: 'POST',
            url: '/Personal/Watchlist/DeleteWatchlist',
            data: { Id: watchListId, Type: 2 },
            dataType: 'json',
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Watchlist Job Deleted successfully!</p>', 'notice', KeysUtils.reload);
                    self.ShowMain();
                }
                else {
                    KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                }
            }
        });
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

        console.log(data);
        self.JobQuote(quote);
    }
    self.SubmitSupplierInfo = function (data) {
        var formData = KeysUtils.toData(data.Model);
        formData.append("__RequestVerificationToken", token);
        $.ajax({
            type: 'POST',
            url: '/Companies/Onboarding/AddNewServiceSupplier',
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
}
