using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportConnection
{
    // connection follower's position will be
    // set based on the connection leader
    private ITeleportable connectionLeader;
    private ITeleportable connectionFollower;

    private PortalChain portalChain = new PortalChain();
    public PortalChain PortalChain { get => portalChain; } 

    public TeleportConnection(ITeleportable connectionLeader)
    {
        this.connectionLeader = connectionLeader;

        this.connectionLeader.OnTeleported += LeaderTeleported;
    }

    public void DisconnectFollower()
    {
        if (connectionFollower != null)
        {
            connectionFollower.OnTeleported -= FollowerTeleported;
            portalChain.Reset();
        }
    }
    public void SetFollower(ITeleportable follower, PortalLink portalLink)
    {
        if (follower != null)
        {
            DisconnectFollower();
            connectionFollower = follower;
            connectionFollower.OnTeleported += FollowerTeleported;
            portalChain.BaseLink = portalLink;
        }
    }

    public void DestroyConnection()
    {
        connectionLeader.OnTeleported -= LeaderTeleported;
        if(connectionFollower != null)
            connectionFollower.OnTeleported -= FollowerTeleported;
    }

    public void Reset()
    {
        portalChain.Reset();
    }

    private void LeaderTeleported(object teleportable, TeleportEventArgs teleportEventArgs)
    {
        portalChain.HandleLeaderPortal(teleportEventArgs.portal);
    }
    
    private void FollowerTeleported(object teleportable, TeleportEventArgs teleportEventArgs)
    {
        portalChain.HandleFollowerPortal(teleportEventArgs.portal);
    }
}
