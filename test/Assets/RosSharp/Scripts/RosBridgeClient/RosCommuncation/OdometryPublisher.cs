using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class OdometryPublisher : UnityPublisher<MessageTypes.Nav.Odometry>
    {
        public Transform PublishedTransform;     // 机器人在Unity中的Transform
        public Rigidbody PublishedRigidbody;     // 机器人在Unity中的刚体组件

        private MessageTypes.Nav.Odometry message;
        private Vector3 previousPosition;
        private Quaternion previousRotation;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
            previousPosition = PublishedTransform.localPosition;
            previousRotation = PublishedTransform.localRotation;
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Nav.Odometry();
            message.header = new MessageTypes.Std.Header();
            message.pose = new MessageTypes.Geometry.PoseWithCovariance();
            message.twist = new MessageTypes.Geometry.TwistWithCovariance();
            message.pose.pose = new MessageTypes.Geometry.Pose();
            message.pose.covariance = new double[36]; // 默认协方差
            message.twist.twist = new MessageTypes.Geometry.Twist();
            message.twist.covariance = new double[36]; // 默认协方差
        }

        private void UpdateMessage()
        {
            // 获取当前位姿和速度
            message.header.Update();
            message.header.frame_id = "odom"; // 参考坐标系
            message.child_frame_id = "base_footprint"; // 机器人坐标系

            // 更新机器人在世界中的位姿,delete Unity2Ros()
            message.pose.pose.position = GetGeometryPoint(PublishedTransform.localPosition.Unity2Ros());
            message.pose.pose.orientation = GetGeometryQuaternion(PublishedTransform.localRotation.Unity2Ros());

            // 更新线速度和角速度,delete Unity2Ros()
            Vector3 linearVelocity = PublishedRigidbody.velocity.Unity2Ros();
            Vector3 angularVelocity = PublishedRigidbody.angularVelocity.Unity2Ros();

            message.twist.twist.linear = GetGeometryVector3(linearVelocity);
            message.twist.twist.angular = GetGeometryVector3(angularVelocity);

            Publish(message);
        }

        private static MessageTypes.Geometry.Point GetGeometryPoint(Vector3 position)
        {
            MessageTypes.Geometry.Point point = new MessageTypes.Geometry.Point();
            point.x = position.x;
            point.y = position.y;
            point.z = position.z;
            return point;
        }

        private static MessageTypes.Geometry.Quaternion GetGeometryQuaternion(Quaternion rotation)
        {
            MessageTypes.Geometry.Quaternion quaternion = new MessageTypes.Geometry.Quaternion();
            quaternion.x = rotation.x;
            quaternion.y = rotation.y;
            quaternion.z = rotation.z;
            quaternion.w = rotation.w;
            return quaternion;
        }

        private static MessageTypes.Geometry.Vector3 GetGeometryVector3(Vector3 vector3)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = vector3.x;
            geometryVector3.y = vector3.y;
            geometryVector3.z = vector3.z;
            return geometryVector3;
        }
    }
}
