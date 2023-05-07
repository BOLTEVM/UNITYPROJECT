using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class checkResizeRectTansform : MonoBehaviour
{
    // Start is called before the first frame update
    public float a, b;
    public float L0, M0, HL0;
    public int D0;
    GenerateDataStream xmlScript;
    void Start()
    {
        a = transform.GetComponent<RectTransform>().sizeDelta[0];
        b = transform.GetComponent<RectTransform>().sizeDelta[1];

        xmlScript = transform.GetComponent<GenerateDataStream>();
        L0 = xmlScript.L_width;
        M0 = xmlScript.M_width;
        D0 = xmlScript.nb__div;
        HL0 = xmlScript.HL_width;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float a1 = transform.GetComponent<RectTransform>().sizeDelta[0];
        float b1 = transform.GetComponent<RectTransform>().sizeDelta[1];


        if (a1!=a || b1!=b || L0 != xmlScript.L_width || M0 != xmlScript.M_width || D0 != xmlScript.nb__div || HL0 != xmlScript.HL_width)
        {
            a = a1;
            b = b1;
            L0 = xmlScript.L_width;

            xmlScript.replot();
        }

      
    }
}
