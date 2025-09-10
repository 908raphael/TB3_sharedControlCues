using UnityEngine;

public class MinimapIndicator : MonoBehaviour
{
    public Transform target; // 需要在小地图上显示的对象
    public RectTransform miniMapRect; // 小地图的RectTransform
    public float mapScale = 1f; // 小地图的缩放比例

    private Vector3 offset;

    void Start()
    {
        if (target != null && miniMapRect != null)
        {
            // 计算目标对象相对于小地图中心的偏移
            offset = target.position;
        }
    }

    void Update()
    {
        if (target != null && miniMapRect != null)
        {
            // 将世界坐标转换为小地图坐标
            Vector3 relativePos = target.position - transform.parent.position;
            Vector2 miniMapPos = new Vector2(relativePos.x, relativePos.z) * mapScale;

            // 限制指示器在小地图范围内
            miniMapPos = Vector2.ClampMagnitude(miniMapPos, miniMapRect.sizeDelta.x / 2);

            // 设置指示器的位置
            transform.localPosition = miniMapPos;
        }
    }
}
