/*
© Siemens AG, 2017-2018
Author: Dr. Martin Bischoff (martin.bischoff@siemens.com)

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at
<http://www.apache.org/licenses/LICENSE-2.0>.
Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

// Added allocation free alternatives
// UoK , 2019, Odysseas Doumas (od79@kent.ac.uk / odydoum@gmail.com)

using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class NavigationGoalPublisher : UnityPublisher<MessageTypes.Geometry.PoseStamped>
    {
        //public Transform PublishedTransform;

        //public GameObject indicator;
        public string FrameId = "Unity";

        private MessageTypes.Geometry.PoseStamped message;

        private bool sendGoal = false;
        private Vector3 goalPosition;
        private Quaternion goalRotation;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void FixedUpdate()
        {
            UpdateMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Geometry.PoseStamped
            {
                header = new MessageTypes.Std.Header()
                {
                    frame_id = FrameId
                }
            };
        }

        private void UpdateMessage()
        {
            if (sendGoal){
                message.header.Update();
                GetGeometryPoint(goalPosition.Unity2Ros(), message.pose.position);
                GetGeometryQuaternion(goalRotation.Unity2Ros(), message.pose.orientation);
                Publish(message);
                sendGoal = false;
            }
                
        }

        private static void GetGeometryPoint(Vector3 position, MessageTypes.Geometry.Point geometryPoint)
        {
            geometryPoint.x = position.x;
            geometryPoint.y = position.y;
            geometryPoint.z = position.z;
        }

        private static void GetGeometryQuaternion(Quaternion quaternion, MessageTypes.Geometry.Quaternion geometryQuaternion)
        {
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
        }

        public void SetGoalActive(Vector3 position, Quaternion roataion)
        {
            goalPosition = position;
            goalRotation = roataion;
            sendGoal = true;
            //GameObject indicator = GameObject.Find("GoalIndicator");
            //indicator.SetActive(true);
        }

    }
}
