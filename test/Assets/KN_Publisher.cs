using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class KN_Publisher : UnityPublisher<MessageTypes.Std.Float32>
    {
        public float KN;
        private MessageTypes.Std.Float32 message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Float32();
            message.data = KN;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.data = KN;
            Publish(message);
        }

    }

}
