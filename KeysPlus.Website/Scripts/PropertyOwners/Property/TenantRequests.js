function PageViewModel(dataVm) {
    var token = $("input[name = '__RequestVerificationToken']").val();
    var self = this;
    self.ValidImgFiles = KeysFiles.validImgFiles;
    self.ValidFileTypes = KeysFiles.validFileTypes;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectViewProps(item);
    });
    self.SelectedItem = ko.observable();
    self.MainView = ko.observable(true);
    self.DetailView = ko.observable(false);
    self.AcceptView= ko.observable (false);
    self.AcceptJobView= ko.observable(false);
    self.DeclineView = ko.observable(false);
    self.SelectedRadioButton = ko.observable("true");
    self.ShowInputForm = ko.computed({
        read: function () {
            if (self.SelectedRadioButton() == "true") { return true; }
            else { return false; }
        },
        write: function () { }
    });
    self.MarketJob = ko.observable();
    self.Job = ko.observable();
    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
        self.AcceptView(false);
        self.AcceptJobView(false);
        self.DeclineView(false);
    }
    self.ShowAccept = function (item) {
        debugger;
        self.MainView(false);
        self.DetailView(false);

        if ((item.RequestType()) == "Job Request"){
            self.AcceptJobView(true);
            var newMarketJob = new EntityViewModel(KeysExtendsDic.MarketJob);
            newMarketJob.Model.RequestId = ko.observable(item.Model.Id());
            newMarketJob.Model.PropertyId = ko.observable(item.Model.PropertyId());
            newMarketJob.Model.JobDescription(item.Model.RequestMessage());
            self.MarketJob(newMarketJob);
            var newJob = new EntityViewModel(KeysExtendsDic.NewJob);
            newJob.Model.PropertyId = ko.observable(item.Model.PropertyId());
            newJob.Model.JobDescription(item.Model.RequestMessage());
            self.Job(newJob);
            if (!item.Model.IsViewed()) {
                sendView(item);
            }
        }
        else {
            self.AcceptView(true);
        }
        
        self.DeclineView(false);
        self.SelectedItem(item);
       
    }
    self.ShowDetail = function (data) {
        self.MainView(false);
        self.AcceptView(false);
        self.AcceptJobView(false);
        self.DeclineView(false);
        self.DetailView(true);
        self.SelectedItem(data);
        if (!data.Model.IsViewed()) {
            sendView(data);
        }
    }

    self.ShowDecline = function (data) {
        self.MainView(false);
        self.DetailView(false);
        self.AcceptView(false);
        self.AcceptJobView(false);
        self.DeclineView(true);
        self.SelectedItem(data);
        if (!data.Model.IsViewed()) {
            sendView(data);
        }
    }
    self.ShowMain = function () {
        self.MainView(true);
        self.DetailView(false);
        self.AcceptView(false);
        self.AcceptJobView(false);
        self.DeclineView(false);
    }

    self.Decline = function (data) {
        var formData = KeysUtils.toFormData(data.Model, token);
        var url = '/Tenants/Home/DeclineRequest';
        KeysUtils.post(url, formData, self.postSuccess);
    }

    self.Accept = function (data) {
        var formData = KeysUtils.toFormData(data.Model, token);
        var url = '/Tenants/Home/AcceptRequest';
        KeysUtils.post(url, formData, self.postSuccess);
    }
    self.postSuccess = function (result) {
        if (result.Success) {
            KeysUtils.notification.show('<p>' + result.Message + '</p>', 'notice', KeysUtils.reload);
        }
        else {
            KeysUtils.notification.showErrorMsg();
        }
    }

    function sendView(data) {
        debugger;
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/RequestViewed',
            data: { id: data.Model.Id(), __RequestVerificationToken: token },
            success: function (result) {
                if (result.Updated) {
                    self.SelectedItem().Model.IsViewed(true);
                }
            },
            error: function () { },
            fail: function () { }
        });
    }

    self.AcceptRequest = function (data) {
        var formData = KeysUtils.toFormData({ requestId: data.Model.Id() }, token);
        var url = '/PropertyOwners/Manage/AcceptTenantRequest';
        KeysUtils.post(url, formData, postSuccess);
    }

    function postSuccess(result) {
        if (result.Success) {
            KeysUtils.notification.show('<p>' + result.Msg + '</p>', 'notice', KeysUtils.reload);
        }
        else {
            KeysUtils.notification.showErrorMsg();
        }
    }
    self.AddMarketJob = function (data) {
     
        var formData = KeysUtils.toFormData(data.Model, token);
        for (var pair of formData.entries()) {
            console.log(pair[0] + ', ' + pair[1]);
        }
        $.ajax({
            type: 'POST',
            url: '/PropertyOwners/Property/AddJobRequest',
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Job request accepted!</p>', 'success', KeysUtils.reload);
                }
                else {
                    KeysUtils.notification.showErrorMsg();
                }
            },
            error: function () {
                location.reload(); console.log("new market job error");
            },
            fail: function () { location.reload(); console.log("new market job fail"); }
        });
    }

    self.AddJob = function (data) {
        debugger;
        var formData = KeysUtils.toFormData(data.Model, token);
        formData.append('JobRequestId', self.SelectedItem().Model.Id());
        $.ajax({
            type: 'POST',
            url: '/Jobs/Home/SaveJob',
            data: formData,// postdata,
            contentType: false,
            processData: false,
            success: function (result) {
                if (result.Success) {
                    KeysUtils.notification.show('<p>Job added successfully!</p>', 'success', KeysUtils.reload);
                }
                else {
                    KeysUtils.notification.showErrorMsg();
                }
            },
            error: function () { console.log("new job success error"); },
            fail: function () { console.log("new job success fail"); }
        });
    }
}