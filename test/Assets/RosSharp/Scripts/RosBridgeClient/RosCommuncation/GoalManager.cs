using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;

public class GoalManager : MonoBehaviour
{
    public NavigationGoalPublisher navigationGoalPublisher;
    public GameObject Nav_Line;
    public GameObject Goal_indicator;
    public GoalCanclePublisher goalCanclePublisher;
    public ShowIndicator showIndicator;
    public Transform robotTransform;
    private GameObject targetPoint;
    //public GoalStatusSubscriber goalStatusSubscriber;
    public List<GameObject> targetPoints; 
    private Queue<GameObject> goalQueue = new Queue<GameObject>();
    private Dictionary<GameObject, float> targetDistances = new Dictionary<GameObject, float>();
    private bool Finished = false;
    private bool isFoundOject = false;
    private Vector3 ojectPosition;
    private Quaternion ojectRotation;

    private string name_temp;
    private bool startFlag = true;

    class TargetObject
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
    }


    void Awake()
    {
        // reset the goal status when the scene is loaded
        goalCanclePublisher.CancelGoal();
    }

    void Start()
    {
        CancelGoal();
        name_temp = DataManager.Instance.modeName;
    }
    

    void Update()
    {
        CaculateDistance();
        SendNearestGoal();
        if (targetDistances.Count == 0)
        {
            Finished = true;
        }
        HandleGoals();
    }

    private void HandleGoals()
    {
        if (DataManager.Instance.reachGoal && targetDistances.Count > 0)
        {
            //isFoundOject = false;
            DataManager.Instance.reachGoal = false;
            var closestTarget = targetDistances.OrderBy(kvp => kvp.Value).First();
            targetPoints.Remove(closestTarget.Key);
            targetDistances.Remove(closestTarget.Key);
        }
        else if (targetDistances.Count == 0)
        {
            CancelGoal();
            Goal_indicator.SetActive(false);
        }
    }
    

    
    
    void CaculateDistance()
    {
        //遍历List
        foreach (var item in targetPoints)
        {
            float distance = Vector3.Distance(robotTransform.position, item.transform.position);
            targetDistances[item] = distance;

            // if (distance < 0.3f)
            // {
            //     CancelGoal();
            //     DataManager.Instance.reachGoal = true;
            // }
        }
        bool hasCloseTarget = targetDistances.Values.Any(v => v < 0.3f);
        if (hasCloseTarget)
        {
            //CancelGoal();
            DataManager.Instance.reachGoal = true;
        }
        
    }
    
    
    IEnumerator PublishGoalDelayed(Vector3 pos, Quaternion rot)
    {
        yield return new WaitForSeconds(0.3f);
        PublishGoal(pos, rot);
    }

    
    void SendNearestGoal()
    {
        if (targetDistances.Count == 0) return;
        
        var closestTarget = targetDistances.OrderBy(kvp => kvp.Value).First();
        
        if (targetPoint == null || closestTarget.Key != targetPoint)
        {
            //CancelGoal();
            targetPoint = closestTarget.Key;
            DataManager.Instance.modeName = name_temp;
            PublishGoal(closestTarget.Key.transform.position, closestTarget.Key.transform.rotation);
            showIndicator.Show(closestTarget.Key.transform.position);
            //Nav_Line.SetActive(true);
            //Debug.Log("Send Nearest Goal！");
        }
        
    }

    void CancelGoal()
    {
        goalCanclePublisher.CancelGoal();
    }
    
    void PublishGoal(Vector3 position, Quaternion rotation)
    {
        navigationGoalPublisher.SetGoalActive(position, rotation);

    }

    public void OnGoalReached()
    {
        //SendNextGoal();
    }
    public void findTarget(string targetName, Vector3 position, Quaternion rotation)
    {
        //CancelGoal();
        //isFoundOject = true;
        GameObject target = new GameObject(targetName);
        
        target.transform.position = position;
        target.transform.rotation = rotation;
        if(!targetPoints.Exists(x => x.name == targetName))
             targetPoints.Add(target);
        //startFlag = true;
        
    }
}
