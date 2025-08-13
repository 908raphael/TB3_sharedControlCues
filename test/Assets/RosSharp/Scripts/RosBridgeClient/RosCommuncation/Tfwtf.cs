using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Tf2;
using RosSharp.RosBridgeClient.MessageTypes.Std;
using RosSharpGeometry = RosSharp.RosBridgeClient.MessageTypes.Geometry; // 给RosSharp的命名空间加个别名

namespace RosSharp.RosBridgeClient
{
    public class Tfwtf : MonoBehaviour
    {
        public UnityEngine.Transform PublishedTransform;
        public string childFrameId = "base_link";  
        public string parentFrameId = "map";    
        public float publishRate = 0.1f;  // 发布频率

        private RosSocket rosSocket;
        private string tfPublisherId;
        private float timeSinceLastPublish = 0f;

        void Start()
        {
            rosSocket = new RosSocket(new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol("ws://localhost:9090"));
            tfPublisherId = rosSocket.Advertise<TFMessage>("/tf");  // 广播到 /tf 主题
        }

        void Update()
        {
            timeSinceLastPublish += UnityEngine.Time.deltaTime;

            if (timeSinceLastPublish >= publishRate)
            {
                PublishTf();
                timeSinceLastPublish = 0f;
            }
        }

        private void PublishTf()
        {
            // 创建 TransformStamped 消息
            RosSharpGeometry.TransformStamped transformStamped = new RosSharpGeometry.TransformStamped
            {
                header = new Header
                {
                    frame_id = parentFrameId,
                    stamp = GetCurrentRosTime()
                },
                child_frame_id = childFrameId,
                transform = new RosSharpGeometry.Transform
                {
                    translation = new RosSharpGeometry.Vector3
                    {
                        x = PublishedTransform.position.x,
                        y = PublishedTransform.position.y,
                        z = PublishedTransform.position.z
                    },
                    rotation = new RosSharpGeometry.Quaternion
                    {
                        x = PublishedTransform.rotation.x,
                        y = PublishedTransform.rotation.y,
                        z = PublishedTransform.rotation.z,
                        w = PublishedTransform.rotation.w
                    }
                }
            };

            // 包装成 TFMessage 消息
            TFMessage tfMessage = new TFMessage
            {
                transforms = new RosSharpGeometry.TransformStamped[] { transformStamped }
            };

            // 发布消息
            rosSocket.Publish(tfPublisherId, tfMessage);
        }

        private MessageTypes.Std.Time GetCurrentRosTime()
        {
            float time = UnityEngine.Time.time;
            return new MessageTypes.Std.Time
            {
                secs = (uint)(time),
                nsecs = (uint)((time % 1) * 1e9)
            };
        }

        private void OnApplicationQuit()
        {
            rosSocket.Close();
        }
    }
}
