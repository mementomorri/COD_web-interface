
let chartLoadT = document.getElementById('chartLoadT')
let chartLoadP = document.getElementById('chartLoadP')
let chexBlockT = document.getElementById('chexBlockT')
trendChBox()


    var ctx = document.getElementById('tChartIndex').getContext('2d');
    var TChart = new Chart(ctx, {
        type: 'line',
        data: {
            datasets: []
        },
        options: {
            scales: {
                xAxes: [{
                    display: true,
                    type: 'time',
                    time: {
                        parser: 'YYYY-MM-DD HH:mm',
                        displayFormats: {
                            millisecond: 'DD/MM/YY HH:mm:ss',
                            second: 'DD/MM/YY HH:mm:ss',
                            minute: 'DD/MM/YY HH:mm',
                            hour: 'DD/MM/YY HH',
                            month: 'DD/MM'
                        }
                    },
                    ticks: {
                        autoSkip: true,
                        maxTicksLimit: 5,
                        maxRotation: 0,
                        minRotation: 0,
                        callback: function (value, index, values) {
                            return value.split(" ");
                        },
                       
                        fontColor: 'rgb(95, 99, 157)'
                    },
                }],
                yAxes: [{
                    ticks: {
                        beginAtZero: false,
                        fontColor: 'rgb(95, 99, 157)'

                    },
                }],
            },
            tooltips: {
                mode: false,
                callbacks: {
                    title: function () { },
                    label: function () { }
                }
            },
            legend: { labels: { fontColor: 'white' } },
        }
    });

var ctx = document.getElementById('pChartIndex').getContext('2d');
var PChart = new Chart(ctx, {
    type: 'line',
    data: {
        datasets: []
    },
    options: {
        scales: {
            xAxes: [{
                display: true,
                type: 'time',
                time: {
                    parser: 'YYYY-MM-DD HH:mm',
                    displayFormats: {
                        millisecond: 'DD/MM/YY HH:mm:ss',
                        second: 'DD/MM/YY HH:mm:ss',
                        minute: 'DD/MM/YY HH:mm',
                        hour: 'DD/MM/YY HH',
                        month: 'DD/MM'
                    }
                },
                ticks: {
                    autoSkip: true,
                    maxTicksLimit: 5,
                    maxRotation: 0,
                    minRotation: 0,
                    callback: function (value, index, values) {
                        return value.split(" ");
                    },

                    fontColor: 'rgb(95, 99, 157)'
                },
            }],
            yAxes: [{
                ticks: {
                    beginAtZero: false,
                    fontColor: 'rgb(95, 99, 157)'

                },
            }],
        },
        tooltips: {
            mode: false,
            callbacks: {
                title: function () { },
                label: function () { }
            }
        },
        legend: { labels: { fontColor: 'white' } },
    }
});



function trendChBox() {
    //анимация загрузки
    chartLoadT.style.display = 'block';
    chartLoadP.style.display = 'block';
    //блок нажатия кнопок (div поверх chexbox)
    chexBlockT.style.display = 'block';
    chexBlockP.style.display = 'block';
    let checkboxes = document.querySelectorAll('div.bottom-list input[type="checkbox"]'); //запрашиваем все элементы с типом "checkbox"
    let checkboxesChecked = []; 
    for (let i = 0; i < checkboxes.length; i++) { 
        if (checkboxes[i].checked) {                                     //если чекбокс активен
            checkboxesChecked.push(checkboxes[i].value);                 //добавляем значение value в массив
        } 
    }

    $.ajax({
        url: "/Home/ChartData",
        type: "GET",
        data: {chek: checkboxesChecked.join('&')},
        success: function (data) {
            dataChartParse(data)
        },
        error: function (error) {
            console.log(error)
        }
    });
}

const palitT = ['rgba(97, 169, 253)', 'rgba(102, 247, 204)', 'rgba(175, 226, 247)', 'rgba(61, 83, 189)', 'rgba(196, 22, 117)', 'rgba(254, 158, 11)', 'rgba(190, 185, 128)', 'rgba(46, 78, 204)', 'rgba(215, 83, 75)', 'rgba(102, 247, 204)']
const palitP = ['rgba(97, 169, 253)', 'rgba(102, 247, 204)', 'rgba(175, 226, 247)', 'rgba(61, 83, 189)', 'rgba(196, 22, 117)', 'rgba(254, 158, 11)', 'rgba(190, 185, 128)', 'rgba(46, 78, 204)', 'rgba(215, 83, 75)', 'rgba(102, 247, 204)']
const chartNames = {
    '[q1]SHUKH/AI/AI1/Out': 'T улица',
    '[q1]SHUK/AI/AI1/Out': 'T маш.зал 1',
    '[q1]SHUK/AI/AI2/Out': 'T маш.зал 2',
    '[q1]SHUK/AI/AI3/Out': 'T маш.зал 3',
    '[q1]SHUK/AI/AI4/Out': 'T маш.зал 4',
    '[q1]SHUK/AI/AI5/Out': 'T эл.щитовая',
    '[q1]SHUKH/AI/AI2/Out': 'T после потр. осн.',
    '[q1]SHUKH/AI/AI3/Out': 'T после потр. рез.',
    '[q1]SHUKH/AI/AI6/Out': 'P после потр. осн.',
    '[q1]SHUKH/AI/AI7/Out': 'P после потр. рез.',
    '[q1]SHUKH/AI/AI4/Out': 'T до потр. осн.',
    '[q1]SHUKH/AI/AI5/Out': 'T до потр. рез.',
    '[q1]SHUKH/AI/AI11/Out': 'P на выкиде Н1-Н3',
    '[q1]SHUKH/AI/AI10/Out': 'P на всасе Н1-Н3',
}

function removeData(chart) {
    chart.data.datasets.splice(0, chart.data.datasets.length)
    chart.update();
}


function addData(data, name, i, max, min, minDate, maxDate) {
    let nameArr = name.split('&')
    name = nameArr[i]
    let maxScaleT = 0, minScaleT = 0, maxScaleP = 0, minScaleP = 0
    if (chartNames[name][0] === 'T') {
        data.push({ x: maximumDate(maxDate), y: data[data.length - 1].y })
        data.unshift({ x: minimumDate(minDate), y: data[0].y })

        TChart.data.datasets.push({
            label: chartNames[name],
            data: data,
            backgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
            borderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHitRadius: 15,
            pointBorderColor: 'transparent',
            pointBackgroundColor: 'transparent',
            pointHoverBorderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHoverBackgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
        })

        console.log(data)


        console.log(data[data.length - 1])
        console.log(data[data.length - 1].y)

        //вычисление максимального значения шкалы по Y
        maxScaleT = max[i] > maxScaleT ? max[i] : maxScaleT
        if (maxScaleT < 50) {
            TChart.options.scales.yAxes[0].ticks.max = 50
        } else if (maxScaleT < 100) {
            TChart.options.scales.yAxes[0].ticks.max = 100
        } else {
            TChart.options.scales.yAxes[0].ticks.max = maxScaleT
        }
        //вычисление минимального значения шкалы по Y
        minScaleT = min[i] < maxScaleT ? max[i] : maxScaleT
        if (minScaleT >= 0) {
            TChart.options.scales.yAxes[0].ticks.min = 0
        } else if (minScaleT > -30) {
            TChart.options.scales.yAxes[0].ticks.min = -30
        } else {
            TChart.options.scales.yAxes[0].ticks.min = minScaleT
        }

        TChart.update();
        //alert(TChart.data.datasets.length)

    } else {
        data.push({ x: maximumDate(maxDate), y: data[data.length - 1].y })
        data.unshift({ x: minimumDate(minDate), y: data[0].y })

        PChart.data.datasets.push({
            label: chartNames[name],
            data: data,
            backgroundColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
            borderColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHitRadius: 15,
            pointBorderColor: 'transparent',
            pointBackgroundColor: 'transparent',
            pointHoverBorderColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHoverBackgroundColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
        })
        //вычисление максимального значения шкалы по Y
        maxScaleP = max[i] > maxScaleP ? max[i] : maxScaleP
        if (maxScaleP < 250) {
            PChart.options.scales.yAxes[0].ticks.max = 250
        } else if (maxScaleP < 500) {
            PChart.options.scales.yAxes[0].ticks.max = 500
        } else {
            PChart.options.scales.yAxes[0].ticks.max = maxScaleP
        }
        //вычисление минимального значения шкалы по Y
        minScaleP = min[i] < minScaleP ? max[i] : minScaleP
        if (minScaleP >= 0) {
            PChart.options.scales.yAxes[0].ticks.min = 0
        } else if (minScaleP > -30) {
            PChart.options.scales.yAxes[0].ticks.min = -30
        } else {
            PChart.options.scales.yAxes[0].ticks.min = minScaleP        }

        PChart.update();
    }

}

function dataChartParse(data) {
    data = JSON.parse(data)
    let max = []
    let min = []
    let maxDate = []
    let minDate = []
    removeData(TChart)
    removeData(PChart)

    for (let i = 0; i < data.length; i++) {
        minDate.push(data[i].data[0].x)
        maxDate.push(data[i].data[data[i].data.length - 1].x)
    }
    for (let i = 0; i < data.length; i++) {
        max.push(data[i].MaxValue)
        min.push(data[i].MaxValue)
        addData(data[i].data, data[i].label, i, max, min, minDate, maxDate)
    } 
    //после завершения алгоритма скрыть анимацию загрузки
    chartLoadT.style.display = 'none';
    chartLoadP.style.display = 'none';
    //и блокировку нажатий chexbox'ов
    chexBlockT.style.display = 'none';
    chexBlockP.style.display = 'none';
}


function maximumDate(date) {
    let maxDateParse = 0
    maxDateParse = date.map(Date.parse)
    return date[maxDateParse.indexOf(Math.max(...maxDateParse))]
}

function minimumDate(date) {
    let minDateParse = 0
    minDateParse = date.map(Date.parse)
    return date[minDateParse.indexOf(Math.min(...minDateParse))]
}