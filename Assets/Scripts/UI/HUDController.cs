using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
{
    //
    public TimeManager timeManager;
    public GeneratorManager generatorManager;

    //---------------------------
    public Text timeLeftLabel;
    public Image powerValue;

	// Use this for initialization
	void Start ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
    }
	
	// Update is called once per frame
	void Update ()
    {
        timeLeftLabel.text = StringUtils.FormatSeconds(timeManager.RemainingTime);
        powerValue.fillAmount = generatorManager.PowerRatio;
    }
}
