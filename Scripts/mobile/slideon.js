var slideMenu = document.getElementById("menuVertical");
var slideButton = document.getElementById("touchMenu");
var turnArrow = document.getElementById("arrowTurn");

function slideOn() {
	if (slideMenu.style.webkitTransform == "translateX(200px)") {
		arrowTurn.style.webkitTransform = ("rotate(180deg)");
		slideMenu.style.webkitTransform = ("translateX(0px)");
	} 
	else {
		arrowTurn.style.webkitTransform = ("rotate(0deg)");
		slideMenu.style.webkitTransform = ("translateX(200px)");
	}
}