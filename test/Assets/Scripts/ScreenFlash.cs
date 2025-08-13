
using UnityEngine;

public class ScreenFlash : MonoBehaviour
{
    public Color flashColor = Color.red;   // color
    public float flashDuration = 0.1f;     // flashing time
    private bool isFlashing = false;
    private float flashTimer = 0f;

    public GameObject screenOverlay;

    void Update()
    {
        if (isFlashing)
        {
            flashTimer += Time.deltaTime;
            if (flashTimer >= flashDuration)
            {
                // 切换颜色
                screenOverlay.SetActive(!screenOverlay.activeSelf);
                flashTimer = 0f;
            }
        }
    }

    public void StartFlash()
    {
        isFlashing = true;
        flashTimer = 0f;
        screenOverlay.SetActive(true);  // 启动闪烁效果
    }

    public void StopFlash()
    {
        isFlashing = false;
        screenOverlay.SetActive(false);  // 停止闪烁效果
    }
}
