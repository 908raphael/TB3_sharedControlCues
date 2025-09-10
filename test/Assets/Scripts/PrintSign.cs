using UnityEngine;

public class PrintSign : MonoBehaviour
{
    
    public Transform target;
    public GameObject sign;
    public float triggerDistance = 0.5f;
    
    private bool hasTriggered = false;
    void Update()
    {
        if (hasTriggered)
            return;

        // 计算当前机器人和目标之间的距离
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance <= triggerDistance)
        {
            hasTriggered = true;
            TriggerEvent();
        }
    }

    void TriggerEvent()
    {
        DataManager.Instance.collectCount++;
        DataManager.Instance.targetReached();
        sign.SetActive(false);
        sign.layer = 0;
        gameObject.GetComponent<BoxCollider>().isTrigger = false;
        //DataManager.Instance.reachGoal = true;
    }
}