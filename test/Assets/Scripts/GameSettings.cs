using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Settings/GameSettings")]
public class GameSettings : ScriptableObject
{
    [Header("Robot Settings")]
    [Range(10f, 30f)]
    public float movementSpeed = 20f;
    public bool isAuto = false;
    public bool isTele = false;

    [Header("Visual Cues Settings")]
    public bool WarnLine = true;

    [Range(0f, 1f)]
    public float musicVolume = 0.8f;

    [Range(0f, 1f)]
    public float effectsVolume = 0.9f;
}
