// Use this for the property owner dashboard javascript
function OwnerPropertyDashBoard(data) {
    var propertyData = chartData.makeData(data.PropDashboardData);
    var propertyOptions = chartOptions.makeOptions(data.PropDashboardData);
    var propertyChart = KeysChart.drawDoughnut('property-chart', propertyData, propertyOptions);
    document.getElementById('property-chart-legend').innerHTML = propertyChart.generateLegend();

    var rentAppData = chartData.makeData(data.RentAppsDashboardData);
    var rentAppOptions = chartOptions.makeOptions(data.RentAppsDashboardData);
    var rentAppChart = KeysChart.drawDoughnut('rental-application-chart', rentAppData, rentAppOptions);
    document.getElementById('rental-application-chart-legend').innerHTML = rentAppChart.generateLegend();

    var maintenanceData = chartData.makeData(data.JobsDashboardData);
    var maintenanceOptions = chartOptions.makeOptions(data.JobsDashboardData);
    var maintenanceChart = KeysChart.drawDoughnut('maintenance-chart', maintenanceData, maintenanceOptions);
    document.getElementById('maintenance-chart-legend').innerHTML = maintenanceChart.generateLegend();

    var rentalRequestData = chartData.makeData(data.RequestDashboardData);
    var rentalRequestOptions = chartOptions.makeOptions(data.RequestDashboardData);
    var rentalRequestChart = KeysChart.drawDoughnut('rental-request-chart', rentalRequestData, rentalRequestOptions);
    document.getElementById('rental-request-chart-legend').innerHTML = rentalRequestChart.generateLegend();

    var jobQuoteData = chartData.makeData(data.JobQuotesDashboardData);
    var jobQuoteOptions = chartOptions.makeOptions(data.JobQuotesDashboardData);
    var jobQuoteChart = KeysChart.drawDoughnut('job-quote-chart', jobQuoteData, jobQuoteOptions);
    document.getElementById('job-quote-chart-legend').innerHTML = jobQuoteChart.generateLegend();
}
