/**
 * Provides requestAnimationFrame in a cross browser way.
 */
window.requestAnimFrame = (function() {
	return window.requestAnimationFrame
			|| window.webkitRequestAnimationFrame
			|| window.mozRequestAnimationFrame
			|| window.oRequestAnimationFrame
			|| window.msRequestAnimationFrame
			|| function(/* function FrameRequestCallback */callback, /*
																		 * DOMElement
																		 * Element
																		 */
					element) {
				window.setTimeout(callback, 1000 / 60);
			};
})();

function initGL(canvas) {
	var gl = null;
	try {
		gl = canvas.getContext("experimental-webgl");
		gl.viewportWidth = canvas.width;
		gl.viewportHeight = canvas.height;
		// gl.globalAlpha=0.5;
	} catch (e) {
	}
	if (!gl) {
		alert("Could not initialise WebGL, sorry :-(");
		return;
	}
	return gl;
}

function getShader(gl, id) {
	var shaderScript = document.getElementById(id);
	if (!shaderScript) {
		return null;
	}

	var str = "";
	var k = shaderScript.firstChild;
	while (k) {
		if (k.nodeType == 3) {
			str += k.textContent;
		}
		k = k.nextSibling;
	}

	var shader;
	if (shaderScript.type == "x-shader/x-fragment") {
		shader = gl.createShader(gl.FRAGMENT_SHADER);
	} else if (shaderScript.type == "x-shader/x-vertex") {
		shader = gl.createShader(gl.VERTEX_SHADER);
	} else {
		return null;
	}

	gl.shaderSource(shader, str);
	gl.compileShader(shader);

	if (!gl.getShaderParameter(shader, gl.COMPILE_STATUS)) {
		alert(gl.getShaderInfoLog(shader));
		return null;
	}

	return shader;
}

var shaderProgram;

function initShaders(gl) {
	var fragmentShader = getShader(gl, "shader-fs");
	var vertexShader = getShader(gl, "shader-vs");

	shaderProgram = gl.createProgram();
	gl.attachShader(shaderProgram, vertexShader);
	gl.attachShader(shaderProgram, fragmentShader);
	gl.linkProgram(shaderProgram);

	if (!gl.getProgramParameter(shaderProgram, gl.LINK_STATUS)) {
		alert("Could not initialise shaders");
	}

	gl.useProgram(shaderProgram);

	shaderProgram.vertexPositionAttribute = gl.getAttribLocation(shaderProgram,
			"aVertexPosition");
	gl.enableVertexAttribArray(shaderProgram.vertexPositionAttribute);

	shaderProgram.pMatrixUniform = gl.getUniformLocation(shaderProgram,
			"uPMatrix");
	shaderProgram.mvMatrixUniform = gl.getUniformLocation(shaderProgram,
			"uMVMatrix");
}

var position = [];

var mvMatrix = mat4.create();
var pMatrix = mat4.create();

function setMatrixUniforms(gl) {
	gl.uniformMatrix4fv(shaderProgram.pMatrixUniform, false, pMatrix);
	gl.uniformMatrix4fv(shaderProgram.mvMatrixUniform, false, mvMatrix);
}

var squareVertexPositionBuffer;

function initBuffers(gl) {

	squareVertexPositionBuffer = gl.createBuffer();
	gl.bindBuffer(gl.ARRAY_BUFFER, squareVertexPositionBuffer);
	vertices = [ 1.0, 1.0, 0.0, -1.0, 1.0, 0.0, 1.0, -1.0, 0.0, -1.0, -1.0, 0.0 ];
	gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
	squareVertexPositionBuffer.itemSize = 3;
	squareVertexPositionBuffer.numItems = 4;
}

var eyePos = [ 0, 0, 10 ];
var lookAt = [ 0, 0, 0 ];

function drawScene(gl) {
	gl.viewport(0, 0, gl.viewportWidth, gl.viewportHeight);
	gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	mat4.perspective(45, gl.viewportWidth / gl.viewportHeight, 0.1, 1000.0, // sichtweite
	pMatrix);

	mat4.lookAt(eyePos, // hier befinde ich mich
	[ lookAt[0], lookAt[1], lookAt[2] ], // da guck ich hin
	[ 0, 1, 0 ], // das ist oben
	mvMatrix);
	// mat4.identity(mvMatrix); //mit dem gehts nicht

	for (var i = 0; i < position.length; i++) {
		drawSquareOnPosition(gl, position[i].X, position[i].Y, position[i].Z);
	}
}

function drawSquareOnPosition(gl, x, y, z) {
	mat4.translate(mvMatrix, [ x, y, z ]);
	gl.bindBuffer(gl.ARRAY_BUFFER, squareVertexPositionBuffer);
	gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute,
			squareVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);
	setMatrixUniforms(gl);
	gl.drawArrays(gl.TRIANGLE_STRIP, 0, squareVertexPositionBuffer.numItems);
	mat4.rotateY(mvMatrix, Math.PI / 2);// 90grad rotieren und nochmal zeichen
										// damit auch von der seite sichtbar
	gl.bindBuffer(gl.ARRAY_BUFFER, squareVertexPositionBuffer);
	gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute,
			squareVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);
	setMatrixUniforms(gl);
	gl.drawArrays(gl.TRIANGLE_STRIP, 0, squareVertexPositionBuffer.numItems);
	mat4.rotateY(mvMatrix, Math.PI / -2);
	mat4.translate(mvMatrix, [ -x, -y, -z ]);
}

function handleKeyEvent(event) {
	if (event.keyCode == 39) {
		// pfeilRechts
		eyePos[0] += 1;
	} else if (event.keyCode == 38) {
		// pfeilHoch
		eyePos[2] -= 1;
	} else if (event.keyCode == 40) {
		// pfeilRunter
		eyePos[2] += 1;
	} else if (event.keyCode == 37) {
		// pfeilLinks
		eyePos[0] -= 1;
	}
};

function gotSources(sourceInfos) {
	var vId;
	for (var i = 0; i != sourceInfos.length; ++i) {
		var sourceInfo = sourceInfos[i];
		var option = document.createElement("option");
		option.value = sourceInfo.id;
		if (sourceInfo.kind == 'video'){
			alert(sourceInfo.id);
		}
		if (sourceInfo.kind == 'video') {
			vId=sourceInfo.id;
		}
	}
	if(!vId){
		alert('no cam!');
		return;
	}
	// Grab elements, create settings, etc.
	var video = document.getElementById("video"),
		videoObj = { "video": {optional: [{sourceId: vId}]}},
		errBack = function(error) {
			console.log("Video capture error: ", error.code); 
		};

	// Put video listeners into place
	if(navigator.getUserMedia) { // Standard
		navigator.getUserMedia(videoObj, function(stream) {
			video.src = stream;
			video.play();
		}, errBack);
	} else if(navigator.webkitGetUserMedia) { // WebKit-prefixed
		navigator.webkitGetUserMedia(videoObj, function(stream){
			video.src = window.webkitURL.createObjectURL(stream);
			video.play();
		}, errBack);
	}
}

function initCam() {
	if (typeof MediaStreamTrack === 'undefined') {
		alert('This browser does not support MediaStreamTrack.\n\nTry Chrome Canary.');
	} else {
		MediaStreamTrack.getSources(gotSources);
	}
}

function webGLStart() {
	var canvas = document.getElementById("drawArea");
	var gl = initGL(canvas);
	initShaders(gl);
	initBuffers(gl);

	gl.clearColor(0.0, 0.0, 0.0, 0.0);
	gl.enable(gl.DEPTH_TEST);

	window.addEventListener("keyup", handleKeyEvent, true);

	register('/datachannel/eyePosition', function(msg) {
		eyePos[0] = msg.X;
		eyePos[1] = msg.Y;
		eyePos[2] = msg.Z;
	});

	register('/datachannel/lookAt', function(msg) {
		lookAt[0] = msg.X;
		lookAt[1] = msg.Y;
		lookAt[2] = msg.Z;
	});

	register('/datachannel/handPosition', function(msg) {
		position[position.length] = {
			X : Math.round(msg.X),
			Y : Math.round(msg.Y),
			Z : Math.round(msg.Z)
		};
	});
	// server begrüßen->sorgt dafür, dass der server alle bisher bekannten
	// positionen sendet
	send('/drawing/hello', 'Hallo Server');
	var mainFunc = function() {
		// wird regelmäßig vom browser afgerufen
		window.requestAnimFrame(mainFunc, canvas);
		drawScene(gl);
	};
	mainFunc();
	initCam();
}
