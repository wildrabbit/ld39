using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameResult
{
    None = -1,
    AllHappy = 0, //Win
    ExistsDeadOrBreakdown,
    AllInBreakdown,
    AllDead
}

public class GameplayManager : MonoBehaviour
{
    [Header("Dependencies")]
    public TimeManager timeManager;
    public GeneratorManager generatorManager;
    public CharacterManager characterManager;
    public NodeManager nodeManager;
    public RoomManager roomManager;
    public FurnitureManager furnitureManager;
    public ResourceManager resourceManager;
    public TouchManager touchManager;
    public HUDController hudController;

    public List<IGameplaySystem> systems;

    public Transform sceneRoot;

    [Header("Mood")]
    public AudioSource bgSound;
    public Lightning lightning;
    public ParticleSystem rain;

    GameResult result;

    public bool GameStarted
    {
        get
        {
            return startIssued;
        }
    }

    public bool GameFinished
    {
        get
        {
            return result != GameResult.None;
        }
    }

    public bool Paused
    {
        get
        {
            return paused;
        }
    }

    bool startIssued = false;
    bool paused = false;

    void Awake()
    {
        bgSound.Stop();
        lightning.enabled = false;
        rain.Stop();
        systems = new List<IGameplaySystem>();

        RegisterSystems();
    }

    void RegisterSystems()
    {
        systems.Add(timeManager);
        systems.Add(generatorManager);
        systems.Add(characterManager);
        systems.Add(furnitureManager);
        systems.Add(roomManager);
        systems.Add(resourceManager);
        systems.Add(touchManager);
        systems.Add(hudController);
    }
    void Start()
    {
        startIssued = false;
        paused = false;
        result = GameResult.None;

        // Build managers
        foreach(IGameplaySystem system in systems)
        {
            system.Initialise(this);
        }
        nodeManager.BuildGraph();

        sceneRoot.gameObject.SetActive(false);

}

    private void Update()
    {
        if (!startIssued && Input.anyKeyDown)
        {
            sceneRoot.gameObject.SetActive(true);
            GameStart();
            startIssued = true;
            return;
        }

        GameResult lastResult = result;
        float delta = Time.deltaTime;
        foreach (IGameplaySystem system in systems)
        {
            system.UpdateSystem(delta);
        }

        if (lastResult == GameResult.None && lastResult != result)
        {
            foreach(IGameplaySystem system in systems)
            {
                system.GameFinished(result);
            }
        }
        else if (result != GameResult.None && Input.anyKeyDown)
        {
            Restart();
        }
    }

    public void AllCharactersDeadOrBrokenDown(GameResult endResult)
    {
        // Stop all systems.
        if (result == GameResult.None)
            result = endResult;
    }

    public void TimeFinished()
    {
        // Resolve game over
        // Poll character state and update result
        if (result == GameResult.None)
            result = characterManager.ResolveGameStatus();
    }

    public void Pause(bool value)
    {
        paused = value;
    }

    public void GameStart()
    {
        foreach(IGameplaySystem system in systems)
        {
            system.StartGame();
        }

        result = GameResult.None;
        bgSound.Play();
        lightning.enabled = true;
        rain.Play();
    }

    public void Restart()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
