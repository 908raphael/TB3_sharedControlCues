using UnityEngine;
using UnityEngine.UI;

public class Blinking : MonoBehaviour
{
    public float blinkInterval = 0.5f; // 闪烁的时间间隔
    private Image imageComponent;
    private bool isVisible = true;
    private float timer;

    void Start()
    {
        // 获取 Image 组件
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            Debug.LogError("BlinkingImage: No Image component found on this GameObject.");
        }
    }

    void Update()
    {
        if (imageComponent == null)
            return;

        // 计时并切换可见性
        timer += Time.deltaTime;
        if (timer >= blinkInterval)
        {
            isVisible = !isVisible;
            imageComponent.enabled = isVisible; // 切换 Image 的显示状态
            timer = 0f;
        }
    }
}
