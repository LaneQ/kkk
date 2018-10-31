function PageViewModel(dataVm) {
    var self = this;
    for (var key in dataVm) {
        self[key]=dataVm[key]
    }
    self.Request = ko.observable();
    self.MainView = ko.observable(true);
    self.SendRequestView = ko.observable(false);
    self.Items().forEach(function (item) {
        KeysUtils.injectViewProps(item);
        item.CalendarMonthYear = ko.computed(function () {
            return moment(item.NextPaymenDate()).format('MMM') + ' ' + moment(item.NextPaymenDate()).format('YYYY');
        });
    });
    self.ShowMain = function () {
        var result = confirm("Do you want to leave?");
        if (result) {
            self.MainView(true);
            self.SendRequestView(false);
        }
    }
    self.ShowRequestForm = function (item) {
        var req = new EntityViewModel(KeysExtendsDic.Request);
        req.Model.RequestTypeId = ko.observable(1);
        req.Model.PropertyId = item.Model.PropertyId,
        req.Model.RecipientId = item.LandlordId();
        req.Model.ToOwner = true;
        req.Model.ToTenant = false;
        req.AddressString = item.AddressString;
        req.Landlordname = item.Landlordname;
        req.LandlordPhone = item.LandlordPhone;
        self.Request(req);
        self.MainView(false);
        self.SendRequestView(true);
    }
    self.SendRequest = function (req) {
        var token = $("input[name = '__RequestVerificationToken']").val();
        var formData = KeysUtils.toFormData(req.Model, token);
        $.ajax({
            type: 'POST',
            url: '/Rental/Home/SubmitPropertyRequest',
            data: formData,
            dataType: 'json',
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Request Send Successfully.</p>', 'notice', KeysUtils.reload);
                }
                else {
                    KeysUtils.notification.show(KeysUtils.notification.errorMsg, 'notice');
                }
            },
            error: function (result) {
                KeysUtils.notification.show(KeysUtils.notification.errorMsg, 'notice');
            }
        });

    }
}