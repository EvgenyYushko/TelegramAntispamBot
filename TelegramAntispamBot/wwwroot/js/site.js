$('.menu-btn').on('click', function (e) {
    e.preventDefault()
    $('.menu').toggleClass('menu_active ');
    $('.container').toggleClass('container_active ');
})
