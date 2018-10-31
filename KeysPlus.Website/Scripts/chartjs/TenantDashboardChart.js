function RentalViewModel(data) {
    var self = this;
    self.PropertyId = ko.observable(data.PropertyId);
    self.Address = ko.observable(data.Address);
    self.Landlordname = ko.observable(data.Landlordname);
    self.RentalPaymentType = ko.observable(data.RentalPaymentType);
    self.TargetRent = ko.observable(data.PaymentAmount);
    self.PaymentDueDate = ko.observable(data.PaymentDueDate);
    self.PaymentStartDate = ko.observable(data.PaymentStartDate);
    self.NextPaymenDate = ko.observable(data.NextPaymenDate);
    self.CalendarMonthYear = ko.computed(function () {
        return moment(self.NextPaymenDate()).format('MMM') + ' ' + moment(self.NextPaymenDate()).format('YYYY');
    });
}

var Rentals = function (data) {
    var self = this;
    self.TenantRentals = ko.observableArray();
    data.TenantRentalDashboardData.forEach(function (item) {
        self.TenantRentals.push(new RentalViewModel(item));
    });
}

function TenantDashBoard(data) {
    console.log("TenantData", data.RentAppsDashboardData);
    var rentAppData = chartData.makeData(data.RentAppsDashboardData);
    var rentAppOptions = chartOptions.makeOptions(data.RentAppsDashboardData);
    var rentAppChart = KeysChart.drawDoughnut('rent-chart', rentAppData, rentAppOptions);
    document.getElementById('rent-chart-legend').innerHTML = rentAppChart.generateLegend();
    
    var landlordRequestData = chartData.makeData(data.LandLordRequestDashboardData);
    var landlordRequestOptions = chartOptions.makeOptions(data.LandLordRequestDashboardData);
    var landlordRequestChart = KeysChart.drawDoughnut('landlord-request-chart', landlordRequestData, landlordRequestOptions);
    document.getElementById('landlord-request-legend').innerHTML = landlordRequestChart.generateLegend();

    var tenantRequestData = chartData.makeData(data.TenantRequestDashboardData);
    var tenantRequestOptions = chartOptions.makeOptions(data.TenantRequestDashboardData);
    var requestChart = KeysChart.drawDoughnut('tenant-request-chart', tenantRequestData, tenantRequestOptions);
    document.getElementById('tenant-request-chart-legend').innerHTML = requestChart.generateLegend();

    

}