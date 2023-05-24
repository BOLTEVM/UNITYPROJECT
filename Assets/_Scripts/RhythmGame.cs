using UnityEngine;

public class RhythmGame : MonoBehaviour
{
    public Transform[] lanePositions; // Array of lane positions
    public Transform capsuleMesh; // Reference to the Capsule Mesh object

    public float laneWidth = 1.0f; // Width of each lane
    public float laneSpeed = 5.0f; // Speed at which the lanes move

    public float perfectOffset = 0.1f; // Time window to hit a note perfectly
    public float goodOffset = 0.2f; // Time window to hit a note with some tolerance

    public AudioSource audioSource; // Reference to the AudioSource component
    private float songTime = 0f; // Current song time

    private void Update()
    {
        // Move the lanes vertically
        float verticalMovement = Input.GetAxis("Vertical") * laneSpeed * Time.deltaTime;
        transform.Translate(0f, verticalMovement, 0f);

        // Move the lanes horizontally
        float horizontalMovement = Input.GetAxis("Horizontal") * laneSpeed * Time.deltaTime;
        transform.Translate(horizontalMovement, 0f, 0f);

        // Update the song time
        songTime = audioSource.time;

        // Check for note hits
        foreach (Transform lanePosition in lanePositions)
        {
            // Calculate the distance between the capsule mesh and the lane position
            float distance = Vector3.Distance(capsuleMesh.position, lanePosition.position);

            // Check if the distance is within the hit window
            if (distance < laneWidth / 2)
            {
                // Calculate the timing offset
                float timingOffset = Mathf.Abs(songTime % 1f - 0.5f);

                // Check the timing offset against the hit windows
                if (timingOffset < perfectOffset)
                {
                    // Perfect hit
                    IncreaseScore(100);
                }
                else if (timingOffset < goodOffset)
                {
                    // Good hit
                    IncreaseScore(50);
                }
            }
        }
    }

    private void IncreaseScore(int points)
    {
        Debug.Log("Score: " + points);
    }
}
