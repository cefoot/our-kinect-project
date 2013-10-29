require(
		[ 'dojox/cometd', 'dojo/dom', 'dojo/dom-construct', 'dojo/domReady!' ],
		function(cometd, dom, doc) {
			cometd.configure({
				url : location.protocol + '//' + location.host
						+ config.contextPath + '/socketBtn',
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
			cometd.handshake();
		});
