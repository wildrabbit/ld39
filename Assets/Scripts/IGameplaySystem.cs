public interface IGameplaySystem
{
    void Initialise(GameplayManager gm);
    void StartGame();
    void UpdateSystem(float dt);
    void PauseGame(bool value);
    void GameFinished(GameResult result);
}