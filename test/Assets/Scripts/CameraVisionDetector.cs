using UnityEngine;
using System.Collections.Generic;

public class FOVDetector : MonoBehaviour
{
    public string targetTag = "Target";
    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public GoalManager goalManager;

    private HashSet<Transform> visibleTargets = new HashSet<Transform>();  // declare a HashSet to store visible targets

    void Update()
    {
        DetectVisibleTargets();
    }

    void DetectVisibleTargets()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        HashSet<Transform> currentlyVisible = new HashSet<Transform>();

        foreach (GameObject target in targets)
        {
            Vector3 directionToTarget = target.transform.position - transform.position;
            float distance = directionToTarget.magnitude;

            if (distance < viewDistance)
            {
                float angle = Vector3.Angle(transform.forward, directionToTarget.normalized);

                if (angle < viewAngle / 2)
                {
                    Ray ray = new Ray(transform.position, directionToTarget);
                    if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
                    {
                        if (hit.transform == target.transform)
                        {
                            currentlyVisible.Add(target.transform);

                            // first time the target is detected
                            if (!visibleTargets.Contains(target.transform))
                            {
                                visibleTargets.Add(target.transform);
                                Debug.Log("target show up: " + target.name + " coordinate: " + target.transform.position);
                                goalManager.findTarget(target.name, target.transform.position, target.transform.rotation);
                            }
                        }
                    }
                }
            }
        }
        //remove the targets that are not currently visible
        //visibleTargets.RemoveWhere(t => !currentlyVisible.Contains(t));
    }
}



// using UnityEngine;
//
// public class FOVDetector : MonoBehaviour
// {
//     public string targetTag = "target";
//     public float viewDistance = 10f;   // 视野距离
//     public float viewAngle = 60f;      // 视野角度 (例如 60度)
//
//     void Update()
//     {
//         DetectVisibleTargets();
//     }
//
//     void DetectVisibleTargets()
//     {
//         GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
//
//         foreach (GameObject target in targets)
//         {
//             Vector3 directionToTarget = target.transform.position - transform.position;
//             float distance = directionToTarget.magnitude;
//
//             if (distance < viewDistance)
//             {
//                 float angle = Vector3.Angle(transform.forward, directionToTarget.normalized);
//
//                 if (angle < viewAngle / 2)
//                 {
//                     Ray ray = new Ray(transform.position, directionToTarget);
//                     if (Physics.Raycast(ray, out RaycastHit hit, viewDistance))
//                     {
//                         if (hit.transform == target.transform)
//                         {
//                             Debug.Log("目标在视野角度内，坐标：" + target.transform.position);
//                         }
//                     }
//                 }
//             }
//         }
//     }
// }