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

    //---- 
    List<Character> characters;

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
            characters[i].SetLayer(on ? layerOn : layerOff);
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

    public void Update()
    {

    }
}
