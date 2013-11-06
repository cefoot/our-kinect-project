	function init()
    {
        // Initialize
        var gl = initWebGL(
            // The id of the Canvas Element
            "example",
            // The ids of the vertex and fragment shaders
            "vshader", "fshader",
            // The vertex attribute names used by the shaders.
            // The order they appear here corresponds to their index
            // used later.
            [ "vNormal", "vColor", "vPosition"],
            // The clear color and depth values
            [ 0, 0, 0, 1 ], 10000);
        if (!gl) {
          return;
        }

        gl.console.log("Starting init...");

        // Set up a uniform variable for the shaders
        gl.uniform3f(gl.getUniformLocation(gl.program, "lightDir"), 0, 0, 1);

        // Create a box. On return 'gl' contains a 'box' property with
        // the BufferObjects containing the arrays for vertices,
        // normals, texture coords, and indices.
        gl.box = makeBox(gl);

        // Set up the array of colors for the cube's faces
        var colors = new Uint8Array(
            [  0, 0, 1, 1,   0, 0, 1, 1,   0, 0, 1, 1,   0, 0, 1, 1,     // v0-v1-v2-v3 front
               1, 0, 0, 1,   1, 0, 0, 1,   1, 0, 0, 1,   1, 0, 0, 1,     // v0-v3-v4-v5 right
               0, 1, 0, 1,   0, 1, 0, 1,   0, 1, 0, 1,   0, 1, 0, 1,     // v0-v5-v6-v1 top
               1, 1, 0, 1,   1, 1, 0, 1,   1, 1, 0, 1,   1, 1, 0, 1,     // v1-v6-v7-v2 left
               1, 0, 1, 1,   1, 0, 1, 1,   1, 0, 1, 1,   1, 0, 1, 1,     // v7-v4-v3-v2 bottom
               0, 1, 1, 1,   0, 1, 1, 1,   0, 1, 1, 1,   0, 1, 1, 1 ]    // v4-v7-v6-v5 back
                                                );

        // Set up the vertex buffer for the colors
        gl.box.colorObject = gl.createBuffer();
        gl.bindBuffer(gl.ARRAY_BUFFER, gl.box.colorObject);
        gl.bufferData(gl.ARRAY_BUFFER, colors, gl.STATIC_DRAW);

        // Create some matrices to use later and save their locations in the shaders
        gl.mvMatrix = new J3DIMatrix4();
        gl.u_normalMatrixLoc = gl.getUniformLocation(gl.program, "u_normalMatrix");
        gl.normalMatrix = new J3DIMatrix4();
        gl.u_modelViewProjMatrixLoc =
                gl.getUniformLocation(gl.program, "u_modelViewProjMatrix");
        gl.mvpMatrix = new J3DIMatrix4();

        // Enable all of the vertex attribute arrays.
        gl.enableVertexAttribArray(0);
        gl.enableVertexAttribArray(1);
        gl.enableVertexAttribArray(2);

        // Set up all the vertex attributes for vertices, normals and colors
        gl.bindBuffer(gl.ARRAY_BUFFER, gl.box.vertexObject);
        gl.vertexAttribPointer(2, 3, gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, gl.box.normalObject);
        gl.vertexAttribPointer(0, 3, gl.FLOAT, false, 0, 0);

        gl.bindBuffer(gl.ARRAY_BUFFER, gl.box.colorObject);
        gl.vertexAttribPointer(1, 4, gl.UNSIGNED_BYTE, false, 0, 0);

        // Bind the index array
        gl.bindBuffer(gl.ELEMENT_ARRAY_BUFFER, gl.box.indexObject);

        return gl;
    }

    width = -1;
    height = -1;

    //canvas setzten / falls sich die browser größe geändert hat
    function reshape(gl)
    {
        var canvas = document.getElementById('example');
        var windowWidth = window.innerWidth - 20;
        var windowHeight = window.innerHeight - 40;
        if (windowWidth == width && windowHeight == height)
            return;

        width = windowWidth;
        height = windowHeight;
        canvas.width = windowWidth;
        canvas.height = windowHeight;

        // Set the viewport and projection matrix for the scene
        gl.viewport(0, 0, width, height);
        gl.perspectiveMatrix = new J3DIMatrix4();
        gl.perspectiveMatrix.perspective(30, width/height, 1, 10000);
        gl.perspectiveMatrix.lookat(0, 0, 12, 0, 0, 0, 0, 1, 0);
    }

    function drawPicture(gl)
    {
        // Make sure the canvas is sized correctly.
        reshape(gl);

        // Clear the canvas
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

        // Make a model/view matrix.
        gl.mvMatrix.makeIdentity();
        gl.mvMatrix.rotate(currentAngleY, 1,0,0);
        gl.mvMatrix.rotate(currentAngleX, 0,1,0);

        // Construct the normal matrix from the model-view matrix and pass it in
        gl.normalMatrix.load(gl.mvMatrix);
        gl.normalMatrix.invert();
        gl.normalMatrix.transpose();
        gl.normalMatrix.setUniform(gl, gl.u_normalMatrixLoc, false);

        // Construct the model-view * projection matrix and pass it in
        gl.mvpMatrix.load(gl.perspectiveMatrix);
        gl.mvpMatrix.multiply(gl.mvMatrix);
        gl.mvpMatrix.setUniform(gl, gl.u_modelViewProjMatrixLoc, false);

        // Draw the cube
        gl.drawElements(gl.TRIANGLES, gl.box.numIndices, gl.UNSIGNED_BYTE, 0);

        currentAngleX += incAngleX;
        if (currentAngleX > 360)
            currentAngleX -= 360;
        currentAngleY += incAngleY;
        if (currentAngleY > 360)
            currentAngleY -= 360;
    }    
    
    function angleChanged(){
    	var angles = new Object();
    	angles.incX = incAngleX;
    	angles.incY = incAngleY;
    	send('/qube/incAngle', angles);
    }

    function handleMotionEvent(event) {
	    incAngleX = -(Math.round(event.accelerationIncludingGravity.x*100)/100)/10.0;
	    incAngleY = (Math.round(event.accelerationIncludingGravity.y*100)/100)/10.0;
	    z = Math.round(event.accelerationIncludingGravity.z*100)/100;
	    // Do something awesome.
    	angleChanged();
	};   

    function handleKeyEvent(event) {
    	if(event.keyCode==39){
    		//pfeilRechts
    		incAngleX += 0.05;
    	}else if(event.keyCode==38){
    		//pfeilHoch
    		incAngleY -= 0.05;
    	}else if(event.keyCode==40){
    		//pfeilRunter
    		incAngleY += 0.05;
    	}else if(event.keyCode==37){
    		//pfeilLinks
    		incAngleX -= 0.05;
    	}
	    // Do something awesome.
    	angleChanged();
	};

    function start()
    {
    	register('/qube/incAngle',function(angles){
    		incAngleX = angles.incX;
    		incAngleY = angles.incY;
    	});
        var c = document.getElementById("example");
        var w = Math.floor(window.innerWidth * 0.9);
        var h = Math.floor(window.innerHeight * 0.9);

        c.width = w;
        c.height = h;

        var gl = init();
        if (!gl) {
          return;
        }
        currentAngleX = 0;
        currentAngleY = 0;
        incAngleX = 0;
        incAngleY = 0;
        var mainFunc = function() {
        	//wird regelmäßig vom browser afgerufen
            window.requestAnimFrame(mainFunc, c);
            drawPicture(gl);
        };
        //initial aufrufen
        mainFunc();

		window.addEventListener("devicemotion", handleMotionEvent, true);
		window.addEventListener("keyup", handleKeyEvent, true);
    }