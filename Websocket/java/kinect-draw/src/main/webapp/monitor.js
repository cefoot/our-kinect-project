/**
 * 
 */


// --------------------------------------------------------------------------
// Mouse and Keyboard handlers
// --------------------------------------------------------------------------

var mouseTracking = -1, startX, startY;

var speed = 1;

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

function handleKeyDown(event) {
	var direction = getDirection([ camX, camY, camZ ], [ lookAtX, lookAtY,
			lookAtZ ]);
	// rotate around y
	// z' = z*cos q - x*sin q
	// x' = z*sin q + x*cos q
	var strafeDir = [];
	var rAngle = Math.PI / 2;
	strafeDir[2] = direction[2] * Math.cos(rAngle) - direction[0]
			* Math.sin(rAngle);
	strafeDir[0] = direction[2] * Math.sin(rAngle) + direction[0]
			* Math.cos(rAngle);
	var valChanged = false;
	switch (String.fromCharCode(event.keyCode)) {
	case "A":
		lookAtX += strafeDir[0] * speed;
		lookAtZ += strafeDir[2] * speed;
		valChanged = true;
		break;
	case "D":
		lookAtX -= strafeDir[0] * speed;
		lookAtZ -= strafeDir[2] * speed;
		valChanged = true;
		break;
	case "W":
		lookAtX += direction[0] * speed;
		lookAtY += direction[1] * speed;
		lookAtZ += direction[2] * speed;
		valChanged = true;
		break;
	case "S":
		lookAtX -= direction[0] * speed;
		lookAtY -= direction[1] * speed;
		lookAtZ -= direction[2] * speed;
		valChanged = true;
		break;
	case "X":
		visibleLookAt();
		break;
	case "R":
		radius += 0.05;
		valChanged = true;
		break;
	case "F":
		radius -= 0.05;
		if (radius < 0.5)
			radius = 0.5;
		valChanged = true;
		break;
	case "E":
		speed++;
		break;
	case "Q":
		speed--;
		if (speed < 1){
			speed = 1;
		}
		break;
	}
	// only set if changed
	if (valChanged) {
		spherical2Cartesian();
	}
}

function handleMouseDown(event) {
	if (event.button == 0) // left button
		mouseTracking = 0;
	else if (event.button == 1)
		mouseTracking = 1;
	startX = event.clientX;
	startY = event.clientY;
	prevAlpha = alpha;
	prevBeta = beta;
	prevRadius = radius;
}

function handleMouseUp(event) {
	mouseTracking = -1;
}

function handleMouseMove(event) {

	if (mouseTracking == -1) {
		return;
	}
	var xx = event.clientX;
	var yy = event.clientY;
	var deltaX = -xx + startX;
	var deltaY = yy - startY;
	if (mouseTracking == 0) {
		alpha = prevAlpha + deltaX * 0.01;
		beta = prevBeta + deltaY * 0.01;
		if (beta > 1.5)
			beta = 1.5;
		if (beta < -1.5)
			beta = -1.5;
	} else if (mouseTracking == 1) {
		radius = prevRadius - deltaY * 0.01;
	}
	spherical2Cartesian();

}


// --------------------------------------------------------------------------
// Main function
// --------------------------------------------------------------------------

function main() {
	lookAtX = 0;
	lookAtY = 0;
	lookAtZ = 0;
	// Try to get a WebGL context
	canvas = document.getElementById("canvas");
	canvas.width = canvas.clientWidth;
	canvas.height = canvas.clientHeight;

	gl = WebGLUtils.setupWebGL(canvas, {
		depth : true
	});

	if (gl != null) {
		gl.desiredWidth = canvas.clientWidth;
		gl.desiredHeight = canvas.clientHeight;
		// init gl stuff
		init();

		// handlers for keyboard and mouse
		document.onkeydown = handleKeyDown;
		canvas.onmousedown = handleMouseDown;
		document.onmouseup = handleMouseUp;
		document.onmousemove = handleMouseMove;

		// rendering callback
		window.requestAnimFrame(render, canvas);
	}
}