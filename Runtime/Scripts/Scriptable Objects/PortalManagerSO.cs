using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//[CreateAssetMenu(fileName = "PortalManagerSO", menuName = "Portals/PortalManagerSO")]
public class PortalManagerSO : ScriptableObject, IList<IPortal>
{
    private List<PortalPair> portalPairs = new List<PortalPair>();

    // this is if one of the portals moves or something. Not sure what to do here yet though
    public void UpdatePortalPair(PortalPair portalPair)
    {
    }

    public void AddPortalPair(PortalPair portalPair)
    {
        if (portalPairs.Contains(portalPair))
            return;

        portalPairs.Add(portalPair);
        IPortal leftPortal = new StandardPortal(portalPair.LeftHolder.PortalPlane);
        IPortal rightPortal = new StandardPortal(portalPair.RightHolder.PortalPlane);
        leftPortal.SetOtherPortal(rightPortal);
        rightPortal.SetOtherPortal(leftPortal);
        portals.Add(leftPortal);
        portals.Add(rightPortal);
    }

    public void RemovePortalPair(PortalPair portalPair)
    {
        if (!portalPairs.Contains(portalPair))
            return;

        portalPairs.Remove(portalPair);
        PortalPlane leftPlane = portalPair.LeftHolder.PortalPlane;
        int leftIndex = portals.FindIndex(x => x.Transform == leftPlane);
        if(leftIndex >= 0)
        {
            portals.RemoveAt(leftIndex);
        }
        PortalPlane rightPlane = portalPair.RightHolder.PortalPlane;
        int rightIndex = portals.FindIndex(x => x.Transform == rightPlane);
        if (rightIndex >= 0)
        {
            portals.RemoveAt(rightIndex);
        }
    }

    private PortalPlane[] portalPlanes;
    private List<IPortal> portals = new List<IPortal>();

    public int Count => portals.Count;

    public bool IsReadOnly => true;

    public IPortal this[int index] { get => portals[index]; set => portals[index] = value; }

    public IEnumerator<IPortal> GetEnumerator()
    {
        foreach(IPortal p in portals)
        {
            yield return p;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int IndexOf(IPortal item) => portals.IndexOf(item);

    public void Insert(int index, IPortal item) => portals.Insert(index, item);

    public void RemoveAt(int index) => portals.RemoveAt(index);

    public void Add(IPortal item) => portals.Add(item);

    public void Clear() => portals.Clear();

    public bool Contains(IPortal item) => portals.Contains(item);

    public void CopyTo(IPortal[] array, int arrayIndex) => portals.CopyTo(array, arrayIndex);

    public bool Remove(IPortal item) => portals.Remove(item);
}
