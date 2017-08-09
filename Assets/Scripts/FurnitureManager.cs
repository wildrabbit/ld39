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
        return EnvironmentActivityCheckResult.Success;
    }

    public void CharacterArrivedToNode(Character chara, Node node)
    {
        if (assignedCharacters.ContainsKey(node.furnitureKey) && assignedCharacters[node.furnitureKey] != chara.name)
        {
            // Kick 'em out!
        }
        else
        {
            assignedCharacters[node.furnitureKey] = chara.name;
            furnitureTable[node.furnitureKey].SetOperationState(true);
            CharacterActivity nodeActivity = (furnitureTable[node.furnitureKey].activity == CharacterActivity.Count)
                ? CharacterActivity.Idle
                : furnitureTable[node.furnitureKey].activity;

            // TODO: Add context
            ActivityContext ctxt = null;

            if (CanActivityStartAt(nodeActivity, node, ctxt) != EnvironmentActivityCheckResult.Success)
            {
                // Logger
                return;
            }

            CharacterActivityCheckResult checkResult = chara.CanCharacterEngageInActivity(nodeActivity, null);
            if (checkResult != CharacterActivityCheckResult.Success)
            {
                if (checkResult == CharacterActivityCheckResult.RoomIsLit)
                {
                    chara.Talk(SpeechEntry.NoLight);
                }
                return; 
            }
            chara.SetCurrentActivity(nodeActivity);
        }
    }

    public void CharacterLeftNode(Character chara, Node node)
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

    // Update is called once per frame
    void Update ()
    {
		
	}
}
