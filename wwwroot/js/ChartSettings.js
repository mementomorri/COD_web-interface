let TChart_scale_min = 0, TChart_scale_max = 100, PChart_scale_min = 0, PChart_scale_max = 1000;

var ctx = document.getElementById('tChart').getContext('2d');
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
                    
                },

            }
            ],
            yAxes: [{
                ticks: {
                    //min: 0, 
                    //max: 100 
                }
            }]
        },
        tooltips: {
            callbacks: {
                label: function (tooltipItem, data) {
                    var label = data.datasets[tooltipItem.datasetIndex].label || '';

                    if (label) {
                        label += ': ';
                    }
                    label += Math.round(tooltipItem.yLabel * 100) / 100;
                    return label;
                }
            }
        }
    }
});


var ctx = document.getElementById('pChart').getContext('2d');
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
                    }
                },
            }
            ],
            yAxes: [{
                ticks: {
                    min: 0,
                    max: 1000
                }
            }]
        },
        tooltips: {
            callbacks: {
                label: function (tooltipItem, data) {
                    var label = data.datasets[tooltipItem.datasetIndex].label || '';

                    if (label) {
                        label += ': ';
                    }
                    label += Math.round(tooltipItem.yLabel * 100) / 100;
                    return label;
                }
            }
        }
    }
});

let ChartData = {
    startData: '',
    endData: '',
    data: []
}

initialization() //инициализация

function initialization() {
    dateInitialization()
    chboxFromMemory()
    getStartToEndTimeData(ChartData.startData, ChartData.endData)
    console.log(ChartData.data)
    chartDateChange('T')
    chartDateChange('P')
}

function hideArrowIfDateFutute(pfef) {
    let date = new Date(document.getElementById(pfef + '_currentDate').value)
    let dateNow = new Date()

    if (Date.parse(dateNow) - Date.parse(date) <= 86400000) { //day = 86400000ms
        document.getElementById(pfef + '_disable_arrow').style.display = 'block'
        document.getElementById(pfef + '_arrow_right').classList.add('arrow-6-grey')
    } else {
        document.getElementById(pfef + '_disable_arrow').style.display = 'none'
        document.getElementById(pfef + '_arrow_right').classList.remove('arrow-6-grey')
    }
}


function chboxToMemory() {
    let arr = document.querySelectorAll('input[type="checkbox"]:checked');
    let list = []

    for (let i = 0; i < arr.length; i++) {
        list.push(arr[i].id)
    }

    document.cookie = list.join() + ";expires=Tue, 19 Jan 2038 03:14:07 GMT";
}

function chboxFromMemory() {
    if (document.cookie != '') {

        let arr = document.querySelectorAll('input[type="checkbox"]:checked');
        let list = document.cookie.split(',');

        for (let i = 0; i < arr.length; i++) {
            arr[i].checked = false
        }

        for (let i = 0; i < list.length; i++) {
            document.getElementById(list[i]).checked = true;
        }
    }
}

function dateInitialization() {
    let currDate = new Date()
    let startDate = new Date(currDate)

    startDate.setDate(currDate.getDate() - 8)

    document.getElementById('T' + '_currentDate').value = currDate.toISOString().slice(0, 10)
    document.getElementById('P' + '_currentDate').value = currDate.toISOString().slice(0, 10)    

    document.getElementById('T' + '_endDate').value = currDate.toISOString().slice(0, 10);
    document.getElementById('T' + '_startDate').value = startDate.toISOString().slice(0, 10);

    document.getElementById('P' + '_endDate').value = currDate.toISOString().slice(0, 10);
    document.getElementById('P' + '_startDate').value = startDate.toISOString().slice(0, 10);

    hideArrowIfDateFutute('T');
    hideArrowIfDateFutute('P');

    ChartData.startData = startDate;
    ChartData.endData = currDate;
}

function getStartToEndTimeData(startDate, endDate) {
    $.ajax({
        url: "/Home/GetChartData",
        type: "GET",
        data: { start: getDateCsharp(startDate), end: getDateCsharp(endDate) },
        success: function (data) {
            console.log(JSON.parse(data))
            ChartData.data = JSON.parse(data)
            chartDraw()
        },
        error: function () {
            setTimeout(() => getStartToEndTimeData(startDate, endDate), 1000)
        }
    });
}

function getDateCsharp(date) {
    let day = date.getDate();           //
    let month = date.getMonth() + 1;    // +1, месяцы начинаются с 0
    let year = date.getFullYear();      //
    let hour = date.getHours();         //
    let minute = date.getMinutes();     // 
    let second = date.getSeconds();     //

    return day + "/" + month + "/" + year + " " + hour + ':' + minute + ':' + second;
}


// палитра цветов для графиков
let palitT = ['rgba(252, 92, 101)', 'rgba(253, 150, 68)', 'rgba(254, 211, 48)', 'rgba(38, 222, 129)', 'rgba(43, 203, 186)', 'rgba(235, 59, 90)', 'rgba(250, 130, 49)', 'rgba(247, 183, 49)', 'rgba(32, 191, 107)', 'rgba(15, 185, 177)']
let palitP = ['rgba(69, 170, 242)', 'rgba(75, 123, 236)', 'rgba(165, 94, 234)', 'rgba(209, 216, 224)', 'rgba(119, 140, 163)', 'rgba(45, 152, 218)', 'rgba(56, 103, 214)', 'rgba(136, 84, 208)', 'rgba(165, 177, 194)', 'rgba(75, 101, 132)']

//соотвие адресов сигналов именам
let chartNames = {
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


function asdchartDraw() {
    name = '[q1]SHUK/AI/AI1/Out'
    data = ChartData.data.data.week[1].data

    TChart.data.datasets.push({
        label: chartNames[name].slice(2),
        data: data,
        backgroundColor: palitT[0].slice(0, -1) + ', 0.2)',
        borderColor: palitT[0].slice(0, -1) + ', 1.0)',
        //backgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
       // borderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
        pointHitRadius: 15,
        pointBorderColor: 'transparent',
        pointBackgroundColor: 'transparent',
        pointHoverBorderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
        pointHoverBackgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
    })

    TChart.update();
}

function dataIntervalSelect(pref) {
    let interval = document.getElementById(pref + '_selectDateRange').value
    let data;

    if (interval == 0 || interval == 3) {
        data = ChartData.data.data.month.slice();
    } else if (interval == 1) {
        data = ChartData.data.data.week.slice();
    } else if (interval == 2) {
        data = ChartData.data.data.day.slice();
    } else {
        alert('ошибка интервала времени')
    }

    return data;
}

function getlistDrawedLables() {
    let list = []
    let listAdres = []

    for (let i = 0; i < TChart.data.datasets.length; i++) {
        list.push('T ' + TChart.data.datasets[i].label)
    }

    for (let i = 0; i < PChart.data.datasets.length; i++) {
        list.push('P ' + PChart.data.datasets[i].label)
    }

    for (key in chartNames) {
        if (list.includes(chartNames[key])){
            listAdres.push(key)
        } 
    }

    return listAdres
}

function getListCheboxes() {
    let checkboxes = document.querySelectorAll('input[type="checkbox"]'); //запрашиваем все элементы с типом "checkbox"
    let checkboxesChecked = [];

    for (let i = 0; i < checkboxes.length; i++) {
        if (checkboxes[i].checked) {                                     //если чекбокс активен
            checkboxesChecked.push(checkboxes[i].value);                 //добавляем значение value в массив
        }
    }

    return checkboxesChecked
}

function drawLablesToDrawData(drawLables, dataArr) {
    let data = {
        T: [],
        P: [],
    }
    dataArr.forEach((elem) => {
        if (drawLables.includes(elem.label)){
            if (chartNames[elem.label][0] == 'T') {
                data.T.push(elem)
            } else if (chartNames[elem.label][0] == 'P') {
                data.P.push(elem)
            }
        }
    })

    return data
}

function chartDraw() {
    let dataArr = dataIntervalSelect('T') // массив данных в зависимсости от интервала (день, неделя, месяц)

    let listDrawedLables = getlistDrawedLables() // список названий отрисованных графиков
    let listChboxLables = getListCheboxes() //список значений выбранных чекбоксов

    let drawLables = []

    listChboxLables.forEach((element) => {
        if (!listDrawedLables.includes(element)) {
            drawLables.push(element)                                                //если список отрисованных лейблов не содержит список выбранных чекбоксами, то добавить в drawLables
        }
    });

    listDrawedLables.forEach((element) => {
        if (!listChboxLables.includes(element)) {                                                //если список отрисованных лейблов не содержит список выбранных чекбоксами, то удаляем из dataset

            TChart.data.datasets.forEach( (arr, index) => {
                if ( arr.label == chartNames[element].slice(2) ) {
                    TChart.data.datasets.splice(index, 1)
                }
            })

            PChart.data.datasets.forEach((arr, index) => {
                if (arr.label == chartNames[element].slice(2)) {
                    PChart.data.datasets.splice(index, 1)
                }
            })
        }
    });

    let ChartArr = drawLablesToDrawData(drawLables, dataArr)

    ChartArr.T.forEach(elem => {
        TChart.data.datasets.push({
            label: chartNames[elem.label].slice(2),
            data: elem.data,
            backgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
            borderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHitRadius: 15,
            pointBorderColor: 'transparent',
            pointBackgroundColor: 'transparent',
            pointHoverBorderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHoverBackgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
        })

    })

    ChartArr.P.forEach(elem => {
        PChart.data.datasets.push({
            label: chartNames[elem.label].slice(2),
            data: elem.data,
            backgroundColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
            borderColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHitRadius: 15,
            pointBorderColor: 'transparent',
            pointBackgroundColor: 'transparent',
            pointHoverBorderColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
            pointHoverBackgroundColor: palitP[PChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
        })

    })



    TChart.update();
    PChart.update();
}


function chek_radio_T() {
    tContent = document.getElementById('T_scale_container')

    let T_radio = document.getElementsByName('T_scale');

    if (T_radio[0].checked) {
        tContent.style.display = 'block'
        document.getElementById('T_scale_min').value = TChart_scale_min
        document.getElementById('T_scale_max').value = TChart_scale_max
    } else {
        tContent.style.display = 'none'
    }
}

function chek_radio_P() {
    pContent = document.getElementById('P_scale_container')

    let P_radio = document.getElementsByName('P_scale');

    if (P_radio[0].checked) {
        pContent.style.display = 'block'
        document.getElementById('P_scale_min').value = PChart_scale_min
        document.getElementById('P_scale_max').value = PChart_scale_max
    } else {
        pContent.style.display = 'none'
    }

}


function checkboxChange() {
    chboxToMemory()
    chartDraw()
}

function chartDateChange(pref) {
    let dateCode = parseInt(document.getElementById(pref + '_selectDateRange').value)
    let endDate = new Date(document.getElementById(pref + '_currentDate').value)
    let startDate = new Date(endDate)

    if (dateCode == 0) {
        startDate.setMonth(endDate.getMonth() - 1)
    } else if (dateCode == 1) {
        startDate.setDate(endDate.getDate() - 7)
    } else if (dateCode == 2) {
        startDate.setDate(endDate.getDate() - 1)
    } else if (dateCode == 3) {
        startDate = new Date(document.getElementById(pref + '_startDate').value)
        endDate = new Date(document.getElementById(pref + '_endDate').value)
    }

    if (dateCode != 3) {
        document.getElementById(pref + '_startDate').value = startDate.toISOString().slice(0, 10)
        document.getElementById(pref + '_endDate').value = document.getElementById(pref + '_currentDate').value
    }

    if (pref == 'T') {
        TChart.options.scales.xAxes[0].ticks.min = Date.parse(startDate)
        TChart.options.scales.xAxes[0].ticks.max = Date.parse(endDate)

        TChart.update();
    } else if (pref == 'P') {
        PChart.options.scales.xAxes[0].ticks.min = Date.parse(startDate)
        PChart.options.scales.xAxes[0].ticks.max = Date.parse(endDate)

        PChart.update();
    }



    hideArrowIfDateFutute(pref)

    refreshData(startDate, endDate, pref)
}


function leftArrowClick(pref, element) {
    element.classList.remove('arrow-left-anim');
    setTimeout(function () {
        element.classList.add('arrow-left-anim');
    }, 10)

    let endDate = new Date(document.getElementById(pref + '_currentDate').value)
    let dateCode = parseInt(document.getElementById(pref + '_selectDateRange').value)


    if (dateCode == 0) {
        endDate.setMonth(endDate.getMonth() - 1)
    } else if (dateCode == 1) {
        endDate.setDate(endDate.getDate() - 7)
    } else if (dateCode == 2) {
        endDate.setDate(endDate.getDate() - 1)
    }

    document.getElementById(pref + '_currentDate').value = endDate.toISOString().slice(0, 10)
    hideArrowIfDateFutute(pref)

  
    //let startDate = new Date(document.getElementById(pref + '_startDate').value)

    //refreshData(startDate, endDate, pref)
    chartDateChange(pref)
}

function rightArrowClick(pref, element) {
    element.classList.remove('arrow-right-anim');
    setTimeout(function () {
        element.classList.add('arrow-right-anim');
    }, 10)

    let endDate = new Date(document.getElementById(pref + '_currentDate').value)
    let dateCode = parseInt(document.getElementById(pref + '_selectDateRange').value)

    if (dateCode == 0) {
        endDate.setMonth(endDate.getMonth() + 1)
    } else if (dateCode == 1) {
        endDate.setDate(endDate.getDate() + 7)
    } else if (dateCode == 2) {
        endDate.setDate(endDate.getDate() + 1)
    }

    document.getElementById(pref + '_currentDate').value = endDate.toISOString().slice(0, 10)
    hideArrowIfDateFutute(pref)

    //let startDate = new Date(document.getElementById(pref + '_startDate').value)

    //refreshData(startDate, endDate, pref)
    chartDateChange(pref)
}



function refreshData(startDate, endDate, pref) {

    if (startDate < ChartData.startData) {
        $.ajax({
            url: "/Home/GetChartData",
            type: "GET",
            data: { start: getDateCsharp(startDate), end: getDateCsharp(ChartData.startData) },
            success: function (data) {
                data = JSON.parse(data)
                console.log(data)


                ChartData.data.data.day.forEach((elem, index) => {
                    data.data.day.forEach((newElem) => {
                        if (newElem.label == elem.label) {
                            elem.data = newElem.data.concat(elem.data)
                            elem.MaxValue = Math.max(elem.MaxValue, newElem.MaxValue)
                            elem.MinValue = Math.min(elem.MinValue, newElem.MinValue)
                        }
                    })
                })

                ChartData.data.data.week.forEach((elem, index) => {
                    data.data.week.forEach((newElem) => {
                        if (newElem.label == elem.label) {
                            elem.data = newElem.data.concat(elem.data)
                            elem.MaxValue = Math.max(elem.MaxValue, newElem.MaxValue)
                            elem.MinValue = Math.min(elem.MinValue, newElem.MinValue)
                        }
                    })
                })

                ChartData.data.data.month.forEach((elem, index) => {
                    data.data.month.forEach((newElem) => {
                        if (newElem.label == elem.label) {
                            elem.data = newElem.data.concat(elem.data)
                            elem.MaxValue = Math.max(elem.MaxValue, newElem.MaxValue)
                            elem.MinValue = Math.min(elem.MinValue, newElem.MinValue)
                        }
                    })
                })

                ChartData.startData = startDate

                removeData(TChart)
                removeData(PChart)

                chartDraw()

                console.log(ChartData)
            },
            error: function () {
                setTimeout(() => getStartToEndTimeData(startDate, endDate), 1000)
            }
        });
    }
}

function removeData(chart) {
    chart.data.datasets.splice(0, chart.data.datasets.length)
    chart.update();
}
























    function getFirstTimeData(startDate, endDate) {
        $.ajax({
            url: "/Home/GetChartData",
            type: "GET",
            data: { start: getDateCsharp(startDate), end: getDateCsharp(endDate) },
            success: function (data) {
                data = JSON.parse(data)
                chartData.startDate = startDate
                chartData.endDate = endDate
                chartData.data = data.data
                dataChartPreparation()
            },
            error: function () {
                setTimeout(() => getFirstTimeData(startDate, endDate), 1000)

            }
        });
    }


    function getActualData(startDate, endDate) {
        let newData = {}

        if (chartData.startDate <= startDate && chartData.endDate > endDate) {
            return chartData
        }


        if (chartData.startDate > startDate) {
            $.ajax({
                url: "/Home/GetChartData",
                type: "GET",
                data: { start: getDateCsharp(startDate), end: getDateCsharp(endDate) },
                success: function (data) {
                    newData = JSON.parse(data)
                    chartData.startDate = startDate
                    chartData.data.day.concat(newData.data.day)
                    chartData.data.week.concat(newData.data.week)
                    chartData.data.month.concat(newData.data.month)

                },
                error: function (error) {
                    console.log(error)
                }
            });
        }

        if (chartData.endDate < endDate) {
            $.ajax({
                url: "/Home/GetChartData",
                type: "GET",
                data: { start: getDateCsharp(startDate), end: getDateCsharp(endDate) },
                success: function (data) {
                    newData = JSON.parse(data)
                    chartData.endDate = endDate
                    newData.data.day.concat(chartData.data.day)
                    newData.data.week.concat(chartData.data.week)
                    newData.data.month.concat(chartData.data.month)
                    chartData = newData
                },
                error: function (error) {
                    console.log(error)
                }
            });
        }
        console.log(chartData.data)
        return chartData
    }










    function aaacheckboxChange(pref) {
        let startDate = new Date(document.getElementById(pref + '_startDate').value)
        let endDate = new Date(document.getElementById(pref + '_currentDate').value)
        //getActualData(startDate, endDate) 
        dataChartPreparation(pref)

        chboxToMemory()
    }

    function dataChartPreparation() {
        let startDate = new Date(document.getElementById('T' + '_startDate').value)
        let endDate = new Date(document.getElementById('T' + '_currentDate').value)
        delta = endDate - startDate
        data = []
        chartData = getActualData(startDate, endDate)

        if (delta > 86400000 * 7) { // > week => month (day = 86400000ms)
            data = chartData.data.month
        } else if (delta > 86400000) { // > day => week (day = 86400000ms)
            data = chartData.data.week
        } else { // less day => day
            data = chartData.data.day
        }



        let max = []
        let min = []

        removeData(TChart)
        removeData(PChart)

        //for (let i = 0; i < data.length; i++) {
        //    max.push(dataset[i].MaxValue)
        //    min.push(dataset[i].MinValue)
        //}

        data.forEach(function callback(element, index) {
            if (getListCheboxes().includes(element.label)) {
                addData(element.data, element.label, index, element.MaxValue, element.MinValue)

            }
        })
    }

    function addData(data, name, i, max, min) {

        let maxScaleT = 0, minScaleT = 0, maxScaleP = 0, minScaleP = 0
        if (chartNames[name][0] === 'T') {
            TChart.data.datasets.push({
                label: chartNames[name].slice(2),
                data: data,
                backgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
                borderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
                pointHitRadius: 15,
                pointBorderColor: 'transparent',
                pointBackgroundColor: 'transparent',
                pointHoverBorderColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 1.0)',
                pointHoverBackgroundColor: palitT[TChart.data.datasets.length % 10].slice(0, -1) + ', 0.2)',
            })



            //вычисление максимального значения шкалы по Y
            maxScaleT = max[i] > maxScaleT ? max[i] : maxScaleT
            if (maxScaleT < 30) {
                TChart_scale_max = 30
            } else {
                TChart_scale_max = maxScaleT + 10
            }

            //вычисление минимального значения шкалы по Y
            minScaleT = min[i] < maxScaleT ? min[i] : maxScaleT
            if (minScaleT >= 10) {
                TChart_scale_min = 10
            } else if (minScaleT == 0) {
                TChart_scale_min = 0
            } else {
                TChart_scale_min = minScaleT - 10
            }

            changeTchartScale()

        } else {
            PChart.data.datasets.push({
                label: chartNames[name].slice(2),
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
                PChart_scale_max = 250
            } else {
                PChart_scale_max = maxScaleP + 20
            }
            //вычисление минимального значения шкалы по Y
            minScaleP = min[i] < maxScaleP ? min[i] : maxScaleP
            if (minScaleP > 150) {
                PChart_scale_min = 150
            } else if (minScaleP == 0) {
                PChart_scale_min = 0
            } else {
                PChart_scale_min = minScaleP - 20
            }

            changePchartScale()
        }
    }

    function changeTchartScale() {

        if (document.getElementsByName('T_scale')[0].checked) {

            let t_min_value = parseInt(document.getElementById('T_scale_min').value);
            let t_max_value = parseInt(document.getElementById('T_scale_max').value);

            t_min_value = t_min_value < -100 ? -100 : t_min_value
            t_min_value = t_min_value >= 90 ? 90 : t_min_value
            t_max_value = t_max_value > 100 ? 100 : t_max_value
            t_max_value = t_max_value <= -90 ? -90 : t_max_value

            if (t_max_value <= t_min_value) {
                if (t_max_value <= 90) { t_max_value = t_min_value + 10 }
                else { t_min_value = t_min_value - 10 }
            }

            document.getElementById('T_scale_min').value = t_min_value
            document.getElementById('T_scale_max').value = t_max_value

            TChart.options.scales.yAxes[0].ticks.min = parseInt(document.getElementById('T_scale_min').value)
            TChart.options.scales.yAxes[0].ticks.max = parseInt(document.getElementById('T_scale_max').value)

        } else {
            delete TChart.options.scales.yAxes[0].ticks.min
            delete TChart.options.scales.yAxes[0].ticks.max

            TChart.update()
        }

        TChart.update();
    }

    function changePchartScale() {

        if (document.getElementsByName('P_scale')[0].checked) {


            let p_min_value = parseInt(document.getElementById('P_scale_min').value);
            let p_max_value = parseInt(document.getElementById('P_scale_max').value);

            p_min_value = p_min_value < -1000 ? -1000 : p_min_value
            p_min_value = p_min_value >= 990 ? 990 : p_min_value
            p_max_value = p_max_value > 1000 ? 1000 : p_max_value
            p_max_value = p_max_value <= -990 ? -990 : p_max_value

            if (p_max_value <= p_min_value) {
                if (p_max_value <= 990) { p_max_value = p_min_value + 10 }
                else { p_min_value = p_min_value - 10 }
            }
            document.getElementById('P_scale_min').value = p_min_value
            document.getElementById('P_scale_max').value = p_max_value

            PChart.options.scales.yAxes[0].ticks.min = parseInt(document.getElementById('P_scale_min').value)
            PChart.options.scales.yAxes[0].ticks.max = parseInt(document.getElementById('P_scale_max').value)
        } else {
            delete PChart.options.scales.yAxes[0].ticks.min
            delete PChart.options.scales.yAxes[0].ticks.max
        }

        PChart.update();
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

    function chek_radio_T() {
        tContent = document.getElementById('T_scale_container')

        let T_radio = document.getElementsByName('T_scale');

        if (T_radio[0].checked) {
            tContent.style.display = 'block'
            document.getElementById('T_scale_min').value = TChart_scale_min
            document.getElementById('T_scale_max').value = TChart_scale_max
        } else {
            tContent.style.display = 'none'

        }

        changeTchartScale()
    }

    function chek_radio_P() {
        pContent = document.getElementById('P_scale_container')

        let P_radio = document.getElementsByName('P_scale');

        if (P_radio[0].checked) {
            pContent.style.display = 'block'
            document.getElementById('P_scale_min').value = PChart_scale_min
            document.getElementById('P_scale_max').value = PChart_scale_max
        } else {
            pContent.style.display = 'none'
        }

        changePchartScale()
    }

    function chageValue(pfef) {
        let dateRangeCode = document.getElementById(pfef + '_selectDateRange').value

        if (dateRangeCode == 3) {
            document.getElementById(pfef + '_swipeData').style.display = 'none'
            document.getElementById(pfef + '_manualChangeData').style.display = 'flex'
        } else {
            document.getElementById(pfef + '_swipeData').style.display = 'flex'
            document.getElementById(pfef + '_manualChangeData').style.display = 'none'
        }

        chartDateChange(pfef)
    }



