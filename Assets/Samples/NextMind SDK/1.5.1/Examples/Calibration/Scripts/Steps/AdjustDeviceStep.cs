using NextMind.Devices;
using NextMind.Examples.Steps;
using NextMind.Examples.Utility;
using UnityEngine;
using UnityEngine.Video;

using ContactGrade = NextMind.Core.Event.ContactEvent.Type;

namespace NextMind.Examples.Calibration
{
    /// <summary>
    /// Implementation of an <see cref="AbstractStep"/> managed by the <see cref="StepsManager"/>.
    /// During this step, the user has to adjust the sensor on his head. It displays a validation message when the values are reliable enough.
    /// </summary>
    public class AdjustDeviceStep : AbstractStep
    {
        [SerializeField]
        private CanvasFader troubleshootingCanvasFader;
        [SerializeField]
        private VideoPlayer troubleshootingVideoPlayer;

        [SerializeField]
        private ContactQualitySlider contactQualitySlider = null;
        [SerializeField]
        private ContactQualitySlider helpPopupContactQualitySlider = null;

        private readonly float timeBeforeHelpDisplay = 30f;
        private float helpCurrentTimer = 0f;

        #region AbstractStep implementation

        public override void UpdateStep()
        {
            // Show the help part if the average value remain under Good during timeBeforeHelpDisplay seconds.
            var neuroManager = NeuroManager.Instance;
            if (neuroManager.ConnectedDevices.Count > 0)
            {
                Device connectedDevice = neuroManager.ConnectedDevices[0];

                float normalizedContactGrade = connectedDevice.GetNormalizedContactGrade();
                contactQualitySlider.CurrentGlobalScore = normalizedContactGrade;
                helpPopupContactQualitySlider.CurrentGlobalScore = normalizedContactGrade;
                
                // If the troubleshooting canvas is already visible, stop here.
                if (troubleshootingCanvasFader.IsCanvasVisible)
                {
                    return;
                }

                ContactGrade contactGrade = connectedDevice.GetContactGrade();
                switch (contactGrade)
                {
                    case ContactGrade.NO_CONTACT:
                    case ContactGrade.WEAK:
                    case ContactGrade.MEDIUM:
                        helpCurrentTimer += Time.deltaTime;

                        if (helpCurrentTimer > timeBeforeHelpDisplay)
                        {
                            troubleshootingCanvasFader.gameObject.SetActive(true);
                        }

                        break;

                    case ContactGrade.GOOD:
                    case ContactGrade.PERFECT:
                        helpCurrentTimer = 0;
                        break;
                }
            }
        }

        public void OnCloseTroubleshootingPanel()
        {
            helpCurrentTimer = 0f;

            troubleshootingVideoPlayer.Stop();
            troubleshootingVideoPlayer.targetTexture.Release();

            troubleshootingCanvasFader.StartFade(false);
        }

        /// <summary>
        /// Reset values on exiting step.
        /// </summary>
        public override void OnExitStep()
        {
            helpCurrentTimer = 0f;

            if (troubleshootingCanvasFader.IsCanvasVisible)
            {
                troubleshootingCanvasFader.StartFade(false, true);
            }
        }

        #endregion
    }
}