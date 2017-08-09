using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour, IGameplaySystem
{
    const float kSecsInHour = 3600;

    public float electricityBackETAHours = 24;
    public float scale = 1440.0f; // 1 day / minute

    GameplayManager gameplayManager;

    [HideInInspector]
    public bool finished = false;

    public float TotalSeconds
    {
        get { return kSecsInHour * electricityBackETAHours; }
    }

    public float RemainingTime
    {
        get { return TotalSeconds - (scale * elapsed); }
    }


    public float ScaledDelta
    {
        get { return Time.deltaTime * scale; }
    }

    float elapsed = 0.0f;

    public void Initialise(GameplayManager _gameplayManager)
    {
        gameplayManager = _gameplayManager;
    }

    public void StartGame()
    {
        elapsed = 0.0f;
        finished = false;             
    }

    private void Update()
    {
        if (finished) return;

        elapsed += Time.deltaTime;
        
        if (RemainingTime <= 0)
        {
            finished = true;
            // Notify time out
        }
    }
}
