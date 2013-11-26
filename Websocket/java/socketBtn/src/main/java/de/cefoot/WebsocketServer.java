package de.cefoot;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import javax.inject.Inject;

import org.cometd.annotation.Listener;
import org.cometd.annotation.Service;
import org.cometd.annotation.Session;
import org.cometd.bayeux.server.BayeuxServer;
import org.cometd.bayeux.server.ConfigurableServerChannel;
import org.cometd.bayeux.server.LocalSession;
import org.cometd.bayeux.server.ServerChannel;
import org.cometd.bayeux.server.ServerMessage;
import org.cometd.bayeux.server.ServerSession;

@Service
public class WebsocketServer {
	@Inject
	private BayeuxServer bayeuxServer;
	@Session
	private LocalSession sender;
	
	HashMap<String, Object> curRotation = null;
	
	List<Object> drawingPositions = new ArrayList<>();
	
	public WebsocketServer(){
		curRotation = new HashMap<String, Object>();
		curRotation.put("incX", 1);
		curRotation.put("incY", 1);
	}

	@Listener("/drawing/hello")
	public void processDrawingHello(ServerSession session, ServerMessage message) {
		String channelName = "/datachannel/handPosition";// Initialize the channel, making it persistent and lazy
		bayeuxServer.createIfAbsent(channelName,
				new ConfigurableServerChannel.Initializer() {
					public void configureChannel(
							ConfigurableServerChannel channel) {
						channel.setPersistent(true);
						channel.setLazy(true);
					}
				});

		// Publish to all subscribers
		ServerChannel channel = bayeuxServer.getChannel(channelName);
		for (Object curPos : drawingPositions) {
			channel.publish(sender, curPos, null);
		}
	}

	@Listener("/datachannel/handPosition")
	public void processHandPos(ServerSession session, ServerMessage message) {
		drawingPositions.add(message.getData());
		System.out.println("handPos:" + message.getData());
	}

	@Listener("/url/")
	public void processClientHello(ServerSession session, ServerMessage message) {
		System.out.println("url:" + message.getData());
	}

	@Listener("/qube/incAngle")
	public void processAngle(ServerSession session, ServerMessage message) {		
		curRotation = (HashMap<String, Object>) message.getData();	
	}

	@Listener("/qube/hello")
	public void processHello(ServerSession session, ServerMessage message) {		
		// Create the channel name using the stock symbol
		String channelName = "/qube/incAngle";// Initialize the channel, making it persistent and lazy
		bayeuxServer.createIfAbsent(channelName,
				new ConfigurableServerChannel.Initializer() {
					public void configureChannel(
							ConfigurableServerChannel channel) {
						channel.setPersistent(true);
						channel.setLazy(true);
					}
				});

		// Publish to all subscribers
		ServerChannel channel = bayeuxServer.getChannel(channelName);
		channel.publish(sender, curRotation, null);
	}		

	@Listener("/paint/")
	public void procesClientTicketMove(ServerSession session, ServerMessage message) {
		System.out.println(message.getData());
	}

}
