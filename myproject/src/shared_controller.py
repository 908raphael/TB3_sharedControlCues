#!/usr/bin/env python

import rospy
from geometry_msgs.msg import Twist
from std_msgs.msg import Float32
from std_msgs.msg import Bool
from sensor_msgs.msg import LaserScan
import numpy as np
from std_msgs.msg import Float32MultiArray

FRONT_ANGLE = 0
LEFT_ANGLE = 90
RIGHT_ANGLE = 270
BACK_ANGLE = 180


class SharedControl:
    def __init__(self):
        # Initialize the node, subscribers, publisher, and rate
        rospy.init_node('shared_control', anonymous=True)
        
        self.navigation_cmd = Twist()
        self.haptic_cmd = Twist()

        self.lidar_data = LaserScan()
        self.isTeleop = Bool()
        self.isAuto = Bool()
        self.isFullControl = Bool()
        self.kCN = Float32()
        self.CB = Bool()

        self.arbitration_value = 0
        
        self.navigation_sub = rospy.Subscriber('/nav_vel', Twist, self.navigation_callback)
        self.haptic_sub = rospy.Subscriber('/haptic_vel', Twist, self.haptic_callback)
        self.lidar_sub = rospy.Subscriber('/scan', LaserScan, self.lidar_callback)
        self.teleop_sub = rospy.Subscriber('/tele_code', Bool, self.teleop_callback)
        self.auto_sub = rospy.Subscriber('/auto_code', Bool, self.auto_callback)
        self.full_control_sub = rospy.Subscriber('/full_conrtol_code', Bool, self.full_control_callback)
        self.kn_sub = rospy.Subscriber('/KN', Float32, self.KN_callback)
        self.cb_sub = rospy.Subscriber('/CB', Bool, self.CB_callback)
        # self.switch_mode_sub = rospy.Subscriber('/switch_mode', Bool, self.switch_mode_callback)

        self.cmd_pub = rospy.Publisher('/cmd_vel', Twist, queue_size=10)
        self.arbitration_value_pub = rospy.Publisher('/arbitration_value', Float32, queue_size=10)
        self.input_cmd_vel_pub = rospy.Publisher('/input_value', Twist, queue_size=10)
        self.nip_pub = rospy.Publisher('/NIP', Float32MultiArray, queue_size=10)
        self.inter_pub = rospy.Publisher('/intercept', Float32, queue_size=10)
        self.kHN_pub = rospy.Publisher('/kHN', Float32, queue_size=10)
        #self.nipZ_pub = rospy.Publisher('/NIPZ', Float32, queue_size=10)
        self.rate = rospy.Rate(10)
    
    
    def lidar_callback(self, msg):
        self.lidar_data = msg
    
        
    def front_lidar_data_processing(self, data):

        if len(data.ranges) != 360:
            rospy.logwarn("Expected 360 ranges, but got {}".format(len(data.ranges)))
            return 0.0  
        
        front_safety = self.areas_processing(data, FRONT_ANGLE)
        return front_safety

    def back_lidar_data_processing(self, data):

        if len(data.ranges) != 360:
            rospy.logwarn("Expected 360 ranges, but got {}".format(len(data.ranges)))
            return 0.0  
        
        back_safety = self.areas_processing(data, BACK_ANGLE)
        return back_safety



    def areas_processing(self, data, angle):
        # get the front 45 degrees of the lidar data
        # front_indices = list(range(315, 360)) + list(range(0, 46))  # [315, 359] + [0, 45]
        area_indices = list(range(angle-45, angle+45))
        ##print(area_indices)
        
        # get the front ranges
        area_ranges = [data.ranges[i] for i in area_indices]
        
        # filter invalid values（inf or NaN）
        valid_area_ranges = [r for r in area_ranges if not (np.isinf(r) or np.isnan(r)) and r != 0]
        
        if len(valid_area_ranges) == 0:    
            safety = 1.0
        else:
            # find the minimum distance in the front
            min_area_distance = min(valid_area_ranges)
            # print("Min front distance: {}".format(min_front_distance))

            current_speed = self.navigation_cmd.linear.x

            # safe distance is proportional to the current speed
            safe_distance = max(0.25, 0.5 * current_speed) # minimum safe distance is 0.2 meters

            max_distance = data.range_max # maximum range of the lidar equals to 3.5 meters 
            
            if min_area_distance <= safe_distance:
                safety = 0.0
            else:
                # linearly interpolate the safety value
                safety = min(1.0, (min_area_distance - safe_distance) / (max_distance - safe_distance))
        #print("Safety value: {}".format(safety))

        return safety

    def density_processing(self, data):
        pass

    def KN_callback(self,msg):
        self.kCN = msg

    def CB_callback(self,msg):
        self.CB = msg.data

    def teleop_callback(self, msg):
        self.isTeleop = msg.data

    def auto_callback(self, msg):
        self.isAuto = msg.data
    
    def full_control_callback(self, msg):
        self.isFullControl = msg.data
        
    def navigation_callback(self, msg):
        self.navigation_cmd = msg

    def haptic_callback(self, msg):
        self.haptic_cmd = msg
        
    def run(self):
        kCN = 1
        kHN = 0
        NIPE = Float32MultiArray()
        NIP = [0.0,0.0,0.0]
        CIP = [0.0,0.0,0.0]
        HIP = [0.0,0.0,0.0]
        alpha = 1
        while not rospy.is_shutdown():
            
            # print(self.isTeleop, self.isAuto, self.isFullControl)
            # Process the lidar data
            #if(self.isFullControl == True and self.isTeleop == False and self.isAuto == False):
            #    alpha = 0
            #elif(self.isAuto == True and self.isTeleop == False and self.isFullControl == False):
            #    alpha = 1
            #else:
            #    alpha = self.lidar_data_processing(self.lidar_data)
                 
            front_safty = self.front_lidar_data_processing(self.lidar_data)
            back_safty = self.back_lidar_data_processing(self.lidar_data)

            combined_cmd = Twist()
            #if(self.isFullControl == True and safty > 0.2):
            #    combined_cmd.linear.x = 0.6 * self.haptic_cmd.linear.x
            #    combined_cmd.angular.z = 0.6 * self.haptic_cmd.angular.z
            #else:
            #    combined_cmd.linear.x = alpha * (self.navigation_cmd.linear.x) + (1 - alpha) * self.haptic_cmd.linear.x
            #    combined_cmd.angular.z = alpha * (self.navigation_cmd.angular.z) + (1 - alpha) * self.haptic_cmd.angular.z
            #combined_cmd.linear.x = alpha * (self.navigation_cmd.linear.x) + (1 - alpha) * self.haptic_cmd.linear.x
            #combined_cmd.angular.z = alpha * (self.navigation_cmd.angular.z) + (1 - alpha) * self.haptic_cmd.angular.z
            kCN = float(self.kCN.data) 
            kHN = 1 - kCN

            CIP[2] = -(self.navigation_cmd.linear.x * 500)
            CIP[0] = self.navigation_cmd.angular.z * 300
            HIP[2] = -(self.haptic_cmd.linear.x * 500)  
            HIP[0] = self.haptic_cmd.angular.z * 300
            #CIP_force = KCN * 
            

            



            #NIP[2] = (CIP[2] * (1 - ((1 - kCN) * intercep))) + ((1 - kCN) * intercep * HIP[2])
            #NIP[0] = (CIP[0] * (1 - ((1 - kCN) * intercep))) + ((1 - kCN) * intercep * HIP[0])

            
            
                #NIP[2] = (CIP[2] * (1 - ((1 - kCN) * intercep))) + ((1 - kCN) * intercep * HIP[2])
                #NIP[0] = (CIP[0] * (1 - ((1 - kCN) * intercep))) + ((1 - kCN) * intercep * HIP[0])

                #NIP[2] *= intercep

            NIP[2] = (CIP[2] * kCN) + (kHN * HIP[2])
            NIP[0] = (CIP[0] * kCN) + (kHN * HIP[0])
            intercep = 1
            inter = False
            if(front_safty < 0.1 and NIP[2] < 0 and kCN < 1 and self.CB):
                intercep = max(0.1, front_safty / 0.1)
                inter = True
                #kHN *= intercep
                #kCN = 1 - kHN
                NIP[2] *= intercep
            elif(back_safty < 0.1 and NIP[2] > 0 and kCN < 1 and self.CB):
                intercep = max(0.03, back_safty / 0.1)
                NIP[2] *= intercep
                inter = True

            #NIP[0] = (CIP[0] * kCN) + ((1 - kCN) * HIP[0])


            #if(safety < 1):
                #NIP[2] = NIP[2] * safety
                #NIP[0] = NIP[0] * safety
            #print(safty)

            NIPE.data = NIP
            combined_cmd.linear.x = -NIP[2] / 500
            #combined_cmd.linear.y = -100
            combined_cmd.angular.z = NIP[0] / 300
            self.cmd_pub.publish(combined_cmd)
            #self.cmd_pub.publish(self.haptic_cmd)
            self.arbitration_value_pub.publish(kHN * intercep)
            self.input_cmd_vel_pub.publish(self.haptic_cmd)
            self.nip_pub.publish(NIPE)
            self.inter_pub.publish(intercep)
            self.kHN_pub.publish(kHN)
            #self.nipZ_pub.publish(NIP[2])
            #print("vel.x: {}".format(combined_cmd.linear.x))
            #print("vel.z: {}".format(combined_cmd.angular.z))
            #print("NIP.x: {}".format(NIP[0]))
            #print(kHN)  
            self.rate.sleep()
            





        


if __name__ == '__main__':
    try:
        shared_control = SharedControl()
        shared_control.run()
    except rospy.ROSInterruptException:
        pass