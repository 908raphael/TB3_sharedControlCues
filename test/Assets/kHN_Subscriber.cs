using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

namespace RosSharp.RosBridgeClient
{
    public class kHN_Subscriber : UnitySubscriber<MessageTypes.Std.Float32>
    {
        //public RosConnector rosConnector;

        public float kHN;
       


        protected override void ReceiveMessage(MessageTypes.Std.Float32 message)
        {
            kHN = message.data;

        }

        // Update is called once per frame
        void Update()
        {
            
        }

    }
}

