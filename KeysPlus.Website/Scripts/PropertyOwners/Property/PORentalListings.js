function PageViewModel(dataVm) {
    var self = this;
    self.ExtendDic = KeysExtendsDic.RentalListing;
    for (var key in dataVm) {
        self[key] = dataVm[key];
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, KeysExtendsDic.RentalListing);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
}