$('.menu-btn').on('click', function (e) {
    e.preventDefault()
    $('.menu').toggleClass('menu_active ');
    $('.container').toggleClass('container_active ');
})

//функции дял показа сообщения об ошибке
function showCronModal(message) {
		const modal = document.getElementById("cronModal");
		const textElem = document.getElementById("cronModalText");
		textElem.textContent = message;
		modal.style.display = "flex";
	}

	function closeCronModal() {
		document.getElementById("cronModal").style.display = "none";
	}
