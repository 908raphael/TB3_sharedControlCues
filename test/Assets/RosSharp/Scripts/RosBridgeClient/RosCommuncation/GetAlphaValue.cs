using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RosSharp.RosBridgeClient {

    public class GetAlphaValue : UnitySubscriber<MessageTypes.Std.Float32>
    {
        
        private float alphaValue;
  
        public float GetValue()
        {
            return alphaValue;
        }

        protected override void ReceiveMessage(MessageTypes.Std.Float32 message)
        {
            alphaValue = message.data;
            //Debug.Log("Received message: " + message.data);
        }
    }

}