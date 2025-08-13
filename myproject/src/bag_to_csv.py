#!/usr/bin/env python3
"""
bag_to_csv.py  <bagfile>  <outdir>  <topic1> [topic2 ...]
  • Writes one CSV per topic
  • Handles Twist, Odometry, LaserScan, MoveBaseActionGoal, std_msgs.*
  • Falls back to str(msg) for unknown types
"""

import rosbag, csv, sys, re, datetime, pathlib, inspect
from move_base_msgs.msg import MoveBaseActionGoal

def bag_start_stamp(bag, bagfile):
    # 1) try to grab YYYY-MM-DD-HH-MM-SS from filename
    m = re.search(r'\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}', bagfile)
    if m:
        return m.group(0)
    # 2) fallback to bag header time
    ts = datetime.datetime.utcfromtimestamp(bag.get_start_time())
    return ts.strftime('%Y-%m-%d-%H-%M-%S')


def make_header(msg):
    if hasattr(msg, 'linear') and hasattr(msg, 'angular'):          # Twist
        return ['stamp','lin_x','lin_y','lin_z','ang_x','ang_y','ang_z']
    if hasattr(msg, 'pose') and hasattr(msg, 'twist'):              # Odometry
        return ['stamp','pos_x','pos_y','pos_z',
                        'ori_x','ori_y','ori_z','ori_w',
                        'lin_x','ang_z']
    if hasattr(msg, 'ranges'):                                      # LaserScan
        return ['stamp'] + [f'range_{i}' for i in range(len(msg.ranges))]
    if isinstance(msg, MoveBaseActionGoal):                         # MoveBase goal
        return ['stamp','goal_x','goal_y','goal_z',
                        'ori_x','ori_y','ori_z','ori_w']
    if hasattr(msg, 'data'):                                        # std_msgs/*
        return ['stamp','data']
    return ['stamp','raw']

def write_row(writer, t, msg):
    if hasattr(msg, 'linear') and hasattr(msg, 'angular'):          # Twist
        w = msg
        writer.writerow([t, w.linear.x, w.linear.y, w.linear.z,
                            w.angular.x, w.angular.y, w.angular.z])

    elif hasattr(msg, 'pose') and hasattr(msg, 'twist'):            # Odometry
        p = msg.pose.pose
        o = p.orientation
        writer.writerow([t, p.position.x, p.position.y, p.position.z,
                            o.x, o.y, o.z, o.w,
                            msg.twist.twist.linear.x,
                            msg.twist.twist.angular.z])

    elif hasattr(msg, 'ranges'):                                    # LaserScan
        writer.writerow([t] + list(msg.ranges))

    elif isinstance(msg, MoveBaseActionGoal):                       # Goal
        p = msg.goal.target_pose.pose.position
        o = msg.goal.target_pose.pose.orientation
        writer.writerow([t, p.x, p.y, p.z, o.x, o.y, o.z, o.w])

    elif hasattr(msg, 'data'):                                      # std_msgs/*
        writer.writerow([t, msg.data])

    else:                                                           # Fallback
        writer.writerow([t, str(msg)])

# ---------- main ----------
if len(sys.argv) < 4:
    print("Usage: bag_to_csv.py <bagfile> <outdir> <topic1> [topic2 ...]")
    sys.exit(1)

bagfile, outdir, *topics = sys.argv[1:]
out_dir = pathlib.Path(outdir)
out_dir.mkdir(parents=True, exist_ok=True)


with rosbag.Bag(bagfile) as bag:
    session_stamp = bag_start_stamp(bag, bagfile)

    for topic in topics:
        messages = list(bag.read_messages(topics=[topic]))
        if not messages:
            print(f"No messages on {topic}. Skipping.")
            continue

        session_dir = out_dir / session_stamp
        session_dir.mkdir(exist_ok=True)
        
        csv_file = session_dir / f"{topic.strip('/').replace('/','_')}_{session_stamp}.csv"
        with open(csv_file, 'w', newline='') as f:
            w = csv.writer(f)
            w.writerow(make_header(messages[0][1]))  # header from first msg
            for _, msg, t in messages:
                write_row(w, t.to_sec(), msg)
        print(f"wrote {csv_file} ({len(messages)} msgs)")
