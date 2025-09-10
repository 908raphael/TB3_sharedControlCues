using UnityEngine;

public class CubeController : MonoBehaviour
{
    public float moveSpeed = 5f; // 控制Cube的移动速度

    void Update()
    {
        // 获取WASD键输入
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D键
        float verticalInput = Input.GetAxis("Vertical");     // W/S键

        // 计算移动方向
        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);

        // 更新Cube的位置
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);
    }
}
