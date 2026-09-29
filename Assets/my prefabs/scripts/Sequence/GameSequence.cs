using UnityEngine;
using UnityEngine.SceneManagement;

// Owns scene switching only: menu <-> gameplay, and restart. Doesn't hold
// any scene name itself -- the caller (MainMenu/GameOverMenu) supplies it
// via its own Inspector field, so which scene is which is picked in the
// Editor rather than hardcoded here. Gameplay state (enemy activation,
// death detection, GameOver) lives in PlayingSequence, not here.
public static class GameSequence
{
    public static void StartGame(SelectedCharacter.CharacterType character, string gameplaySceneName)
    {
        SelectedCharacter.Current = character;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public static void Restart()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Time.timeScale = 1f;

        if (activeScene.buildIndex >= 0)
        {
            Debug.Log($"Restarting scene at build index {activeScene.buildIndex}: {activeScene.name}");
            SceneManager.LoadScene(activeScene.buildIndex, LoadSceneMode.Single);
            return;
        }

        Debug.Log($"Restarting scene by name: {activeScene.name}");
        SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
    }

    public static void BackToMenu(string menuSceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
}
