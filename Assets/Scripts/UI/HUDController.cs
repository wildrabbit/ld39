using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour, IGameplaySystem
{
    [System.Serializable]
    public class ActivityIconMapping
    {
        public CharacterActivity activity;
        public Sprite icon;
    }

    [System.Serializable]
    public class StatusIconMapping
    {
        public CharacterStatus status;
        public Sprite icon;
    }

    //---------------------------
    [Header("Widgets")]
    public Text timeLeftLabel;
    public Image powerValue;
    public Button soundToggle;
    public Text soundLabel;
    public Transform charAvatarContainer;
    public Button resetButton;

    public GameObject endPanel;
    public Text endMessage;

    [Header("Prefabs")]
    public CharPanel charPanelPrefab;

    GameplayManager gameplayManager;

    public List<ActivityIconMapping> mappings = new List<ActivityIconMapping>();
    public List<StatusIconMapping> statusMappings = new List<StatusIconMapping>();

    // Use this for initialization
    public void Initialise(GameplayManager gpManager)
    {
        gameplayManager = gpManager;

        // It could probably be better placed on StartGame, but then we have a StartGame dependency between the char. manager and this
        gameplayManager.characterManager.CharactersReady -= OnCharactersReady;
        gameplayManager.characterManager.CharactersReady += OnCharactersReady;

        endPanel.gameObject.SetActive(false);

    }
    public void StartGame ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(gameplayManager.timeManager.RemainingTime);
        powerValue.fillAmount = gameplayManager.generatorManager.PowerRatio;
        soundToggle.onClick.AddListener(OnSoundClicked);
        resetButton.onClick.AddListener(OnResetClicked);
        soundLabel.text = (AudioListener.pause) ? "Sound: OFF" : "Sound: ON";
    }

    public Sprite GetActivityIcon(CharacterActivity activity)
    {
        ActivityIconMapping mapping = mappings.Find(x => x.activity == activity);
        return mapping == null ? null : mapping.icon;
    }

    public Sprite GetStatusIcon(CharacterStatus status)
    {
        StatusIconMapping mapping = statusMappings.Find(x => x.status == status);
        return mapping == null ? null : mapping.icon;
    }

    public void OnCharactersReady()
    {
        IEnumerator<Character> chars = gameplayManager.characterManager.GetCharactersIterator();
        while (chars.MoveNext())
        {
            CharPanel panel = Instantiate<CharPanel>(charPanelPrefab);
            panel.Init(this, gameplayManager.characterManager, chars.Current);
            panel.transform.SetParent(charAvatarContainer);
            Vector3 pos = panel.transform.position;
            pos.z = 0;
            panel.transform.position = pos;

            panel.transform.localScale = Vector3.one;

        }
    }
	
	// Update is called once per frame
	public void UpdateSystem (float dt)
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(gameplayManager.timeManager.RemainingTime);
        powerValue.fillAmount = gameplayManager.generatorManager.PowerRatio;
    }

    void OnSoundClicked()
    {
        AudioListener.pause = !AudioListener.pause;             
        soundLabel.text = (AudioListener.pause) ? "Sound: OFF" : "Sound: ON";
    }

    void OnResetClicked()
    {
        gameplayManager.Restart();
    }

    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
        endPanel.gameObject.SetActive(true);
        string msg = "";
        switch (result)
        {
            case GameResult.None:
                break;
            case GameResult.AllHappy:
                msg = "Phew! What a night! Luckily you had everything under control, great job!";
                break;
            case GameResult.ExistsDeadOrBreakdown:
                msg = "You lost some members of your family, but hey, it could have been worse, right?";
                break;
            case GameResult.AllInBreakdown:
                msg = "Seems like your family wasn't suited to life without electricity, huh?";
                break;
            case GameResult.AllDead:
                msg = "WTF, Really??";
                break;
            default:
                break;
        }
        endMessage.text = msg;
    }
}
