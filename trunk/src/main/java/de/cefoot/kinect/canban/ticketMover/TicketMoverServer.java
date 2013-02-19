package de.cefoot.kinect.canban.ticketMover;

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
    public void processClientHello(ServerSession session, ServerMessage message)
    {
        System.out.printf("Received greeting '%s' from remote client %s%n", message.getData(), session.getId());
        sendBack();
    }

	private void sendBack() {
		System.out.println("send");
        // Create the channel name using the stock symbol
        String channelName = "/hello/world";

        // Initialize the channel, making it persistent and lazy
        bayeuxServer.createIfAbsent(channelName, new ConfigurableServerChannel.Initializer()
        {
            public void configureChannel(ConfigurableServerChannel channel)
            {
                channel.setPersistent(true);
                channel.setLazy(true);
            }
        });

        // Publish to all subscribers
        ServerChannel channel = bayeuxServer.getChannel(channelName);
        channel.publish(sender, "Hallo Zurück", null);
        System.out.println("aaaand gone");
	}

}
