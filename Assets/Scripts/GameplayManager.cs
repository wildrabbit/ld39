using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameResult
{
    None,
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

    public Transform sceneRoot;

    [Header("Mood")]
    public AudioSource bgSound;
    public Lightning lightning;
    public ParticleSystem rain;

    GameResult result;

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
    }

    void Start()
    {
        startIssued = false;
        paused = false;
        result = GameResult.None;

        // Build managers
        timeManager.Initialise(this);
        generatorManager.Initialise(this);
        characterManager.Initialise(this);
        furnitureManager.Initialise(this);
        roomManager.Initialise(this);
        resourceManager.Initialise(this);
        touchManager.Initialise(this);
        hudController.Initialise(this);
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
        }

        // TODO: Determine if we should update
    }


    public void AllCharactersDeadOrBrokenDown(bool allDead)
    {
        // Stop all systems.
        result = allDead ? GameResult.AllDead : GameResult.AllInBreakdown;
    }

    public void TimeFinished()
    {
        // Resolve game over
        // Poll character state and update result
        result = characterManager.ResolveGameStatus();
    }

    public void Pause(bool value)
    {
        paused = value;
    }

    public void GameStart()
    {
        timeManager.StartGame();
        characterManager.StartGame();
        generatorManager.StartGame();
        furnitureManager.StartGame();
        roomManager.StartGame();
        resourceManager.StartGame();
        hudController.StartGame();
        // add touch manager when relevant

        bgSound.Play();
        lightning.enabled = true;
        rain.Play();
    }
}
