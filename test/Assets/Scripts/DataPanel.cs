using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DataPanel : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject robot;
    public TextMeshProUGUI collisionText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI targetText;
    // private int collisionCount = 0;
    private float time = 0.0f;
    private float countDown = 180.0f;
    private bool timerActive = false;
    private bool locker = false;
    

    // Update is called once per frame
    void Update()
    {
        CollisionCounter();
        TimeCounter();
        TargetCounter();
    }

    void TimeCounter(){
        if(timerActive && !DataManager.Instance.taskFinished){
            time += Time.deltaTime;
            countDown = 180.0f - time;
            //countDown = 105.0f;
            timeText.text = "Time: " + countDown.ToString("F2");
        }
        // else if(timerActive && !locker ){
        //     DataManager.Instance.Records.Add(new DataManager.Data(){DataName = "Time", Value = time});
        //     locker = true;
        // }
    
    }

    void CollisionCounter(){
        collisionText.text = "Collisions: " + DataManager.Instance.collisionCount;
    }

    void TargetCounter()
    {
        targetText.text = "Targets: " + DataManager.Instance.collectCount;
    }

    public void ActiveTimer(){
        timerActive = !timerActive;
    }
}
