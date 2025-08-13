using System.Collections.Generic;
using UnityEngine;
using RosSharp.RosBridgeClient;

public class RealPathCreate : MonoBehaviour
{
    // LineRenderer 用于绘制真实路径
    public LineRenderer lineRenderer;

    // 订阅路径和 Twist（速度）数据的组件
    public PathSubscriber pathSubscriber; // 获取导航路径点
    public TwistSubscriber twistSubscriber; // 获取当前速度输入

    // LineRenderer 的属性
    public float lineWidth = 0.03f;
    public Color lineColor = Color.green;

    // 路径点列表
    private List<Vector3> realPathPoints = new List<Vector3>();
    private List<Vector3> navigationPoints = new List<Vector3>();

    // 当前的线速度和角速度（已经是加权后的结果）
    private float currentLinear; // 线速度 linear.x
    private float currentAngular; // 角速度 angular.z

    // 当前机器人的位置和朝向角度
    private Vector3 currentPosition;
    private float theta = 0f; // 朝向角度（弧度）

    void Start()
    {
        // 初始化 LineRenderer
        InitializeLineRenderer();

        // 获取初始导航路径点
        navigationPoints = pathSubscriber.GetPathPoints();

        if (navigationPoints == null || navigationPoints.Count == 0)
        {
            Debug.LogError("Navigation points are not set or empty!");
            return;
        }

        //currentLinear = twistSubscriber.GetLinear; // 只使用 linear.x
        //currentAngular = twistSubscriber.GetAngular; // 只使用 angular.z

        // 设置初始位置为导航路径的第一个点
        currentPosition = navigationPoints[0];
        realPathPoints.Add(currentPosition);

        // 设置 LineRenderer 的第一个点
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, currentPosition);
    }

    void InitializeLineRenderer()
    {
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = lineWidth;
            lineRenderer.positionCount = 0;
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;
            lineRenderer.startColor = lineColor;
            lineRenderer.endColor = lineColor;
        }
    }

    void Update()
    {
        navigationPoints = pathSubscriber.GetPathPoints();

        Vector3 linearVector = twistSubscriber.GetLinear();
        Vector3 angularVector = twistSubscriber.GetAngular();
        currentLinear = linearVector.x;    // 只使用 linear.x
        currentAngular = angularVector.z;  // 只使用 angular.z

        // 获取时间间隔
        float dt = Time.deltaTime;

        // 计算新的位置
        // 假设 linear.x 是前进速度，角速度是围绕 Z 轴旋转
        float deltaX = currentLinear * Mathf.Cos(theta) * dt;
        float deltaY = currentLinear * Mathf.Sin(theta) * dt;
        currentPosition += new Vector3(deltaX, deltaY, 0f);
        theta += currentAngular * dt;



        // 将新的位置添加到路径点列表
        realPathPoints.Add(currentPosition);

        if (realPathPoints.Count > navigationPoints.Count)
        {
            realPathPoints.RemoveAt(0);
        }

        // 更新 LineRenderer
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        lineRenderer.positionCount = realPathPoints.Count;
        lineRenderer.SetPosition(realPathPoints.Count - 1, realPathPoints[realPathPoints.Count - 1]);
    }
}
