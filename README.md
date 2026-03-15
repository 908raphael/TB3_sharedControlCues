# MSc Dissertation – Human-Robot Teleoperation Simulation

This repository contains a Unity-based robot simulation integrated with a ROS backend. It demonstrates distributed control, sensor communication, and real-time interaction between Unity and ROS.

---

## 🚀 Getting Started

Follow the steps below to set up the project on your local machine (This guide is written specifically for Windows and Ubuntu 20.04).

### 📥 1. Clone the Repository

```bash
git clone https://github.com/imorange/Mobile-Robot-Project.git
cd Mobile-Robot-Project
```

### 📳 2. Connect the Haptic device to your machine

- [Read documentation and **install their drivers (OpenHaptics for Windows Developer Edition v3.5)**](https://support.3dsystems.com/s/article/OpenHaptics-for-Windows-Developer-Edition-v35?language=en_US)

- Run Touch Smart Setup and initialise Haptic device

### 🎮 3. Open the Unity Project

- Open [Unity Hub](https://unity.com/download)

- Click Add → Add Existing Project

- Select the folder Mobile-Robot-Project/MyUnityProject

- Open it with Unity 2021.3+ (or your required version)

### 🤖 4. Set Up the ROS Package (Ubuntu 20.04)

#### [Ubuntu Install of ROS Noetic](https://wiki.ros.org/noetic/Installation/Ubuntu) is essential. Follow the steps in the link if not been configured on your own machine.

```bash
# Clone the GitHub repository into your Catkin workspace
cd ~/catkin_ws/src
git clone https://github.com/RcO2Rob/Dis-Project.git

# Restructure the directory
cd ~/catkin_ws/src
mv Dis-Project/myproject .

# Modify move_base launch file
roscd turtlebot3_navigation/launch/
sudo vim move_base.launch

# Edit line 4 to <arg name="cmd_vel_topic" default="/nav_vel" />

# Build catkin workspace
cd ~/catkin_ws
catkin_make
source devel/setup.bash
```

### 📶 5. Set Up ROS# Unity Connection

```bash
# Install rosbridge_server in WSL
sudo apt update
sudo apt install ros-noetic-rosbridge-server

# Find IP address of WSL
hostname -I
```
#### Use that IP address in any RosConnector and modify the port address
<p align="center">
<img src="Images/RosconnectorGuide.png"/>
</p>

### 📶 6. Set Up Turtlebot3 Packages

#### [Turtlebot3 Packages](https://emanual.robotis.com/docs/en/platform/turtlebot3/quick-start/) are essential. Follow the steps in the link if not been configured on your own machine.

```bash
# Move map files to Ubuntu root directory. 
cd ~/catkin_ws/src/maps
mv room2.yaml ~/
mv room2.pgm ~/
```
#### Modify room2.pgm path in room2.yaml

<p align="center">
<img src="Images/yaml file modification.png"/>
</p>

```bash
# Move all shell scripts to catkin_ws, then launch shell script on a separate terminal
./run_experiment.sh
```

### ✅ 7. Run your experiment scene, and everything should be ready to go!

## Brief Scene Descriptions
Scenes Main Task A, B, and C differ by the location of target objects. In all four conditions (modes), they differ only in the clues presented.
- Condition A has no cues 
- Condition B has a haptic guidance force
- Condition C has a direction indicator line and an autonomy level indicator bar, and haptic guidance force
- Condition D combines the cues from B and C, and mini-map, collision detection lines, and anti-collision braking cue.
