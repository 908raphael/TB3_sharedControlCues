// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using RosSharp.RosBridgeClient;

// public class MiniMapController : MonoBehaviour, IPointerClickHandler
// {
//     public GameObject enlargedMapPanel; 

//     public GameObject indicator;
//     private TwistSubscriber twistSubscriber;

//     void Start()
//     {
//         GameObject twistScript = GameObject.Find("RosConnector");
//         TwistSubscriber twistSubscriber = twistScript.GetComponent<TwistSubscriber>();
//     }

//     public void OnPointerClick(PointerEventData eventData)
//     {
//         enlargedMapPanel.SetActive(true);
//         twistSubscriber.enabled = false;
        
//         indicator.SetActive(true);

//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RosSharp.RosBridgeClient;

public class MiniMapController : MonoBehaviour, IPointerClickHandler
{
    public GameObject enlargedMapPanel; 
    public GameObject indicator;

    private TwistSubscriber twistSubscriber;

    void Start()
    {
        GameObject twistScript = GameObject.Find("RosConnector");
        
        if (twistScript != null)
        {
            // 尝试获取 TwistSubscriber 组件
            twistSubscriber = twistScript.GetComponent<TwistSubscriber>();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 检查 enlargedMapPanel 是否已赋值
        if (enlargedMapPanel != null)
        {
            enlargedMapPanel.SetActive(true);
        }

        // 检查 twistSubscriber 是否已赋值
        if (twistSubscriber != null)
        {
            twistSubscriber.enabled = false;
        }
        // 检查 indicator 是否已赋值
        if (indicator != null)
        {
            indicator.SetActive(true);
        }
    }

    public void CloseEnlargedMap()
    {
        enlargedMapPanel.SetActive(false);  
        twistSubscriber.enabled = true;
    }
}
