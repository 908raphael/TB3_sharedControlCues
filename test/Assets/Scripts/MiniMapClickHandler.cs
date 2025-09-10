using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MiniMapClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("MiniMap Setting")]
    public GameObject goal;             // destination
    public Camera miniMapCamera;          // camera of minimap
    public RectTransform miniMapRect;     // minimap RectTransform
    public Vector3 destinationOffset;     // world offset of destination

    //
    public void OnPointerClick(PointerEventData eventData)
    {
        Vector2 localPoint;

        // transform screen point to local point in rect
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            miniMapRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        if (isInside)
        {
            // get size of minimap
            Vector2 size = miniMapRect.sizeDelta;

            // normalize local point
            float normalizedX = (localPoint.x + size.x / 2) / size.x;
            float normalizedY = (localPoint.y + size.y / 2) / size.y;

            // get world size of minimap
            float worldHeight = miniMapCamera.orthographicSize * 2;
            float worldWidth = worldHeight * miniMapCamera.aspect;

            // calculate world position
            float worldX = (normalizedX - 0.5f) * worldWidth + miniMapCamera.transform.position.x;
            float worldZ = (normalizedY - 0.5f) * worldHeight + miniMapCamera.transform.position.z;

            Vector3 destination = new Vector3(worldX, 0, worldZ) + destinationOffset;

            SetDestination(destination);
        }
    }

    private void SetDestination(Vector3 dest)
    {
        Debug.Log("Set the desdination: " + dest);
        goal.transform.position = dest;
    }
}
