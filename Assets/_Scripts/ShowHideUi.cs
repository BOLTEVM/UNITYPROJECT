using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideUi : MonoBehaviour
{
    public Canvas canvas;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = true; // Enable the UI element
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = false; // Disable the UI element
        }
    }
}
