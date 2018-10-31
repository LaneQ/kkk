function PageViewModel(dataVm, dic) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    self.ExtendDic = dic;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, self.ExtendDic);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
    self.Job = ko.observable();
    self.RemoveJobRadioButton = ko.observable('true');
    self.ShowInputForm = ko.computed(function () {
        if (self.RemoveJobRadioButton() === "true") {
            return false;
        }
        else {
            var newJob = new EntityViewModel(KeysExtendsDic.NewJob);
            newJob.Note = null;
            newJob.PropertyId = ko.observable(self.SelectedItem().Model.PropertyId());
            newJob.JobRequestId = ko.observable(self.SelectedItem().Model.Id());
            self.Job(newJob);
            return true;
        }
    });
    self.AddJob = function (data) {
        var formData = KeysUtils.toFormData(data.Model, token);
        $.ajax({
            url: "/Jobs/Home/RemoveJobFromMarketDIY",
            data: formData,
            method: "POST",
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Job added successfully!</p>', 'success', KeysUtils.reload);
                }
                else {
                    KeysUtils.notification.showErrorMsg();
                }
            }
        });
    }
}