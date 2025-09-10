using UnityEngine;

public class CameraFixedFollow : MonoBehaviour
{
    public Transform robot; // 拖放机器人对象到此字段
    public Vector3 offset = new Vector3(0, 1.5f, 0); // 相机相对于机器人的偏移量

    void LateUpdate()
    {
        if (robot != null)
        {
            // 将相机的位置和机器人的位置同步，加上偏移
            transform.position = robot.position + robot.TransformDirection(offset);

            // 将相机的旋转同步为机器人的旋转
            transform.rotation = robot.rotation;
        }
    }
}
