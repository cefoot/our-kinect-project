/**
 * 
 */
// --------------------------------------------------------------------------
// Handler
// --------------------------------------------------------------------------
var isPainting = false;

function handleTouchStart(event) {
	if (register == undefined) {
		return;
	}
	if (isPainting) {
		send('/paint/', 'stop');
	} else {
		send('/paint/', 'start');
	}
}

function onPaint(msg) {
	if (msg == 'start') {
		isPainting = true;
		document.getElementById('recording').style.background = 'red';
	} else {
		isPainting = false;
		document.getElementById('recording').style.background = 'transparent';
	}
}

var isLiveView = false;

function imgRecieved(msg){
	if (isLiveView){
		return;
	}
    var image = document.getElementById('videoImgTarget');
    image.style.display = 'inline';
    image.src = msg;
}

/*function gotSources(sourceInfos) {
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
		isLiveView = false;
		console.debug('no cam!');
		return;
	}
	// Grab elements, create settings, etc.
	var video = document.getElementById("videoel"), videoObj = {
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
	} else if (navigator.webkitGetUserMedia) { // WebKit-prefixed -> chrome
		isLiveView = true;
		navigator.webkitGetUserMedia(videoObj, function(stream) {
			video.src = window.webkitURL.createObjectURL(stream);
			video.play();
		}, errBack);
	}
	//timer der alle x millisekunden das hintergrund bild per websocket sendet ---> flaschenhals...
	//time = setInterval(function() {
	//	var videoCanvas = document.getElementById('videoImg');
	//	var ctx = videoCanvas.getContext('2d');
	//	ctx.drawImage(video, 0, 0, 320, 240);
	//	var data = videoCanvas.toDataURL('image/jpeg', 1.0);
	//	send('/cam/img', data);
	//}, 250);
}*/

function initScreenListener() {
	register('/cam/img', imgRecieved);
	register('/paint/', onPaint);
	register('/datachannel/eyePosition', function(msg) {
		camX = msg.X;
		camY = msg.Y;
		camZ = msg.Z;
		// ein stück zurück
		var direction = getDirection([ camX, camY, camZ ], [ lookAtX, lookAtY,
				lookAtZ ]);
		camX -= direction[0] * 1;
		camY -= direction[1] * 1;
		camZ -= direction[2] * 1;
	});

	register('/datachannel/lookAt', function(msg) {
		lookAtX = msg.X;
		lookAtY = msg.Y;
		lookAtZ = msg.Z;
	});
	
	send('/client/hello', 'Hello i am a screen');
}

function initCam() {
	if (!confirm('Would you like to see your live img?')){
		//dann eben nicht-> keine kamera hier
		return;
	}
	var vid = document.getElementById('videoel');
	navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia || navigator.msGetUserMedia;
	window.URL = window.URL || window.webkitURL || window.msURL || window.mozURL;
	if (!navigator.getUserMedia) {
		alert('This browser does not support MediaStreamTrack.\n\nTry Chrome Canary.');
	} else {
		var videoSelector = {video : true};
					if (window.navigator.appVersion.match(/Chrome\/(.*?) /)) {
						var chromeVersion = parseInt(window.navigator.appVersion.match(/Chrome\/(\d+)\./)[1], 10);
						if (chromeVersion < 20) {
							videoSelector = "video";
						}
					};
				
					navigator.getUserMedia(videoSelector, function( stream ) {
						if (vid.mozCaptureStream) {
							vid.mozSrcObject = stream;
						} else {
							vid.src = (window.URL && window.URL.createObjectURL(stream)) || stream;
						}
						vid.play();
					}, function() {
						insertAltVideo(vid);
						document.getElementById('gum').className = "hide";
						document.getElementById('nogum').className = "nohide";
						alert("There was some problem trying to fetch video from your webcam, using a fallback video instead.");
					});
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

		gl.clearColor(0.0, 0.0, 0.0, 0.0);// transarent damit das video
											// gesehen wird

		// rendering callback
		window.requestAnimFrame(render, canvas);
	}
	document.ontouchstart = handleTouchStart;
	initCam();
}