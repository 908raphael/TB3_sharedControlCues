using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RosSharp.RosBridgeClient;

public class LifeBar : MonoBehaviour
{
    // Start is called before the first frame update
    public GetAlphaValue getAlphaValue;

    public Image lifeBar;

    private float alphaValue;

    // Update is called once per frame
    void Update()
    {
        alphaValue = getAlphaValue.GetValue();
        lifeBar.fillAmount = alphaValue;

    }
}
