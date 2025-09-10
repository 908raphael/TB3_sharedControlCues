using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountCollision : MonoBehaviour
{

    private bool vib = false;
    //public HapticControl hc;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("wall") || collision.gameObject.CompareTag("obstacle") || collision.gameObject.CompareTag("target"))
        {
            DataManager.Instance.collisionCount++;
            Debug.Log("Collision detected" );
            StartCoroutine(vibrationCoroutine());
            //HapticPlugin.setVibrationValues("Default Device", new double[] { 0, 1, 0 }, 0.5f, 50, 0.5);
        }


    }

    private void Update()
    {
        if (vib)
        {
            //HapticPlugin.setVibrationValues("Default Device", new double[] { 1, 0, 0 }, 1.5f, 50, 0.2);
            HapticPlugin.setVibrationValues("Default Device", new double[] { 1, 0, 0 }, 2f, 150, 0.2);
        }
    }

    IEnumerator vibrationCoroutine()
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        vib = true;
        yield return new WaitForSeconds(0.5f);
        vib = false;

    }
}
