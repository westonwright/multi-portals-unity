using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PortalChain
{
    private PortalLink baseLink;
    public PortalLink BaseLink { set => baseLink = value; }

    public int Count
    {
        get
        {
            if (baseLink == null) return 0;
            else return baseLink.LinkCount;
        }
    }

    public void Reset()
    {
        baseLink = null;
    }

    public void HandleLeaderPortal(IPortal portal)
    {
        if (baseLink == null) baseLink = new PortalLink(portal.OtherPortal, null);
        else
        {
            // one should be Portal the other should be OtherPortal
            if (baseLink.Portal == portal) baseLink = baseLink.ChildLink; // this can be null;
            else baseLink = new PortalLink(portal.OtherPortal, baseLink);
        }
    }

    public void HandleFollowerPortal(IPortal portal)
    {
        if(baseLink == null) baseLink = new PortalLink(portal, null);
        else
        {
            if (baseLink.DeepestLink.Portal.OtherPortal == portal)
            {
                if (!baseLink.RemoveDeepestLink())
                {
                    baseLink = null;
                }
            }
            else baseLink.AddLinkToBottom(new PortalLink(portal, null));
        }
    }

    public Vector3 TransformFollowerPosition(Vector3 position)
    {
        if (baseLink == null) return position;
        else return baseLink.TransformPosition(position);
    }
    public Vector3 TransformFollowerDirection(Vector3 direction)
    {
        if (baseLink == null) return direction;
        else return baseLink.TransformDirection(direction);
    }
    public float TransformFollowerScale(float scale)
    {
        if (baseLink == null) return scale;
        else return baseLink.TransformScale(scale);
    }
}

public class PortalLink
{
    private IPortal portal;
    public IPortal Portal { get => portal; }
    private PortalLink childLink;
    public PortalLink ChildLink {  get => childLink; set => childLink = value; }

    public PortalLink(IPortal portal, PortalLink childLink)
    {
        this.portal = portal;
        this.childLink = childLink;
    }

    public int LinkCount { 
        get 
        {
            if (childLink == null) return 1;
            return childLink.LinkCount + 1;
        } 
    }

    public PortalLink DeepestLink
    {
        get
        {
            if (childLink == null) return this;
            return childLink.DeepestLink;
        }
    }

    public void AddLinkToBottom(PortalLink newLink)
    {
        if(childLink == null) childLink = newLink;
        else childLink.AddLinkToBottom(newLink);
    }

    public bool RemoveDeepestLink()
    {
        if(childLink == null) return false;
        RemoveDeepest();
        return true;
    }

    private bool RemoveDeepest()
    {
        if (childLink == null) return false;
        if (!childLink.RemoveDeepest())
        {
            childLink = null;
        }
        return true;
    }

    public Vector3 TransformPosition(Vector3 position)
    {
        position = portal.TeleportPosition(position);
        return childLink == null ? position : childLink.TransformPosition(position);
    }
    public Vector3 TransformDirection(Vector3 direction)
    {
        direction = portal.TeleportDirection(direction);
        return childLink == null ? direction : childLink.TransformDirection(direction);
    }
    public float TransformScale(float scale)
    {
        scale = portal.TeleportScale(scale);
        return childLink == null ? scale : childLink.TransformScale(scale);
    }
}
