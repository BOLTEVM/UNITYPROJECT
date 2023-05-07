using UnityEngine;
using UnityEngine.UI;

namespace NextMind.Examples.NeuroTagGallery
{
    /// <summary>
    /// Class used to change the description of a step when we click on an object.
    /// </summary>
    public class DescriptionModifier : MonoBehaviour
    {
        [Header("Description text")]

        /// <summary>
        /// Where to print the new text.
        /// </summary>
        [SerializeField]
        private Text descriptionText;

        /// <summary>
        /// The text to concatenate.
        /// </summary>
        [TextArea, SerializeField]
        private string newDescription;

        [Header("Lines")]

        /// <summary>
        /// The underline to turn on.
        /// </summary>
        [SerializeField]
        private GameObject selectedLine;

        /// <summary>
        /// The underlines to turn off.
        /// </summary>
        [SerializeField]
        private GameObject otherLine1;
        [SerializeField]
        private GameObject otherLine2;

        /// <summary>
        /// The original description text that was displayed.
        /// </summary>
        private string defaultDescriptionText;

        private void Awake()
        {
            defaultDescriptionText = descriptionText.text;
        }

        private void OnMouseDown()
        {
            descriptionText.text = defaultDescriptionText + "\n\n" + newDescription;

            selectedLine.SetActive(true);
            otherLine1.SetActive(false);
            otherLine2.SetActive(false);
        }
    }
}
