using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{
    public class HapticControl1 : UnityPublisher<MessageTypes.Geometry.Twist>
    {
        public HapticPlugin hapticPlugin; 
        public LaserScanReader laserScanReader;
        public GameObject goalManager;
        public RosConnector rosConnector;
        public GameObject Local_nav_line;
        public GameObject Human_control_line;

        public double[] position3 = {0.0,0.0,0.0};
        public double[] velocity3 = {0.0,0.0,0.0};
        public double[] jointAngles = {0.0,0.0,0.0};
        public double[] gimbalAngles = {0.0,0.0,0.0};

        //public Vector3 fv = new Vector3 ((float)0.0, (float)0.0, (float)0.0);
        public float f;

        [SerializeField] float forceThreadholdUp = 2.7f;
        [SerializeField] float forceThreadholdLow = 1f;  
        
        public double[] currentForce = {0.0,0.0,0.0};

        private MessageTypes.Geometry.Twist message;
        private bool activeState = false;  
        private bool autoControlSignal = false;


        private string lastState;
        private bool isCountdownRunning = false;
        
        
        // private Dictionary<string, object> directions;     

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }
        

        private void Update()
        {
            hapticPlugin.UpdateButtonStatus();
            FiniteStateMachine();
            UpdateMessage();
        }

        private void InitializeMessage()
        {   
            HapticPlugin.setAnchorPosition("Default Device", new double[] {0,0,0});
            message = new MessageTypes.Geometry.Twist();
            message.linear = new MessageTypes.Geometry.Vector3();
            message.angular = new MessageTypes.Geometry.Vector3();
            initial_cmd_vel(message);
            Publish(message);
        }

        private void UpdateMessage()
        {
           if (hapticPlugin != null && activeState)
            {
                HapticPlugin.getPosition("Default Device", position3);
                HapticPlugin.getVelocity("Default Device", velocity3);
                HapticPlugin.getJointAngles("Default Device", jointAngles, gimbalAngles);
                //Debug.Log(jointAngles[2]);

                var result = laserScanReader.caculateMinDistance();
                bool[] directions = result.Item1;
                float distance = result.Item2;

                HandleMovement(directions[0], new double[] { 0, 0, 20 }, jointAngles, position3[2], position3[0], false, distance);  // 前
                HandleMovement(directions[1], new double[] { 0, 0, -20 }, jointAngles, position3[2], position3[0], true, distance); // 后
                //HandleDirection(directions[2], new double[] { 40, 0, 0 }, velocity3, position3[2], position3[0], false, 0.1);  // 左
                //HandleDirection(directions[3], new double[] { -40, 0, 0 }, velocity3, position3[2], position3[0],false, 0.1);  // 右

            }
            
        }

        private void HandleMovement(bool direction, double[] springValues, double[] jointAngles, double linearValue, double angularValue, bool isrear, double distance) {
           
            
            //SetRepulsiveForce(distance, direction, springValues);
            
            if (jointAngles[2] < 0.35 || jointAngles[2] > 0.5 || jointAngles[0] < -0.05 || jointAngles[0] > 0.05) {
                
                message.linear = GetGeometryVector3(linearValue);  // map to x-axis or other axis
                message.angular = GetGeometryRotate(angularValue); // map to y-axis or other axis
            }else{
                initial_cmd_vel(message);
            }
            Publish(message);
        }

        private void SetRepulsiveForce(double distance, bool direction, double[] springValues)
        {
            double force = CalculateForce(distance);
            
            // set spring force
            if (direction && DataManager.Instance.isWithRepulsiveForce) {
                HapticPlugin.setSpringValues("Default Device", springValues, force);
            }
        }

        private double CalculateForce(double distance) {
            double minDistance = 0.1; // minimum distance
            double maxDistance = 6.0; // maximum distance
            double maxForce = 0.05;    // maximum force
            double minForce = 0.0;    // minimum force

            // limit the distance to the range [minDistance, maxDistance]
            distance = Math.Clamp(distance, minDistance, maxDistance);
            
            double force = maxForce * Math.Exp(-distance);

            // limit the force to the range [minForce, maxForce]
            return Math.Clamp(force, minForce, maxForce);
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

        public MessageTypes.Geometry.Twist GetMessage()
        {
            return message;
        }

        public void SetVibrationForce(double force)
        {
            HapticPlugin.setVibrationValues("Default Device", new double[] {0,1,0}, force, 50, 0.5);
        }

        public void SetDeviceActive()
        {   
            activeState = true;
            GameObject image = GameObject.Find("WarnBroad");
            //GameObject goalManager = GameObject.Find("GoalManager");
            goalManager.SetActive(true);
            image.SetActive(false);
            
        }
        
        private void CheckUserInput()
        {
            if (DataManager.Instance.isWithAssitanceCues)
            {
                //over the thresholds then user get the full control
                bool x_thresholds = jointAngles[0] < -0.15 || jointAngles[0] > 0.15; // left and right
                bool y_thresholds = jointAngles[2] < 0.15 || jointAngles[2] > 0.8; // forward and backward
                if (x_thresholds || y_thresholds)
                {
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
                    Local_nav_line.SetActive(false);
                }
                else
                {
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
                    Local_nav_line.SetActive(true);
                }
            
                //shared control state, control percentage depends on alpha value
                bool isXinput = jointAngles[0] < -0.05 || jointAngles[0] > 0.05; // left and right
                bool isYinput = jointAngles[2] < 0.34 || jointAngles[2] > 0.48; // forward and backward
                if ((isXinput || isYinput ))
                {
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = true;
                }
                else
                {
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                }
            }
                
        }

        private void FiniteStateMachine()
        {
            HapticPlugin.getCurrentForce("Default Device", currentForce);
            Vector3 force = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);
            f = force.magnitude;
            //Debug.Log(DataManager.Instance.modeName);
           // Debug.Log("Current force on device: " + f);

            bool x_thresholds = position3[0] < -35 || position3[0] > 35; // left and right
            bool y_thresholds = position3[2] < 5 || position3[2] > 45; // forward and backward

            bool x_lowt = position3[0] < -5 || position3[0] > 5;
            bool y_lowt = position3[2] < 23 || position3[2] > 35;
            if (DataManager.Instance.modeName == "training")
            {
                Debug.Log(DataManager.Instance.modeName);
                rosConnector.GetComponent<IsAutoPublisher>().autoOperation = false;
                rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
            }
            else
            {
                //if (force.magnitude < forceThreadholdLow && DataManager.Instance.nav_vel_signal)
                if (force.magnitude < forceThreadholdLow && DataManager.Instance.nav_vel_signal)
                {
                    //Computer control
                    if (lastState == "user" && !isCountdownRunning)
                    {
                        StartCoroutine(CountdownBeforeSwitch(force));
                        SwitchToComputerControl();
                    }
                    else if (lastState != "user")
                    {
                        //Debug.Log("success");
                        SwitchToComputerControl();
                    }
                    //Human_control_line.SetActive(false);
                    //Local_nav_line.SetActive(true);
                }
                //else if (x_thresholds || y_thresholds || !DataManager.Instance.nav_vel_signal)
                else if (force.magnitude > forceThreadholdUp || !DataManager.Instance.nav_vel_signal)
                {
                    //User control
                    rosConnector.GetComponent<IsAutoPublisher>().autoOperation = false;
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
                    lastState = "user";
                    //Human_control_line.SetActive(true);
                    //Local_nav_line.SetActive(false);
                }
                else 
                {
                    //Shared control
                    rosConnector.GetComponent<IsAutoPublisher>().autoOperation = true;
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = true;
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
                }
            }
        }
        
        private IEnumerator CountdownBeforeSwitch(Vector3 originalForce)
        {
            isCountdownRunning = true;
            yield return new WaitForSeconds(1f);

            // check the current state again after the countdown
            HapticPlugin.getCurrentForce("Default Device", currentForce);
            Vector3 currentForceVec = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);
            Debug.Log(lastState);
            if (currentForceVec.magnitude < forceThreadholdLow)
            {
                SwitchToComputerControl();
                lastState = "computer";
            }

            isCountdownRunning = false;
        }

        private void SwitchToComputerControl()
        {
            rosConnector.GetComponent<IsAutoPublisher>().autoOperation = true;
            rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
            rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
            lastState = "computer";
        }

    }
}
