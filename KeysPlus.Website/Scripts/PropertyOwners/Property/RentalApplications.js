function PageViewModel(dataVm) {
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
    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
    }
    self.ShowDetail = function (data) {
        self.MainView(false);
        self.DetailView(true);
        self.SelectedItem(data);
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/ApplicationViewed',
            data: { id: data.Model.Id() },
            success: function (result) {
                if (result.Updated) {
                    self.SelectedItem().Model.IsViewedByOwner(true);
                    //$("#newApplicationCount").load(location.href + " #newApplicationCount");
                }
            },
            error: function () { },
            fail: function () { }
        });
    }
    self.AcceptApplication = function (data) {
        var appDetails = new AcceptRentalApplicationModel(data);
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/ApplicationAccepted',
            data: ko.toJSON(appDetails),
            contentType: 'application/json',
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Application successfully accepted!</p>', 'notice', KeysUtils.reload);
                }
            },
            error: function () { debugger; },
            fail: function () { },
        });
    }
    self.DeclineApplication = function (data) {
        var appDetails = new AcceptRentalApplicationModel(data);
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/ApplicationDeclined',
            data: ko.toJSON(appDetails),
            contentType: 'application/json',
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Application has been declined!</p>', 'notice');
                    self.Items.remove(data);
                }
            },
            error: function () { debugger; },
            fail: function () { },
        });
    }
    console.log(self);
}
function AcceptRentalApplicationModel(item) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    self.__RequestVerificationToken = token;
    self.TenantId = ko.observable(item.Model.PersonId());
    self.PropertyId = ko.observable(item.Model.PropertyId());
    self.ApplicationId = ko.observable(item.Model.Id());
    self.RentalListingId = ko.observable(item.Model.RentalListingId());
}