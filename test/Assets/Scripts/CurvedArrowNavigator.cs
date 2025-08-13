using UnityEngine;
using System.Collections.Generic;

public class CurvedArrowNavigator : MonoBehaviour
{
    [Header("导航线设置")]
    public int linePoints = 50;
    public float lineWidth = 0.5f;
    public float arrowLength = 20f;
    public float heightAboveGround = 1f;
    public float predictionTime = 2f;
    
    [Header("视觉效果")]
    public Material arrowMaterial;
    public Gradient lineColor;
    public float pulseSpeed = 1f;
    public float pulseAmount = 0.2f;
    
    [Header("转向预测")]
    public float steeringWeight = 1f;
    public float smoothing = 0.5f;
    
    private LineRenderer lineRenderer;
    private Rigidbody vehicleRb;
    private List<Vector3> controlPoints;
    private Vector3 lastPosition;
    private Vector3 lastForward;
    private bool initialized = false;
    
    void Start()
    {
        CreateDefaultMaterial();
        SetupDefaultGradient();
        InitializeLineRenderer();
        
        vehicleRb = GetComponent<Rigidbody>();
        if (vehicleRb == null)
        {
            Debug.LogWarning("没有找到Rigidbody组件，添加一个默认的Rigidbody");
            vehicleRb = gameObject.AddComponent<Rigidbody>();
        }
        
        controlPoints = new List<Vector3>();
        lastPosition = transform.position;
        lastForward = transform.forward;
        initialized = true;
        
        // 立即更新一次箭头位置
        UpdateControlPoints();
        DrawCurvedArrow();
    }
    
    void CreateDefaultMaterial()
    {
        if (arrowMaterial == null)
        {
            Debug.Log("创建默认材质");
            // 检查是否使用URP
            if (Shader.Find("Universal Render Pipeline/Unlit") != null)
            {
                arrowMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            }
            else
            {
                // 降级使用标准着色器
                arrowMaterial = new Material(Shader.Find("Standard"));
            }
            arrowMaterial.SetColor("_BaseColor", Color.cyan);
            arrowMaterial.SetColor("_EmissionColor", Color.cyan);
        }
    }
    
    void SetupDefaultGradient()
    {
        if (lineColor == null || lineColor.colorKeys.Length == 0)
        {
            Debug.Log("设置默认颜色渐变");
            lineColor = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.cyan, 0.0f);
            colorKeys[1] = new GradientColorKey(Color.blue, 1.0f);
            
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f);
            
            lineColor.SetKeys(colorKeys, alphaKeys);
        }
    }
    
    void InitializeLineRenderer()
    {
        Debug.Log("初始化LineRenderer");
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        lineRenderer.material = arrowMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = linePoints;
        lineRenderer.useWorldSpace = true;
        lineRenderer.colorGradient = lineColor;
        
        // 确保LineRenderer可见
        lineRenderer.enabled = true;
        
        // 设置一些其他重要的属性
        lineRenderer.receiveShadows = false;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.allowOcclusionWhenDynamic = false;
        
        // 测试线条是否正确显示
        Vector3[] testPoints = new Vector3[2];
        testPoints[0] = transform.position;
        testPoints[1] = transform.position + Vector3.forward * 5f;
        lineRenderer.SetPositions(testPoints);
    }
    
    void Update()
    {
        if (!initialized) return;
        
        UpdateControlPoints();
        DrawCurvedArrow();
        AnimateArrow();
    }
    
    void UpdateControlPoints()
    {
        controlPoints.Clear();
        
        Vector3 startPos = transform.position + Vector3.up * heightAboveGround;
        controlPoints.Add(startPos);
        
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        Vector3 steering = (transform.forward - lastForward) / Time.deltaTime;
        
        float predictionDistance = Mathf.Max(velocity.magnitude * predictionTime, 5f); // 确保最小长度
        Vector3 predictedDirection = Vector3.Lerp(transform.forward, 
            transform.forward + steering * steeringWeight, 
            smoothing);
        
        Vector3 midPoint = startPos + predictedDirection * (predictionDistance * 0.5f);
        controlPoints.Add(midPoint);
        
        Vector3 endPoint = startPos + predictedDirection * predictionDistance;
        controlPoints.Add(endPoint);
        
        lastPosition = transform.position;
        lastForward = transform.forward;
    }
    
    void DrawCurvedArrow()
    {
        if (controlPoints.Count < 3) return;
        
        Vector3[] points = new Vector3[linePoints];
        
        for (int i = 0; i < linePoints; i++)
        {
            float t = i / (float)(linePoints - 1);
            points[i] = CalculateBezierPoint(t, controlPoints[0], 
                controlPoints[1], controlPoints[2]);
        }
        
        lineRenderer.SetPositions(points);
    }
    
    Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        return uu * p0 + 2 * u * t * p1 + tt * p2;
    }
    
    void AnimateArrow()
    {
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        lineRenderer.startWidth = lineWidth * pulse;
    }
    
    void OnValidate()
    {
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth * 0.2f;
        }
    }
    
    void OnDrawGizmos()
    {
        // 在Scene视图中绘制控制点，便于调试
        if (controlPoints != null && controlPoints.Count >= 3)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 point in controlPoints)
            {
                Gizmos.DrawSphere(point, 0.3f);
            }
        }
    }
}