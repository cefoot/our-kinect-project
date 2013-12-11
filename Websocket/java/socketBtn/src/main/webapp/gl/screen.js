var rotStp = 0.005;

function mouseMove(event) {
	var rotX = rotStp * -event.webkitMovementX;
	var rotY = rotStp * -event.webkitMovementY;

	// rotate around x
	// y' = y*cos q - z*sin q
	// z' = y*sin q + z*cos q
	lookAt[1] = (lookAt[1] - eyePos[1]) * Math.cos(rotY)
			- (lookAt[2] - eyePos[2]) * Math.sin(rotY) + eyePos[1];
	lookAt[2] = (lookAt[1] - eyePos[1]) * Math.sin(rotY)
			+ (lookAt[2] - eyePos[2]) * Math.cos(rotY) + eyePos[2];

	// rotate around y
	// z' = z*cos q - x*sin q
	// x' = z*sin q + x*cos q
	lookAt[2] = (lookAt[2] - eyePos[2]) * Math.cos(rotX)
			- (lookAt[0] - eyePos[0]) * Math.sin(rotX) + eyePos[2];
	lookAt[0] = (lookAt[2] - eyePos[2]) * Math.sin(rotX)
			+ (lookAt[0] - eyePos[0]) * Math.cos(rotX) + eyePos[0];

	// console.debug("rotX:"+rotX+"||rotY:"+rotY+"||pos:"+eyePos +
	// "||lookAt:"+lookAt);
}

var speed = 1;

function btnFaster() {
	speed++;
	document.getElementById("lblSpeed").innerText = "Speed:" + speed;
}

function btnSlower() {
	speed--;
	if (speed < 1) {
		speed = 1;
	}
	document.getElementById("lblSpeed").innerText = "Speed:" + speed;
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
function getDirection(position, lookat) {
	var direction = [];
	direction[0] = (lookat[0] - position[0]);
	direction[1] = (lookat[1] - position[1]);
	direction[2] = (lookat[2] - position[2]);

	length = Math.sqrt(direction[0] * direction[0] + direction[1]
			* direction[1] + direction[2] * direction[2]);
	direction[0] /= length;
	direction[1] /= length;
	direction[2] /= length;

	return direction;

}

function handleKeyEvent(event) {
	var direction = getDirection(eyePos, lookAt);
	// rotate around y
	// z' = z*cos q - x*sin q
	// x' = z*sin q + x*cos q
	var strafeDir = [];
	var rAngle = Math.PI / 2;
	strafeDir[2] = direction[2] * Math.cos(rAngle) - direction[0]
			* Math.sin(rAngle);
	strafeDir[0] = direction[2] * Math.sin(rAngle) + direction[0]
			* Math.cos(rAngle);
	if (event.keyCode == 39) {
		// pfeilRechts
		eyePos[0] -= strafeDir[0] * speed;
		lookAt[0] -= strafeDir[0] * speed;
		eyePos[2] -= strafeDir[2] * speed;
		lookAt[2] -= strafeDir[2] * speed;
	} else if (event.keyCode == 38) {
		// pfeilHoch
		eyePos[0] += direction[0] * speed;
		lookAt[0] += direction[0] * speed;
		eyePos[1] += direction[1] * speed;
		lookAt[1] += direction[1] * speed;
		eyePos[2] += direction[2] * speed;
		lookAt[2] += direction[2] * speed;
	} else if (event.keyCode == 40) {
		// pfeilRunter
		eyePos[0] -= direction[0] * speed;
		lookAt[0] -= direction[0] * speed;
		eyePos[1] -= direction[1] * speed;
		lookAt[1] -= direction[1] * speed;
		eyePos[2] -= direction[2] * speed;
		lookAt[2] -= direction[2] * speed;
	} else if (event.keyCode == 37) {
		// pfeilLinks
		eyePos[0] += strafeDir[0] * speed;
		lookAt[0] += strafeDir[0] * speed;
		eyePos[2] += strafeDir[2] * speed;
		lookAt[2] += strafeDir[2] * speed;
	} else if (event.keyCode == 27) {
		// Esc=>mouseEvent abhängen
		document.getElementById("drawArea").removeEventListener("mousemove",
				mouseMove);
	} else if (event.keyCode == 187) {
		// plus
		document.getElementById("btnFaster").click();
	} else if (event.keyCode == 189) {
		// minus
		document.getElementById("btnSlower").click();
	} else {
		console.debug(event.keyCode);
	}
};

window.addEventListener("keydown", handleKeyEvent);