using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowHideUi : MonoBehaviour
{
    public Canvas canvas;
    public MeshRenderer mesh;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = true; // Enable the UI element
            mesh.enabled = false; // hide hologram
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            canvas.enabled = false; // Disable the UI element
            mesh.enabled = true; // show hologram
        }
    }
}
