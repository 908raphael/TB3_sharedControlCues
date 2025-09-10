using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;

public class EnlargedMapController : MonoBehaviour, IPointerClickHandler
{
    public GameObject markerPrefab; // 标记点的预制体
    
    public GameObject indicator;

    private TwistSubscriber twistSubscriber;

    void Start()
    {
        GameObject twistScript = GameObject.Find("RosConnector");
        TwistSubscriber twistSubscriber = twistScript.GetComponent<TwistSubscriber>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       //twistSubscriber.enabled = false;
    }
}
