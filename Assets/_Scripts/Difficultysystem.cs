using UnityEngine;

public class DifficultySystem : MonoBehaviour
{
    public Transform[] capsuleMeshes; // Array of mesh capsules
    public float minSize = 0.5f; // Minimum size of the capsules
    public float maxSize = 1.5f; // Maximum size of the capsules
    public float minSpeed = 3.0f; // Minimum speed of the capsules
    public float maxSpeed = 8.0f; // Maximum speed of the capsules

    public int maxDifficultyLevel = 10; // Maximum difficulty level
    public int scoreThresholdPerLevel = 100; // Score threshold per difficulty level

    private int currentDifficultyLevel = 1; // Current difficulty level

    private void Update()
    {
        // Update difficulty level based on score
        int currentScore = YourScoreSystem.GetCurrentScore();
        int newDifficultyLevel = Mathf.Min(currentScore / scoreThresholdPerLevel + 1, maxDifficultyLevel);

        // Check if the difficulty level has changed
        if (newDifficultyLevel != currentDifficultyLevel)
        {
            currentDifficultyLevel = newDifficultyLevel;
            ApplyDifficultyLevel();
        }
    }

    private void ApplyDifficultyLevel()
    {
        float sizeRatio = (float)currentDifficultyLevel / maxDifficultyLevel;
        float speedRatio = 1f - sizeRatio;

        // Scale the capsules based on the difficulty level
        foreach (Transform capsuleMesh in capsuleMeshes)
        {
            float newSize = Mathf.Lerp(minSize, maxSize, sizeRatio);
            capsuleMesh.localScale = new Vector3(newSize, newSize, newSize);

            float newSpeed = Mathf.Lerp(minSpeed, maxSpeed, speedRatio);
            capsuleMesh.GetComponent<RhythmObject>().SetSpeed(newSpeed);
        }
    }
}
