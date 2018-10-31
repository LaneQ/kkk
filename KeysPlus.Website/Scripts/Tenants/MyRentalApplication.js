var token = $("input[name = '__RequestVerificationToken']").val();
function PageViewModel(dataVm) {
    var self = this;
    self.ExtendDic = KeysExtendsDic.RentalApp;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, self.ExtendDic);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
    console.log(self.Items());
}



