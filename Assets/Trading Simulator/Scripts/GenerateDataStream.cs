using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GenerateDataStream : MonoBehaviour
{

    //public initial capital and benefit
    public float capital;
    public float initialCapital=1000;
    Text capital_txt;
    Text benefit_txt;

    // line parameters
    [Range(0,15)]
    public float L_width = 5f;

    [Range(0, 15)]
    public float M_width = 5f;

    [Range(2, 10)]
    public int nb__div = 5;

    [Range(0, 10)]
    public int HL_width = 1;

    [Range(0, 500)]
    public int nb_initial_Ticks = 200;

    public int arraySize = 100000;

    // line prefab for UP and DOWN behaviour
    public GameObject prefab_UP, prefab_DOWN, prefab_HORIZ;

    //time counting event 
    float elapsed=100000, elapsed2;

    //these two arrays contain the values for the line chart
    public float[] x_value;
    public float[] y_Open_value;
    public float[] y_Close_value;
    public float[] y_min_value;
    public float[] y_Max_value;

    //maximum values for x and y
    public float xmax, ymax, xmin, ymin;

    //tick_latency [s]
    public float tick_latency=1;
    //tick_duration [s]
    public float tick_duration = 10;

    //this is the value of the market at the instant given
    float y_value;

    //this is the initial conditions for the simulation
    public float min_seed_value = 100;
    public float max_seed_value = 200;
    public float vol=10;

    //these are the gameobjects containing the markers
    GameObject[] goMaxMin;
    GameObject[] goOpenClose;

    // intermediate vectors
    Vector3[] vMax ;
    Vector3[] vmin ;
    Vector3[] vop ;
    Vector3[] vcl ;

    //this is the horizontal line
    Transform lineH;

    //these are the variables used to change the size of the render of the chart
    float tf_FactorA;
    float tf_FactorB;
    float a;
    float b;

    // colors for the markers
    public Color colUP, colDOWN;

    //fork
    public float fork = 1;

    //variables for trading
    Text buy_txt, sell_txt;
    Button buy_but, sell_but, close_but;


    // selling and buying values
    float set_buy=0;
    float set_sel=0;
    float y_buy, y_sell;

    //this is the trading state
    //    0--> none,  1-->buy  2-->sell
    public int tradingState=0;

    // line horizontal for trading
    public GameObject tradingLine;

    //volume value
    InputField input_Volume_txt;
    int val_volume;


    //generates the initial data of the market
    public void generate_initial_data()
    {
        //we use float values
        x_value = new float[arraySize];
        y_Open_value = new float[arraySize];
        y_Close_value = new float[arraySize];
        y_min_value = new float[arraySize];
        y_Max_value = new float[arraySize];


        //initialize the values of the gameobjects
        goMaxMin = new GameObject[arraySize];
        goOpenClose = new GameObject[arraySize];

        float ym = Random.Range(min_seed_value, max_seed_value);

        //maximum limits
        xmax = -100000000;
        ymax = -100000000;

        xmin = 100000000;
        ymin = 100000000;

        for (int i = 0; i < nb_initial_Ticks; i++)
        {

            //obtaining values of the finantial behaviour
            ym = ym + Random.Range(-vol, vol);
            float yM = Mathf.Round(ym * Random.Range(1+vol/1000,1+vol/100));

            float yo = Random.Range(ym, yM);
            float yc = Random.Range(ym, yM);

            x_value[i] = i;
            y_Open_value[i] =yo;
            y_Close_value[i] =yc;
            y_min_value[i] =ym;
            y_Max_value[i] =yM;

            xmax = Mathf.Max(xmax, x_value[i]);
            ymax = Mathf.Max(ymax, y_Max_value[i]);

            xmin = Mathf.Min(xmin, x_value[i]);
            ymin = Mathf.Min(ymin, y_min_value[i]);

         }

        //obtainning the value of y AND INITIAL CONDITIONS FOR SIMULATION
        y_value = Random.Range(y_min_value[nb_initial_Ticks-1], y_Max_value[nb_initial_Ticks-1]);
        y_Open_value[nb_initial_Ticks] = y_value;
        y_min_value[nb_initial_Ticks] = y_value;
        y_Max_value[nb_initial_Ticks] = y_value;
        x_value[nb_initial_Ticks] = nb_initial_Ticks;

        goOpenClose[nb_initial_Ticks] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
        goMaxMin[nb_initial_Ticks] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));



    }


    // in the start function the data is loaded and the chart is drawn
    void Start()
    {
        //get trading variables
        buy_txt = GameObject.Find("buy_txt").GetComponent<Text>();
        sell_txt = GameObject.Find("sell_txt").GetComponent<Text>();
        buy_but= GameObject.Find("buy_but").GetComponent<Button>();
        sell_but = GameObject.Find("sell_but").GetComponent<Button>();
        input_Volume_txt= GameObject.Find("input_volume").GetComponent<InputField>();

        //get horizontal line
        lineH = GameObject.Find("positioningLineH").transform;

        //initialcapital
        capital = initialCapital;
        capital_txt = GameObject.Find("capital_txt").GetComponent<Text>();
        updateCaptital(0);
        benefit_txt = GameObject.Find("benefit_txt").GetComponent<Text>();
        //generate data
        generate_initial_data();

        //change ticks of the subtitle
        GameObject.FindGameObjectWithTag("subtitle").GetComponent<Text>().text = "" + nb_initial_Ticks + " ticks";

        //get initial volume
        changeVolume();
        plotChart();

       

    }


    // this function is called when a variable is changed in the inspector
    public void replot()
    {
        clearChart();

        //Debug.Log("reploting");

        //new limits for the chart
        for (int i = 0; i < nb_initial_Ticks; i++)
        {
            xmax = Mathf.Max(xmax, x_value[i]);
            ymax = Mathf.Max(ymax, y_Max_value[i]);

            xmin = Mathf.Min(xmin, x_value[i]);
            ymin = Mathf.Min(ymin, y_min_value[i]);
        }


        plotChart();
        //change ticks of the subtitle
        GameObject.FindGameObjectWithTag("subtitle").GetComponent<Text>().text = "" + nb_initial_Ticks + " ticks";
    }


    //deletes the chart data
    public void clearChart()
    {

        // get containers for the lines and the markers
        GameObject[] lines = GameObject.FindGameObjectsWithTag("line");

        for (int i = 0; i < lines.Length; i++)
        {
            Destroy(lines[i]);
        }
        
    }

    //draws the chart
    public void plotChart()
    {

        //graphic parameters
        Vector2 sizeChart=transform.GetComponent<RectTransform>().sizeDelta;

        //size of the chart in two components
        a = sizeChart[0];
        b = sizeChart[1];


        // get containers for the lines and the markers
        GameObject line_container = GameObject.Find("lines");
        GameObject Hline_container = GameObject.Find("Hlines");


        //reference to the gameobjects used in the plot
        vMax = new Vector3[arraySize];
        vmin = new Vector3[arraySize];
        vop = new Vector3[arraySize];
        vcl = new Vector3[arraySize];

       
        GameObject[] horiz_line = new GameObject[nb__div+1]; 


        tf_FactorA = b / (ymax-ymin);
        tf_FactorB = -ymin / (ymax - ymin)*b;


        
        // create horizontal lines
        for (int j = 0; j <=nb__div; j++)
        {
            horiz_line[j] = GameObject.Instantiate(prefab_HORIZ, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            horiz_line[j].transform.SetParent(Hline_container.transform);

            float y_HLvalue = (float)j/nb__div * (ymax - ymin) + ymin;

            horiz_line[j].transform.localPosition = new Vector3(0, (y_HLvalue)* tf_FactorA + tf_FactorB - b / 2,0);

            horiz_line[j].transform.GetComponent<RectTransform>().sizeDelta = new Vector2( a, HL_width);
            horiz_line[j].transform.right = transform.right;

            // set text with the value
            horiz_line[j].transform.GetChild(0).GetComponent<Text>().text = "$" + Mathf.Round(y_HLvalue);

        }



        for (int i=0; i<nb_initial_Ticks;i++)
        {
            //get maximum point
            vMax[i] = new Vector3(x_value[i]*a/xmax-a/2, y_Max_value[i]*tf_FactorA+tf_FactorB-b/2,0);
            
            //get the minimum point
            vmin[i] = new Vector3(x_value[i] * a / xmax - a / 2, y_min_value[i] *tf_FactorA +tf_FactorB- b / 2,0);

            //get the open point
            vop[i] = new Vector3(x_value[i] * a / xmax - a / 2, y_Open_value[i] * tf_FactorA + tf_FactorB - b / 2, 0);

            //get the close point
            vcl[i] = new Vector3(x_value[i] * a / xmax - a / 2, y_Close_value[i] * tf_FactorA + tf_FactorB - b / 2, 0);


            
            //instantiate a line between max and min markers
            if (y_Close_value[i]>=y_Open_value[i])
            {
                goMaxMin[i] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            }
            else
            {
                goMaxMin[i] = GameObject.Instantiate(prefab_DOWN, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            }

            //set the text to the limits
            goMaxMin[i].transform.GetChild(2).GetComponent<Text>().text = "$"+ Mathf.Round(y_Max_value[i]);
            goMaxMin[i].transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_min_value[i]);


            goMaxMin[i].transform.SetParent(line_container.transform);

            Vector3 dir = (vMax[i] - vmin[i]) / 2;

            goMaxMin[i].transform.localPosition = vmin[i] + dir;
            goMaxMin[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2*dir.magnitude, L_width);
            goMaxMin[i].transform.right = dir;


            //instantiate a line between open and close markers
            if (y_Close_value[i] >= y_Open_value[i])
            {
                goOpenClose[i] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));

                goOpenClose[i].transform.SetParent(line_container.transform);

                dir = (vcl[i] - vop[i]) / 2;

                goOpenClose[i].transform.localPosition = vop[i] + dir;
                goOpenClose[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, M_width);
                goOpenClose[i].transform.right = dir;


                //set the text to the limits
                goOpenClose[i].transform.GetChild(2).GetComponent<Text>().text = "$" + Mathf.Round(y_Close_value[i]);
                goOpenClose[i].transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_Open_value[i]);


            }
            else   //invert direction
            {
                goOpenClose[i] = GameObject.Instantiate(prefab_DOWN, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));


                goOpenClose[i].transform.SetParent(line_container.transform);

                dir = (vop[i] - vcl[i]) / 2;

                goOpenClose[i].transform.localPosition = vcl[i] + dir;
                goOpenClose[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, M_width);
                goOpenClose[i].transform.right = dir;


                //set the text to the limits
                goOpenClose[i].transform.GetChild(2).GetComponent<Text>().text = "$" + Mathf.Round(y_Open_value[i]);
                goOpenClose[i].transform.GetChild(3).GetComponent<Text>().text = "$" + Mathf.Round(y_Close_value[i]);
            }
            
            

        }


    }

    // this changes the values of a marker given its index
    void setMarker_mM(float px, float pym, float pyM)
    {
        int i = (int)px;

        //get the open point
        vmin[i] = new Vector3(i * a / xmax - a / 2, pym * tf_FactorA + tf_FactorB - b / 2, 0);
        //get the close point
        vMax[i] = new Vector3(i * a / xmax - a / 2, pyM * tf_FactorA + tf_FactorB - b / 2, 0);


        Vector3 dir = (vMax[i] - vmin[i]) / 2;

        goMaxMin[i].transform.localPosition = vmin[i] + dir;
        goMaxMin[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, L_width);
        goMaxMin[i].transform.right = dir;
    }

    void setMarker_oc(float px, float pyo, float pyc)
    {
        int i = (int)px;

        //get the open point
        vop[i] = new Vector3(i * a / xmax - a / 2, pyo* tf_FactorA + tf_FactorB - b / 2, 0);
        //get the close point
        vcl[i] = new Vector3(i * a / xmax - a / 2, pyc* tf_FactorA + tf_FactorB - b / 2, 0);

        if (pyc > pyo)
        {
            Vector3 dir = (vcl[i] - vop[i]) / 2;

            goOpenClose[i].transform.localPosition = vop[i] + dir;
            goOpenClose[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, M_width);
            goOpenClose[i].transform.right = dir;
            goOpenClose[i].transform.GetComponent<Image>().color = colUP;
            goMaxMin[i].transform.GetComponent<Image>().color = colUP;
        }
        else
        {
            Vector3 dir = (vop[i] - vcl[i]) / 2;

            goOpenClose[i].transform.localPosition = vcl[i] + dir;
            goOpenClose[i].transform.GetComponent<RectTransform>().sizeDelta = new Vector2(2 * dir.magnitude, M_width);
            goOpenClose[i].transform.GetComponent<Image>().color = colDOWN;
            goMaxMin[i].transform.GetComponent<Image>().color = colDOWN;

            goOpenClose[i].transform.right = dir;
        }

    }



    //////////////////////////////
    // trading operations
    // ///////////////////////////
    public void updateTradingValues()
    {

        //forks
        set_buy = y_value + fork;
        set_sel = y_value - fork;

        buy_txt.text="$"+Mathf.Round((set_buy)*100)/100;
        sell_txt.text = "$" + Mathf.Round((set_sel)*100)/100;


        // benefit
        if (tradingState == 1)
        {
            benefit_txt.text = "$"+ Mathf.Round((float)val_volume*(y_value - y_buy)*100)/100;
            if (y_value - y_buy>0)
            {
                benefit_txt.color = colUP;
            }
            else
            {
                benefit_txt.color = colDOWN;
            }
        }
        else if(tradingState==2)
        {
            benefit_txt.text = "$" + Mathf.Round((float)val_volume*(-y_value + y_sell) * 100) / 100;
            if (-y_value + y_sell>0)
            {
                benefit_txt.color = colUP;
            }
            else
            {
                benefit_txt.color = colDOWN;
            }

        }
        else if(tradingState==0)
        {
            benefit_txt.text = "$" + 0;
            benefit_txt.color = Color.black;
        }

    } 


    public void changeVolume()
    {
        val_volume = int.Parse(input_Volume_txt.text);
    }


    public void performBuy()
    {
        tradingState = 1;

        y_buy = set_buy;

        tradingLine.transform.localPosition = new Vector3(0, y_buy * tf_FactorA + tf_FactorB - b / 2, 0);
        tradingLine.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(3, a);

    }

    public void performSell()
    {
        tradingState = 2;

        y_sell = set_sel;

        tradingLine.transform.localPosition = new Vector3(0, y_sell * tf_FactorA + tf_FactorB - b / 2, 0);
        tradingLine.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(3,a);

        
    }

    public void closeTransaction()
    {
        

        tradingLine.transform.position =new Vector3(0,-1000000000000,0);

        if(tradingState==1)
        {
            updateCaptital((float)val_volume*(y_value - y_buy));
        }
        else
        {
            updateCaptital((float)val_volume*(y_sell- y_value));
        }

        tradingState = 0;

    }

    void updateCaptital(float a)
    {
        capital += a;
        capital_txt.text = "$" + Mathf.Round( capital*100)/100;

    }






    //////////////////////////////
    //**SIMULATION EVENTS **//
    // ///////////////////////////
    
    public void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        elapsed2 += Time.fixedDeltaTime;

        if (elapsed >tick_latency)
        {
            y_value = y_value + Random.Range(-vol/5, vol/5);

            //TRADING VALUES MUST BE UPDATED
            updateTradingValues();

            y_min_value[nb_initial_Ticks-1 ] = Mathf.Min(y_value,y_min_value[nb_initial_Ticks -1]);
            y_Max_value[nb_initial_Ticks-1 ] = Mathf.Max(y_value, y_Max_value[nb_initial_Ticks -1]);

            //CHECK IF LIMITS ARE OUT OF CHART 
            if (y_min_value[nb_initial_Ticks - 1] < ymin || y_Max_value[nb_initial_Ticks - 1] > ymax)
            {
                replot();

            }

            //position of the line that show the value of the market at that moment
            lineH.localPosition = new Vector3(0, y_value * tf_FactorA + tf_FactorB - b / 2, 0);
            lineH.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(a,3);
            //set value of the y_value
            lineH.transform.GetChild(1).GetComponent<Text>().text = "$" + Mathf.Round( y_value);
            lineH.transform.forward = transform.forward;

            setMarker_oc((float) nb_initial_Ticks - 1, y_Open_value[nb_initial_Ticks], y_value);
            setMarker_mM((float) nb_initial_Ticks - 1, y_min_value[nb_initial_Ticks - 1], y_Max_value[nb_initial_Ticks - 1]);
                
            elapsed = 0;

           

            if(elapsed2>tick_duration)
            {
                y_Close_value[nb_initial_Ticks ] = y_value;

                //increase the points by one
                nb_initial_Ticks += 1;


                //obtainning the value of y AND NEXT CONDITIONS FOR SIMULATION
                //y_value = y_value + Random.Range(-vol / 5, vol / 5);
                y_Open_value[nb_initial_Ticks] = y_value;
                y_min_value[nb_initial_Ticks] = y_value;
                y_Max_value[nb_initial_Ticks] = y_value;
                x_value[nb_initial_Ticks] = nb_initial_Ticks;
                goOpenClose[nb_initial_Ticks] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
                goMaxMin[nb_initial_Ticks] = GameObject.Instantiate(prefab_UP, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));

                replot();

                

                elapsed2 = 0;
            }
           
        }
    }

}