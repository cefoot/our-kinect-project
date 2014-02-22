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
