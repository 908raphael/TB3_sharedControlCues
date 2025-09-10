using UnityEngine;

public class CurvedArrowController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float arrowLength = 5f;      // 箭头的总长度
    public float maxBendAngle = 45f;    // 箭头最大弯曲角度
    public int pointCount = 10;         // Line Renderer 的点数

    void Start()
    {
        // 初始化 Line Renderer 的点数
        lineRenderer.positionCount = pointCount;
    }

    void Update()
    {
        float localRotationY = transform.localEulerAngles.y;
        // 获取当前的弯曲角度（可以根据需要动态设置）
        //float currentBendAngle = maxBendAngle * Mathf.Sin(Time.time); // 示例：随时间弯曲变化

        float bendAngle = Mathf.Clamp(localRotationY, -maxBendAngle, maxBendAngle);
        float bendInRadians = bendAngle * Mathf.Deg2Rad;

        // 计算每个点的坐标，使箭头逐渐弯曲
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float bendFactor = Mathf.Sin(t * Mathf.PI); // 曲线弯曲度
            float x = Mathf.Sin(bendAngle * Mathf.Deg2Rad) * bendFactor * arrowLength * t;
            float z = Mathf.Cos(bendAngle * Mathf.Deg2Rad) * arrowLength * t;

            lineRenderer.SetPosition(i, new Vector3(x, 0, z));
        }
    }
}