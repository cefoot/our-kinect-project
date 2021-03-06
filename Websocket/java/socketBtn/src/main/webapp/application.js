require(
		[ 'dojox/cometd', 'dojo/dom', 'dojo/dom-construct', 'dojo/domReady!' ],
		function(cometd, dom, doc) {
			cometd.configure({
				url : location.protocol + '//' + location.host
						+ '/socketBtn',
				logLevel : 'warn'
			});
			cometd.addListener(
							'/meta/handshake',
							function(message) {
								if (message.successful) {
									dom.byId('status').innerHTML = '<div>CometD handshake successful</div>';
				    	            cometd.subscribe('/paint/', function(message){
				    	            	dom.byId('status').innerHTML = '<div>Painting:'+message.data+'</div>';
				    	            });
									cometd.publish('/service/hello',
											'Hello, World');
								} else {
									dom.byId('status').innerHTML = '<div>CometD handshake failed</div>';
								}
							});
			var isPaint = false;
			dom.byId('btn').onclick = function() {
				if (!isPaint) {
					isPaint=true;
					cometd.publish('/paint/', 'start');
				} else {
					isPaint=false;
					cometd.publish('/paint/', 'stop');
				}
			};
			function handleMotionEvent(event) {
			    var x = Math.round(event.accelerationIncludingGravity.x*100)/100;
			    var y = Math.round(event.accelerationIncludingGravity.y*100)/100;
			    var z = Math.round(event.accelerationIncludingGravity.z*100)/100;
				cometd.publish('/handyMotion/', x+':'+y+':'+z);
			    // Do something awesome.
			};

			window.addEventListener("devicemotion", handleMotionEvent, true);
			cometd.handshake();
		});
