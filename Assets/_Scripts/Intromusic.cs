using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    public AudioSource audioSource; // Reference to the AudioSource component
    public KeyCode startKey = KeyCode.Space; // Key to start the game

    private bool isMusicPlaying = true;

    private void Start()
    {
        audioSource.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(startKey))
        {
            if (isMusicPlaying)
            {
                audioSource.Stop();
                isMusicPlaying = false;
            }
        }
    }
}
