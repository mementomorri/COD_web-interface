function changeSw1(elem,swtch) {   
    if (elem.style.display == 'none') {
        document.getElementById(swtch).style.display = 'block'
    } else {
        document.getElementById(swtch).style.display = 'none'
    }
}

function changeSwitch(elem, swtch) {
    let el = document.getElementById(elemet)
    let sw = document.getElementById(swtch)

    if (el.style.display == 'none') {
        sw.style.display = 'block'
    } else {
        sw.style.display = 'none'
    }
}



$('Count2_value').bind('DOMSubtreeModified', function () {
    console.log('changed');
});