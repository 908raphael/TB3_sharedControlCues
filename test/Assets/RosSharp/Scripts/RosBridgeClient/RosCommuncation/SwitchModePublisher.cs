using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class SwitchModePublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        private bool swithchMode;
        private MessageTypes.Std.Bool message;

         protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            swithchMode = false;
            message = new MessageTypes.Std.Bool();
            message.data = swithchMode;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                swithchMode = !swithchMode;
            }
            message.data = swithchMode;
            Publish(message);
        }

    }
}
