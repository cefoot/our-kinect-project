package de.cefoot;

import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Stack;

import javax.inject.Inject;

import org.cometd.annotation.Listener;
import org.cometd.annotation.Service;
import org.cometd.annotation.Session;
import org.cometd.bayeux.MarkedReference;
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

	public WebsocketServer() {
		curRotation = new HashMap<String, Object>();
		curRotation.put("incX", 1);
		curRotation.put("incY", 1);
	}

	@Listener("/drawing/hello")
	public void processDrawingHello(ServerSession session, ServerMessage message) {
		String channelName = "/datachannel/positionData";
		// Publish to all subscribers
		ServerChannel channel = getChannel(channelName);
		for (Object drawing : drawings) {
			channel.publish(sender, drawing, null);
		}
	}

	@Listener("/datachannel/clear")
	public void processClear(ServerSession session, ServerMessage message) {
		drawings.clear();
		newDrawing = true;
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
		String channelName = "/qube/incAngle";// Initialize the channel, making
												// it persistent and lazy
		ServerChannel channel = getChannel(channelName);
		// Publish to all subscribers
		channel.publish(sender, curRotation, null);
	}

	private ServerChannel getChannel(String channelName) {
		MarkedReference<ServerChannel> channelRef = bayeuxServer.createChannelIfAbsent(channelName,
				new ConfigurableServerChannel.Initializer() {
					public void configureChannel(ConfigurableServerChannel channel) {
						channel.setPersistent(true);
						channel.setLazy(true);
					}
				});
		
		ServerChannel channel = channelRef.getReference();
		return channel;
	}

	Map<String, Double> drawClr = new HashMap<>();
	Stack<Map<String, Object>> drawings = new Stack<>();
	boolean newDrawing = true;

	@Listener("/datachannel/drawPosition")
	public void processHandPos(ServerSession session, ServerMessage message) {
		try {
			long x = (long) message.getDataAsMap().get("X");
			long y = (long) message.getDataAsMap().get("Y");
			long z = (long) message.getDataAsMap().get("Z");
			addPosition(x, y, z);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private void addPosition(long x, long y, long z) {
		if (newDrawing) {
			drawings.push(new HashMap<String, Object>());
			newDrawing = false;
			drawings.peek().put("color", new HashMap<String, Double>(drawClr));
		}
		List<Map<String, Long>> curDraw = getCurrentPositionList();
		Map<String, Long> position = new HashMap<>();
		position.put("X", x);
		position.put("Y", y);
		position.put("Z", z);
		curDraw.add(position);
	}

	private List<Map<String, Long>> getCurrentPositionList() {
		if (!drawings.peek().containsKey("positions")) {
			drawings.peek().put("positions", new LinkedList<Map<String, Long>>());
		}
		List<Map<String, Long>> linkedList = (List<Map<String, Long>>) drawings.peek().get("positions");
		return linkedList;
	}

	@Listener("/paint/")
	public void procesClientTicketMove(ServerSession session,
			ServerMessage message) {
		if ("start".equals(message.getData())) {
			drawClr.put("r", Math.random());
			drawClr.put("g", Math.random());
			drawClr.put("b", Math.random());
			getChannel("/paint/clr").publish(sender, drawClr, null);
		} else if ("stop".equals(message.getData())) {
			newDrawing = true;
		}
	}

}
