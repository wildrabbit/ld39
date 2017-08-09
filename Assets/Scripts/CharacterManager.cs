﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour, IGameplaySystem
{
    [Serializable]
    public class SpeechIcons
    {
        public SpeechEntry entry;
        public Sprite icon;
        public string message;
    }
    
    [Header("Entity Prefabs")]
    public Character adultTemplate;
    public Character childTemplate;

    [Header("Config")]
    public List<CharacterConfig> characterData;
    public List<SpeechIcons> speechData;

    GameplayManager gameplayManager;

    public Transform characterRoot;

    public string layerOff;
    public string layerOn;
    public string layerSelected;

    //---- 
    List<Character> characters;

    Character currentCharacter = null;

    public Action<Character, Character> SelectionChanged;
    public Action CharactersReady;

    public void Initialise(GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
    }

    public void StartGame()
    {
        characters = new List<Character>();
        for (int i = 0; i < characterData.Count; ++i)
        {
            CharacterConfig cfg = characterData[i];
            Character prefab = cfg.kid ? childTemplate : adultTemplate;
            Character newChara = Instantiate<Character>(prefab);
            newChara.InitFromConfig(gameplayManager, cfg, characterRoot, cfg.startNode, cfg.startNode.room);
            newChara.StartGame();
            characters.Add(newChara);
        }
        if (CharactersReady != null)
        {
            CharactersReady();
        }

        gameplayManager.roomManager.OnRoomLightsSwitched -= OnRoomLightsSwitched;
        gameplayManager.roomManager.OnRoomLightsSwitched += OnRoomLightsSwitched;
    }

    void OnRoomLightsSwitched(string room, bool on)
    {
        List<Character> roomChars = new List<Character>();
        FindCharactersInRoom(room, roomChars);
        for (int i = 0; i < characterData.Count; ++i)
        {
            if (characters[i].currentRoom == room && !IsCharacterSelected(characters[i]) /*&& */)
            {
                characters[i].SetLayer(on || gameplayManager.furnitureManager.IsCharacterUsingFurniture(characters[i]) ? layerOn : layerOff);
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
            if (!gameplayManager.roomManager.IsRoomLit(currentCharacter.currentRoom))
                gameplayManager.roomManager.SwitchLights(currentCharacter.currentRoom, true, true);
        }
        else
        {
            if (currentCharacter != chara)
            {
                currentCharacter.SetSelected(false);
                currentCharacter = chara;
                if (chara != null)
                {
                    currentCharacter.SetSelected(true);
                    if (!gameplayManager.roomManager.IsRoomLit(currentCharacter.currentRoom))
                        gameplayManager.roomManager.SwitchLights(currentCharacter.currentRoom, true, true);
                }
            }
            else
            {
                currentCharacter.SetSelected(false);
                currentCharacter = null; // DESELECT
            }
        }
        
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
        RoomStatus room = gameplayManager.roomManager.GetRoom(name);
        if (room != null)
        {
            return room.lightsOn ? layerOn : layerOff;
        }
        return layerOff;
    }

    public void CharacterArrivedToNode(Character chara, Node node)
    {
        if (!string.IsNullOrEmpty(node.furnitureKey))
        {
            gameplayManager.furnitureManager.CharacterArrivedToNode(chara, node);
            if (gameplayManager.furnitureManager.IsCharacterUsingFurniture(chara))
            {
                chara.SetLayer(currentCharacter == chara ? layerSelected : layerOn);
            }
            else
            {
                if (currentCharacter != chara)
                {
                    chara.SetLayer(gameplayManager.roomManager.IsRoomLit(chara.currentRoom) ? layerOff : layerOn);
                }
                else
                {
                    chara.SetLayer(layerSelected);
                }
            }
        }
    }

    public void CharacterLeftNode(Character chara, Node node)
    {
        if (!string.IsNullOrEmpty(node.furnitureKey))
        {
            gameplayManager.furnitureManager.CharacterLeftNode(chara, node);
            // Reposition character layer
            if (currentCharacter != chara)
            {
                chara.SetLayer(gameplayManager.roomManager.IsRoomLit(chara.currentRoom) ? layerOff : layerOn);
            }
            else
            {
                chara.SetLayer(layerSelected);
            }
        }
    }

    public IEnumerator<Character> GetCharactersIterator()
    {
        return characters.GetEnumerator();
    }

    public Sprite GetSpeechIcon(SpeechEntry entry)
    {
        SpeechIcons spData = speechData.Find(x => x.entry == entry);
        if (spData == null)
        {
            return null;
        }
        return spData.icon;
    }

    public string GetSpeechMessage(SpeechEntry entry)
    {
        SpeechIcons spData = speechData.Find(x => x.entry == entry);
        if (spData == null)
        {
            return null;
        }
        return spData.message;
    }

    public bool ExistsCharacterAtNode(Node n)
    {
        for (int i = 0; i < characters.Count; ++i)
        {
            if (characters[i].currentNode == n)
            {
                return true;
            }
        }
        return false;
    }

    public GameResult ResolveGameStatus()
    {
        GameResult result = GameResult.AllHappy;
        int numDead = 0;
        int numInBreakdown = 0;
        int numChars = characters.Count;
        for (int i = 0; i < numChars; ++i)
        {
            if (characters[i].Dead)
            {
                numDead++;
            }
            else if (characters[i].Breakdown)
            {
                numInBreakdown++;
            }
        }
        if (numDead == numChars)
        {
            return GameResult.AllDead;
        }
        else if (numDead == numInBreakdown)
        {
            return GameResult.AllInBreakdown;
        }
        else if (numInBreakdown + numDead > 0)
        {
            return GameResult.ExistsDeadOrBreakdown;
        }
        return result;
    }
}
