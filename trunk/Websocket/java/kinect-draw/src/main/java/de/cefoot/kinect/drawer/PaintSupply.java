package de.cefoot.kinect.drawer;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;

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

import de.cefoot.kinect.drawer.data.Color;
import de.cefoot.kinect.drawer.data.Position;

@Service
public class PaintSupply {
	@Inject
	private BayeuxServer bayeuxServer;
	@Session
	private LocalSession sender;

	private ServerChannel getChannel(String channelName) {
		MarkedReference<ServerChannel> channelRef = bayeuxServer
				.createChannelIfAbsent(channelName,
						new ConfigurableServerChannel.Initializer() {
							public void configureChannel(
									ConfigurableServerChannel channel) {
								channel.setPersistent(true);
								channel.setLazy(true);
							}
						});

		ServerChannel channel = channelRef.getReference();
		return channel;
	}

	@Listener("/paint/")
	public void procesClientTicketMove(ServerSession session,
			ServerMessage message) {
		if ("start".equals(message.getData())) {
			curColor = Color.getRandomColor();
			positions.put(curColor, new ArrayList<Position>());
		} else if ("stop".equals(message.getData())) {
			curColor = Color.getRandomColor();
		}
	}
	
	private static boolean sendingClear = false;
	
	@Listener("/datachannel/clear")
	public void clear(ServerSession session, ServerMessage message) {
		if (!sendingClear){
			positions.clear();
		}
	}

	@Listener("/client/hello")
	public void clienHello(ServerSession session, ServerMessage message) {
		//momentan wird an alle gesendet, daher vorher clear
		sendingClear = true;
		getChannel("/datachannel/clear").publish(sender, "clear!", null);
		sendingClear = false;
		for (Entry<Color, List<Position>> entry : positions.entrySet()) {
			for (Position pos : entry.getValue()) {
				getChannel("/drawing/newPos").publish(sender,
						curPositionData(entry.getKey() ,pos), null);
			}
		}
	}

	Color curColor = Color.getRandomColor();

	Map<Color, List<Position>> positions = new HashMap<>();

	/**
	 * erhält positionen von der Kinect hebt diese auf und sendet sie an die
	 * clients
	 * 
	 * @param session
	 * @param message
	 */
	@Listener("/datachannel/drawPosition")
	public void processHandPos(ServerSession session, ServerMessage message) {
		try {
			if (!positions.containsKey(curColor))
				return;
			Position pos = new Position((long) message.getDataAsMap().get("X"),// X
					(long) message.getDataAsMap().get("Y"),// Y
					(long) message.getDataAsMap().get("Z"));// Z
			positions.get(curColor).add(pos);
			getChannel("/drawing/newPos").publish(sender, curPositionData(curColor, pos),
					null);
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	private Object curPositionData(Color color, Position pos) {
		Map<String, Object> data = new HashMap<>();
		data.putAll(pos);
		data.putAll(color);
		return data;
	}

}
