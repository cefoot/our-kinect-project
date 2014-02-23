/**
 * 
 */
// --------------------------------------------------------------------------
// Handler
// --------------------------------------------------------------------------
var isPainting = false;

function handleTouchStart(event) {
	if (register == undefined){
		return;
	}
	if (isPainting) {
		send('/paint/', 'stop');
	} else {
		send('/paint/', 'start');
	}
}

function onPaint(msg){
	if(msg == 'start'){
		isPainting = true;
		document.getElementById('recording').style.background = 'red';
	} else {
		isPainting = false;
		document.getElementById('recording').style.background = 'transparent';
	}
}

function gotSources(sourceInfos) {
	var vId;
	for (var i = 0; i != sourceInfos.length; ++i) {
		var sourceInfo = sourceInfos[i];
		var option = document.createElement("option");
		option.value = sourceInfo.id;
		if (sourceInfo.kind == 'video') {
			vId = sourceInfo.id;
		}
	}
	if (!vId) {
		alert('no cam!');
		return;
	}
	// Grab elements, create settings, etc.
	var video = document.getElementById("video"), videoObj = {
		"video" : {
			optional : [ {
				sourceId : vId
			} ]
		}
	}, errBack = function(error) {
		console.log("Video capture error: ", error.code);
	};

	// Put video listeners into place
	if (navigator.getUserMedia) { // Standard
		navigator.getUserMedia(videoObj, function(stream) {
			video.src = stream;
			video.play();
		}, errBack);
	} else if (navigator.webkitGetUserMedia) { // WebKit-prefixed
		navigator.webkitGetUserMedia(videoObj, function(stream) {
			video.src = window.webkitURL.createObjectURL(stream);
			video.play();
		}, errBack);
	}
}

// --------------------------------------------------------------------------
// functions
// --------------------------------------------------------------------------

function initScreenListener(){
	register('/paint/', onPaint);
	register('/datachannel/eyePosition', function(msg) {
		camX = msg.X;
		camY = msg.Y;
		camZ = msg.Z;
		// ein stück zurück
		var direction = getDirection([ camX, camY, camZ ], [ lookAtX, lookAtY, lookAtZ ]);
		camX -= direction[0] * 1;
		camY -= direction[1] * 1;
		camZ -= direction[2] * 1;
	});

	register('/datachannel/lookAt', function(msg) {
		lookAtX = msg.X;
		lookAtY = msg.Y;
		lookAtZ = msg.Z;
	});
}

function initCam() {
	if (typeof MediaStreamTrack === 'undefined') {
		alert('This browser does not support MediaStreamTrack.\n\nTry Chrome Canary.');
	} else {
		MediaStreamTrack.getSources(gotSources);
	}
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
		
		gl.clearColor(0.0, 0.0, 0.0, 0.0);//transarent damit das video gesehen wird

		// rendering callback
		window.requestAnimFrame(render, canvas);
	}
	document.ontouchstart = handleTouchStart;
	initCam();
}