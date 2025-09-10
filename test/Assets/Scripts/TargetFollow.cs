using UnityEngine;

public class TargetFollow : MonoBehaviour
{
    public Transform robot; 
    public Vector3 offset = new Vector3(0, 1.5f, 0); 
    public Vector3 rotationDegrees = new Vector3(90, 0, 0);
    public bool followRotation = true;

    void Update()
    {
        if (robot != null && followRotation)
        {   
            transform.position = robot.position + robot.TransformDirection(offset);

            Quaternion additionalRotation = Quaternion.Euler(rotationDegrees);
            transform.rotation = robot.rotation * additionalRotation;
        }else{
            Vector3 newPosition = new Vector3(0, robot.position.y + offset.y, robot.position.z);
            transform.position = newPosition + robot.TransformDirection(offset);

            Quaternion additionalRotation = Quaternion.Euler(rotationDegrees);
            transform.rotation = additionalRotation;
        }
    }
}
