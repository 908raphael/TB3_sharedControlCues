using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;

public class ClearCostmapServiceClient : MonoBehaviour
{
    private RosSocket rosSocket;

    private string serviceName = "/move_base/clear_costmaps";

    private void Start()
    {
        rosSocket = GetComponent<RosConnector>().RosSocket;

        EmptyRequest request = new EmptyRequest();

        // call service
        rosSocket.CallService<EmptyRequest, EmptyResponse>(serviceName, ResponseCallback, request);
    }

    // callback function
    private void ResponseCallback(EmptyResponse response)
    {
        Debug.Log("Costmaps cleared.");
    }
}

