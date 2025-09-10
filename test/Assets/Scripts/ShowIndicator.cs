using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowIndicator : MonoBehaviour
{

    public Vector3 rotationDegrees = new Vector3(90, 0, 0);
    public Vector3 offset = new Vector3(0, 0, 0);

    public void Show(Vector3 position)
    {     
        transform.position = position + offset;

        Quaternion additionalRotation = Quaternion.Euler(rotationDegrees);
        transform.rotation = additionalRotation;

    }
}
