function PageViewModel(dataVm) {
    var self = this;
    for (var key in dataVm) {
        self[key] = dataVm[key]
    }
    self.Items().forEach(function (item) {
        KeysUtils.injectExtends(item.Model, KeysExtendsDic.Request);
        KeysUtils.injectViewProps(item);
    });
    KeysUtils.injectOpProps(self);
    console.log(self.Items());
}
