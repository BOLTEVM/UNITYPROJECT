using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using NextMind.Core.Event;
using System.Collections.Generic;

namespace NextMind.Examples.Utility
{
    /// <summary>
    /// The EventNotifier is a component listening to all events (about devices connection, bluetooth, etc...).
    /// It displays message on a Text element, colored regarding the event severity (Status, Warning, Error).
    /// </summary>
    public class EventNotifier : MonoBehaviour
    {
        /// <summary>
        /// The canvas group on which text is displayed. Used to show/hide elements using its alpha value.
        /// </summary>
        [SerializeField]
        private CanvasGroup canvasGroup = null;

        /// <summary>
        /// The text element.
        /// </summary>
        [SerializeField]
        private Text messageText = null;

        /// <summary>
        /// The button used to hide the displayed message.
        /// </summary>
        [SerializeField]
        private Image closeButtonImage = null;

        /// <summary>
        /// The background which will change color following events Severity as well.
        /// </summary>
        [SerializeField]
        private Image background = null;

        /// <summary>
        /// The coroutine used to fade in/out the canvasGroup.
        /// </summary>
        private Coroutine showCoroutine;

        /// <summary>
        /// The events waiting to be displayed;
        /// </summary>
        private Queue<EventBase> eventsQueue;

        /// <summary>
        /// Are some events currently waiting to be displayed ?
        /// </summary>
        private bool IsQueueEmpty => eventsQueue.Count == 0;

        /// <summary>
        /// The coroutine used to manage the eventsQueue unqueueing.
        /// </summary>
        private Coroutine unqueueCoroutine;

        /// <summary>
        /// The last event added to the queue.
        /// </summary>
        private EventBase lastEnqueuedEvent;
        /// <summary>
        /// The event which message is currently displayed on the UI.
        /// </summary>
        private EventBase currentlyDisplayedEvent;

        /// <summary>
        /// The time allowed to display a message.
        /// </summary>
        private float displayTime;
        private const float maximumDisplayTime = 15f;
        private const float minimumDisplayTime = 3f;

        #region Unity methods

        private void Start()
        {
            eventsQueue = new Queue<EventBase>();

            var neuroManager = NeuroManager.Instance;
            if (neuroManager != null)
            {
                // Start listening to wanted events.
                neuroManager.onGlobalStatusEvent.AddListener(OnGlobalStatusEvent);
                neuroManager.onDeviceEvent.AddListener(OnDeviceEvent);
            }
        }

        private void OnDestroy()
        {
            var neuroManager = NeuroManager.Instance;
            if (neuroManager != null)
            {
                // Stop listening to wanted events.
                neuroManager.onGlobalStatusEvent.RemoveListener(OnGlobalStatusEvent);
                neuroManager.onDeviceEvent.RemoveListener(OnDeviceEvent);
            }
        }

        #endregion

        #region Events handling 

        /// <summary>
        /// Method triggered when receiving a GlobalStatusEvent.
        /// </summary>
        /// <param name="globalEvent">The event received</param>
        private void OnGlobalStatusEvent(EventBase globalEvent)
        {
            // Add to the queue of the events waiting to be displayed.
            EnqueueEvent(globalEvent);
        }

        /// <summary>
        /// Method triggered when receiving a DeviceEvent.
        /// </summary>
        /// <param name="deviceID">The device's id</param>
        /// <param name="deviceEvent">The event received</param>
        private void OnDeviceEvent(int deviceID, EventBase deviceEvent)
        {
            // Do not display contact events on Notifier.
            if (deviceEvent is ContactEvent)
            {
                return;
            }

            // Add to the queue of the events waiting to be displayed.
            EnqueueEvent(deviceEvent);
        }

        private void EnqueueEvent(EventBase evt)
        {
            // Don't add 2 identical events in a row. An identical event can be displayed only if the same message has been fully shown.
            if (!IsQueueEmpty && evt == lastEnqueuedEvent
                || IsQueueEmpty && evt == currentlyDisplayedEvent)
            {
                return;
            }

            // Display messages during a shorter amount of time if the queue is not empty.
            displayTime = currentlyDisplayedEvent != null ? minimumDisplayTime : maximumDisplayTime;

            eventsQueue.Enqueue(evt);
            lastEnqueuedEvent = evt;

            // Start the unqueue coroutine if not already started.
            if (unqueueCoroutine == null)
            {
                unqueueCoroutine = StartCoroutine(UnqueueMessages());
            }
        }

        #endregion

        private IEnumerator UnqueueMessages()
        {
            while (!IsQueueEmpty)
            {
                EventBase evt = eventsQueue.Dequeue();

                currentlyDisplayedEvent = evt;

                SetMessageUI(evt.ToString(), evt.Severity);

                // If this event is the last to display, set the display time to the maximum.
                if (IsQueueEmpty)
                {
                    displayTime = maximumDisplayTime;
                }

                // Stop the coroutine if it is running.
                if (showCoroutine != null)
                {
                    StopCoroutine(showCoroutine);
                }
                // Show the panel.
                showCoroutine = StartCoroutine(Show(true));
                yield return new WaitUntil(() => showCoroutine == null);
            }

            currentlyDisplayedEvent = null;

            unqueueCoroutine = null;
        }

        private void SetMessageUI(string text, EventBase.EventSeverity severity)
        {
            messageText.text = text;
            switch (severity)
            {
                case EventBase.EventSeverity.Status:
                    // Blue
                    messageText.color = new Color32(0x00, 0x11, 0x7A, 0xFF);
                    background.color = new Color32(0xA8, 0xD0, 0xFF, 0xB2);
                    break;
                case EventBase.EventSeverity.Warning:
                    // Yellow
                    messageText.color = new Color32(0x72, 0x3F, 0x00, 0xFF);
                    background.color = new Color32(0xFF, 0xDF, 0x99, 0xB2);
                    break;
                case EventBase.EventSeverity.Error:
                    // Red
                    messageText.color = new Color32(0x7A, 0x00, 0x00, 0xFF);
                    background.color = new Color32(0xFF, 0x8F, 0x8F, 0xB2);
                    break;
            }

            // Apply the same color on the text and the button.
            closeButtonImage.color = messageText.color;
        }

        /// <summary>
        /// Method triggered when user click on close button.
        /// </summary>
        public void OnClickOnClose()
        {
            // Stop the coroutine if it is running.
            if (showCoroutine != null)
            {
                StopCoroutine(showCoroutine);
            }

            // Fade the panel out.
            showCoroutine = StartCoroutine(Show(false));
        }

        /// <summary>
        /// The coroutine fading in and out the message canvasGroup.
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        private IEnumerator Show(bool show)
        {
            float t = 0, duration = 1f, timer = 0f;
            float startValue = canvasGroup.alpha;
            float targetValue = show ? 1 : 0;

            while (t < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, t);

                timer += Time.deltaTime;
                t = timer / duration;
                yield return null;
            }

            canvasGroup.alpha = targetValue;

            if (show)
            {
                timer = 0;
                while (timer < displayTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }

                yield return StartCoroutine(Show(false));
            }

            showCoroutine = null;
        }
    }
}