using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Rosgraph;


namespace  RosSharp.RosBridgeClient{
    
    public class ClockPublisher : MonoBehaviour
    {
        public float publishRate = 0.1f;  // 发布频率
        private RosSocket rosSocket;
        private string clockPublisherId;
        private float timeSinceLastPublish = 0f;

        void Start()
        {
            rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol("ws://localhost:9090"));
            clockPublisherId = rosSocket.Advertise<Clock>("clock");
        }

        void Update()
        {
            timeSinceLastPublish += Time.deltaTime;

            if (timeSinceLastPublish >= publishRate)
            {
                PublishClock();
                timeSinceLastPublish = 0f;
            }
        }

        private void PublishClock()
        {
            Clock clockMessage = new Clock
            {
                clock = new RosSharp.RosBridgeClient.MessageTypes.Std.Time
                {
                    secs = (uint)(Time.time),
                    nsecs = (uint)((Time.time % 1) * 1e9)
                }
            };

            rosSocket.Publish(clockPublisherId, clockMessage);
        }

        private void OnApplicationQuit()
        {
            rosSocket.Close();
        }
    }
}