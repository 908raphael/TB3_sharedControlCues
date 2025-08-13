using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TargetObjectPlacer : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public string roomName;  // 房间名称（可选）
        public Transform[] positions;  // 该房间可放置物体的位置
    }

    public Room[] rooms;  // 5 个房间，每个房间有多个可用点位
    public GameObject[] targetObjects;  // 4 个目标物体

    void Start()
    {
        PlaceObjects();
    }

    void PlaceObjects()
    {
        if (targetObjects.Length > rooms.Length)
        {
            Debug.LogError("目标物体数量多于房间数量！");
            return;
        }

        List<int> availableRoomIndices = Enumerable.Range(0, rooms.Length).ToList();
        List<int> selectedRooms = new List<int>();

        // 随机选择 4 个不同的房间
        for (int i = 0; i < targetObjects.Length; i++)
        {
            int randomIndex = Random.Range(0, availableRoomIndices.Count);
            selectedRooms.Add(availableRoomIndices[randomIndex]);
            availableRoomIndices.RemoveAt(randomIndex);
        }

        // 在选中的房间里随机选择点位放置目标物体
        for (int i = 0; i < targetObjects.Length; i++)
        {
            Room room = rooms[selectedRooms[i]];
            if (room.positions.Length == 0)
            {
                Debug.LogWarning($"Room {room.roomName} no available space！");
                continue;
            }

            Transform chosenPosition = room.positions[Random.Range(0, room.positions.Length)];
            //Instantiate(targetObjects[i], chosenPosition.position, Quaternion.identity);
            targetObjects[i].transform.position = chosenPosition.position;
        }
    }
}