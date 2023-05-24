using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TradingViewColorUpdater : MonoBehaviour
{
    public string tradingViewAPIURL = "https://api.example.com/tradingview"; // Replace with the actual TradingView API URL
    public Renderer targetRenderer; // Reference to the renderer component of the object you want to change the material color

    private Material material;
    private Color defaultColor;

    private void Start()
    {
        // Get the material from the renderer
        material = targetRenderer.material;

        // Store the default color
        defaultColor = material.GetColor("_FresnelColor");

        // Start updating the color
        StartCoroutine(UpdateColorRoutine());
    }

    private IEnumerator UpdateColorRoutine()
    {
        while (true)
        {
            // Request data from the TradingView API
            using (UnityWebRequest webRequest = UnityWebRequest.Get(tradingViewAPIURL))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Parse the response JSON
                    string response = webRequest.downloadHandler.text;
                    Color newColor = ParseColorFromAPIResponse(response);

                    // Update the material color
                    material.SetColor("_FresnelColor", newColor);
                }
                else
                {
                    Debug.LogError("Failed to fetch data from TradingView API: " + webRequest.error);
                }
            }

            yield return new WaitForSeconds(5f); // Wait for 5 seconds before updating again
        }
    }

    private Color ParseColorFromAPIResponse(string response)
    {
        // Parse the JSON response and extract the color value
        // Implement your own logic to extract the color value from the API response

        // Example: Assuming the API response contains a hex color code
        string hexColor = "FF0000"; // Replace with the actual color value from the API
        Color color = ColorUtility.TryParseHtmlString("#" + hexColor, out Color parsedColor) ? parsedColor : defaultColor;

        return color;
    }
}
