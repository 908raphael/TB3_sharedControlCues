using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class ResetInitialPose : UnityPublisher<MessageTypes.Geometry.PoseWithCovarianceStamped>
    {
        
        private MessageTypes.Geometry.PoseWithCovarianceStamped message;
        public bool newPos = false;
      
        //private MessageTypes.Std.Header header;

        protected override void Start()
        {
            base.Start();
            InitializeMessage();
            ResetPose();
        }
        
        private void InitializeMessage()
        {
            
            if(newPos)
            {
                message = new MessageTypes.Geometry.PoseWithCovarianceStamped()
                {
                    header = new MessageTypes.Std.Header
                    {
                        frame_id = "map"
                    },
                    pose = new MessageTypes.Geometry.PoseWithCovariance()
                    {
                        covariance = new double[36]
                    {
                        0.25, 0.0, 0.0, 0.0, 0.0, 0.0, 
                        0.0, 0.25, 0.0, 0.0, 0.0, 0.0, 
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.06853892326654787
                    },
                        pose = new MessageTypes.Geometry.Pose()
                        {
                            orientation = new MessageTypes.Geometry.Quaternion()
                            {
                                w = 7.549789954891874e-08,
                                x = 0,
                                y = 0,
                                z = 0.9999999999999971,
                            },
                            position = new MessageTypes.Geometry.Point()
                            {
                                x = 14.802102088928223,
                                y = -0.21421435475349426,
                                z = 0
                            }
                        }
                    }
                };

                
            }
            else
            {
                message = new MessageTypes.Geometry.PoseWithCovarianceStamped()
                {
                    header = new MessageTypes.Std.Header
                    {
                        frame_id = "map"
                    },
                    pose = new MessageTypes.Geometry.PoseWithCovariance()
                    {
                        covariance = new double[36]
                    {
                        0.25, 0.0, 0.0, 0.0, 0.0, 0.0,
                        0.0, 0.25, 0.0, 0.0, 0.0, 0.0,
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.0,
                        0.0, 0.0, 0.0, 0.0, 0.0, 0.06853892326654787
                    },
                        pose = new MessageTypes.Geometry.Pose()
                        {
                            orientation = new MessageTypes.Geometry.Quaternion()
                            {
                                w = 1,
                                x = 0,
                                y = 0,
                                z = 0.005000011650818053,
                            },
                            position = new MessageTypes.Geometry.Point()
                            {
                                x = -1.183135986328125,
                                y = 0.16419613361358643,
                                z = 0
                            }
                        }
                    }
                };
            }
        }

        
        public void ResetPose()
        {
            Publish(message);
            Debug.Log("Published reset initial pose message to ROS.");
        }

       
    }
}

