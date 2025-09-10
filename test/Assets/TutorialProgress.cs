using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialProgress : MonoBehaviour
{

    public Transform robot;
    public GameObject UI, UI2;
    public GameObject Progress1, Progress2;
   
    public TextMeshProUGUI tutorialText;
    public float triggerDistance = 0.5f;
    private bool triggered = false;
    private bool triggered2 = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(Progress1.transform.position, robot.position);
        if (distance <= triggerDistance && !triggered)
        {
            UI.SetActive(true);
            DataManager.Instance.isPaused = true;
            triggered = true;
            tutorialText.text = "Task: Try the navigation";
        }

        distance = Vector3.Distance(Progress2.transform.position, robot.position);
        if (distance <= triggerDistance && !triggered2)
        {
            UI2.SetActive(true);
            DataManager.Instance.isPaused = true;
            DataManager.Instance.isWithNavigation = false;
            triggered2 = true;
            tutorialText.text = "Task: Get to the target on the right";
        }
    }

    public void NavigationProgress()
    {
        DataManager.Instance.isWithNavigation = true;
        DataManager.Instance.forceComputer = true;
        DataManager.Instance.isPaused = false;
        UI.SetActive(false);
    }

    public void NavigationProgress2()
    {
        DataManager.Instance.isWithNavigation = true;
        DataManager.Instance.forceComputer = true;
        DataManager.Instance.isPaused = false;
        UI2.SetActive(false);
    }

}
