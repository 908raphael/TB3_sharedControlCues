using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class StartUI : MonoBehaviour
{
    private GameObject Ui_1;
    private GameObject Ui_2;
    private GameObject ui_warn;
    public TMP_InputField user_id;
    public TMP_InputField trial_index;
    
    private void Awake()
    {
        Ui_2 = GameObject.FindWithTag("UI2");
        Ui_1 = GameObject.FindWithTag("UI1");
        ui_warn = GameObject.FindWithTag("UIPopUp");
        ui_warn.SetActive(false);
        
    }

    public void showUI2()
    {
        //Debug.Log();
        if (user_id.text != "" || trial_index.text != "")
        {
            Ui_2.SetActive(true);
            Ui_1.SetActive(false);
        }
        else
        {
            ui_warn.SetActive(true);
        }
        
    }

    public void hideUI2()
    {
        Ui_2.SetActive(false);
    }

    public void closeWarn()
    {
        ui_warn.SetActive(false);
    }
}
