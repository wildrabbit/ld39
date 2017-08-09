using System.Collections;
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

    void Update()
    {
        if (gameplayManager.GameFinished) return;

        Update(timeManager.ScaledDelta);

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
    void Update (float delta)
    {
        float depletionScaler = 1.0f / timeManager.scale;
        float totalDecrease = 0.0f;
		for (int i = depleters.Count - 1; i >= 0; --i)
        {
            float localRate = maxGeneratorPower * delta / (depleters[i].timeToDepleteInHours * 3600);
            totalDecrease += localRate;
            depleters[i].Update(delta);
            if (depleters[i].Finished)
            {
                // Notify depleter removed??
                depleters.RemoveAt(i);
            }
        }

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
            return true;
        }
        return false;
    }
}
