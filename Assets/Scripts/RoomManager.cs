using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomData
{
    public string name;
    public ContinuousPowerDepleter depleter; // Lighting alone!
    public GameObject lights;
    public bool willStartLit = false;

    public Node[] furnitureNodes;
}

[System.Serializable]
public class ActivityEntry
{
    public Character character;
    public CharacterActivity activity;
}

[System.Serializable]
public class RoomStatus
{
    public string roomName;
    public RoomData data; // shortcut
    public Dictionary<string, ActivityEntry> occupierList = new Dictionary<string, ActivityEntry>(); // TODO: Check moving chars
    public Dictionary<string, string> furnitureMappings = new Dictionary<string, string>();
    public bool lightsOn;
}

public class RoomManager : MonoBehaviour
{
    [Header("Managers")]
    public GeneratorManager generatorManager;
    public NodeManager nodeManager;

    [Header("Config")]
    public List<RoomData> rooms;

    Dictionary<string, RoomStatus> roomsStatus = new Dictionary<string, RoomStatus>();

    private void Start()
    {
        // Initialise rooms and mappings: Everything to false!
        for (int i = 0; i < rooms.Count; ++i)
        {
            RoomStatus status = new RoomStatus();
            status.roomName = rooms[i].name;
            status.data = rooms[i];
            status.occupierList.Clear();
            status.furnitureMappings.Clear();
            for (int j = 0; j < status.data.furnitureNodes.Length; ++j)
            {
                Node furnitureNode = status.data.furnitureNodes[j];
                status.furnitureMappings[furnitureNode.name] = furnitureNode.furnitureKey;
            }
            roomsStatus[status.roomName] = status;

            SwitchLights(status.roomName, status.data.willStartLit);
            
       }

        
    }

    public void SwitchLights(string room, bool enabled)
    {
        if (!roomsStatus.ContainsKey(room))
        {
            return;
        }

        RoomStatus status = roomsStatus[room];
        status.data.lights.SetActive(enabled);
        if (enabled)
        {
            generatorManager.AddDepleter(status.data.depleter);
        }
        else
        {
            generatorManager.RemoveDepleter(status.data.depleter.source);
        }        
    }
}
