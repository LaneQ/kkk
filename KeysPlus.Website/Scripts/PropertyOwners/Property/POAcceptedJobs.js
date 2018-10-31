function PageViewModel(dataVm) {
    var self = this;
    self.ExtentDic = KeysExtendsDic.Job;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.ExtendDic = KeysExtendsDic.Job;
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, KeysExtendsDic.Job);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
    self.SelectedMarketJob = ko.observable();
    //self.ShowDetail = function (item) {
    //    viewModel.SelectedItem(item);
    //    viewModel.DetailView(true);
    //    viewModel.MainView(false);
    //    viewModel.EditView(false);
    //    viewModel.DeleteView(false);
    //    var marketJobId = item.Model.JobRequestId();
    //    if (marketJobId) {
    //        $.ajax({
    //            type: 'GET',
    //            url: '/Jobs/Home/GetMarketJob',
    //            data: { id: marketJobId },
    //            dataType: "json",
    //            success: function (response) {
    //                if (response.Success) {
    //                    var marketJob = {};
    //                    ko.mapping.fromJS(response.data, {}, marketJob);
    //                    marketJob.StreetAddress = KeysUtils.toAddressStreet(marketJob.Address);
    //                    marketJob.CitySub = KeysUtils.toCitySub(marketJob.Address);
    //                    self.SelectedMarketJob(marketJob); // Progress
    //                }
    //            }
    //        });
    //    }
    //}
}