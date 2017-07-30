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
    public string name;
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

    public System.Action<string, bool> OnRoomLightsSwitched;

    public AudioSource flick;

    private void Start()
    {
        flick = GetComponent<AudioSource>();

        // Initialise rooms and mappings: Everything to false!
        for (int i = 0; i < rooms.Count; ++i)
        {
            RoomStatus status = new RoomStatus();
            status.name = rooms[i].name;
            status.data = rooms[i];
            status.occupierList.Clear();
            status.furnitureMappings.Clear();
            for (int j = 0; j < status.data.furnitureNodes.Length; ++j)
            {
                Node furnitureNode = status.data.furnitureNodes[j];
                status.furnitureMappings[furnitureNode.name] = furnitureNode.furnitureKey;
            }
            roomsStatus[status.name] = status;

            SwitchLights(status.name, status.data.willStartLit, false);
            
       }
    }

    public void SwitchLights(string room, bool enabled, bool playSound = false)
    {
        RoomStatus status;
        if (!roomsStatus.TryGetValue(room, out status))
        {
            return;
        }

        SwitchLights(status, enabled, playSound);
    }

    public void SwitchLights(RoomStatus status, bool enabled, bool playSound = false)
    {
        status.lightsOn = enabled;
        status.data.lights.SetActive(status.lightsOn);
        if (status.lightsOn)
        {
            generatorManager.AddDepleter(status.data.depleter);
        }
        else
        {
            generatorManager.RemoveDepleter(status.data.depleter.source);
        }

        if (playSound && flick.clip != null)
        {
            flick.Play();
        }
        
        if (OnRoomLightsSwitched != null)
        {
            OnRoomLightsSwitched(status.name, enabled);
        }
    }

    public void ToggleRoomLights(string room)
    {
        RoomStatus status;
        if (!roomsStatus.TryGetValue(room, out status))
        {
            return;
        }

        SwitchLights(status, !status.lightsOn, true);
    }

    public RoomStatus GetRoom(string name)
    {
        RoomStatus status = null;
        roomsStatus.TryGetValue(name, out status);
        return status;
    }

    public bool IsRoomLit(string name)
    {
        RoomStatus room = GetRoom(name);
        if (room == null) return true;
        return room.lightsOn;
    }
}
