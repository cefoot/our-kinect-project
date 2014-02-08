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
//  Camera variables and helper function
// --------------------------------------------------------------------------
var deltaAlpha = 0;
var deltaBeta = 0;
var alpha = 0.44, prevAlpha;
var beta = 0.44, prevBeta;
var radius = 5, prevRadius;
var camX, camY, camZ;

function setValue(uiElem, value){
	if(uiElem != undefined && uiElem.hasOwnProperty("valueAsNumber")){
		uiElem.valueAsNumber = value;
	}
}

function spherical2Cartesian() {
    camX = radius * Math.cos(beta) * Math.sin(alpha);
    camZ = radius * Math.cos(beta) * Math.cos(alpha);
    camY = radius * Math.sin(beta);
    setValue(document.getElementById("alpha"), alpha);
    setValue(document.getElementById("beta"), beta);
    setValue(document.getElementById("radius"), radius);
}


// --------------------------------------------------------------------------
//  Storage for the loaded model
// Models is defined in l3dmodels.js
// --------------------------------------------------------------------------

var myModel = new Models(), myGrid = new Model(), myAxis = new Models();


// --------------------------------------------------------------------------
//  Shader stuff
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
        program.uniforms[uniforms[i]] = gl.getUniformLocation(program, uniforms[i]);
    }

    return(program);
}






// Callback called each time the browser wants us to draw another frame
function render(time)
{           	
    // Clear color and depth buffers
    gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);
	
    // Place Camera
    mat4.identity(Matrices.view);
    mat4.lookAt(vec3.create([camX, camY, camZ]), vec3.create([0, 0, 0]), vec3.create([0, 1, 0]), Matrices.view);

    // Clear model matrix 
    mat4.identity(Matrices.model);

    // set phong to be the active program
    gl.useProgram(phongProgram);

    // compute all derived matrices (normal, viewModel, PVM)
    Matrices.compDerived();
    
    // set matrices
    gl.uniformMatrix4fv(phongProgram.uniforms["uMat4ViewModel"], false, Matrices.getVM());
    gl.uniformMatrix3fv(phongProgram.uniforms["uMat3Normal"], false, Matrices.getN());
    gl.uniformMatrix4fv(phongProgram.uniforms["uMat4PVM"], false, Matrices.getPVM());


    // set the light direction uniform
    var uVec3LightDir = [1.0, 0.5, 1.0,0.0];
    var res2 = vec3.create();
    mat4.multiplyVec4(Matrices.getVM(), uVec3LightDir, res2);
    vec3.normalize(res2);
    gl.uniform3fv(phongProgram.uniforms["uVec3LightDir"], res2);

    myAxis.render();

    // set the textured phong program as active
    gl.useProgram(phongTexProgram);

    // set the matrices
    gl.uniformMatrix4fv(phongTexProgram.uniforms["uMat4PVM"], false, Matrices.getPVM());
    gl.uniformMatrix4fv(phongTexProgram.uniforms["uMat4ViewModel"], false, Matrices.getVM());
    gl.uniformMatrix3fv(phongTexProgram.uniforms["uMat3Normal"], false, Matrices.getN());
    // set the light dir
    gl.uniform3fv(phongTexProgram.uniforms["uVec3LightDir"], res2);

    // render the loaded model
    // note: if the model has no texture use phongProgram instead
    myModel.render();

    // set the color program as active
    gl.useProgram(colorProgram);

    // set the matrix
    gl.uniformMatrix4fv(colorProgram.uniforms["uMat4PVM"], false, Matrices.getPVM());

    // render the grid
    myGrid.render();

    // just checking
    checkError();
    
    // Send the commands to WebGL
    gl.flush();
	
    // Request another frame
    window.requestAnimFrame(render, canvas);
}

function checkError()
{
    var error = gl.getError();
	
    if (error) 
        console.log("error: " + error);
}

 

// --------------------------------------------------------------------------
//  Mouse and Keyboard handlers
// --------------------------------------------------------------------------

var mouseTracking = -1, startX, startY;

function handleKeyDown(event) {
	var valChanged = false;
    switch (String.fromCharCode(event.keyCode)) {
        case "A": alpha -= 0.05; valChanged = true; break;
        case "D": alpha += 0.05; valChanged = true; break;
        case "W": beta += 0.05; if (beta > 1.5) beta = 1.5; valChanged = true; break;
        case "S": beta -= 0.05; if (beta < -1.5) beta = -1.5; valChanged = true; break;
        case "R": radius += 0.05; valChanged = true; break;
        case "F": radius -= 0.05; if (radius < 0.5) radius = 0.5; valChanged = true; break;
    }
    //only set if changed
    if(valChanged){
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
    }
    else if (mouseTracking == 1) {
        radius = prevRadius - deltaY*0.01;
    }
    spherical2Cartesian();

}


// --------------------------------------------------------------------------
//  Init GL settings, programs and models
// --------------------------------------------------------------------------

//Called when we have the context
function init() {

    // init cam variables;
    spherical2Cartesian();

    // set the viewport to be the whole canvas
    gl.viewport(0, 0, canvas.clientWidth, canvas.clientHeight);

    // projection matrix. 
    var ratio = canvas.clientWidth / canvas.clientHeight;
    mat4.perspective(60, ratio, 0.1, 1000, Matrices.proj);


    // Set the background clear color to gray.
    gl.clearColor(0.8, 0.8, 0.8, 1.0);
    // general gl settings
    gl.enable(gl.CULL_FACE);
    gl.enable(gl.DEPTH_TEST);

    // create the shader programs
    phongProgram = createProgram("phong_vs", "phong_fs",
        ["aVec4Position", "aVec3Normal"],
        ["uMat4PVM", "uMat3Normal", "uMat4ViewModel", "uVec3LightDir", "uVec4Diffuse", "uVec4Specular", "uFloatShininess"]);

    colorProgram = createProgram("color_vs", "color_fs",
        ["aVec4Position"],
        ["uMat4PVM", "uVec4Diffuse"]);

    phongTexProgram = createProgram("phong_texture_vs", "phong_texture_fs",
         ["aVec4Position", "aVec3Normal", "aVec2TexCoord"],
        ["uMat4PVM", "uMat3Normal", "uMat4ViewModel", "uSamp2DTexID", "uVec3LightDir", "uVec4Diffuse", "uVec4Specular", "uFloatShininess"]);

    // init the matrices
    Matrices.init();

    // init the models for rendering
    var myGridMat = new Material();
    myGridMat.diffuse = new Float32Array([1.0, 1.0, 1.0, 1.0]);
    myGrid = createGrid(1, 3, 12);
    myGrid.setMaterial(myGridMat);
    myAxis = createAxis(3);
    // just in case
    checkError();
}


// --------------------------------------------------------------------------
//  Main function
// --------------------------------------------------------------------------


function main()
{		
    // Try to get a WebGL context    
    canvas = document.getElementById("canvas");    
    
    gl = WebGLUtils.setupWebGL(canvas, { depth: true });
            
    if (gl != null)
    {
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
