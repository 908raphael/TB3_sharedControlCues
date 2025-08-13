using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideMinimap : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject target;
    CanvasGroup canvasGroup;

    void Start()
    {
        canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();  // 自动添加 CanvasGroup
        }
    }

    void Update()
    {
        if (DataManager.Instance.isWithMap)
        {
            canvasGroup.alpha = 1;  // 显示 UI
            canvasGroup.blocksRaycasts = true;
        }
        else
        {
            canvasGroup.alpha = 0;  // 隐藏 UI
            canvasGroup.blocksRaycasts = false;
        }
    }


}
