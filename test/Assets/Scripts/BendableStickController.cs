using UnityEngine;

public class BendableTailStick : MonoBehaviour
{
    public LineRenderer lineRenderer;

    public Camera Camera;
    public int segmentCount = 20;         
    public float bendSpeed = 1f;         
    public float maxBendAngle = 1.0f;      
    private float currentBendAngle = 0f;  

    public Transform startPoint;          
    public Transform midPoint;            
    public Transform endPoint;            

    public Transform robotTransform;   
    public float resetAngleThreshold = 5f;     

    private float previousRobotAngle = 0f;  
    

    void Start()
    {
        Camera.gameObject.SetActive(true);
        lineRenderer.positionCount = segmentCount;
        lineRenderer.sortingLayerName = "ArrowLayer"; 
        lineRenderer.sortingOrder = 5;
    }

    void Update()
    {
        
        float currentRobotAngle = robotTransform.eulerAngles.y;

        // caculate the 计算与上一次角度的差值
        float robotRotationDelta = Mathf.DeltaAngle(previousRobotAngle, currentRobotAngle);

        // update current bend angle
        currentBendAngle = Mathf.Clamp(currentBendAngle + robotRotationDelta * bendSpeed * Time.deltaTime, -maxBendAngle, maxBendAngle);

        // record the current robot rotation
        previousRobotAngle = currentRobotAngle;

        if (Mathf.Abs(currentBendAngle) < resetAngleThreshold)
        {
            currentBendAngle = Mathf.Lerp(currentBendAngle, 0, Time.deltaTime * 2);
        }

        if (Mathf.Abs(currentBendAngle) < maxBendAngle){

            Vector3 controlPoint1 = midPoint.position; // mid-point as a controlpoint 1
            Vector3 controlPoint2 = endPoint.position + endPoint.right * currentBendAngle * 1f; // Tail shape control

            // Create Bezier curve and set LineRenderer's position
            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1); 
                Vector3 bezierPosition = CalculateBezierPoint(t, startPoint.position, controlPoint1, endPoint.position, controlPoint2);
                lineRenderer.SetPosition(i, bezierPosition);
            }
        }

    }
 
    // caculate Bezier curve position
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // start point
        p += 3 * uu * t * p1; // mid-point
        p += 3 * u * tt * p2; // second point
        p += ttt * p3;        // end point
        return p;
    }
}
