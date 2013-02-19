package de.cefoot.kinect.canban.ticketMover;

import java.util.HashMap;
import java.util.Map;
import java.util.Map.Entry;

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
public class TicketMoverServer {
	@Inject
	private BayeuxServer bayeuxServer;
	@Session
	private LocalSession sender;

	@Listener("/service/hello")
	public void processClientHello(ServerSession session, ServerMessage message) {
		sendTickets();
	}

	Map<String, Map<String, Object>> tickets = new HashMap<String, Map<String, Object>>();

	@Listener("/ticket/move")
	public void procesClientTicketMove(ServerSession session, ServerMessage message) {
		Map<String, Object> dataMap = message.getDataAsMap();
		String id = dataMap.get("id").toString();
		if (!tickets.containsKey(id)) {
			return;
		}
		((Map<String,Object>)tickets.get(id).get("style")).put("left", dataMap.get("x"));
		((Map<String,Object>)tickets.get(id).get("style")).put("top", dataMap.get("y"));
	}

	@Listener("/ticket/add")
	public void procesClientTicketAdd(ServerSession session, ServerMessage message) {
		Map<String, Object> dataMap = message.getDataAsMap();
		tickets.put(dataMap.get("id").toString(), dataMap);
	}

	private void sendTickets() {
		// Create the channel name using the stock symbol
		String channelName = "/ticket/add";

		// Initialize the channel, making it persistent and lazy
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
		Map<String, Map<String, Object>> curTicks = tickets;
		for (Entry<String, Map<String, Object>> ticket : curTicks.entrySet()) {
			channel.publish(sender, ticket.getValue(), null);
		}
	}

}
