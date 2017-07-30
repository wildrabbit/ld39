using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class CharacterManager : MonoBehaviour
{
    [Header("Dependencies")]
    public RoomManager roomManager;
    public NodeManager nodeManager;

    [Header("Entity Prefabs")]
    public Character adultTemplate;
    public Character childTemplate;

    [Header("Config")]
    public List<CharacterConfig> characterData;

    public Transform characterRoot;

    public string layerOff;
    public string layerOn;
    public string layerSelected;

    //---- 
    List<Character> characters;

    Character currentCharacter = null;

    public System.Action<Character, Character> SelectionChanged;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        characters = new List<Character>();
        for (int i = 0; i < characterData.Count; ++i)
        {
            CharacterConfig cfg = characterData[i];
            Character prefab = cfg.kid ? childTemplate : adultTemplate;
            Character newChara = Instantiate<Character>(prefab);
            newChara.SetDependencies(this, nodeManager);
            newChara.InitFromConfig(cfg, characterRoot, cfg.startNode, cfg.startNode.room);
            newChara.transform.parent = characterRoot;

            characters.Add(newChara);
        }

        roomManager.OnRoomLightsSwitched -= OnRoomLightsSwitched;
        roomManager.OnRoomLightsSwitched += OnRoomLightsSwitched;
    }

    void OnRoomLightsSwitched(string room, bool on)
    {
        List<Character> roomChars = new List<Character>();
        FindCharactersInRoom(room, roomChars);
        for (int i = 0; i < characterData.Count; ++i)
        {
            if (characters[i].currentRoom == room && !IsCharacterSelected(characters[i]))
            {
                characters[i].SetLayer(on ? layerOn : layerOff);
            }
        }
    }

    void FindCharactersInRoom(string room, List<Character> chars)
    {
        for (int i = 0; i < characters.Count; ++i)
        {
            if (characters[i].currentRoom == room)
            {
                chars.Add(characters[i]);
            }
        }
    }

    public void ToggleCharacterSelection(Character chara)
    {
        Character old = currentCharacter;
        if (currentCharacter == null)
        {
            currentCharacter = chara; // NEW SELECT
            currentCharacter.SetSelected(true);
        }
        else
        {
            if (currentCharacter != chara)
            {
                currentCharacter.SetSelected(false);
                currentCharacter = chara;
                currentCharacter.SetSelected(true);
            }
            else
            {
                currentCharacter.SetSelected(false);
                currentCharacter = null; // DESELECT
            }
        }
        Debug.LogFormat("Selected: {0}", currentCharacter == null ? "NONE" : currentCharacter.name);

        if (SelectionChanged != null)
        {
            SelectionChanged(currentCharacter, old);
        }
    }

    public bool IsCharacterSelected(Character test)
    {
        return test == currentCharacter;
    }

    public Character GetSelected()
    {
        return currentCharacter;
    }

    public string GetLayerForRoom(string name)
    {
        RoomStatus room = roomManager.GetRoom(name);
        if (room != null)
        {
            return room.lightsOn ? layerOn : layerOff;
        }
        return layerOff;
    }
}
