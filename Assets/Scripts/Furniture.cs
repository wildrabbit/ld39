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
        Switch,
        Manning
    }

    public UseType useType;

    public float breakdownRate; // Probably too much
    public Skills requiredRepair; // Mechanic / Electric / Mason?
    public int requiredRepairLevel;
    public List<RequiredSkill> requiredSkills; // Use/manning

    public float rate;
    public bool continuous;
    public bool startEnabled;
    public ContinuousPowerDepleter depleter;
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

    void Update()
    {
        if (!isEnabled) return;
        // Do stuff
    }
}