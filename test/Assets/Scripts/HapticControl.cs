using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class HapticControl : UnityPublisher<MessageTypes.Geometry.Twist>
    {
        public HapticPlugin hapticPlugin; 
        public LaserScanReader laserScanReader;
        public GameObject goalManager;
        public RosConnector rosConnector;
        public GameObject Local_nav_line;
        public GameObject Human_control_line;
        public NIP_Subscriber NIPS;
        public KN_Publisher KP;

        public int linearRatio = 500;
        public int angularRatio = 300;
        public double[] position3 = {0.0,0.0,0.0};
        public double[] velocity3 = {0.0,0.0,0.0};
        public double[] jointAngles = {0.0,0.0,0.0};
        public double[] gimbalAngles = {0.0,0.0,0.0};
        public double[] NIP = {0.0,0.0,0.0};
        public double[] movement = {0.0,0.0,0.0};

        //public Vector3 fv = new Vector3 ((float)0.0, (float)0.0, (float)0.0);
        public float f;

        [SerializeField] float forceThreadholdUp = 2.7f;
        [SerializeField] float forceThreadholdLow = 1f;
        [SerializeField] float CAThresholdUp = 1.2f;
        [SerializeField] float CAThresholdLow = 1.2f;

        public double[] currentForce = {0.0,0.0,0.0};

        private MessageTypes.Geometry.Twist message;
        private bool activeState = false;  
        private bool autoControlSignal = false;


        public string lastState = "shared";
        private bool isCountdownRunning = false;
        private bool isCountdownShared = false;

        public float align = 1f;
        public bool intentLock = false;
        public float avgAlign;
        public float avgForce;

        Queue<float> alignHistory = new Queue<float>();
        int historySize = 20; // last 50 frames (~1s at 50Hz)

        Queue<float> forceHistory = new Queue<float>();
        int forcehistorySize = 15; 


        public float controlAlpha = 0f;


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
            //HapticPlugin.EnableVibration();
            message = new MessageTypes.Geometry.Twist();
            message.linear = new MessageTypes.Geometry.Vector3();
            message.angular = new MessageTypes.Geometry.Vector3();
            initial_cmd_vel(message);
            Publish(message);
        }

        void UpdateAlignStats(float currentAlign)
        {
            alignHistory.Enqueue(currentAlign);
            if (alignHistory.Count > historySize)
                alignHistory.Dequeue();

            avgAlign = alignHistory.Average();
            
        }

        void UpdateForceStats(float currentForce)
        {
            forceHistory.Enqueue(currentForce);
            if (forceHistory.Count > forcehistorySize)
                forceHistory.Dequeue();

            avgForce = forceHistory.Average();

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
                //Debug.Log("Distance: " + distance);


                NIP = NIPS.NIP;
                movement[2] = -(NIP[2] - position3[2]);
                movement[0] = -(NIP[0] - position3[0]);
                //if (jointAngles[2] < 0.35 || jointAngles[2] > 0.5 || jointAngles[0] < -0.05 || jointAngles[0] > 0.05)
                //if (Mathf.Abs((float) movement[2]) > 15 || Mathf.Abs((float) movement[0]) > 15)
                if ((rosConnector.GetComponent<IsTelePublisher>().sharedControl || rosConnector.GetComponent<IsFullControlPublisher>().fullControl) && (!DataManager.Instance.isPaused))
                {
                    //message.linear = GetGeometryVector3(movement[2]);  // map to x-axis or other axis
                    //message.angular = GetGeometryRotate(movement[0]); // map to y-axis or other axis
                    message.linear = GetGeometryVector3(position3[2], linearRatio);
                    message.angular = GetGeometryRotate(position3[0], angularRatio);
                    //Debug.Log("Haptic X: " + message.angular);
                    //Debug.Log("Haptic Z: " + message.linear);
                }
                else
                {
                    initial_cmd_vel(message);
                }
                Publish(message);


            }
            
        }

        /*
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
        */

        private void SetRepulsiveForce(double distance, bool direction, double[] springValues)
        {
            double force = CalculateForce(distance);
            
            // set spring force
            if (direction && DataManager.Instance.isWithRepulsiveForce) {
                //HapticPlugin.setSpringValues("Default Device", springValues, force);
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

        private static MessageTypes.Geometry.Vector3 GetGeometryVector3(double vec, int linearRatio)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = (-vec / linearRatio);
            geometryVector3.y = 0;
            geometryVector3.z = 0;
            return geometryVector3;
        }

        private static MessageTypes.Geometry.Vector3 GetGeometryRotate(double vec, int angularRatio)
        {
            MessageTypes.Geometry.Vector3 geometryVector3 = new MessageTypes.Geometry.Vector3();
            geometryVector3.x = 0;
            geometryVector3.y = 0;
            geometryVector3.z = vec / angularRatio;
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
        
        /*
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
        */

        private void FiniteStateMachine()
        {
            UpdateAlignStats(align);
            HapticPlugin.getCurrentForce("Default Device", currentForce);
            Vector3 force = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);
            f = force.magnitude;
            UpdateForceStats(f);
            //UpdateForceStats(f);
            controlAlpha =  (float) (((f - forceThreadholdLow) / (forceThreadholdUp - forceThreadholdLow)) *0.4  +  align*0.6);
            bool x_thresholds = position3[0] < -35 || position3[0] > 35; // left and right
            bool y_thresholds = position3[2] < 5 || position3[2] > 45; // forward and backward

            bool x_lowt = position3[0] < -5 || position3[0] > 5;
            bool y_lowt = position3[2] < 23 || position3[2] > 35;

            if (DataManager.Instance.isManual)
            {
                hapticPlugin.DisableSpring();
                //SwitchToUserControl();
            }
            else
            {
                hapticPlugin.EnableSpring();
            }

            

            if (DataManager.Instance.modeName == "training")
            {
                Debug.Log(DataManager.Instance.modeName);
                rosConnector.GetComponent<IsAutoPublisher>().autoOperation = false;
                rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
            }
            else if (DataManager.Instance.forceComputer)
            {
                SwitchToComputerControl();
                DataManager.Instance.forceComputer = false;
            }
            else if (lastState == "user")
            {

                if (!DataManager.Instance.nav_vel_signal || !DataManager.Instance.isWithNavigation)
                {
                    SwitchToUserControl();
                }
                //else if (align > 0.4f && DataManager.Instance.nav_vel_signal && intentLock)
                else if (align > 0.2f) // higher align values indicate non-alignment in Twist commands between user and robot
                {
                    SwitchToUserControl();
                }
                /*
                else if (force.magnitude < forceThreadholdLow  && !isCountdownRunning)
                {
                    StartCoroutine(CountdownBeforeSwitch());
                }
                */
                //else if (force.magnitude < (forceThreadholdUp - 0.5f) && force.magnitude > forceThreadholdLow && !isCountdownShared)
                else if (!isCountdownShared)
                {
                    StartCoroutine(CountdownBeforeShared());
                    //SwitchToSharedControl();
                }
                else
                {
                    SwitchToUserControl();
                }
            }
            else if (lastState == "shared")
            {

                if (controlAlpha < CAThresholdLow && DataManager.Instance.nav_vel_signal)
                {
                    SwitchToComputerControl();
                }
                else if (!DataManager.Instance.nav_vel_signal)
                {
                    SwitchToUserControl();
                }
                else if (controlAlpha > CAThresholdUp)
                {
                    intentLock = true;
                    SwitchToUserControl();
                }
                else
                {
                    SwitchToSharedControl();
                }
            }
            else if (lastState == "computer")
            {

                if (!DataManager.Instance.nav_vel_signal || !DataManager.Instance.isWithNavigation)
                {
                    SwitchToUserControl();
                }
                /*
                else if (force.magnitude > forceThreadholdUp || controlAlpha > CAThreshold)
                {
                    intentLock = true;
                    SwitchToUserControl();
                }
                */
                else if (force.magnitude > forceThreadholdLow && !isCountdownShared)
                {
                    StartCoroutine(CountdownBeforeSharedFromComputer());
                }
                else
                {
                    SwitchToComputerControl();
                }
            }
        }

        private IEnumerator CountdownBeforeSharedFromComputer()
        {
            isCountdownShared = true;
            yield return new WaitForSeconds(0.5f);

            HapticPlugin.getCurrentForce("Default Device", currentForce);
            Vector3 currentForceVec = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);

            //if (currentForceVec.magnitude < (forceThreadholdUp - 0.5f) && currentForceVec.magnitude > forceThreadholdLow && align <0.5f && DataManager.Instance.nav_vel_signal)
            if (avgForce > forceThreadholdLow && DataManager.Instance.nav_vel_signal)
            //if (currentForceVec.magnitude > forceThreadholdLow && DataManager.Instance.nav_vel_signal)
            {
                    SwitchToSharedControl();
            }
            isCountdownShared = false;
        }


        private IEnumerator CountdownBeforeShared()
        {
            isCountdownShared = true;
            yield return new WaitForSeconds(0.8f);

            //if (currentForceVec.magnitude < (forceThreadholdUp - 0.5f) && currentForceVec.magnitude > forceThreadholdLow && align <0.5f && DataManager.Instance.nav_vel_signal)
            //if (currentForceVec.magnitude < (forceThreadholdUp - 0.5f) && currentForceVec.magnitude > forceThreadholdLow && DataManager.Instance.nav_vel_signal)
            if (avgAlign < 0.2f && DataManager.Instance.nav_vel_signal)
            {
                
                   SwitchToSharedControl();
            }
            isCountdownShared = false;
        }
        
        /*
        private IEnumerator CountdownBeforeSwitch()
        {
            isCountdownRunning = true;
            yield return new WaitForSeconds(0.5f);

            HapticPlugin.getCurrentForce("Default Device", currentForce);
            Vector3 currentForceVec = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);

            //if (currentForceVec.magnitude < forceThreadholdLow &&  align < 0.5f && DataManager.Instance.nav_vel_signal)
            if (currentForceVec.magnitude < forceThreadholdLow && DataManager.Instance.nav_vel_signal)
            {
                if ((intentLock && align < 0.35f) || !intentLock)
                    SwitchToComputerControl();
            }

            isCountdownRunning = false;
        }
        */

        private void SwitchToComputerControl()
        {
            rosConnector.GetComponent<IsAutoPublisher>().autoOperation = true;
            rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
            rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
            lastState = "computer";
            intentLock = false;
            KP.KN = 1f;
        }

        private void SwitchToUserControl()
        {
            rosConnector.GetComponent<IsAutoPublisher>().autoOperation = false;
            rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
            rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
            lastState = "user";
            KP.KN = 0f;
        }

        private void SwitchToSharedControl()
        {
            rosConnector.GetComponent<IsAutoPublisher>().autoOperation = true;
            rosConnector.GetComponent<IsTelePublisher>().sharedControl = true;
            rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
            lastState = "shared";
            intentLock = false;
            //KP.KN = (float) (1 - (((f - forceThreadholdLow) / (forceThreadholdUp - forceThreadholdLow)) * 0.4 + align * 0.6));
            KP.KN = (float) Mathf.Clamp((1 - ((controlAlpha - CAThresholdLow) / (CAThresholdUp -  CAThresholdLow))),0,1);
        }

    }
}
