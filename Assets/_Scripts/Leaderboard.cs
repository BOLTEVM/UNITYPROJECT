using UnityEngine;
using System.Collections.Generic;

public class LeaderboardSystem : MonoBehaviour
{
    public int maxScores = 10; // Maximum number of scores to keep in the leaderboard

    private List<int> scores = new List<int>(); // List to store the scores

    // Add a score to the leaderboard
    public void AddScore(int score)
    {
        scores.Add(score);
        scores.Sort((a, b) => b.CompareTo(a)); // Sort scores in descending order

        if (scores.Count > maxScores)
        {
            scores.RemoveAt(scores.Count - 1); // Remove the lowest score if the leaderboard is full
        }

        UpdateLeaderboard();
    }

    // Update the leaderboard display
    private void UpdateLeaderboard()
    {
        // Example implementation: Print leaderboard to console
        Debug.Log("Leaderboard:");

        for (int i = 0; i < scores.Count; i++)
        {
            Debug.Log("Rank " + (i + 1) + ": " + scores[i]);
        }
    }
}
