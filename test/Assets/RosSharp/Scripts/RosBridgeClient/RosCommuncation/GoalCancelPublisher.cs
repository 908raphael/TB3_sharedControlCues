using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    
    public class GoalCanclePublisher : UnityPublisher<MessageTypes.Actionlib.GoalID>
    {
        //public Transform PublishedTransform;
        //public string FrameId = "Unity";

        // public GameObject indicator;

        private MessageTypes.Actionlib.GoalID message;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
        }

        private void InitializeMessage()
        {
            message = new MessageTypes.Actionlib.GoalID
            {
                id = ""
            };
        }

        public void CancelGoal()
        {
            Publish(message);
            //Debug.Log("Published cancel goal message to ROS.");
            // indicator.SetActive(false);
        }

    }
}
