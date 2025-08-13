using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient; 

public class HapticPathCreate : MonoBehaviour
{
    public HapticControl hapticControl; // 用于接收触觉输入的 HapticControl
    
    public LineRenderer lineRenderer;
    public Camera Camera;
    public int segmentCount = 20;         // 线段数量，增加到50以获得更平滑的曲线
    public float bendSpeed = 5f;          // 弯曲速度（调整为更适合线条平滑过渡的值）
    public float maxBendOffset = 45f;     // 最大弯曲偏移量（单位与输入数据一致）
    public float resetAngleThreshold = 0.01f;
    
    [Range(0f, 1f)]
    public float inputScale = 0.1f; // 输入缩放
    public float smoothingFactor = 0.2f; // 默认平滑因子
    private float smoothedInput = 0f;

    private float currentBendAngle = 0f;
    
    public Transform startPoint;  // 起点
    public Transform midPoint;    // 中点
    public Transform endPoint;    // 终点

    //private float currentHapticInput = 0f; // 上一次触觉输入

    void Start()
    {
        // 初始化 LineRenderer
        InitializeLineRenderer();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if(!DataManager.Instance.isWithDirectionLine) return;
        bool hasValidInput = false;

        double currentHapticInput = hapticControl.jointAngles[0];

        // 检查并处理新的触觉输入
        if (hapticControl != null && (currentHapticInput < -0.05 || currentHapticInput > 0.05))
        {
            // 获取触觉输入
            double hapticInput = hapticControl.position3[0]; 
            // 根据输入大小计算最大弯曲偏移量
            maxBendOffset = CalculateMaxBendOffset((float)hapticInput);
            // 缩放输入
            float scaledInput = (float)hapticInput * inputScale; 
            // 应用指数平滑
            smoothedInput = Mathf.Lerp(smoothedInput, scaledInput, smoothingFactor);

            currentBendAngle += (float)smoothedInput * bendSpeed * Time.deltaTime; // 乘以速度和时间增量

            currentBendAngle = Mathf.Clamp(currentBendAngle, -maxBendOffset, maxBendOffset); // 确保在 -maxBendOffset 到 maxBendOffset 之间

            if (Mathf.Abs(currentBendAngle) >= resetAngleThreshold)
            {
                hasValidInput = true;
            }else{
                lineRenderer.enabled = false;
            }
        }else{
            
            lineRenderer.enabled = false;
        }
        if(hasValidInput){
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;

                // 计算新的控制点
                Vector3 controlPoint1 = midPoint.position; // mid-point 作为第一个控制点
                Vector3 controlPoint2 = endPoint.position - endPoint.right * currentBendAngle;

                // 创建贝塞尔曲线并设置 LineRenderer 的位置
                for (int i = 0; i < segmentCount; i++)
                {
                    float t = (float)i / (segmentCount - 1);
                    Vector3 bezierPosition = CalculateBezierPoint(t, startPoint.position, controlPoint1, endPoint.position, controlPoint2);
                    lineRenderer.SetPosition(i, bezierPosition);
                }
            }
        }else{
            if (lineRenderer != null)
            {   
                currentBendAngle = 0f;
                lineRenderer.enabled = false;
            }
        }
    }

    private void InitializeLineRenderer()
    {

        lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.01f;
        lineRenderer.positionCount = segmentCount;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.startColor = Color.blue; 
        lineRenderer.endColor = Color.blue;
    }

    // 计算贝塞尔曲线位置
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // 起点
        p += 3 * uu * t * p1; // 第一个控制点
        p += 3 * u * tt * p2; // 第二个控制点
        p += ttt * p3;         // 终点
        return p;
    }

    public void ResetCurve()
    {
        // 重置弯曲角度
        currentBendAngle = 0f;
        
        // 可选：重置平滑输入
        smoothedInput = 0f;
        
        if(lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private float CalculateMaxBendOffset(float inputValue)
    {
        // 定义变化率的最小和最大值，根据实际情况调整
        float mininputValue = 0f;
        float maxinputValue = 35f; // 这是一个示例值，需要根据你的输入数据进行调整

        // 使用 Mathf.InverseLerp 将变化率归一化到 [0, 1] 之间
        float normalizedRate = Mathf.InverseLerp(mininputValue, maxinputValue, Mathf.Abs(inputValue));

        // 使用 Mathf.Lerp 将归一化的变化率映射到 [0.01, 0.08] 之间
        float newMaxBendOffset = Mathf.Lerp(0.01f, 0.08f, normalizedRate);

        // 确保结果在 [0.01, 0.08] 之间
        newMaxBendOffset = Mathf.Clamp(newMaxBendOffset, 0.01f, 0.08f);

        return newMaxBendOffset;
    }

}
