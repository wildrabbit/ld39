using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureManager : MonoBehaviour
{
    Dictionary<string, Furniture> furnitureTable = new Dictionary<string, Furniture>();
    Dictionary<string, string> assignedCharacters = new Dictionary<string, string>();

	// Use this for initialization
	void Start ()
    {
        Furniture[] pieces = FindObjectsOfType<Furniture>();
        furnitureTable.Clear();
        for (int i = 0; i < pieces.Length; ++i)
        {
            pieces[i].InitGame(this);
            furnitureTable[pieces[i].name] = pieces[i];
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
            
            if (chara.CanCharacterEngageInActivity(nodeActivity, null) != CharacterActivityCheckResult.Success)
            {
                // Character status / resource checks (would resource-dependent ones belong here -as they are- or on the environment check?)
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

    CharacterActivity GetActivity(string furnitureKey)
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
