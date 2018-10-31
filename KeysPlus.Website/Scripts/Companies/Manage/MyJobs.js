var token = $("input[name = '__RequestVerificationToken']").val();
function PageViewModel(dataVm) {
    var self = this;
    self.ExtendDic = KeysExtendsDic.Job;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, self.ExtendDic);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
    self.SelectedMarketJob = ko.observable();
    self.ViewDetail = function (data) {
        self.ShowDetail(data);
        if(data.Model.JobRequestId()){
            $.ajax({
                type: 'GET',
                url: '/Jobs/Home/GetMarketJob',
                data: { id: data.Model.JobRequestId() },
                dataType: "json",
                success: function (response) {
                    if (response.Success) {
                        var marketJob = ko.mapping.fromJS(response.data);
                        marketJob.StreetAddress = KeysUtils.toAddressStreet(marketJob.Address);
                        marketJob.CitySub = KeysUtils.toCitySub(marketJob.Address);
                        self.SelectedMarketJob(marketJob); // Progress
                    }
                }
            });
        }
    }
    console.log(self);
}