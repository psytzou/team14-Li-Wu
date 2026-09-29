using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Tracks whether gameplay is currently active in the loaded scene: enables
// all EnemyBehaviors once a "Player"-tagged object is found (game started),
// and detects the player's death (chatemplate.cs destroys the GameObject at
// hp <= 0) to fire GameOverEvent. Re-scans on every scene load, since a menu
// scene has no player and a restart needs a fresh scan. Scene switching
// itself lives in GameSequence, not here. No VFX/UI -- GameOverEvent is the
// hook for your own UI to show its panel.
public static class PlayingSequence
{
    public static bool GameOver { get; private set; }
    public static bool GameWon { get; private set; }
    public static event Action GameOverEvent;
    public static event Action GameWonEvent;

    private static bool gameplayActive;
    private static PlayerController playerController;
    private static chatemplate playerCharacter;
    private static bool runnerCreated;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded += (scene, mode) => SetUpScene();
    }

    private static void SetUpScene()
    {
        GameOver = false;
        GameWon = false;

        if (!runnerCreated)
        {
            GameObject runner = new GameObject("PlayingSequence (auto)");
            runner.hideFlags = HideFlags.HideInHierarchy;
            UnityEngine.Object.DontDestroyOnLoad(runner);
            runner.AddComponent<PlayingSequenceRunner>();
            runnerCreated = true;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        gameplayActive = playerObject != null;

        if (gameplayActive)
        {
            playerController = playerObject.GetComponent<PlayerController>();
            playerCharacter = playerObject.GetComponent<chatemplate>();

            EnemyBehavior[] enemies = UnityEngine.Object.FindObjectsByType<EnemyBehavior>(FindObjectsSortMode.None);
            foreach (EnemyBehavior enemy in enemies)
                enemy.enabled = true;
        }
        else
        {
            playerController = null;
            playerCharacter = null;
        }
    }

    private static void Tick()
    {
        if (!gameplayActive || GameOver || GameWon) return;

        if (playerCharacter == null)
        {
            EndGame();
            return;
        }

        // Endless mode has no automatic win when the board is temporarily
        // clear. The spawner will continue adding enemies until Game Over.
    }

    private static void EndGame()
    {
        GameOver = true;
        gameplayActive = false;

        if (playerController != null)
            playerController.enabled = false;

        EnemyBehavior[] activeEnemies = UnityEngine.Object.FindObjectsByType<EnemyBehavior>(FindObjectsSortMode.None);
        foreach (EnemyBehavior enemy in activeEnemies)
            if (enemy != null)
                enemy.enabled = false;

        Debug.Log("Game Over -- character died.");
        GameOverEvent?.Invoke();
    }

    private static void WinGame()
    {
        GameWon = true;
        gameplayActive = false;

        if (playerController != null)
            playerController.enabled = false;

        Debug.Log("You Win -- all enemies defeated.");
        GameWonEvent?.Invoke();
    }

    // Only exists to give the static PlayingSequence a per-frame Update() call.
    private class PlayingSequenceRunner : MonoBehaviour
    {
        void Update() => Tick();
    }
}
