using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour, IGameplaySystem
{
    public List<ResourceData> resourceConfig = new List<ResourceData>();

    GameplayManager gameplayManager;
    Dictionary<string, int> resources = new Dictionary<string, int>();

	// Use this for initialization
	public void Initialise (GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
	}
	
	// Update is called once per frame
	public void StartGame()
    {
        resources.Clear();
        for (int i = 0; i < resourceConfig.Count; ++i)
        {
            ResourceData config = resourceConfig[i];
            resources[config.name] = config.initialAmount;
        }

    }

    public void UseResource(string resource)
    {
        // Notify
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
