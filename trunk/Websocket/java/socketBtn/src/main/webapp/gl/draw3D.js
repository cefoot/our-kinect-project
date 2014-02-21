//COPY OF draw.js 
/**
 * Provides requestAnimationFrame in a cross browser way.
 */
window.requestAnimFrame = (function() {
	return window.requestAnimationFrame || window.webkitRequestAnimationFrame
			|| window.mozRequestAnimationFrame || window.oRequestAnimationFrame
			|| window.msRequestAnimationFrame
			|| function(/* function FrameRequestCallback */callback, /*
																		 * DOMElement
																		 * Element
																		 */
			element) {
				window.setTimeout(callback, 1000 / 60);
			};
})();

var lastPos;
var painting=false;

function addPosition(x, y, z, r, g, b) {
	if (lastPos != undefined){
		var clr = new Material();
	   	clr.diffuse[0] = r;//r 
	   	clr.diffuse[1] = g;//g 
	   	clr.diffuse[2] = b;//b
		var mdl = createVector([lastPos.X, lastPos.Y, lastPos.Z],
					[x, y, z], 
					1, clr, true);
		pipes.add(mdl);
	}
	lastPos={
			X : x,
			Y : y,
			Z : z
		};
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

function initBasicListener() {

	register('/datachannel/clear', function(msg) {
		pipes = new Models();
	});

	register('/drawing/newPos', function(msg) {
		addPosition(msg.x, msg.y, msg.z, msg.r, msg.g, msg.b);
	});

}

