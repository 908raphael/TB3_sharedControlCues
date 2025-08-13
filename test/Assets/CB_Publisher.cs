using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace RosSharp.RosBridgeClient
{
    public class CB_Publisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        public bool CB;
        private MessageTypes.Std.Bool message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Bool();
            message.data = CB;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.data = CB;
            Publish(message);
        }


    }

}
