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
public class Room
{
    public string name;
    public RoomData data; // shortcut
    public Dictionary<string, ActivityEntry> occupierList = new Dictionary<string, ActivityEntry>(); // TODO: Check moving chars
    public Dictionary<string, string> furnitureMappings = new Dictionary<string, string>();
    public bool lightsOn;
}

public class RoomManager : MonoBehaviour, IGameplaySystem
{
    GameplayManager gameplayManager;

    [Header("Config")]
    public List<RoomData> roomConfig;

    Dictionary<string, Room> roomTable = new Dictionary<string, Room>();

    public System.Action<string, bool> OnRoomLightsSwitched;

    AudioSource flick;

    void Start()
    {
        flick = GetComponent<AudioSource>();
    }

    public void StartGame()
    {
        foreach(Room room in roomTable.Values)
        {
            SwitchLights(room.name, room.data.willStartLit, false);
        }
    }

    public void Initialise(GameplayManager _gpManager)
    {
        gameplayManager = _gpManager;
        for (int i = 0; i < roomConfig.Count; ++i)
        {
            Room status = new Room();
            status.name = roomConfig[i].name;
            status.data = roomConfig[i];
            status.occupierList.Clear();
            status.furnitureMappings.Clear();
            for (int j = 0; j < status.data.furnitureNodes.Length; ++j)
            {
                Node furnitureNode = status.data.furnitureNodes[j];
                status.furnitureMappings[furnitureNode.name] = furnitureNode.furnitureKey;
            }
            roomTable[status.name] = status;
        }
    }

    public void SwitchLights(string room, bool enabled, bool playSound = false)
    {
        Room status;
        if (!roomTable.TryGetValue(room, out status))
        {
            return;
        }

        SwitchLights(status, enabled, playSound);
    }

    public void SwitchLights(Room status, bool enabled, bool playSound = false)
    {
        status.lightsOn = enabled;
        status.data.lights.SetActive(status.lightsOn);
        if (status.lightsOn)
        {
            gameplayManager.generatorManager.AddDepleter(status.data.depleter);
        }
        else
        {
            gameplayManager.generatorManager.RemoveDepleter(status.data.depleter.source);
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
        Room status;
        if (!roomTable.TryGetValue(room, out status))
        {
            return;
        }

        SwitchLights(status, !status.lightsOn, true);
    }

    public Room GetRoom(string name)
    {
        Room status = null;
        roomTable.TryGetValue(name, out status);
        return status;
    }

    public bool IsRoomLit(string name)
    {
        Room room = GetRoom(name);
        if (room == null) return true;
        return room.lightsOn;
    }

    public void UpdateSystem(float dt)
    {
    }

    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
    }
}
