package de.cefoot;

import java.util.HashMap;
import java.util.Map;

import javax.inject.Inject;

import org.cometd.annotation.Listener;
import org.cometd.annotation.Service;
import org.cometd.annotation.Session;
import org.cometd.bayeux.server.BayeuxServer;
import org.cometd.bayeux.server.LocalSession;
import org.cometd.bayeux.server.ServerMessage;
import org.cometd.bayeux.server.ServerSession;

@Service
public class WebsocketServer {
	@Inject
	private BayeuxServer bayeuxServer;
	@Session
	private LocalSession sender;

	@Listener("/service/hello")
	public void processClientHello(ServerSession session, ServerMessage message) {
		System.out.println("clientConnected");
	}

	Map<String, Map<String, Object>> tickets = new HashMap<String, Map<String, Object>>();

	@Listener("/paint/")
	public void procesClientTicketMove(ServerSession session,
			ServerMessage message) {
		System.out.println(message.getData());
	}

	////don't care 
//	@Listener("/handyMotion/")
//	public void procesClientMotion(ServerSession session,
//			ServerMessage message) {
//		System.out.println(message.getData());
//	}

}
