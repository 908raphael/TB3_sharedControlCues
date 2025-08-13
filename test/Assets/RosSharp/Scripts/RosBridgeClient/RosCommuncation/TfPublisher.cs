
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


using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class TfPublisher : UnityPublisher<MessageTypes.Tf2.TFMessage>
    {
        public Transform PublishedTransform;
        public string childFrameId = "base_link";  
        public string parentFrameId = "map";    
        public float publishRate = 0.1f;

        private MessageTypes.Geometry.TransformStamped message;
        private MessageTypes.Tf2.TFMessage tfMessage;

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
            // Initialize the TransformStamped message
            message = new MessageTypes.Geometry.TransformStamped
            {
                header = new MessageTypes.Std.Header { frame_id = parentFrameId },
                child_frame_id = childFrameId,
                transform = new MessageTypes.Geometry.Transform
                {
                    translation = new MessageTypes.Geometry.Vector3(),
                    rotation = new MessageTypes.Geometry.Quaternion(),
                }
            };

            // Initialize the TFMessage with an empty array of TransformStamped
            tfMessage = new MessageTypes.Tf2.TFMessage
            {
                transforms = new MessageTypes.Geometry.TransformStamped[1]  // We are going to publish one transform for now
            };
        }

        private void UpdateMessage()
        {
            // Update the header timestamp
            message.header.Update();

            // Get the current transform values from the Unity object
            Vector3 translation = PublishedTransform.position.Unity2Ros();
            Quaternion rotation = PublishedTransform.rotation.Unity2Ros();

            // Update the translation and rotation in the ROS message
            GetGeometryVector3(translation, message.transform.translation);
            GetGeometryQuaternion(rotation, message.transform.rotation);

            // Assign the updated TransformStamped message to the TFMessage
            tfMessage.transforms[0] = message;

            // Publish the TFMessage containing the TransformStamped array
            Publish(tfMessage);
        }

        private static void GetGeometryVector3(Vector3 vector3, MessageTypes.Geometry.Vector3 geometryVector3)
        {
            geometryVector3.x = vector3.x;
            geometryVector3.y = vector3.y;
            geometryVector3.z = vector3.z;
        }

        private static void GetGeometryQuaternion(Quaternion quaternion, MessageTypes.Geometry.Quaternion geometryQuaternion)
        {
            geometryQuaternion.x = quaternion.x;
            geometryQuaternion.y = quaternion.y;
            geometryQuaternion.z = quaternion.z;
            geometryQuaternion.w = quaternion.w;
        }
    }
}
