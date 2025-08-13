using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class NIP_Subscriber : UnitySubscriber<MessageTypes.Std.Float32MultiArray>
    {
        //public RosConnector rosConnector;

        public double[] NIP = { 0.0, 0.0, 0.0 };
        private double[] currentposition = { 0.0, 0.0, 0.0 };
        private float[] temp = { 0.0f, 0.0f, 0.0f };
        public GameObject anchor;
        private Vector3 targetPosition;
        public float anchorSpeed;


        protected override void ReceiveMessage(MessageTypes.Std.Float32MultiArray message)
        {
            temp = message.data;
            NIP[0] = temp[0];
            NIP[2] = temp[2];
            //Debug.Log("0: " + NIP[0]);
            //Debug.Log("2: "+ NIP[2]);
        }

        // Update is called once per frame
        void Update()
        {
            targetPosition = new Vector3(temp[0] / 1000f, 0, temp[2] / 1000f);
            //HapticPlugin.setSpringValues("Default Device", NIP, 0.005);
            //HapticPlugin.setAnchorPosition("Default Device", NIP);
            //Debug.Log("0: " + temp[0] / 100);
            //Debug.Log("2: " + temp[2] / 100);
            //Debug.Log("TargetPosition: " + targetPosition);
            if(DataManager.Instance.isWithGuidanceForce)
            {
                anchor.transform.localPosition = Vector3.Lerp(anchor.transform.localPosition, targetPosition, anchorSpeed);
            }
            else
            {
                anchor.transform.localPosition = new Vector3(0, 0, 0);
            }
            //anchor.transform.localPosition =


        }

    }
}

