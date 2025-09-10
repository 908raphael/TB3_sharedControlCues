using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

namespace RosSharp.RosBridgeClient
{
    public class Intercept_Subscriber : UnitySubscriber<MessageTypes.Std.Float32>
    {
        //public RosConnector rosConnector;

        public bool intercept;
        public float inter;
        public GameObject text;


        protected override void ReceiveMessage(MessageTypes.Std.Float32 message)
        {
            inter = message.data;
           
        }

        // Update is called once per frame
        void Update()
        {
            intercept = true;
            if(inter == 1f) 
            {
               intercept = false;
                    
            }
            text.SetActive(intercept);
        }

    }
}

