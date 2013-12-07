/**
 * 
 */
function mouseMove(event) {
	
	console.debug("X"+event.webkitMovementX);
	console.debug("Y"+event.webkitMovementY);
}

function lockMouse() {
	elem = document.getElementById("drawArea");
	// Start by going fullscreen with the element. Current implementations
	// require the element to be in fullscreen before requesting pointer
	// lock--something that will likely change in the future.
	// elem.requestFullscreen = elem.requestFullscreen ||
	// elem.mozRequestFullscreen ||
	// elem.mozRequestFullScreen || // Older API upper case 'S'.
	// elem.webkitRequestFullscreen;
	// elem.requestFullscreen();
	elem.webkitRequestPointerLock();
	elem.addEventListener("mousemove", mouseMove);

}
function getDirection(position,lookat)
{
	var direction;
	direction[0] = (lookat[0]-position[0]); 
	direction[1] = (lookat[1]-position[1]);
	direction[2] = (lookat[2]-position[2]);
	
	length=Math.sqrt(direction[0]*direction[0]+direction[1]*direction[1]+direction[2]*direction[2]);
	direction[0] /= length;
	direction[1] /= length;
	direction[2] /= length;
		
	return direction;
	
	}

function handleKeyEvent(event) {
	var direction = getDirection(eyePos,lookAt);
	if (event.keyCode == 39) {
		// pfeilRechts
		eyePos[0] += 1;
	} else if (event.keyCode == 38) {
		// pfeilHoch
		eyePos[0] += direction[0]  ;
		eyePos[1] += direction[1]  ;
		eyePos[2] += direction[2]  ;
	} else if (event.keyCode == 40) {
		// pfeilRunter
		
		eyePos[0] -= direction[0]  ;
		eyePos[1] -= direction[1]  ;
		eyePos[2] -= direction[2]  ;
	} else if (event.keyCode == 37) {
		// pfeilLinks
		eyePos[0] -= 1;
	} else if (event.keyCode == 27) {
		// Esc=>mouseEvent abhängen
		document.getElementById("drawArea").removeEventListener("mousemove", mouseMove);
	}
};

window.addEventListener("keyup", handleKeyEvent, true);