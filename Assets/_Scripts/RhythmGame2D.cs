using UnityEngine;

public class RhythmGame2D : MonoBehaviour
{
    public Transform[] lanePositions; // Array of lane positions
    public GameObject notePrefab; // Prefab for the notes

    public float laneWidth = 1.0f; // Width of each lane
    public float laneSpeed = 5.0f; // Speed at which the lanes move

    public float perfectOffset = 0.1f; // Time window to hit a note perfectly
    public float goodOffset = 0.2f; // Time window to hit a note with some tolerance

    private void Update()
    {
        // Move the lanes vertically
        float verticalMovement = Input.GetAxis("Vertical") * laneSpeed * Time.deltaTime;
        transform.Translate(0f, verticalMovement, 0f);

        // Spawn notes
        foreach (Transform lanePosition in lanePositions)
        {
            if (Random.Range(0f, 1f) < 0.02f) // Adjust the probability based on your game's needs
            {
                SpawnNoteAtPosition(lanePosition.position);
            }
        }
    }

    private void SpawnNoteAtPosition(Vector3 position)
    {
        GameObject note = Instantiate(notePrefab, position, Quaternion.identity);
        note.GetComponent<Note2D>().Initialize(laneSpeed);
    }

    public void HitNote()
    {
        Debug.Log("Note hit!");
        // Increase score or perform other actions when a note is hit
    }
}
