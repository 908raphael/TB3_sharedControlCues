using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountTarget : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("target"))
        {
            DataManager.Instance.collectCount++;
            Debug.Log("Target detected" ); 
        }
    }
}
