using UnityEngine;
using System.Collections;
using TMPro;

namespace RosSharp.RosBridgeClient
{
    public class GoalStatusSubscriber : UnitySubscriber<MessageTypes.Actionlib.GoalStatusArray>
    {

        public GoalManager goalManager;
        public HapticControl hapticControl;
        private uint statusCode;
        private bool isProcessingGoal = false;
        private bool isProcessingObstruction = false;

        protected override void Start()
        {
            base.Start();
        }

        protected override void ReceiveMessage(MessageTypes.Actionlib.GoalStatusArray message)
        {
            //Debug.Log("it is receiving message.");
            //status = message.status_list[0].text;
            statusCode = message.status_list[0].status;
        }

        private void Update()
        {
            //HandleGoals();
            
        }

        private void HandleGoals()
        {
            if(DataManager.Instance.taskStart && DataManager.Instance.isAuto){
                if (statusCode == 3 && !isProcessingGoal)
                {
                    // haptic cues to remind the user that the goal has been reached
                    hapticControl.SetVibrationForce(0.5f);
                    StartCoroutine(HandleGoalReached());
                    
                }
                else if (statusCode >= 4 && !isProcessingObstruction)
                {
                    // haptic cues to remind the user that the goal has been obstructed
                    hapticControl.SetVibrationForce(0.5f);
                    StartCoroutine(HandleGoalObstructed());
                }
            }
        }

        private IEnumerator HandleGoalObstructed()
        {
            isProcessingObstruction = true;
            Debug.Log("Send the next goal point.");
            goalManager.OnGoalReached();
            yield return new WaitForSeconds(10); 
            isProcessingObstruction = false;
        }

        private IEnumerator HandleGoalReached()
        {
            isProcessingGoal = true;
            Debug.Log("Send the next goal point.");
            goalManager.OnGoalReached();
            yield return new WaitForSeconds(10); 
            isProcessingGoal = false;
        }

            // uint8 PENDING=0
            // uint8 ACTIVE=1
            // uint8 PREEMPTED=2
            // uint8 SUCCEEDED=3
            // uint8 ABORTED=4
            // uint8 REJECTED=5
            // uint8 PREEMPTING=6
            // uint8 RECALLING=7
            // uint8 RECALLED=8
            // uint8 LOST=9
    }
}