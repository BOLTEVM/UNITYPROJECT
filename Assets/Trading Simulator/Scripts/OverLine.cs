using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//this code allows the user to point over a marker and display the values of the stock market in that marker
public class OverLine : MonoBehaviour
{
    // Start is called before the first frame update
    Transform lineTF;

    void Start()
    {
        lineTF = GameObject.FindGameObjectWithTag("positioningLine").transform;
        lineTF.GetComponent<Image>().enabled = false;
    }

    // Update is called once per frame
    public void onEnter()
    {
        lineTF.transform.localPosition = new Vector3(transform.localPosition[0],0,0);

        //set the text and images to true
        lineTF.GetComponent<Image>().enabled = true;
        transform.GetChild(0).GetComponent<Image>().enabled = true;
        transform.GetChild(1).GetComponent<Image>().enabled = true;
        transform.GetChild(2).GetComponent<Text>().enabled = true;
        transform.GetChild(3).GetComponent<Text>().enabled = true;
        transform.SetAsLastSibling();
    }

    public void onExit()
    {
        //set the text and images to false
        lineTF.GetComponent<Image>().enabled = false;
        transform.GetChild(0).GetComponent<Image>().enabled = false;
        transform.GetChild(1).GetComponent<Image>().enabled = false;
        transform.GetChild(2).GetComponent<Text>().enabled = false;
        transform.GetChild(3).GetComponent<Text>().enabled = false;
    }
}
