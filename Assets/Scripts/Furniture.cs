using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FurnitureConfig
{
    [Serializable]
    public struct RequiredSkill
    {
        Skills skillRequired;
        int minSkill;
    }

    public enum UseType
    {
        Toggle,
        OneShot,
        EffectOverTime
    }

    public UseType useType = UseType.EffectOverTime;

    public CharacterActivity activity;

    //public float breakdownRate; // Probably too much
    //public Skills requiredRepair; // Mechanic / Electric / Mason?
    //public int requiredRepairLevel;
    public List<RequiredSkill> requiredSkills; // Use/manning

    public float effectAppliedRate;
    public bool continuous;
    public bool startEnabled;
    public AuxFurniture auxiliary;
}

public class Furniture : MonoBehaviour
{
    public FurnitureConfig cfg;
    bool isEnabled = false;
    public CharacterActivity activity = CharacterActivity.Count;

    public bool IsEnabled
    {
        get
        {
            return isEnabled;
        }
    }
    public void StartGame()
    {
        SetOperationState(cfg.startEnabled);
    }

    public void SetOperationState(bool value)
    {
        isEnabled = value;
        gameObject.SetActive(IsEnabled);
        if (cfg.auxiliary != null)
        {
            cfg.auxiliary.SetDependentItemEnabled(this, value);
        }
    }

    public void LogicUpdate(float dt)
    {
        if (!isEnabled) return;
        // Do stuff
    }
}