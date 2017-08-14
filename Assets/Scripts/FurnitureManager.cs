using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour, IGameplaySystem
{
    GameplayManager gameplayManager;

    Dictionary<string, Furniture> furnitureTable = new Dictionary<string, Furniture>();
    Dictionary<string, string> assignedCharacters = new Dictionary<string, string>();

    List<string> tvNodes = new List<string>();

    public void Initialise(GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
        Furniture[] pieces = FindObjectsOfType<Furniture>();
        furnitureTable.Clear();
        for (int i = 0; i < pieces.Length; ++i)
        {
            furnitureTable[pieces[i].name] = pieces[i];
            furnitureTable[pieces[i].name].SetManager(gameplayManager);
            if (pieces[i].name.StartsWith("tv"))
            {
                tvNodes.Add(pieces[i].name);
            }
        }
        tvNodes.Clear();
    }

    public void StartGame()
    {
        gameplayManager.generatorManager.OnGeneratorPowerDown -= OnPowerOff;
        gameplayManager.generatorManager.OnGeneratorPowerDown += OnPowerOff;
        foreach (Furniture piece in furnitureTable.Values)
        {
            piece.StartGame();
        }
    }

    public EnvironmentActivityCheckResult CanActivityStartAt(CharacterActivity activity, ActivityContext ctxt)
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
            Furniture f = furnitureTable[node.furnitureKey];
            if (f.TryCharacterInteractionStart(chara))
            {
                assignedCharacters[node.furnitureKey] = chara.name;
            }
        }
    }

    public void CancelCharacterActivityAt(Character chara, Node node)
    {
        if (assignedCharacters.ContainsKey(node.furnitureKey))
        {
            furnitureTable[node.furnitureKey].TryCharacterInteractionStop();
            assignedCharacters.Remove(node.furnitureKey);            
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
            return furniture.cfg.activity;
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

    public bool NoTakenTVSeats()
    {
        return !tvNodes.Exists(x => assignedCharacters.ContainsKey(x));        
    }
    public bool IsLastTVSeat(string removeSeat)
    {
        return !tvNodes.Exists(x => x != removeSeat && assignedCharacters.ContainsKey(x));
    }

    void OnPowerOff()
    {
        foreach (Furniture piece in furnitureTable.Values)
        {
            if (assignedCharacters.ContainsKey(piece.name) && piece.cfg.depleter.source != "")
            {
                Character chara = gameplayManager.characterManager.GetCharacter(piece.name);
                if (chara != null && chara.currentNode != null)
                {
                    CancelCharacterActivityAt(chara, chara.currentNode);
                }
            }
        }
    }
}
