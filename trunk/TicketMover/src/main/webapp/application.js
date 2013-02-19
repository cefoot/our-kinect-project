require(['dojox/cometd', 'dojo/dom', 'dojo/dom-construct', 'dojo/domReady!'], function(cometd, dom, doc)
{
    cometd.configure({
        url: location.protocol + '//' + location.host + config.contextPath + '/ticketMover',
        logLevel: 'info'
    });

    cometd.addListener('/meta/handshake', function(message)
    	    {
    	        if (message.successful)
    	        {
    	            dom.byId('status').innerHTML += '<div>CometD handshake successful</div>';
    	            cometd.subscribe('/hello/world', function(message)
    	            		{
    	            			dom.byId('status').innerHTML += '<div>';
    	            			dom.byId('status').innerHTML += message.data;
    	            			dom.byId('status').innerHTML += '</div>';
    	            		});
    	            cometd.subscribe('/ticket/move', function(message){
    	            	var ticket = dom.byId(message.data.id);
    	            	ticket.style.left = message.data.x;
    	            	ticket.style.top = message.data.y;
    	            });
    	        }
    	        else
    	        {
    	        	dom.byId('status').innerHTML += '<div>CometD handshake failed</div>';
    	        }
    	    });

    dom.byId('greeter').onclick = function()
    {
    	cometd.publish('/service/hello', 'Hello, World');
    	cometd.publish('/hello/world', 'Hello, World from da Button');
    };
    dom.byId('jira111').onmousedown = function()
    {
    	var styleLeft = parseFloat(this.style.left);
    	var styleTop = parseFloat(this.style.top);
    	console.log(this.style.left);
    	console.log(this.style.top);
    	var mouseEvent = arguments[0];
    	var startX = mouseEvent.pageX - styleLeft;
    	var startY = mouseEvent.pageY - styleTop;
    	this.onmousemove = function(){
    		//console.log(arguments);
        	var mouseEvent = arguments[0];
        	this.style.left = (mouseEvent.pageX - startX)+"px";
        	this.style.top = (mouseEvent.pageY - startY)+"px";
        	var pos = new Object();
        	pos.x = this.style.left;
        	pos.y = this.style.top;
        	pos.id = this.id;
        	cometd.publish('/ticket/move', pos);
    	};
    	this.onmouseup = function(){
    		this.onmousemove = null;
    	};
    };
    cometd.handshake();
});
