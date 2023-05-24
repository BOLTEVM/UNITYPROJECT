using UnityEngine;
using UnityEngine.XR;

public class PerspectiveToggle : MonoBehaviour
{
    public Camera vrCamera; // Reference to the VR camera
    public Camera nonVrCamera; // Reference to the non-VR camera

    private bool isVrEnabled = false; // Flag to track if VR mode is enabled

    private void Start()
    {
        SetVrMode(isVrEnabled);
    }

    private void Update()
    {
        // Check for the toggle input (e.g., a button press)
        if (Input.GetKeyDown(KeyCode.V))
        {
            // Toggle VR mode
            isVrEnabled = !isVrEnabled;
            SetVrMode(isVrEnabled);
        }
    }

    private void SetVrMode(bool enabled)
    {
        if (enabled)
        {
            XRSettings.LoadDeviceByName("OpenVR"); // Load the VR device
            XRSettings.enabled = true; // Enable XR mode
            vrCamera.gameObject.SetActive(true); // Enable the VR camera
            nonVrCamera.gameObject.SetActive(false); // Disable the non-VR camera
        }
        else
        {
            XRSettings.LoadDeviceByName(""); // Unload the VR device
            XRSettings.enabled = false; // Disable XR mode
            vrCamera.gameObject.SetActive(false); // Disable the VR camera
            nonVrCamera.gameObject.SetActive(true); // Enable the non-VR camera
        }
    }
}

