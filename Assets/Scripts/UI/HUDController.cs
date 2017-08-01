using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
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

    //
    [Header("Dependencies")]
    public TimeManager timeManager;
    public GeneratorManager generatorManager;
    public AudioListener listener;
    public CharacterManager charManager;

    //---------------------------
    [Header("Widgets")]
    public Text timeLeftLabel;
    public Image powerValue;
    public Button soundToggle;
    public Text soundLabel;
    public Transform charAvatarContainer;
    public Button resetButton;

    [Header("Prefabs")]
    public CharPanel charPanelPrefab;

    public List<ActivityIconMapping> mappings = new List<ActivityIconMapping>();
    public List<StatusIconMapping> statusMappings = new List<StatusIconMapping>();

    // Use this for initialization
    void Awake()
    {
        charManager.CharactersReady -= OnCharactersReady;
        charManager.CharactersReady += OnCharactersReady;
    }
    void Start ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
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
        IEnumerator<Character> chars = charManager.GetCharactersIterator();
        while (chars.MoveNext())
        {
            CharPanel panel = Instantiate<CharPanel>(charPanelPrefab);
            panel.Init(this, charManager, chars.Current);
            panel.transform.parent = charAvatarContainer;
            Vector3 pos = panel.transform.position;
            pos.z = 0;
            panel.transform.position = pos;

            panel.transform.localScale = Vector3.one;

        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
    }

    void OnSoundClicked()
    {
        AudioListener.pause = !AudioListener.pause;             
        soundLabel.text = (AudioListener.pause) ? "Sound: OFF" : "Sound: ON";
    }

    void OnResetClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
}
