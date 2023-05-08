using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveSwitch : MonoBehaviour
{
    private Animator mAnimtr;

    public void OpenDoor()
    {
        if (mAnimtr != null)
        {
            mAnimtr.SetTrigger("TrUp");
        }
    }

    public void CloseDoor()
    {
        if (mAnimtr != null)
        {
            mAnimtr.SetTrigger("TrDwn");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        mAnimtr = GetComponent<Animator>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            OpenDoor();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            CloseDoor();
    }
}
