using System.Collections;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;
using RosSharp.RosBridgeClient;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class RoleExchanger : MonoBehaviour
{
    public Nav_Twist_Subscriber navTwistSubscriber;
    public GameObject rosConnector;
    private string currentState;
    private double[] currentForce;
    //private List<float> forceHistory;
    private Queue<float> forceHistory;
    
    private Vector3 linearVelocity;
    private Vector3 angularVelocity;
    private bool isBlendingTimerRunning = false;
    
    private float angularSpeed = 0.0f;
    private float linearSpeed = 0.0f;
    private float xForce = 0.0f;
    private float zForce = 0.0f;
        
    [SerializeField] private float thresholdLower;
    [SerializeField] private float thresholdUpper;
    [SerializeField] private float maxDeltaAngular = 0.1f;
    [SerializeField] private float maxDeltaLinear = 0.2f;
    [SerializeField] private float minForce = 0.5f;        
    [SerializeField] private float maxForce = 2f; 
    
    private void Start()
    {
        currentState = "TeleOperation";
        currentForce = new double[3];
        forceHistory = new Queue<float>();
        thresholdLower = 0.5f;
        thresholdUpper = 1.5f;
        
    }
    
    private void Update()
    {
        // Update the current state
        linearVelocity = navTwistSubscriber.linearVelocity;
        angularVelocity = navTwistSubscriber.angularVelocity;
        
        float currentAngularSpeed = angularVelocity.y;
        float currentLinearSpeed = linearVelocity.z;
        
        // Calculate the delta of linear and angular speed
        float deltaLinear = currentLinearSpeed - linearSpeed;
        float deltaAngular = currentAngularSpeed - angularSpeed;
        
        OptimalizeForce(deltaLinear, deltaAngular);
        CheckState();
        switch (currentState)
        {
            case "TeleOperation":
                SetGuidanceForce(xForce,zForce);
                break;
            case "HumanControl":
                break;
            case "SharedControl":
                //SetGuidanceForce(xForce,zForce);
                break;
        }
            
        angularSpeed = currentAngularSpeed;
        linearSpeed = currentLinearSpeed;
        
    }

    private void CheckState()
    {
        HapticPlugin.getCurrentForce("Default Device", currentForce);
        Vector3 force = new Vector3((float)currentForce[0], (float)currentForce[1], (float)currentForce[2]);
        
        if (forceHistory.Count > 500)
        {
            forceHistory.Dequeue();
        }
        forceHistory.Enqueue(force.magnitude);
        
        switch (currentState)
        {
            case "TeleOperation":
                if (CheckUpperThreshold(forceHistory))
                {
                    currentState = "HumanControl";
                    rosConnector.GetComponent<IsAutoPublisher>().autoOperation = false;
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = true;
                    StartBlendingTimer();
                }
                break;
            case "HumanControl":
                if (CheckLowerThreshold(forceHistory))
                {
                    currentState = "TeleOperation";
                    rosConnector.GetComponent<IsAutoPublisher>().autoOperation = true;
                    rosConnector.GetComponent<IsTelePublisher>().sharedControl = false;
                    rosConnector.GetComponent<IsFullControlPublisher>().fullControl = false;
                    StartBlendingTimer();
                }
                break;
        }
    }
    
    private void OptimalizeForce(float deltaLinear, float deltaAngular) 
    {
        deltaAngular = Mathf.Clamp(deltaAngular, -maxDeltaAngular, maxDeltaAngular);
        deltaLinear = Mathf.Clamp(deltaLinear, -maxDeltaLinear, maxDeltaLinear);
            
        // nomalize deltaAngular to [0, 1]
        xForce = Mathf.Lerp(
            minForce, 
            maxForce, 
            (deltaAngular + maxDeltaAngular) / (2 * maxDeltaAngular) 
        );
            
        // nomalize deltaLinear to [0, 1]
        zForce = Mathf.Lerp(
            minForce, 
            maxForce, 
            (deltaLinear + maxDeltaLinear) / ( 2 * maxDeltaLinear) 
        );
    }
    
    private void SetGuidanceForce(float xForce, float zForce) 
    {
        if (!DataManager.Instance.isFullControl)
        {
            float forward_backward = - linearVelocity.z;
            float Left_Right = - angularVelocity.y;
            
            double[] forceDirection = new double[3] {Left_Right, 0.0, forward_backward};
            
            float force = new Vector3(xForce, 0, zForce).magnitude;
                
            // double[] currentForce = new double[3] {0.0, 0.0, 0.0};
            // HapticPlugin.getCurrentForce("Default Device", currentForce);
            // Debug.Log(currentForce[1]);
            HapticPlugin.setConstantForceValues("Default Device", forceDirection, force);
        }
    }
    
    private bool CheckLowerThreshold(Queue<float> forceHistory)
    {
        return forceHistory.Any(f => f < thresholdLower);
    }
    
    private bool CheckUpperThreshold(Queue<float> forceHistory)
    {
        return forceHistory.Any(f => f > thresholdUpper);
    }
    
    private async void StartBlendingTimer()
    {
        if (isBlendingTimerRunning) return; // 避免重复启动计时器
        isBlendingTimerRunning = true;
    
        await Task.Delay(500); // 500ms 平滑过渡时间
    
        // 计时器到期后，检查是否仍处于 S3
        if (currentState == "HumanControl")
        {
            currentState = "HumanControl"; // 进入平等控制
        }
        else
        {
            currentState = "TeleOperation";
        }
    
        isBlendingTimerRunning = false;
    }

    
}
