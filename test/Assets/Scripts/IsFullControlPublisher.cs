namespace RosSharp.RosBridgeClient
{
    public class IsFullControlPublisher : UnityPublisher<MessageTypes.Std.Bool>
    {
        public bool fullControl;
        private MessageTypes.Std.Bool message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Std.Bool();
            message.data = fullControl;
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            message.data = fullControl;
            Publish(message);
        }

    }

}