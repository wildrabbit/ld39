using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharPanel : MonoBehaviour
{
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;

    public Image bg;
    public Image healthBar;
    
    public Image selectionBar;

    public Image statusIcon1;
    public Image statusIcon2;
    public Image activityIcon;

    public Text label;
    public Image portrait;

    public Button info;

    Character charRef;
    CharacterManager charManagerRef;
    HUDController hud;

    // Use this for initialization
    public void Init (HUDController hud, CharacterManager manager, Character chara)
    {
        this.hud = hud;
        charManagerRef = manager;
        charRef = chara;

        label.text = charRef.name;
        portrait.sprite = charRef.defaults.portrait;
        UpdateHealthBar();
        UpdateActivity();
        UpdateStatus();
        selectionBar.gameObject.SetActive(manager.IsCharacterSelected(charRef));
	}

    void UpdateActivity()
    {
        Sprite sp = hud.GetActivityIcon(charRef.currentActivity);
        if (sp == null)
        {
            activityIcon.gameObject.SetActive(false);
        }
        else
        {
            activityIcon.gameObject.SetActive(true);
            activityIcon.sprite = sp;
        }
    }

    void UpdateStatus()
    {
        // Super spaghetti code!
        if (charRef.currentStatus == CharacterStatus.None)
        {
            statusIcon1.gameObject.SetActive(false);
            statusIcon2.gameObject.SetActive(false);
        }
        else if (charRef.currentStatus == CharacterStatus.Dead)
        {
            statusIcon1.gameObject.SetActive(true);
            statusIcon1.sprite = hud.GetStatusIcon(CharacterStatus.Dead);
            statusIcon2.gameObject.SetActive(false);
        }
        else
        {
            bool sick = (charRef.currentStatus & CharacterStatus.Sick) != CharacterStatus.None;
            bool breakdown = (charRef.currentStatus & CharacterStatus.Breakdown) != CharacterStatus.None;
            if (sick && !breakdown)
            {
                statusIcon1.gameObject.SetActive(true);
                statusIcon1.sprite = hud.GetStatusIcon(CharacterStatus.Sick);
                statusIcon2.gameObject.SetActive(false);
            }
            else if(breakdown && !sick)
            {
                statusIcon1.gameObject.SetActive(true);
                statusIcon1.sprite = hud.GetStatusIcon(CharacterStatus.Breakdown);
                statusIcon2.gameObject.SetActive(false);
            }
            else if (sick && breakdown)
            {
                statusIcon1.gameObject.SetActive(true);
                statusIcon1.sprite = hud.GetStatusIcon(CharacterStatus.Sick);
                statusIcon2.gameObject.SetActive(true);
                statusIcon2.sprite = hud.GetStatusIcon(CharacterStatus.Breakdown);
            }
        }
    }

    void UpdateHealthBar()
    {
        float ratio = charRef.health;
        healthBar.fillAmount = ratio;
        if (ratio < Character.kCriticalRatio)
        {
            healthBar.color = criticalColor;
        }
        else if (ratio < Character.kWarningRatio)
        {
            healthBar.color = warningColor;
        }
        else
        {
            healthBar.color = healthyColor;
        }

    }

    // Update is called once per frame
    void Update ()
    {
        if (charRef.Dead)
        {
            statusIcon1.sprite = hud.GetStatusIcon(CharacterStatus.Dead);
            statusIcon2.gameObject.SetActive(false);
            activityIcon.gameObject.SetActive(false);

            healthBar.fillAmount = 0;
            bg.color = Color.black;
            Color pColor = portrait.color;
            pColor.a = 0.2f;
            portrait.color = pColor;
            return;
        }

        UpdateStatus();
        UpdateHealthBar();
        UpdateActivity();
        selectionBar.gameObject.SetActive(charManagerRef.IsCharacterSelected(charRef));
    }
}
