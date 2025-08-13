using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class cmd_vel_Publisher : UnityPublisher<MessageTypes.Geometry.Twist>
    {
        public float cmd;
        private MessageTypes.Geometry.Twist message;
        public GameObject anchor;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.Twist();
            initial_cmd_vel(message);
        }

        void FixedUpdate()
        {
            UpdateMessage();
        }

        private void UpdateMessage()
        {
            //message.linear = GetGeometryVector3((double)anchor.transform.localPosition[2]); 
            //message.angular = GetGeometryRotate(angularValue);
            //Publish(message);
            Debug.Log(GetGeometryVector3((double) (anchor.transform.localPosition[2] * 1000 )).x);
        }

        private static void initial_cmd_vel(MessageTypes.Geometry.Twist message)
        {
            MessageTypes.Geometry.Vector3 init_vector3 = new MessageTypes.Geometry.Vector3();
            init_vector3.x = 0;
            init_vector3.y = 0;
            init_vector3.z = 0;
            message.linear = init_vector3;
            message.angular = init_vector3;
        }


        private static MessageTypes.Geometry.Vector3 GetGeometryVector3(double vec)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = (-vec / 100);
            geometryVector3.y = 0;
            geometryVector3.z = 0;
            return geometryVector3;
        }

        private static MessageTypes.Geometry.Vector3 GetGeometryRotate(double vec)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = 0;
            geometryVector3.y = 0;
            geometryVector3.z = vec / 50;
            return geometryVector3;
        }


    }

}
