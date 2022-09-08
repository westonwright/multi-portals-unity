using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface ITeleportable
{
    public event EventHandler<TeleportEventArgs> OnTeleported;
}

public class TeleportEventArgs : EventArgs
{
    public IPortal portal;
    public TeleportEventArgs(IPortal portal)
    {
        this.portal = portal;
    }
}
