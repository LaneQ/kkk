$(document).ready(function () {
    Chart.pluginService.register({
        beforeDraw: function (chart) {
            if (chart.config.options.elements.center) {
                //Get ctx from string
                var ctx = chart.chart.ctx;

                //Get options from the center object in options
                var centerConfig = chart.config.options.elements.center;
                var fontStyle = centerConfig.fontStyle || 'Arial';
                var txt = centerConfig.text;
                var color = centerConfig.color || '#000';
                var sidePadding = centerConfig.sidePadding || 20;
                var sidePaddingCalculated = (sidePadding / 100) * (chart.innerRadius * 2)
                //Start with a base font of 30px
                ctx.font = "40px " + fontStyle;
                //Get the width of the string and also the width of the element minus 10 to give it 5px side padding
                var stringWidth = ctx.measureText(txt).width;
                var elementWidth = (chart.innerRadius * 2) - sidePaddingCalculated;

                // Find out how much the font can grow in width.
                var widthRatio = elementWidth / stringWidth;
                var newFontSize = Math.floor(30 * widthRatio);
                var elementHeight = (chart.innerRadius * 2);

                // Pick a new font size so it will not be larger than the height of label.
                var fontSizeToUse = Math.min(newFontSize, elementHeight);

                //Set font settings to draw it correctly.
                ctx.textAlign = 'center';
                ctx.textBaseline = 'middle';
                var centerX = ((chart.chartArea.left + chart.chartArea.right) / 2);
                var centerY = ((chart.chartArea.top + chart.chartArea.bottom) / 2);
                ctx.font = fontSizeToUse + "px " + fontStyle;
                ctx.fillStyle = color;

                //Draw text in center
                ctx.fillText(txt, centerX, centerY);
            }
        }
    });
});

var chartColors = {
    Occupied: { code: 'Occupied', color: 'rgb(62, 183, 11)' },
    Vacant: { code: 'Vacant', color: 'rgb(247, 137, 74)' },
    NewItems: { code: 'New', color: 'rgb(252, 68, 68)' },
    Approved: { code: 'Approved', color: 'rgb(62, 183, 11)' },
    Pending: { code: 'Pending', color: 'rgb(247, 137, 74)' },
    InProgress: { code: 'In Progress', color: 'rgb(62, 183, 11)' },
    Resolved: { code: 'Resolved', color: 'rgb(247, 137, 74)' },
    Accepted: { code: 'Accepted', color: 'rgb(62, 183, 11)' },
    Rejected: { code: 'Rejected', color: 'rgb(44, 45, 48)' },
    Viewed: { code: 'Viewed', color: 'rgb(62, 183, 11)' },
    Current: { code: 'Current', color: 'rgb(252, 68, 68)' },
};

var chartData = {};
chartData.makeData = function (data) {
    console.log(data);
    var dataArray = [];
    var colors = [];
    var labels = [];
    for (var prop in data) {
        dataArray.push(data[prop]);
        colors.push(chartColors[prop].color);
        labels.push(data[prop] + ' ' + chartColors[prop].code);
    }
    var cData = {
        datasets: [{
            data: dataArray,
            backgroundColor: colors,
            borderColor: colors,
        }],
        labels: labels
    };
    return cData;
}

var chartOptions = {};
chartOptions.makeOptions = function (data) {
    var sum = 0;
    for (var prop in data) {
        sum += data[prop];
    }
    var options = {
        cutoutPercentage: 98,
        legend: {
            display: false,
            position: 'right',

            labels: {
                usePointStyle: true,
                boxWidth: 15,
            }
        },
        elements: {
            center: {
                text: sum + ' total',
                color: '#204d74',//'#36A2EB', //Default black
                fontStyle: 'Helvetica', //Default Arial
                sidePadding: 20 //Default 20 (as a percentage)
            }
        },
        legendCallback: function (chart) {
            var text = [];
            text.push('<ul>');
            for (var i = 0; i < chart.data.datasets[0].data.length; i++) {
                text.push('<li>');
                text.push('<div class="doughnut-label" style="background-color:' + chart.data.datasets[0].backgroundColor[i] + '; width : 10px; height : 10px; ' + '">' + '</div>' + '&nbsp;&nbsp;');
                if (chart.data.labels[i]) {
                    text.push(' ' + chart.data.labels[i]);
                }
                text.push('</li>');
            }
            text.push('</ul>');
            return text.join("");
        }
    }

    return options
}

var KeysChart = {}
KeysChart.drawDoughnut = function (element, data, options) {
    var ctx = document.getElementById(element).getContext("2d");
    return new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: options
    });
}