function indexView() {
    let chbox = document.getElementById('chek_view');
    if (chbox.checked) {
        //document.getElementById('view1').style.display = 'none'
        //document.getElementById('view2').style.display = 'flex'
        document.body.style.backgroundColor = '#A2A9B9'
        document.getElementById('view1').classList.add('disabled')
        document.getElementById('view2').classList.remove('disabled')
        document.getElementsByClassName('navbar')[0].classList.add('navbar-dark')
        document.getElementById('timestamp').style.color = 'white'
    }
    else {
        //document.getElementById('view1').style.display = 'flex'
        //document.getElementById('view2').style.display = 'none'
        document.body.style.backgroundColor = ''       
        document.getElementById('view1').classList.remove('disabled')
        document.getElementById('view2').classList.add('disabled')
        document.getElementsByClassName('navbar')[0].classList.remove('navbar-dark')
        document.getElementById('timestamp').style.color = 'black'
    }
    return null
}