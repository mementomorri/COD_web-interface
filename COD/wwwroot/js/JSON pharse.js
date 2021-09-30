//функция для кргуовых индикаторов
function setSector(id_n, part) {
    let radius = $(id_n).attr('r'),
        width = 2 * Math.PI * radius,
        result = width / 100 * part;
    $(id_n).css('stroke-dasharray', result + ',' + width)
}

function dataPharse(data) {
    try {
        blinker();
        data.forEach(item => {
            //if (item.id == 'SHRIK1_IBP1_chargeTime'){
            //    console.log(`${item.id} is exist!`)
            //}
            try {
                switch (item.feature) {
                    case 'css':  // изменение стилей 
                        $("#" + item.id).css(item.atribute, item.value)
                        chillAlarmHideTriangle()
                        break;

                    case 'bar':  // выполнение функции для расчета сектора круговой диаграммы
                        setSector("#" + item.id, item.value);
                        break;

                    case 'text':  // замена текста в теге
                        if (item.id == 'Count1_value' || item.id == 'Count2_value' || item.id == 'Count3_value' || item.id == 'Count4_value') {
                            addZeroToCount(item.id, item.value);
                            break;
                        }
                        if (item.id == 'SHRI1_IBP1_charge' || item.id == 'SHRI2_IBP1_charge' || item.id == 'SHRIK1_IBP1_charge') {
                            chargeLevelAnim(item.id, item.value)
                            break;
                        }
                        if (item.id == 'Count4_frequencу') {
                            $("#Count4_frequency").text(item.value)
                        }
                        if (item.id == 'Count2_frequencу') {
                            $("#Count2_frequency").text(item.value)
                        }
                        $("#" + item.id).text(item.value)
                        break;

                    case 'changeClass':  // замена класса   
                        item.value.split('&')[0] == "addClass" ? $("#" + item.id).addClass(item.value.split('&')[1]) : $("#" + item.id).removeClass(item.value.split('&')[1]);
                        break;

                    case 'avt': //автомат ручной
                        $("#" + item.id).css('background-color', item.value.split('&')[1])
                        $("#" + item.id).text(item.value.split('&')[0])
                        break;

                    case 'absent': //отсутсвие элемента
                        $("#" + item.id).css('display', item.value)
                        break;

                    case 'discIndic': //индикатор оборотов вентилятора   

                        item.value = isNaN(+item.value) ? 0 : +item.value
                        item.value = item.value > 4 ? 4 : item.value < 0 ? 0 : item.value

                        for (let i = 0; i < item.value; i++) {
                            $("#" + item.id).children("span").eq(i).css('backgroundColor', '#76bb81')
                        }
                        for (let i = item.value; i < 4; i++) {
                            $("#" + item.id).children("span").eq(i).css('backgroundColor', '#9d9d9d')
                        }
                        break;

                    case 'TS_2020': //метка времени
                        let chbox = document.getElementById('chek_view');
                        //alert(item.value)
                        $('#timestamp').text(item.value)
                        item.atribute == 'red' ? $('#timestamp').css('color', '#FF0A0A') : chbox.checked ? $('#timestamp').css('color', 'white') : $('#timestamp').css('color', 'black')
                        break;

                    case 'status':
                        switch (item.value) {
                            case 1:
                                item.value = 'unknown'
                                break;
                            case 2:
                                item.value = 'onLine'
                                break;
                            case 3:
                                item.value = 'onBattery'
                                break;
                            case 4:
                                item.value = 'onBoost'
                                break;
                            case 5:
                                item.value = 'sleeping'
                                break;
                            case 6:
                                item.value = 'onBypass'
                                break;
                            case 7:
                                item.value = 'rebooting'
                                break;
                            case 8:
                                item.value = 'standBy'
                                break;
                            case 9:
                                item.value = 'onBuck'
                                break;
                            default:
                                item.value = 'Err'
                        }
                        $("#" + item.id).text(item.value)
                        break;

                    case 'timeMinute':
                        item.value = (item.value / 3600 >> 0).toString() + 'ч ' + ((item.value % 3600 >> 0) / 60 >> 0).toString() + 'м';
                        $("#" + item.id).text(item.value)
                        break;
                }
            }
            catch{ }

            
        });
    } catch(e) {
        console.log(e)
    }
}


window.onload = function () {

    startData();

    function startData() {

        $.ajax({
            url: "/Home/startData",
            type: "GET",
            data: { leagueId: 5 },
            success: function (data) {
                data = JSON.parse(data);
                dataPharse(data);
                setTimeout(startData, 500);
            },
            error: function (error) {
                setTimeout(startData, 500);
            }
        });
    }


    function refreshData() {

        $.ajax({
            url: "/Home/startData",

            type: "GET",
            data: { leagueId: 5 },
            success: function (data) {
                data = JSON.parse(data);            
                dataPharse(data);            
                setTimeout(startData, 500);
            },
            error: function (error) {
                setTimeout(startData, 500);
            }
        });
    }   

    //test()
    function test() {
        

        let data = [
            { "id": "MR1_fire3", "feature": "changeClass", "atribute": "addClass", "value": "fire_alarm" },
            { "id": "MR2_fire3", "feature": "changeClass", "atribute": "addClass", "value": "fire_alarm" },
            { "id": "MR3_fire3", "feature": "changeClass", "atribute": "addClass", "value": "fire_alarm" },
            { "id": "MR4_fire3", "feature": "changeClass", "atribute": "addClass", "value": "fire_alarm" },
            { "id": "SwR_fire3", "feature": "changeClass", "atribute": "addClass", "value": "fire_alarm" },


            ]

        dataPharse(data)

    }


};

function chillAlarmHideTriangle() {
    if (document.getElementById('Ch1_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch1_per').style.display = 'none' }
    if (document.getElementById('Ch2_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch2_per').style.display = 'none' }
    if (document.getElementById('Ch3_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch3_per').style.display = 'none' }
    if (document.getElementById('Ch4_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch4_per').style.display = 'none' }
    if (document.getElementById('Ch5_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch5_per').style.display = 'none' }
    if (document.getElementById('Ch6_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch6_per').style.display = 'none' }
    if (document.getElementById('Ch7_r').style.fill == 'rgb(255, 10, 10)') { document.getElementById('Ch7_per').style.display = 'none' }

    if (document.getElementById('Ch1_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch1_per4').style.display = 'none' }
    if (document.getElementById('Ch2_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch2_per4').style.display = 'none' }
    if (document.getElementById('Ch3_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch3_per4').style.display = 'none' }
    if (document.getElementById('Ch4_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch4_per4').style.display = 'none' }
    if (document.getElementById('Ch5_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch5_per4').style.display = 'none' }
    if (document.getElementById('Ch6_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch6_per4').style.display = 'none' }
    if (document.getElementById('Ch7_c4').style.background == 'rgb(255, 10, 10)') { document.getElementById('Ch7_per4').style.display = 'none' }
}

function blinker() {
    elem = document.getElementById('blinker')
    elem.style.opacity = 1
    setTimeout(() => elem.style.opacity = 0.1, 200)
}

function addZeroToCount(id, value) {    
    value = value.toString();
    while (value.length < 6) {
        value = '0' + value
    }
    $("#" + id).text(value);
} 

function chargeLevelAnim(id, value) {
    let elem = document.getElementById(id + 'Anim');
    width = (value * 0.35) + 'px'
    color = value > 65 ? '#7ABD84' : value > 30 ? '#f1c40f' : '#C75C5C'
    elem.setAttribute('width', width);
    elem.setAttribute('fill', color);
}