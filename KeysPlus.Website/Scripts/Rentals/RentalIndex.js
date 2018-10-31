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
    self.TenantInfoView = ko.observable(false);
    self.RentalApplication = ko.observable();
    self.TenantInfo = new TenantInfoVm(KeysExtendsDic.TenantInfo);
    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
        self.ApplyView(false);
        self.TenantInfoView(false);
    }
    self.ShowDetails = function (data) {
        self.MainView(false);
        self.DetailView(true);
        self.ApplyView(false);
        self.TenantInfoView(false);
        self.SelectedItem(data);
        $('.tabular.menu .item').click(function () {
            $('.active').removeClass('active');
            $(this).addClass('active');
        });
    }
    self.ApplyRental = function (data) {
        var application = new EntityViewModel(KeysExtendsDic.RentalApp);
        if (self.IsTenantProfileComplete()) {
            self.MainView(false);
            self.DetailView(false);
            self.TenantInfoView(false);
            self.ApplyView(true);
        }
        else {
            $('.ui.basic.modal').modal('show');
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
            url: '/Rental/Home/AddRentalApplication',
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
    self.SubmitTenantInfo = function (data) {
        var formData = KeysUtils.toData(data.Model);
        formData.append("__RequestVerificationToken", token);
        //for (var pair of formData.entries()) {
        //    console.log(pair[0] + ', ' + pair[1]);
        //}
        $.ajax({
            type: 'POST',
            url: '/Tenants/Home/Onboarding',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Details saved successfully!</p>', 'notice');
                    self.MainView(false);
                    self.DetailView(false);
                    self.TenantInfoView(false);
                    self.ApplyView(true);
                }
                else {
                    KeysUtils.notification.show('<p>Something went wrong please try again later!</p>', 'error');
                }
            }
        });
    }
    self.HasMapData = ko.computed(function () {
        var item = self.SelectedItem();
        if (!item) return false;
        return item.Address.Latitude && item.Address.Longitude;
    });

    self.AddOrRemoveWatchList = function (data) {
        var Id = data.Model.Id();
        if (data.IsInWatchlist()) {
            $.ajax({
                type: "POST",
                url: '/Personal/Watchlist/RemoveFromWatchlist',
                data: { Id: Id, Type: 1 },
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
                data: { Id: Id, ItemType: 1 },
                success: function (response) {
                    if (response.Success) {
                        data.IsInWatchlist(true);
                        //KeysUtils.notification.show('<p>Property successfully added to your Watchlist!</p>', 'notice');
                    }

                }
            });
        }

    }
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
        return self.Errors().length == 0;
    });
}

//When Need to sign in pop up window shows user click Yes to go to Onboarding page
$(".ok").click(function () {
    location.replace("/Tenants/Home/Onboarding");
})