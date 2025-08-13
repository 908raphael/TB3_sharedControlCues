using System;
using System.Collections;
using System.Collections.Generic;

using Unity.VisualScripting;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class Nav_Twist_Subscriber : UnitySubscriber<MessageTypes.Geometry.Twist>
    {
        //public RosConnector rosConnector;
        public Vector3 linearVelocity;
        public Vector3 angularVelocity;
        private bool isMessageReceived;

        public float hapticangularSpeed = 0.0f;
        public float hapticlinearSpeed = 0.0f;
        private float xForce = 0.0f;
        private float zForce = 0.0f;
        public float smoothFactor = 0.2f;
        public HapticControl hc;
        public float align;

        private double[] currentForce = { 0.0, 0.0, 0.0 };
        private double[] currentposition = { 0.0, 0.0, 0.0 };
        public float outputForce = 0;
        public float alpha;
        public double[] outputDirection = new double[2] {0.0,0.0};
        
       // [SerializeField] float forceThreadholdUp = 1.5f;
       // [SerializeField] float forceThreadholdLow = 1f; 
        [SerializeField] private float maxDeltaAngular = 0.1f;
        [SerializeField] private float maxDeltaLinear = 0.2f;
        [SerializeField] private float minForce = 0.5f;        
        [SerializeField] private float maxForce = 1.2f; 
        
        protected override void ReceiveMessage(MessageTypes.Geometry.Twist message)
        {
            linearVelocity = ToVector3(message.linear).Ros2Unity();
            angularVelocity = -ToVector3(message.angular).Ros2Unity();
            isMessageReceived = true;
        }
        
        private static Vector3 ToVector3(MessageTypes.Geometry.Vector3 geometryVector3)
        {
            return new Vector3((float)geometryVector3.x, (float)geometryVector3.y, (float)geometryVector3.z);
        }

        // Update is called once per frame
        void Update()
        {
            HapticPlugin.getPosition("Default Device", currentposition);
            //check if the message is received
            if (angularVelocity.y == 0.0f && linearVelocity.z == 0.0f)
            {
                DataManager.Instance.nav_vel_signal = false;
                hc.align = 1f;
                //hc.locked = true;
            }else
            {
                DataManager.Instance.nav_vel_signal = true;
                alpha = DataManager.Instance.alpha;
                float currentAngularSpeed = angularVelocity.y;
                float currentLinearSpeed = linearVelocity.z;

                GetHumanInput();
                Vector2 hapticDirection = new Vector2((float)(hapticlinearSpeed * 1.2), (float)(hapticangularSpeed * 1.2));
                Vector2 navDirection = new Vector2((float)(currentLinearSpeed * 1.2), (float)(currentAngularSpeed * 1.2));

                hapticDirection.Normalize();
                navDirection.Normalize();

                align = Vector2.Dot(hapticDirection, navDirection);
                align = (1f - align) / 2f;
                hc.align = align;
            }
            
            

            /*
            if (hc.lastState == "user" && DataManager.Instance.nav_vel_signal)
            {
                if (align > 0.5f)
                {
                    hc.locked = true;
                }

                else
                {
                    hc.locked = false;
                }
            }
            */
            
        }

        private void GetHumanInput()
        {
            Vector3 position = new Vector3((float)currentposition[0], (float)currentposition[1], (float)currentposition[2]);
            //Debug.Log(position);
            hapticlinearSpeed = -position[2] / hc.linearRatio;
            hapticangularSpeed = -position[0] / hc.angularRatio;
        }
        
        
        private void OptimalizeForce(float deltaLinear, float deltaAngular) 
        {

            //Debug.Log("deltaLinear: " + deltaLinear);
            //Debug.Log("deltaAngular: " + deltaAngular);
            deltaAngular = Mathf.Clamp(deltaAngular, -maxDeltaAngular, maxDeltaAngular);
            deltaLinear = Mathf.Clamp(deltaLinear, -maxDeltaLinear, maxDeltaLinear);
            
            // linear mapping of x axis 
            xForce = Mathf.Lerp(
                minForce, 
                maxForce, 
                (deltaAngular + maxDeltaAngular) / (2 * maxDeltaAngular) // nomalize deltaAngular to [0, 1]
            );
            // linear mapping of z axis
            zForce = Mathf.Lerp(
                minForce, 
                maxForce, 
                (deltaLinear + maxDeltaLinear) / ( 2 * maxDeltaLinear) // nomalize deltaLinear to [0, 1]
            );
        }

        /*
        private void SetGuidanceForce(float xForce, float zForce, float alpha) 
        {
            if (!DataManager.Instance.isFullControl && DataManager.Instance.taskStart)
            {
                
                float forward_backward = - linearVelocity.z;
                float Left_Right = - angularVelocity.y;
                
                // set force direction by angular and linear velocity
                double[] forceDirection = new double[3] {Left_Right, 0.0, forward_backward};

                //Debug.Log("Force Direction: " + forceDirection[0]);
                //Debug.Log("Force Direction2: " + forceDirection[2]);

                float force = new Vector3(xForce, 0, zForce).magnitude;

                outputForce = force;
                outputDirection[0] = forceDirection[0];
                outputDirection[1] = forceDirection[2];
        



            }

        }
        */

     
    }
}

