/**
 * 
 */

/* Reference to the WebGLContext */
var gl = null;

/* Reference to the canvas DOM object. */
var canvas = null;

/* References to the shader programs */
var phongProgram, colorProgram, phongTexProgram;

// --------------------------------------------------------------------------
// Camera variables and helper function
// --------------------------------------------------------------------------
var deltaAlpha = 0;
var deltaBeta = 0;
var alpha = 0.44, prevAlpha;
var beta = 0.44, prevBeta;
var radius = 5, prevRadius;
var camX, camY, camZ, lookAtX, lookAtY, lookAtZ;

function setValue(uiElem, value) {
	if (uiElem != undefined && uiElem.hasOwnProperty("valueAsNumber")) {
		uiElem.valueAsNumber = value;
	}
}

function spherical2Cartesian() {
	camX = radius * Math.cos(beta) * Math.sin(alpha) + lookAtX;
	camZ = radius * Math.cos(beta) * Math.cos(alpha) + lookAtZ;
	camY = radius * Math.sin(beta) + lookAtY;
}

// --------------------------------------------------------------------------
// Storage for the loaded model
// Models is defined in l3dmodels.js
// --------------------------------------------------------------------------

var myModel = new Models(), pipes = new Models(), myGrid = new Model(), myAxis = new Models();

// --------------------------------------------------------------------------
// Shader stuff
// --------------------------------------------------------------------------

function createProgram(docVS, docFS, attributes, uniforms) {

	// create the shader handles
	var vs = gl.createShader(gl.VERTEX_SHADER);
	var fs = gl.createShader(gl.FRAGMENT_SHADER);

	// get the source code
	vsSource = document.getElementById(docVS);
	fsSource = document.getElementById(docFS);

	// set the source
	gl.shaderSource(vs, vsSource.text);
	gl.shaderSource(fs, fsSource.text);

	// compile shaders
	gl.compileShader(vs);
	gl.compileShader(fs);

	if (!gl.getShaderParameter(vs, gl.COMPILE_STATUS))
		throw ("Shader Error: " + docVS + ": " + gl.getShaderInfoLog(vs));
	if (!gl.getShaderParameter(fs, gl.COMPILE_STATUS))
		throw ("Shader Error: " + docFS + ": " + gl.getShaderInfoLog(fs));

	var program = gl.createProgram();
	gl.attachShader(program, vs);
	gl.attachShader(program, fs);

	for (var i = 0; i < attributes.length; ++i) {
		gl.bindAttribLocation(program, i, attributes[i]);
	}

	gl.linkProgram(program);
	if (!gl.getProgramParameter(program, gl.LINK_STATUS))
		throw ("Program Error: " + gl.getProgramInfoLog(program));

	program.uniforms = {};
	for (var i = 0; i < uniforms.length; ++i) {
		program.uniforms[uniforms[i]] = gl.getUniformLocation(program,
				uniforms[i]);
	}

	return (program);
}

// Callback called each time the browser wants us to draw another frame
function render(time) {
	// Clear color and depth buffers
	gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

	// Place Camera
	mat4.identity(Matrices.view);
	mat4.lookAt(vec3.create([ camX, camY, camZ ]), vec3.create([ lookAtX,
			lookAtY, lookAtZ ]), vec3.create([ 0, 1, 0 ]), Matrices.view);

	// Clear model matrix
	mat4.identity(Matrices.model);

	// set phong to be the active program
	gl.useProgram(phongProgram);

	// compute all derived matrices (normal, viewModel, PVM)
	Matrices.compDerived();

	// set matrices
	gl.uniformMatrix4fv(phongProgram.uniforms["uMat4ViewModel"], false,
			Matrices.getVM());
	gl.uniformMatrix3fv(phongProgram.uniforms["uMat3Normal"], false, Matrices
			.getN());
	gl.uniformMatrix4fv(phongProgram.uniforms["uMat4PVM"], false, Matrices
			.getPVM());

	// set the light direction uniform
	var uVec3LightDir = [ 1.0, 0.5, 1.0, 0.0 ];
	var res2 = vec3.create();
	mat4.multiplyVec4(Matrices.getVM(), uVec3LightDir, res2);
	vec3.normalize(res2);
	gl.uniform3fv(phongProgram.uniforms["uVec3LightDir"], res2);

	myAxis.render();

	// set the textured phong program as active
	gl.useProgram(phongTexProgram);

	// set the matrices
	gl.uniformMatrix4fv(phongTexProgram.uniforms["uMat4PVM"], false, Matrices
			.getPVM());
	gl.uniformMatrix4fv(phongTexProgram.uniforms["uMat4ViewModel"], false,
			Matrices.getVM());
	gl.uniformMatrix3fv(phongTexProgram.uniforms["uMat3Normal"], false,
			Matrices.getN());
	// set the light dir
	gl.uniform3fv(phongTexProgram.uniforms["uVec3LightDir"], res2);

	// render the loaded model
	// note: if the model has no texture use phongProgram instead
	myModel.render();

	// set the color program as active
	gl.useProgram(colorProgram);

	// set the matrix
	gl.uniformMatrix4fv(colorProgram.uniforms["uMat4PVM"], false, Matrices
			.getPVM());

	// render the grid
	myGrid.render();
	updateLookAtMdl();
	pipes.render();

	// just checking
	checkError();

	// Send the commands to WebGL
	gl.flush();

	// Request another frame
	window.requestAnimFrame(render, canvas);
}

function updateLookAtMdl() {
	if (lookAtMdl == undefined)
		return;
	var matrix = lookAtMdl.localTrans;
	mat4.identity(matrix);
	mat4.translate(matrix, [ lookAtX, lookAtY, lookAtZ ]);
}

function checkError() {
	var error = gl.getError();

	if (error)
		console.log("error: " + error);
}

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
		break;
	}
	// only set if changed
	if (valChanged) {
		spherical2Cartesian();
	}
}

var lookAtMdl;
var isVisible;

function visibleLookAt() {
	if (lookAtMdl == undefined) {
		var clr = new Material();
		clr.diffuse[0] = 0.0;// r
		clr.diffuse[1] = 0.0;// g
		clr.diffuse[2] = 0.0;// b
		lookAtMdl = createVector([ 0, 0, 0 ], [ 0.3, 0, 0 ], 0.3, clr, true);
		lookAtMdl.origRender = lookAtMdl.render;
		pipes.add(lookAtMdl);
	}
	if (!isVisible) {
		isVisible = true;
		lookAtMdl.render = function() {
			lookAtMdl.origRender();
		};
	} else {
		isVisible = false;
		lookAtMdl.render = function() {
			// empty
		};
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
// Init GL settings, programs and models
// --------------------------------------------------------------------------

// Called when we have the context
function init() {

	// init cam variables;
	spherical2Cartesian();

	// set the viewport to be the whole canvas
	gl.viewport(0, 0, gl.desiredWidth, gl.desiredHeight);

	// projection matrix.
	mat4.perspective(45, gl.desiredWidth / gl.desiredHeight, 0.1, 1000,
			Matrices.proj);

	// Set the background clear color to gray.
	gl.clearColor(0.8, 0.8, 0.8, 1.0);
	// general gl settings
	gl.enable(gl.CULL_FACE);
	gl.enable(gl.DEPTH_TEST);

	// create the shader programs
	phongProgram = createProgram("phong_vs", "phong_fs", [ "aVec4Position",
			"aVec3Normal" ],
			[ "uMat4PVM", "uMat3Normal", "uMat4ViewModel", "uVec3LightDir",
					"uVec4Diffuse", "uVec4Specular", "uFloatShininess" ]);

	colorProgram = createProgram("color_vs", "color_fs", [ "aVec4Position" ], [
			"uMat4PVM", "uVec4Diffuse" ]);

	phongTexProgram = createProgram("phong_texture_vs", "phong_texture_fs", [
			"aVec4Position", "aVec3Normal", "aVec2TexCoord" ], [ "uMat4PVM",
			"uMat3Normal", "uMat4ViewModel", "uSamp2DTexID", "uVec3LightDir",
			"uVec4Diffuse", "uVec4Specular", "uFloatShininess" ]);

	// init the matrices
	Matrices.init();

	// init the models for rendering
	var myGridMat = new Material();
	myGridMat.diffuse = new Float32Array([ 1.0, 1.0, 1.0, 1.0 ]);
	myGrid = createGrid(1, 3, 12);
	myGrid.setMaterial(myGridMat);
	myAxis = createAxis(3);
	// just in case
	checkError();
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
