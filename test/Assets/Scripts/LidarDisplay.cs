using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace RosSharp.RosBridgeClient
{

    public class LidarDisplay : MonoBehaviour
    {
        private Texture2D mapTexture;
        public RawImage mapImage;
        public Transform robotTransform;
        public float maxDistance = 10f;
        public LaserScanReader laserScanReader; // 您的激光雷达读取类

        void Start()
        {
            mapTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            mapImage.texture = mapTexture;
        }

        void Update()
        {
            // 清除之前的绘制
            ClearMap();

            // 获取激光雷达数据
            float[] ranges = laserScanReader.Scan();

            // 确定激光雷达的角度参数
            float startAngle = -90f; // 起始角度
            float endAngle = 90f;    // 结束角度
            float angleIncrement = (endAngle - startAngle) / (ranges.Length - 1);

            // 处理雷达数据
            List<Vector2> lidarPoints = new List<Vector2>();
            for (int i = 0; i < ranges.Length; i++)
            {
                float distance = ranges[i];
                float angle = startAngle + i * angleIncrement;

                // 忽略无效或超出范围的数据
                if (distance <= 0 || distance > maxDistance)
                    continue;

                // 将角度转换为弧度
                float rad = angle * Mathf.Deg2Rad;

                // 计算本地坐标
                float x = distance * Mathf.Cos(rad);
                float y = distance * Mathf.Sin(rad);

                lidarPoints.Add(new Vector2(x, y));
            }

            // 绘制障碍物点
            foreach (var point in lidarPoints)
            {
                // 将本地坐标转换为世界坐标
                Vector3 localPos = new Vector3(point.x, 0, point.y);
                Vector3 worldPos = robotTransform.TransformPoint(localPos);

                // 将世界坐标转换为纹理坐标
                Vector2 texturePos = WorldToTextureCoords(worldPos);

                // 计算颜色强度
                float distance = point.magnitude;
                Color color = Color.Lerp(Color.red, Color.green, distance / maxDistance);

                // 在纹理上绘制点
                DrawPointOnTexture(texturePos, color);
            }

            // 更新纹理
            mapTexture.Apply();
        }

        void ClearMap()
        {
            // 将纹理清空为黑色或其他底色
            Color[] clearColors = new Color[mapTexture.width * mapTexture.height];
            for (int i = 0; i < clearColors.Length; i++)
            {
                clearColors[i] = Color.black;
            }
            mapTexture.SetPixels(clearColors);
        }

        Vector2 WorldToTextureCoords(Vector3 worldPos)
        {
            // 根据地图比例将世界坐标映射到纹理坐标
            float scale = mapTexture.width / (maxDistance * 2); // 假设纹理覆盖的范围为 2 * maxDistance
            float x = (worldPos.x - robotTransform.position.x) * scale + mapTexture.width / 2;
            float y = (worldPos.z - robotTransform.position.z) * scale + mapTexture.height / 2;
            return new Vector2(x, y);
        }

        void DrawPointOnTexture(Vector2 pos, Color color)
        {
            int x = Mathf.Clamp((int)pos.x, 0, mapTexture.width - 1);
            int y = Mathf.Clamp((int)pos.y, 0, mapTexture.height - 1);
            mapTexture.SetPixel(x, y, color);
        }
    }

}