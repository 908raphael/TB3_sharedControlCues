#!/bin/bash
STAMP=$(date +%Y-%m-%d-%H-%M-%S) 
BAG_NAME="session_$STAMP"
BAG_PATH="/home/raphael/catkin_ws/bags/$BAG_NAME.bag"
roslaunch experiment.launch times:=$STAMP "$@"


roslaunch bag_to_csv.launch bag_file:=$BAG_PATH