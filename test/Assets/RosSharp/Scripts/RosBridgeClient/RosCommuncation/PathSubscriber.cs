using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class PathSubscriber : UnitySubscriber<MessageTypes.Nav.Path>
    {   
        public HapticPlugin hapticPlugin;
        public LineRenderer lineRenderer;
        public Transform origin; // The origin of the path in Unity coordinates
        
        public Transform robotPosition;
        public float forceScale = 1f;
        public float smoothingFactor = 0.1f;
        private Vector3 currentForce;

        public float lineWidth = 0.01f;
        public Color lineColor = Color.green;

        public List<Vector3> pathPoints = new List<Vector3>();
        private MessageTypes.Geometry.PoseStamped[] poses;
        private bool isMessageReceived = false;
        
        private double[] direction = { 0.0, 0.0, 0.0 };
        public double force = 0.5;

        protected override void Start()
        {
            base.Start();
            InitializeLineRenderer();
        }

        private void InitializeLineRenderer()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.widthMultiplier = lineWidth;
                lineRenderer.positionCount = 0;
                lineRenderer.useWorldSpace = true;
                lineRenderer.loop = false;
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
            }
        }

        private void Update()
        {
            if (isMessageReceived && DataManager.Instance.isWithAssitanceCues)
                ProcessMessage();
            
        }
        
        
        public Vector3 Unity2ROS(Vector3 point)
        {
            // 根据公式：ROS坐标 = (z, -x, y)
            return new Vector3(point.z, -point.x, point.y);
        }
        private Vector3 Ros2Unity(Vector3 point)
        {
            // transform the position to Unity coordinate system
            Vector3 position = new Vector3(-point.y, point.z, point.x);
            return position;
        }

        public List<Vector3> GetPathPoints()
        {
            if (pathPoints.Count > 0)
                return pathPoints;
            else
                return null;
        }

        protected override void ReceiveMessage(MessageTypes.Nav.Path message)
        {
            poses = message.poses;
            if (poses != null)
            {
                isMessageReceived = true;
            }
        }
        
        private void ProcessMessage()
        {
            isMessageReceived = false;
            pathPoints.Clear();
            foreach (var poseStamped in poses)
            {
                var position = new Vector3(
                    (float)poseStamped.pose.position.x,
                    (float)poseStamped.pose.position.y,
                    (float)poseStamped.pose.position.z
                );
                //double theta = poseStamped.pose.orientation.x;
                // transform the position to Unity coordinate system
                position = new Vector3(-position.y, 0.03f, position.x);

                if (origin != null)
                {
                    position = origin.TransformPoint(position);
                }
                

                pathPoints.Add(position);
            }

            if (pathPoints.Count > 0)
            {
                
                lineRenderer.positionCount = pathPoints.Count;
                lineRenderer.SetPositions(pathPoints.ToArray());
                
            }
            else
            {
                Debug.Log("PathSubscriber: No points in the path message.");
                lineRenderer.positionCount = 0;
            }
        }
    }
}

