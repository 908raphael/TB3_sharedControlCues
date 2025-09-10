using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RosSharp.RosBridgeClient
{
     public class IsTelePublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        [FormerlySerializedAs("teleOperation")] public bool sharedControl;
        private MessageTypes.Std.Bool message;

         protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Bool();
            message.data = sharedControl;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.data = sharedControl;
            Publish(message);
        }


    }

} 
   