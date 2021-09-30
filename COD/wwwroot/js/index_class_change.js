
let elements = document.querySelectorAll(".article-container");

for (let elem of elements) {
	elem.addEventListener("click", function () {
        this.classList.toggle('flip');
	}, false);

}

//function showAll(event) {
//    alert(this.style.position)
//    event.classList.toggle('bootom-list-show-all')
    
//};

$('body').swipe({
    swipe: function (event, direction, distance, duration, fingerCount, fingerData) {
        if (direction == 'left') {
            $('#carouselExampleIndicators').carousel('next');
        } else if (direction == 'right') {
            $('#carouselExampleIndicators').carousel('prev');

            //let carousel = $('#carouselExampleIndicators');
            //alert(('.active', carousel).index() + 1)
        }
    }
});

let navList = document.getElementsByClassName('menu__item')

for (let elem of navList) {
    elem.addEventListener("click", function () {
        $('#menu__toggle').prop('checked', false)
    }, false);

}

