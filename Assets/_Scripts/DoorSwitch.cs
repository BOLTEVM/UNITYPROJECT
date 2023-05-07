using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitch : MonoBehaviour
{
    public SkinnedMeshRenderer doorRenderer;
    public void OpenDoor()
    {
        // Code to open the door
        // Get the bone transforms for the door
        Transform[] bones = doorRenderer.bones;

        // Find the bone that controls the door's rotation
        Transform doorBone = Array.Find(bones, bone => bone.name == "Close");

        // Rotate the bone to open the door
        doorBone.localRotation = Quaternion.Euler(0f, 90f, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Call the OpenDoor method to open the door
            OpenDoor();
        }
    }

}
