using UnityEngine;
using UnityEngine.UI;

public class Frontend : MonoBehaviour
{
    public VirtualScreen virtualScreen; // Reference to the VirtualScreen script
    public Text tradingViewText; // Reference to the UI Text object for TradingView data
    public Text youtubeText; // Reference to the UI Text object for YouTube data

    private void Start()
    {
        // Ensure the VirtualScreen script is attached to the same GameObject
        if (virtualScreen == null)
        {
            Debug.LogError("VirtualScreen script reference not set!");
        }
    }

    private void Update()
    {
        // Update the UI with the latest data from the VirtualScreen script
        UpdateTradingViewData();
        UpdateYouTubeData();
    }

    private void UpdateTradingViewData()
    {
        if (virtualScreen != null && tradingViewText != null)
        {
            tradingViewText.text = "TradingView Data:\n" +
                                   "Symbol: " + virtualScreen.GetTradingViewSymbol() + "\n" +
                                   "Price: " + virtualScreen.GetTradingViewPrice().ToString();
        }
    }

    private void UpdateYouTubeData()
    {
        if (virtualScreen != null && youtubeText != null)
        {
            string videos = "";
            foreach (var video in virtualScreen.GetYouTubeVideos())
            {
                videos += "- " + video + "\n";
            }

            youtubeText.text = "YouTube Data:\n" + videos;
        }
    }
}
