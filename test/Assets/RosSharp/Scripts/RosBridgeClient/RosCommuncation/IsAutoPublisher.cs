using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
     public class IsAutoPublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        public bool autoOperation;
        private MessageTypes.Std.Bool message;

         protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Bool();
            message.data = autoOperation;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.data = autoOperation;
            Publish(message);
        }

    }

} 
   