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

    [Tooltip("For toggle-like furniture")]
    public ContinuousPowerDepleter depleter = new ContinuousPowerDepleter();

    [Tooltip ("For one-shot and effect over time")]
    public SinglePowerDepleter oneoffDepleter = new SinglePowerDepleter();

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
    GameplayManager gameplayManager;
    public FurnitureConfig cfg;
    bool isEnabled = false;
    
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

    public bool TryCharacterInteractionStart(Character chara)
    {
        switch(cfg.useType)
        {
            case FurnitureConfig.UseType.EffectOverTime:
                {
                    CharacterActivity nodeActivity = (cfg.activity == CharacterActivity.Count)
                        ? CharacterActivity.Idle
                        : cfg.activity;

                    // TODO: Add context
                    ActivityContext ctxt = new ActivityContext();
                    ctxt.room = chara.currentRoom;
                    ctxt.chara = chara.charName;

                    EnvironmentActivityCheckResult envResult = gameplayManager.furnitureManager.CanActivityStartAt(nodeActivity, ctxt);
                    if (envResult != EnvironmentActivityCheckResult.Success)
                    {
                        SpeechHelper.ReportEnvironmentActivityCheck(chara, envResult);
                        return false;
                    }

                    CharacterActivityCheckResult checkResult = chara.CanCharacterEngageInActivity(nodeActivity, null);
                    SpeechHelper.ReportCharacterActivityCheck(chara, checkResult);
                    if (checkResult != CharacterActivityCheckResult.Success)
                    {
                        return false;
                    }

                    SetOperationState(true);
                    chara.SetCurrentActivity(nodeActivity);
                    if (cfg.depleter.source != "" && (cfg.activity != CharacterActivity.TV || gameplayManager.furnitureManager.NoTakenTVSeats()))
                    {
                        gameplayManager.generatorManager.AddDepleter(cfg.depleter);
                    }
                    return true;
                }
            case FurnitureConfig.UseType.Toggle:
                {
                    bool toggleValue = !isEnabled;
                    if (gameplayManager.generatorManager.remainingGenerator < 0)
                    {
                        toggleValue = false;
                    }

                    if (toggleValue)
                    {
                        gameplayManager.generatorManager.AddDepleter(cfg.depleter);
                    }
                    else
                    {
                        gameplayManager.generatorManager.RemoveDepleter(cfg.depleter.source);
                    }
                    SetOperationState(toggleValue);
                    break;
                }
            case FurnitureConfig.UseType.OneShot:
                {
                    // Do stuff
                    break;
                }
        }
        return false;
    }

    public void TryCharacterInteractionStop()
    {
        if (cfg.useType != FurnitureConfig.UseType.EffectOverTime) return;
        if (cfg.depleter.source != "" || (cfg.activity == CharacterActivity.TV && gameplayManager.furnitureManager.IsLastTVSeat(name)))
        {
            gameplayManager.generatorManager.RemoveDepleter(cfg.depleter.source);
        }
        SetOperationState(false);
    }

    public void SetManager(GameplayManager mgr)
    {
        gameplayManager = mgr;
    }
}