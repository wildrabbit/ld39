using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SinglePowerDepleter
{
    public string source;
    public bool absolute;
    public float depletionAmount; // If absolute is true, it'll be the exact amount to deduce. If false, a percentage   
}

[System.Serializable]
public class ContinuousPowerDepleter
{
    public string source = "base";
    public float timeToDepleteInHours = 24; // No scales!
    public float duration = -1; // -1: permanent (until removed for other reasons)
    private float remaining;

    public bool Finished
    {
        get { return duration > 0 && remaining <= 0.0f; }
    }

    public void Reset()
    {
        remaining = duration;
    }

    public void Update(float delta)
    {
        remaining -= delta;
        if (remaining < 0.0f)
        {
            remaining = 0.0f;
        }
    }

    public float GetDepletionPercentRate()
    {
        return 1 / (3600 * timeToDepleteInHours);
    }


}

public class GeneratorManager : MonoBehaviour, IGameplaySystem
{
    GameplayManager gameplayManager;

    // Useful refs
    TimeManager timeManager;
    CharacterManager characterManager;

    [Header("Config data")]
    public float maxGeneratorPower = 100;
    
    public ContinuousPowerDepleter baseDepleter;

    public float showWarningBelowRatio = 0.1f;
    bool showedWarning = false;

    public Action OnGeneratorPowerDown;

    //------------------------

    List<ContinuousPowerDepleter> depleters = new List<ContinuousPowerDepleter>();    

    public float remainingGenerator;

    public float PowerRatio
    {
        get
        {
            return remainingGenerator / maxGeneratorPower;
        }
    }

    public void Initialise(GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
        timeManager = gameplayManager.timeManager;
        characterManager = gameplayManager.characterManager;
    }

    // Use this for initialization
    public void StartGame ()
    {
        remainingGenerator = maxGeneratorPower;        
        depleters.Add(baseDepleter);
	}

    public void UpdateSystem(float dt)
    {
        if (gameplayManager.GameFinished) return;

        LogicUpdate(dt);

        if (PowerRatio < showWarningBelowRatio && !showedWarning)
        {
            IEnumerator<Character> chars = characterManager.GetCharactersIterator();
            while (chars.MoveNext())
            {
                if (!chars.Current.Dead)
                {
                    chars.Current.Talk(SpeechEntry.NoPower);
                }
            }
            showedWarning = true;
        }
    }

    // Update is called once per frame
    void LogicUpdate (float delta)
    {
        float scaledDelta = timeManager.ScaledDelta;
        float totalDecrease = 0.0f;
        float totalRate = 0.0f;
		for (int i = depleters.Count - 1; i >= 0; --i)
        {
            depleters[i].Update(scaledDelta);
            if (depleters[i].Finished)
            {
                // Notify depleter removed??
                depleters.RemoveAt(i);
            }
            else
            {
                float localRate = depleters[i].GetDepletionPercentRate();
                totalRate += localRate;
                //Debug.Log($"Local depleter: {depleters[i].source}, rate: {localRate:0.#######}, accum. Rate: {totalRate:0.#######}");
                Mathf.Clamp01(totalRate);
            }
        }
        totalDecrease = maxGeneratorPower * scaledDelta * totalRate;
        //Debug.Log($"Scaled secs: {scaledDelta:0.###}, decrease amount: {totalDecrease:0.###}");

        remainingGenerator -= totalDecrease;
        
        CheckGeneratorStatus();
	}

    public void AddDepleter(ContinuousPowerDepleter depleter)
    {
        // Check multiple additions?
        depleter.Reset();
        depleters.Add(depleter);
        // Notify add.
    }

    public void RemoveDepleter(string source)
    {
        ContinuousPowerDepleter depleter = depleters.Find(x => x.source == source);
        if (depleter != null)
        {
            depleters.Remove(depleter);
            // Notify removed!
        }
    }

    public void ApplyDepleter(SinglePowerDepleter surge)
    {
        // Use source for logging or other stuff
        float amount = (surge.absolute) ? surge.depletionAmount : surge.depletionAmount * remainingGenerator;
        remainingGenerator -= amount;
        CheckGeneratorStatus();
    }

    public bool CheckGeneratorStatus()
    {
        if (remainingGenerator <= 0)
        {
            if (OnGeneratorPowerDown != null)
            {
                OnGeneratorPowerDown();
            }

            return true;
        }
        return false;
    }
    
    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
    }
}
