require(['dojox/cometd', 'dojo/dom', 'dojo/dom-construct', 'dojo/domReady!'], function(cometd, dom, doc)
{
	var words = ["Lorem","ipsum","dolor","sit","amet ","consetetur","sadipscing",".","elitr ","sed","diam",".","nonumy","eirmod",",","tempor","invidunt","ut","labore","et","dolore","magna","aliquyam","erat ","sed","diam","voluptua ","At","vero","eos","et","accusam","et","justo","duo","dolores","et","ea",".","rebum ","Stet","clita","kasd","gubergren ","no","sea","takimata","sanctus","est","Lorem","ipsum","dolor",",","sit","amet ","Lorem","ipsum","dolor","sit","amet ","consetetur","sadipscing","elitr ","sed","diam","nonumy","eirmod","tempor","invidunt","ut","labore","et","dolore","magna","aliquyam","erat ","sed","diam","voluptua ","At","vero",".","eos",",","et","accusam","et","justo","duo","dolores","et","ea","rebum ","Stet","clita","kasd","gubergren ","no","sea","takimata","sanctus","est","Lorem","ipsum","dolor","sit","amet",",","."];
    var dragStart = function()
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
    var addDiv = function(divObj){
    	var div = document.createElement("div");
    	div.style.left = divObj.style.left;
    	div.style.top = divObj.style.top;
    	div.style.width = divObj.style.width;
    	div.style.height = divObj.style.height;
    	div.style.position = divObj.style.position;
    	div.style.backgroundColor = divObj.style.backgroundColor; 
    	div.onmousedown = dragStart;
    	div.id = divObj.id;
    	var tbl = document.createElement("table");
    	tbl.style.width = "100%";
    	tbl.style.height = "100%";
    	//erste reihe
    	var tr = document.createElement("tr");
    	tr.style.height = "33%";
    	tbl.appendChild(tr);
    	var td = document.createElement("td");
    	td.innerHTML = divObj.id;
    	tr.appendChild(td);
    	tr.appendChild(document.createElement("td"));
    	td = document.createElement("td");
    	td.innerHTML = divObj.bug.project;
    	tr.appendChild(td);
    	//zweite reihe
    	tr = document.createElement("tr");
    	tr.style.height = "67%";
    	tbl.appendChild(tr);
    	td = document.createElement("td");
    	td.colspan = 3;
    	td.style.fontSize = "10px";
    	td.innerHTML = divObj.bug.text;
    	tr.appendChild(td);
    	
    	div.appendChild(tbl);
    	document.body.appendChild(div);
    };
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
    	            cometd.subscribe('/ticket/add', function(message){
    	            	var ticket = dom.byId(message.data.id);
    	            	if(ticket == null){
    	            		addDiv(message.data);
    	            	}
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
    function getRandomColor() {
        var letters = '0123456789ABCDEF'.split('');
        var color = '#';
        for (var i = 0; i < 6; i++ ) {
            color += letters[Math.round(Math.random() * 15)];
        }
        return color;
    }
    var createText = function(){
    	var txt = "";
    	for ( var int = 0; int < 15; int++) {
			txt += words[Math.round(Math.random() * words.length)] + " ";
		}
    	return txt;
    };
    dom.byId('ticketAdder').onclick = function()
    {
    	var sendDiv = new Object();
    	sendDiv.style = new Object();
    	sendDiv.style.left = "250px";
    	sendDiv.style.top = "250px";
    	sendDiv.style.width = "150px";
    	sendDiv.style.height = "150px";
    	sendDiv.style.position = "absolute";
    	sendDiv.style.backgroundColor = getRandomColor();
    	sendDiv.bug = new Object();
    	sendDiv.bug.text = createText();
    	sendDiv.bug.project = words[Math.round(Math.random() * words.length)];
    	sendDiv.id = "jira" + Math.round(Math.random() * 155);
    	addDiv(sendDiv);
    	cometd.publish('/ticket/add', sendDiv);
    };
    cometd.handshake();
});
