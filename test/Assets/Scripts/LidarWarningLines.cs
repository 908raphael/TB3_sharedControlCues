using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class LidarWarningLines : MonoBehaviour
    {
        [Header("References")]
        public Transform robotTransform;         // robot's Transform
        public LaserScanReader laserScanReader;  // LIDAR reader

        [Header("Distances")]
        public float dangerDistance = 0.8f;      // danger distance threshold
        public float safeDistance = 1.0f;        // safe distance threshold

        [Header("Line Settings")]
        public float lineWidth = 0.01f;                          // adjust the line width
        public Vector3 linePositionOffset = Vector3.zero;        
        // public Vector3 lineStartPoint = Vector3.zero;            
        // public Vector3 lineEndPoint = Vector3.zero;              

        [Header("Materials")]
        public Material warningMaterial;         

        [Header("Arc Settings")]
        public float arcAngleRange = 60f;        
        public int arcSegments = 20;             

        private LineRenderer warningLine45;      
        private LineRenderer warningLine315; 
        private LineRenderer warningLineForward;    

        // LIDAR scan parameters
        public float startAngle = 0f;           
        public float angleIncrement = 1f;        

        void Start()
        {
            if (robotTransform == null)
            {
                Debug.LogError("Robot Transform is not assigned.");
                enabled = false;
                return;
            }
            
            warningLine45 = CreateLineRenderer(warningMaterial, "WarningLine45");
            warningLine315 = CreateLineRenderer(warningMaterial, "WarningLine315");
            warningLineForward = CreateLineRenderer(warningMaterial, "WarningLineForward");

            // initialize the warning lines
            InitializeWarningLines();
        }

        void InitializeWarningLines()
        {
            //initialize the warning lines
            Vector3[] arcPoints45 = CreateSemiArc(safeDistance, 45f, arcSegments);
            Vector3[] arcPoints315 = CreateSemiArc(safeDistance, 315f, arcSegments);
            Vector3[] arcPointsForward = CreateSemiArc(safeDistance, 0f, arcSegments);

            // 将点设置到 LineRenderers
            warningLine45.positionCount = arcPoints45.Length;
            warningLine45.SetPositions(arcPoints45);
            warningLine45.enabled = false; 

            warningLine315.positionCount = arcPoints315.Length;
            warningLine315.SetPositions(arcPoints315);
            // initial state
            warningLine315.enabled = false; 

            warningLineForward.positionCount = arcPointsForward.Length;
            warningLineForward.SetPositions(arcPointsForward);
            warningLineForward.enabled = false;
        }

        void Update()
        {
            if(!DataManager.Instance.isWithCollisionLine) return;
            if (laserScanReader == null || robotTransform == null)
            {
                Debug.LogWarning("LaserScanReader or RobotTransform is not assigned.");
                return;
            }

            // get the LIDAR data
            float[] ranges = laserScanReader.Scan();
            if (ranges == null || ranges.Length == 0)
            {
                Debug.LogWarning("No LIDAR data received.");
                return;
            }

            // calculate the indices for 45 and 315 degrees
            int index45 = Mathf.RoundToInt((45f - startAngle) / angleIncrement);
            int index315 = Mathf.RoundToInt((315f - startAngle) / angleIncrement);

            // ensure the indices are within bounds
            index45 = Mathf.Clamp(index45, 0, ranges.Length - 1);
            index315 = Mathf.Clamp(index315, 0, ranges.Length - 1);

            float range45 = ranges[index45];
            float range315 = ranges[index315];
            float range0 = ranges[(int) startAngle];

            // deal with the forward direction
            bool isLine45 = ShowWarningLine(warningLine315, range45);
            bool isLine315 = ShowWarningLine(warningLine45, range315);
            bool isLine0 = ShowWarningLine(warningLineForward, range0);

            /*
            if (isLine45 && isLine315)
            {
                warningLine45.enabled = false;
                warningLine315.enabled = false;
                ShowWarningLine(warningLineForward, Mathf.Min(range45, range315));
            }
            else
            {
                warningLineForward.enabled = false;
            }
            */
            
        }
        
        bool ShowWarningLine(LineRenderer warningLine, float distance)
        {
            if (distance > 0 && distance < safeDistance)
            {
                // calculate the intensity based on distance
                float intensity = 1 - Mathf.Clamp01((distance - dangerDistance)/(safeDistance-dangerDistance));
                // Debug.Log(intensity);
                intensity = Mathf.Clamp(intensity, 0f, 1f);
                Color warningColor = new Color(1f, 0f, 0f, intensity); // red color with alpha based on distance
                
                warningLine.material.SetColor("_Color", warningColor);
                warningLine.enabled = true;
                return true;
            }
            else
            {
                warningLine.enabled = false;
                return false;
            }
        }
        
        Vector3[] CreateSemiArc(float radius, float directionAngle, int segments)
        {
            Vector3[] points = new Vector3[segments + 1];
            float startArc = directionAngle - (arcAngleRange / 2f);
            float endArc = directionAngle + (arcAngleRange / 2f);
            float step = (endArc - startArc) / segments;

            // Debug.Log($"Creating semi-arc: Direction Angle = {directionAngle}°, Start Arc = {startArc}°, End Arc = {endArc}°, Step = {step}°");

            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = startArc + step * i;
                float rad = currentAngle * Mathf.Deg2Rad;
                Vector3 point = new Vector3(radius * Mathf.Sin(rad), 0, radius * Mathf.Cos(rad));
                points[i] = point; 

                // Debug.Log($"Point {i}: {points[i]}");
            }

            return points;
        }

        
        LineRenderer CreateLineRenderer(Material material, string name)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.parent = robotTransform; // follow the robot
            lineObject.transform.localPosition = linePositionOffset; 
            lineObject.transform.localRotation = Quaternion.identity; 
            LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = lineWidth; // line width
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = false; 
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.loop = false;
            lineRenderer.numCapVertices = 2;
            lineRenderer.enabled = false;  
            return lineRenderer;
        }
    }
}
