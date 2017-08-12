using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour, IGameplaySystem
{
    GameplayManager gameplayManager;

    Dictionary<string, Furniture> furnitureTable = new Dictionary<string, Furniture>();
    Dictionary<string, string> assignedCharacters = new Dictionary<string, string>();

    public void Initialise(GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
        Furniture[] pieces = FindObjectsOfType<Furniture>();
        furnitureTable.Clear();
        for (int i = 0; i < pieces.Length; ++i)
        {
            furnitureTable[pieces[i].name] = pieces[i];
        }
    }

    public void StartGame()
    {
        foreach (Furniture piece in furnitureTable.Values)
        {
            piece.StartGame();
        }
    }

    public EnvironmentActivityCheckResult CanActivityStartAt(CharacterActivity activity, Node n, ActivityContext ctxt)
    {
        RoomManager roomManager = gameplayManager.roomManager;

        bool roomLit = roomManager.IsRoomLit(ctxt.room);
        bool theresPowerLeft = gameplayManager.generatorManager.remainingGenerator > 0;
        switch (activity)
        {
            case CharacterActivity.Sleep:
            {
                    return (roomLit) ? EnvironmentActivityCheckResult.DarknessRequired : EnvironmentActivityCheckResult.Success;
            }
            case CharacterActivity.Bath:
            case CharacterActivity.Cooking:
            case CharacterActivity.Dancing:
            case CharacterActivity.Eating:
            case CharacterActivity.Drinking:
            case CharacterActivity.Healing:
            case CharacterActivity.Read:
            case CharacterActivity.WC:
            {
                    if (!roomLit)
                    {
                        return EnvironmentActivityCheckResult.LightRequired;
                    }
                    
                    if ((activity == CharacterActivity.Cooking || activity == CharacterActivity.Dancing) && !theresPowerLeft)
                    {
                        return EnvironmentActivityCheckResult.NoPower;
                    }
                    break;
                    
            }
            case CharacterActivity.Computer:
            case CharacterActivity.TV:
            {
                if (!theresPowerLeft)
                {
                    return EnvironmentActivityCheckResult.NoPower;
                }
                break;
            }
        }
        return EnvironmentActivityCheckResult.Success;
    }

    public void RefreshCharacterActivityAt(Character chara, Node node)
    {
        if (assignedCharacters.ContainsKey(node.furnitureKey) && assignedCharacters[node.furnitureKey] != chara.name)
        {
            // We shouldn't be here!
            Debug.LogWarning("Careful! Found a taken node!");
        }
        else
        {
            assignedCharacters[node.furnitureKey] = chara.name;
            furnitureTable[node.furnitureKey].SetOperationState(true);
            CharacterActivity nodeActivity = (furnitureTable[node.furnitureKey].activity == CharacterActivity.Count)
                ? CharacterActivity.Idle
                : furnitureTable[node.furnitureKey].activity;

            // TODO: Add context
            ActivityContext ctxt = new ActivityContext();
            ctxt.room = chara.currentRoom;
            ctxt.chara = chara.charName;

            EnvironmentActivityCheckResult envResult = CanActivityStartAt(nodeActivity, node, ctxt);
            if (envResult != EnvironmentActivityCheckResult.Success)
            {
                switch(envResult)
                {
                    case EnvironmentActivityCheckResult.NoPower:
                    {
                        chara.Talk(SpeechEntry.NoPower);
                        break;
                    }
                    case EnvironmentActivityCheckResult.DarknessRequired:
                    {
                        chara.Talk(SpeechEntry.NoLight);
                        break;
                    }
                    case EnvironmentActivityCheckResult.LightRequired:
                    {
                        chara.Talk(SpeechEntry.Light);
                        break;
                    }
                    default: break;
                }
                return;
            }

            CharacterActivityCheckResult checkResult = chara.CanCharacterEngageInActivity(nodeActivity, null);
            if (checkResult != CharacterActivityCheckResult.Success)
            {
                return; 
            }
            chara.SetCurrentActivity(nodeActivity);
        }
    }

    public void CancelCharacterActivityAt(Character chara, Node node)
    {
        if (assignedCharacters.ContainsKey(node.furnitureKey))
        {
            assignedCharacters.Remove(node.furnitureKey);
            furnitureTable[node.furnitureKey].SetOperationState(false);
        }
    }

    public bool IsCharacterUsingFurniture(Character testChar)
    {
        foreach(string furnitureKey in assignedCharacters.Keys)
        {
            if (assignedCharacters[furnitureKey] == testChar.name)
            {
                return furnitureTable[furnitureKey].IsEnabled;
            }
        }
        return false;
    }

    public CharacterActivity GetActivity(string furnitureKey)
    {
        Furniture furniture;
        if (furnitureTable.TryGetValue(furnitureKey, out furniture))
        {
            return furniture.activity;
        }
        else
        {
            return CharacterActivity.Count;
        }
    }

    public void UpdateSystem(float dt)
    {
        if (!gameplayManager.GameStarted || gameplayManager.Paused || gameplayManager.GameFinished) return;

        foreach (Furniture p in furnitureTable.Values)
        {
            p.LogicUpdate(dt);
        }
    }

    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
    }
}
