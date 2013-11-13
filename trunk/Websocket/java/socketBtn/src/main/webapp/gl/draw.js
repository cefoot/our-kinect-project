/**
 * Provides requestAnimationFrame in a cross browser way.
 */
window.requestAnimFrame = (function() {
  return window.requestAnimationFrame ||
         window.webkitRequestAnimationFrame ||
         window.mozRequestAnimationFrame ||
         window.oRequestAnimationFrame ||
         window.msRequestAnimationFrame ||
         function(/* function FrameRequestCallback */ callback, /* DOMElement Element */ element) {
           window.setTimeout(callback, 1000/60);
         };
})();


    function initGL(canvas) {
    	var gl = null;
        try {
            gl = canvas.getContext("experimental-webgl");
            gl.viewportWidth = canvas.width;
            gl.viewportHeight = canvas.height;
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

        shaderProgram.vertexPositionAttribute = gl.getAttribLocation(shaderProgram, "aVertexPosition");
        gl.enableVertexAttribArray(shaderProgram.vertexPositionAttribute);

        shaderProgram.pMatrixUniform = gl.getUniformLocation(shaderProgram, "uPMatrix");
        shaderProgram.mvMatrixUniform = gl.getUniformLocation(shaderProgram, "uMVMatrix");
    }


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
        vertices = [
             1.0,  1.0,  0.0,
            -1.0,  1.0,  0.0,
             1.0, -1.0,  0.0,
            -1.0, -1.0,  0.0
        ];
        gl.bufferData(gl.ARRAY_BUFFER, new Float32Array(vertices), gl.STATIC_DRAW);
        squareVertexPositionBuffer.itemSize = 3;
        squareVertexPositionBuffer.numItems = 4;
    }

    var eyePos = [0, 0, 10];

    function drawScene(gl) {
        gl.viewport(0, 0, gl.viewportWidth, gl.viewportHeight);
        gl.clear(gl.COLOR_BUFFER_BIT | gl.DEPTH_BUFFER_BIT);

        mat4.perspective(45, 
        		gl.viewportWidth / gl.viewportHeight, 
        		0.1, 
        		1000.0, //sichtweite
        		pMatrix);

        mat4.lookAt(eyePos, //hier befinde ich mich
        		[position[0].X, position[0].Y, position[0].Z], //da guck ich hin
        		[0, 1, 0], //das ist oben
        		mvMatrix);
//        mat4.identity(mvMatrix); //mit dem gehts nicht
        
        for (var i = 0; i < position.length; i++) {
            drawSquareOnPosition(gl, position[i].X, position[i].Y, position[i].Z);
		}        
    }
    
    function drawSquareOnPosition(gl, x, y, z){
        mat4.translate(mvMatrix, [x, y, z]);
        gl.bindBuffer(gl.ARRAY_BUFFER, squareVertexPositionBuffer);
        gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, squareVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);
        setMatrixUniforms(gl);
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, squareVertexPositionBuffer.numItems);
        mat4.rotateY(mvMatrix, Math.PI/2);//90grad rotieren und nochmal zeichen damit auch von der seite sichtbar
        gl.bindBuffer(gl.ARRAY_BUFFER, squareVertexPositionBuffer);
        gl.vertexAttribPointer(shaderProgram.vertexPositionAttribute, squareVertexPositionBuffer.itemSize, gl.FLOAT, false, 0, 0);
        setMatrixUniforms(gl);
        gl.drawArrays(gl.TRIANGLE_STRIP, 0, squareVertexPositionBuffer.numItems);
        mat4.translate(mvMatrix, [-x, -y, -z]);
        mat4.rotateY(mvMatrix, Math.PI/-2);
    }

    function handleKeyEvent(event) {
    	if(event.keyCode==39){
    		//pfeilRechts
    		eyePos[0] += 1;
    	}else if(event.keyCode==38){
    		//pfeilHoch
    		eyePos[2] -= 1;
    	}else if(event.keyCode==40){
    		//pfeilRunter
    		eyePos[2] += 1;
    	}else if(event.keyCode==37){
    		//pfeilLinks
    		eyePos[0] -= 1;
    	}
	};

	var position = [];
	
    function webGLStart() {
        var canvas = document.getElementById("drawArea");
        var gl = initGL(canvas);
        initShaders(gl);
        initBuffers(gl);

        gl.clearColor(0.0, 0.0, 0.0, 1.0);
        gl.enable(gl.DEPTH_TEST);

		window.addEventListener("keyup", handleKeyEvent, true);
		
		register('/datachannel/handPosition', function(msg){
			position[position.length] = {
					X:msg.X,
					Y:msg.Y,
					Z:msg.Z
			};
		});

        var mainFunc = function() {
        	//wird regelmäßig vom browser afgerufen
            window.requestAnimFrame(mainFunc, canvas);
            drawScene(gl);
        };
        mainFunc();
    }

