using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalFade : MonoBehaviour
{
    public GameObject otherPortalLayer;

    Renderer portalRenderer { get { return GetComponent<Renderer>(); } }
    float fadeSpeed = 1;

    public void FadeIn()
    {
        StopAllCoroutines();
        StartCoroutine(FI());
    }

    IEnumerator FI()
    {
        float fadeProgress = 0;
        Color startColor = new Color(1, 1, 1, 0);
        Color endColor = new Color(1, 1, 1, 1);
        while (fadeProgress <= 1)
        {
            fadeProgress += fadeSpeed * Time.deltaTime;
            startColor = Color.Lerp(startColor, endColor, fadeProgress);
            portalRenderer.material.color = startColor;
            yield return null;
        }
        yield break;
    }

    public void TurnOff()
    {
        otherPortalLayer.GetComponent<MeshRenderer>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
    }

    public void TurnOn()
    {
        otherPortalLayer.GetComponent<MeshRenderer>().enabled = true;
        GetComponent<MeshRenderer>().enabled = true;
    }
}
