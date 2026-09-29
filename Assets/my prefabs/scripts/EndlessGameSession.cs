using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EndlessGameSession : MonoBehaviour
{
    private const string ScoreKeyPrefix = "EndlessBoard.HighScore.";

    public static EndlessGameSession Instance { get; private set; }

    [SerializeField, Range(1, 20)] private int leaderboardSize = 10;

    public int KillCount { get; private set; }
    public int FinalRank { get; private set; }
    public bool IsFinalized { get; private set; }

    public event Action<int> KillCountChanged;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        EnemyBehavior.EnemyDefeated += HandleEnemyDefeated;
        PlayingSequence.GameOverEvent += FinalizeRun;
    }

    private void OnDisable()
    {
        EnemyBehavior.EnemyDefeated -= HandleEnemyDefeated;
        PlayingSequence.GameOverEvent -= FinalizeRun;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void HandleEnemyDefeated(EnemyBehavior enemy, chatemplate killer)
    {
        if (IsFinalized || killer == null) return;

        bool killedByPlayer = killer.GetComponent<PlayerController>() != null || killer.CompareTag("Player");
        if (!killedByPlayer) return;

        KillCount++;
        KillCountChanged?.Invoke(KillCount);
    }

    public void FinalizeRun()
    {
        if (IsFinalized) return;

        IsFinalized = true;
        List<int> scores = LoadScores();
        FinalRank = 1;
        foreach (int score in scores)
        {
            if (score > KillCount)
                FinalRank++;
        }

        scores.Add(KillCount);
        scores.Sort((left, right) => right.CompareTo(left));

        int countToSave = Mathf.Min(leaderboardSize, scores.Count);
        for (int index = 0; index < countToSave; index++)
            PlayerPrefs.SetInt(ScoreKeyPrefix + index, scores[index]);

        for (int index = countToSave; index < leaderboardSize; index++)
            PlayerPrefs.DeleteKey(ScoreKeyPrefix + index);

        PlayerPrefs.Save();
    }

    public IReadOnlyList<int> GetLeaderboard()
    {
        return LoadScores();
    }

    private List<int> LoadScores()
    {
        List<int> scores = new List<int>();
        for (int index = 0; index < leaderboardSize; index++)
        {
            string key = ScoreKeyPrefix + index;
            if (PlayerPrefs.HasKey(key))
                scores.Add(PlayerPrefs.GetInt(key));
        }

        scores.Sort((left, right) => right.CompareTo(left));
        return scores;
    }
}
