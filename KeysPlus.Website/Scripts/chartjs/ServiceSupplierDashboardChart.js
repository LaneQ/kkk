function DrawSerivceSupplierDashboardChart(data) {
    
    var quoteData = chartData.makeData(data.QuoteDashboardData);
    var quoteOptions = chartOptions.makeOptions(data.QuoteDashboardData);
    var quoteChart = KeysChart.drawDoughnut('my-quotes-chart', quoteData, quoteOptions);
    document.getElementById('my-quotes-legend').innerHTML = quoteChart.generateLegend();

    console.log(data.JobDashboardData);
    var quoteData = chartData.makeData(data.JobDashboardData);
    var quoteOptions = chartOptions.makeOptions(data.JobDashboardData);
    var quoteChart = KeysChart.drawDoughnut('my-jobs-chart', quoteData, quoteOptions);
    document.getElementById('my-jobs-legend').innerHTML = quoteChart.generateLegend();

}