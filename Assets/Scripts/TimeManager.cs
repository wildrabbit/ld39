using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour, IGameplaySystem
{
    const float kSecsInHour = 3600;

    public float electricityBackETAHours = 24;
    public float scale = 1440.0f; // 1 day / minute

    GameplayManager gameplayManager;

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
    }


    public void UpdateSystem(float dt)
    {
        if (!gameplayManager.GameStarted || gameplayManager.Paused || gameplayManager.GameFinished) return;

        elapsed += Time.deltaTime;

        if (RemainingTime <= 0)
        {
            if (Mathf.Approximately(scale,0.0f))
            {
                scale = 1.0f;
            }
            elapsed = TotalSeconds / scale;
            
            gameplayManager.TimeFinished();
        }
    }

    public void PauseGame(bool value)
    {
    }

    public void GameFinished(GameResult result)
    {
    }
}
