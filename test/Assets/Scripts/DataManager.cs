using System;
using System.Collections;
using System.Collections.Generic;
using RosSharp.RosBridgeClient;
using UnityEngine;
using TMPro;


public class DataManager : MonoBehaviour
{
    
    public class Data
    {
        public string DataName;
        public float Value;
    }
    
    // Start is called before the first frame update
    public static DataManager Instance { get; private set; }
    public GameObject rosConnector;
    public GameObject endUI;

    public int collisionCount = 0;
    public int targetNum = 0;
    public float alpha = 0;
    public int collectCount = 0;
    public bool taskFinished = false;
    public bool taskStart = false;
    public bool reachGoal = false;
    public bool isWithAssitanceCues = false;
    public bool isWithEnvironmentCues = false;
   
    public bool isWithRepulsiveForce = false;

    public bool isWithGuidanceForce = false;
    public bool isWithNavigation = false; 
    public bool isManual = false; //
    public bool isWithMap = false;
    public bool isWithCollisionLine = false;
    public bool isWithCollisionBrake = false; 
    public bool isWithAutonomyBar = false;
    public bool isWithDirectionLine = false;

    public GameObject CollisionLines;
    public GameObject DirectionLine;
    public CB_Publisher CB;

    public string modeName = null;
    public bool nav_vel_signal = false;
    public List<Data> Records1 = new List<Data>();
    
    [Header("Seconds")]
    public float taskTime = 180f;

    [Header("Mode")] 
    public bool isTeleop;
    public bool isAuto;
    public bool isFullControl;

    public bool isPaused = false;
    public bool forceComputer = false;

    public TextMeshProUGUI endUIText;


    private bool locker = false;
    private float time_temp = 0f;

    private void Awake()
    {
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        time_temp = taskTime;
    }
    
    private void StatusUpdate()
    {
        isTeleop = rosConnector.GetComponent<IsTelePublisher>().sharedControl;
        isAuto = rosConnector.GetComponent<IsAutoPublisher>().autoOperation;
        isFullControl = rosConnector.GetComponent<IsFullControlPublisher>().fullControl;
        alpha = rosConnector.GetComponent<GetAlphaValue>().GetValue();

        if (!isWithCollisionLine)
        {
            CollisionLines.SetActive(false);
        }
        else
        {
            CollisionLines.SetActive(true);
        }
        if (!isWithDirectionLine)
        {
            DirectionLine.SetActive(false);
        }
        else
        {
            DirectionLine.SetActive(true);
        }

        CB.CB = isWithCollisionBrake;

    }
    private void Update()
    {
        StatusUpdate();
        TimeCount();
        if (taskFinished && !locker)
        {
            // when task is finished, show the end UI
            HandleData();
        }else if (collectCount == targetNum && !locker)
        {
            HandleData();
            taskFinished = true;
        }

    }
    private void HandleData()
    {
        Records1.Add(new Data(){DataName = "Collision", Value = collisionCount});
        Records1.Add(new Data(){DataName = "Target", Value = collectCount});
        Records1.Add(new Data(){DataName = "Time", Value = time_temp-taskTime});
        Records1.Add(new Data() {DataName = "isWithNavigation", Value = isWithNavigation?1f:0f});
        Records1.Add(new Data() { DataName = "isWithGuidanceForce", Value = isWithGuidanceForce ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isWithMap", Value = isWithMap ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isWithAutonomyBar", Value = isWithAutonomyBar ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isWithDirectionLine", Value = isWithDirectionLine ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isManual", Value = isManual ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isWithCollisionBrake", Value = isWithCollisionBrake ? 1f : 0f });
        Records1.Add(new Data() { DataName = "isWithCollisionLine", Value = isWithCollisionLine ? 1f : 0f });
        endUI.SetActive(true);
        if(collectCount == targetNum)
        {
            endUIText.text = "All target collected! Task Finished";
        }
        else
        {
            endUIText.text = "Time is up! Task Ended";
        }
        rosConnector.SetActive(false);  
        locker = true;
    }
    public void StartTask()
    {
        taskStart = true;
    }

    public void TimeCount()
    {
        //set the time to 0 when the task is finished
        if (taskTime > 0 && taskStart && !taskFinished)
        {
            taskTime -= Time.deltaTime;
        }
        else if (taskTime <= 0)
        {
            taskFinished = true;
            
        }
    }

    
    public void ModeA()
    {
        modeName = "ModeA";
        isWithNavigation = true;
        isWithGuidanceForce = false;
        isWithMap = false;
        isWithAutonomyBar = false;
        isWithDirectionLine = false;
        isManual = false;
        isWithCollisionBrake = false;
        isWithCollisionLine = false;
    }
    public void ModeB()
    {
        modeName = "ModeB";
        isWithNavigation = true;

        isWithGuidanceForce = true;
        isWithMap = false;
        isWithAutonomyBar = false;
        isWithDirectionLine = false;
        isManual = false;
        isWithCollisionBrake = false;
        isWithCollisionLine = false;

    }
    public void ModeC()
    {
        modeName = "ModeC";

        isWithNavigation = true;
        isWithGuidanceForce = true;
        isWithMap = false;
        isWithAutonomyBar = true;
        isWithDirectionLine = true;
        isManual = false;
        isWithCollisionBrake = false;
        isWithCollisionLine = false;
    }
    public void ModeD()
    {
        modeName = "ModeD";
        isWithNavigation = true;
        isWithGuidanceForce = true;
        isWithMap = true;
        isWithAutonomyBar = true;
        isWithDirectionLine = true;
        isManual = false;
        isWithCollisionBrake = true;
        isWithCollisionLine = true;
    }

    public void ModeE()
    {
        modeName = "ModeE";

        isWithNavigation = true;
        isWithGuidanceForce = true;
        isWithMap = true;
        isWithAutonomyBar = true;
        isWithDirectionLine = true;
        isManual = false;
        isWithCollisionBrake = true;
        isWithCollisionLine = true;

    }

    public void targetReached()
    {
        gameObject.GetComponent<SessionCsvLogger>().LogTarget(this);
    }

}
