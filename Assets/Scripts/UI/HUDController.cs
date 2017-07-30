using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
{
    //
    [Header("Dependencies")]
    public TimeManager timeManager;
    public GeneratorManager generatorManager;
    public AudioListener listener;

    //---------------------------
    [Header("Widgets")]
    public Text timeLeftLabel;
    public Image powerValue;
    public Button soundToggle;
    public Text soundLabel;

    // Use this for initialization
    void Start ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
        soundToggle.onClick.AddListener(OnSoundClicked);
        soundLabel.text = (listener.enabled) ? "Sound: ON" : "Sound: OFF";
    }
	
	// Update is called once per frame
	void Update ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
    }

    void OnSoundClicked()
    {
        listener.enabled = !listener.enabled;
        soundLabel.text = (listener.enabled) ? "Sound: ON" : "Sound: OFF";
    }
}
