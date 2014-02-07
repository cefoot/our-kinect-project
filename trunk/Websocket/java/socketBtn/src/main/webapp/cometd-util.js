require(
		[ 'dojox/cometd', 'dojo/dom', 'dojo/dom-construct', 'dojo/domReady!' ],
		function(cometd, dom, doc) {
			globalCometd=cometd;
			cometd.configure({
				url : location.protocol + '//' + location.host
						+ '/socketBtn',
				logLevel : 'warn'
			});
			cometd.addListener(
							'/meta/handshake',
							function(message) {
								if (message.successful) {
									console.info('cometd:success');
									//lieber nicht
//									globalCometd.subscribe('/url/', function(message){
//										ifrm = document.createElement("IFRAME"); 
//										ifrm.setAttribute("src", message.data); 
//										ifrm.style.width = "100%"; 
//										ifrm.style.height = "100%"; 
//										ifrm.style.position = "absolute"; 
//										ifrm.style.top = 0+"px"; 
//										document.body.appendChild(ifrm); 
//								    });
//									globalCometd.subscribe('/url/rem', function(message){
//										document.body.removeChild(document.getElementsByTagName("iframe")[0]);
//								    });
								} else {
									console.info('cometd:failure');
								}
							});
			cometd.handshake();
		});
function register(channel, callback){
	globalCometd.subscribe(channel, function(message){
		callback(message.data);
    });
};
function send(channel, message){
	globalCometd.publish(channel, message);
};
