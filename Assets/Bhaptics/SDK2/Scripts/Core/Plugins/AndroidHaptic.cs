using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bhaptics.SDK2
{
    public class AndroidHaptic
    {
        protected static AndroidJavaObject androidJavaObject;

        private static readonly object[] PlayParams = new object[1];
        private static readonly object[] PlayParamParams = new object[6];
        private static readonly object[] EmptyParams = new object[0];
        private static readonly object[] IsPlayingParams = new object[1];
        private static readonly object[] StopByRequestIdParams = new object[1];
        private static readonly object[] StopByEventIdParams = new object[1];
        private static readonly object[] PingParams = new object[1];
        private List<HapticDevice> deviceList;

        protected IntPtr AndroidJavaObjectPtr;

        protected IntPtr InitializePtr;
        protected IntPtr PlayPtr;
        protected IntPtr PlayPosPtr;
        protected IntPtr PlayParamPtr;
        protected IntPtr PlayPosParamPtr;
        protected IntPtr StopIntPtr;
        protected IntPtr StopByEventIdPtr;
        protected IntPtr StopAllPtr;
        protected IntPtr SubmitRegisteredPtr;
        protected IntPtr SubmitRegisteredWithTimePtr;
        protected IntPtr RegisterPtr;
        protected IntPtr RegisterReflectedPtr;
        protected IntPtr PingPtr;
        protected IntPtr PingAllPtr;

        // bool methods
        protected IntPtr IsRegisteredPtr;
        protected IntPtr IsPlayingPtr;
        protected IntPtr IsPlayingAnythingPtr;
        protected IntPtr IsPlayingByEventIdPtr;
        protected IntPtr IsPlayingByRequestIdPtr;

        // Streaming methods
        protected IntPtr ToggleStreamPtr;
        protected IntPtr IsStreamingEnablePtr;
        protected IntPtr GetStreamingHostsPtr;

        // show bluetooth
        protected IntPtr ShowBluetoothPtr;
        protected IntPtr RefreshPairingInfoPtr;

        public AndroidHaptic()
        {
            try
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                androidJavaObject =
                    new AndroidJavaObject("com.bhaptics.bhapticsunity.BhapticsManagerWrapper", currentActivity);

                AndroidJavaObjectPtr = androidJavaObject.GetRawObject();

                InitializePtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "initialize");

                PlayPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "play");
                PlayPosPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "playPos");
                PlayParamPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "playParam");
                PlayPosParamPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "playPosParam");
                StopIntPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "stopInt");
                StopByEventIdPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "stopByEventId");
                StopAllPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "stopAll");

                ToggleStreamPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "toggleStreamingEnable");

                SubmitRegisteredPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "submitRegistered");
                SubmitRegisteredWithTimePtr =
                    AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "submitRegisteredWithTime");
                RegisterPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "register");
                RegisterReflectedPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "registerReflected");
                PingPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "ping");
                PingAllPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "pingAll");

                IsRegisteredPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isRegistered");
                IsPlayingPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isPlaying");
                IsPlayingAnythingPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isAnythingPlaying");
                IsPlayingByEventIdPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isPlayingByEventId");
                IsPlayingByRequestIdPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isPlayingByRequestId");

                IsStreamingEnablePtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "isStreamingEnable");
                GetStreamingHostsPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "getStreamingHosts");
                ShowBluetoothPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "showBluetoothSetting");
                RefreshPairingInfoPtr = AndroidJNIHelper.GetMethodID(androidJavaObject.GetRawClass(), "refreshPairing");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("AndroidHaptic {0} {1} ", e.Message, e);
            }

            deviceList = GetDevices();
        }

        public bool CheckBhapticsAvailable()
        {
            if (androidJavaObject == null)
            {
                return false;
            }
            bool result = androidJavaObject.Call<bool>("isBhapticsUser");

            return result;
        }

        public void RefreshPairing()
        {
            if (androidJavaObject == null)
            {
                return;
            }

            CallNativeVoidMethod(RefreshPairingInfoPtr, EmptyParams);

        }

        public List<HapticDevice> GetDevices()
        {
            try
            {
                string[] result = androidJavaObject.Call<string[]>("getDeviceList");
                deviceList = BhapticsHelpers.ConvertToBhapticsDevices(result);

                return deviceList;
            }
            catch (Exception e)
            {
                // Debug.LogErrorFormat("[bHaptics] GetDevices() {0}", e.Message);
            }

            return new List<HapticDevice>();
        }

        public void Initialize(string workspaceId, string sdkKey, string json)
        {
            Debug.LogFormat("[bHaptics] Initialize() {0} {1}", workspaceId, json);
            CallNativeVoidMethod(InitializePtr, new object[] { workspaceId, sdkKey, json });
        }

        public bool IsConnect()
        {

            return false;
        }

        public bool IsPlaying()
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            return CallNativeBoolMethod(IsPlayingAnythingPtr, EmptyParams);
        }

        public bool IsPlayingByEventId(string eventId)
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            IsPlayingParams[0] = eventId;
            return CallNativeBoolMethod(IsPlayingByEventIdPtr, IsPlayingParams);
        }

        public bool IsPlayingByRequestId(int requestId)
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            IsPlayingParams[0] = requestId;
            return CallNativeBoolMethod(IsPlayingByRequestIdPtr, IsPlayingParams);
        }

        public void RefreshPairingInfo()
        {
            if(androidJavaObject == null)
            {
                return;
            }

            CallNativeVoidMethod(RefreshPairingInfoPtr, EmptyParams);
        }



        public int Play(string eventId)
        {
            if (androidJavaObject == null)
            {
                return -1;
            }

            PlayParams[0] = eventId;
            return CallNativeIntMethod(PlayPtr, PlayParams);
        }

        public int PlayParam(string eventId, float intensity, float duration, float angleX, float offsetY)
        {
            if (androidJavaObject == null)
            {
                return -1;
            }
            int pos = (int)0;

            PlayParamParams[0] = eventId;
            PlayParamParams[1] = pos;
            PlayParamParams[2] = intensity;
            PlayParamParams[3] = duration;
            PlayParamParams[4] = angleX;
            PlayParamParams[5] = offsetY;

            return CallNativeIntMethod(PlayPosParamPtr, PlayParamParams);
        }

        public int PlayMotors(int position, int[] motors, int durationMillis)
        {
            if (androidJavaObject == null)
            {
                return -1;
            }
            //
            // PlayMotorsParams[0] = position;
            // PlayMotorsParams[1] = durationMillis;
            // PlayMotorsParams[2] = motors;

            return androidJavaObject.Call<int>("playMotors", position, durationMillis, motors);
        }

        public int PlayGlove(int position, int[] motors, int[] playTimeValues, int[] shapeValues)
        {
            if (androidJavaObject == null)
            {
                return -1;
            }
            return androidJavaObject.Call<int>("playGlove", position, motors, playTimeValues, shapeValues, 6);
        }

        public bool StopByRequestId(int key)
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            StopByRequestIdParams[0] = key;
            return CallNativeBoolMethod(StopIntPtr, StopByRequestIdParams);
        }

        public bool StopByEventId(string eventId)
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            try
            {
                StopByEventIdParams[0] = eventId;
                return CallNativeBoolMethod(StopByEventIdPtr, StopByEventIdParams);
            }
            catch (Exception e)
            {
                // Debug.LogErrorFormat("[bHaptics] StopByEventId() : {0}", e.Message);
            }

            return false;
        }

        public bool Stop()
        {
            if (androidJavaObject != null)
            {
                try
                {
                    return CallNativeBoolMethod(StopAllPtr, EmptyParams);
                }
                catch (Exception e)
                {
                    // Debug.LogErrorFormat("[bHaptics] Stop() : {0}", e.Message);
                }
            }

            return false;
        }

        public void Dispose()
        {
            if (androidJavaObject != null)
            {
                androidJavaObject.Call("quit");
                androidJavaObject = null;
            }
        }

        public void TogglePosition(string address)
        {
            if (androidJavaObject == null)
            {
                return;
            }


            if (androidJavaObject != null)
            {
                androidJavaObject.Call("togglePosition", address);
            }
        }

        public void PingAll()
        {
            if (androidJavaObject == null)
            {
                return;
            }

            CallNativeVoidMethod(PingAllPtr, EmptyParams);
        }

        public void Ping(string address)
        {
            if (androidJavaObject == null)
            {
                return;
            }

            PingParams[0] = address;
            CallNativeVoidMethod(PingPtr, PingParams);
        }


        private void CallNativeVoidMethod(IntPtr methodPtr, object[] param)
        {
            if (androidJavaObject == null)
            {
                return;
            }

            AndroidUtils.CallNativeVoidMethod(AndroidJavaObjectPtr, methodPtr, param);
        }


        private bool CallNativeBoolMethod(IntPtr methodPtr, object[] param)
        {
            if (androidJavaObject == null)
            {
                return false;
            }

            return AndroidUtils.CallNativeBoolMethod(AndroidJavaObjectPtr, methodPtr, param);
        }

        private int CallNativeIntMethod(IntPtr methodPtr, object[] param)
        {
            if (androidJavaObject == null)
            {
                return -1;
            }

            return AndroidUtils.CallNativeIntMethod(AndroidJavaObjectPtr, methodPtr, param);
        }
    }

    class AndroidUtils
    {
        public static void CallNativeVoidMethod(IntPtr androidObjPtr, IntPtr methodPtr, object[] param)
        {
            jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(param);
            try
            {
                AndroidJNI.CallVoidMethod(androidObjPtr, methodPtr, args);
            }
            catch (Exception e)
            {
                // Debug.LogErrorFormat("[bHaptics] CallNativeVoidMethod() : {0}", e.Message);
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(param, args);
            }
        }


        public static bool CallNativeBoolMethod(IntPtr androidObjPtr, IntPtr methodPtr, object[] param)
        {
            jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(param);
            bool res = false;
            try
            {
                res = AndroidJNI.CallBooleanMethod(androidObjPtr, methodPtr, args);
            }
            catch (Exception e)
            {
                // Debug.LogErrorFormat("[bHaptics] CallNativeBoolMethod() : {0}", e.Message);
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(param, args);
            }

            return res;
        }

        public static int CallNativeIntMethod(IntPtr androidObjPtr, IntPtr methodPtr, object[] param)
        {
            jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(param);
            int res = -1;
            try
            {
                res = AndroidJNI.CallIntMethod(androidObjPtr, methodPtr, args);
            }
            catch (Exception e)
            {
                // Debug.LogErrorFormat("[bHaptics] CallNativeIntMethod() : {0}", e.Message);
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(param, args);
            }

            return res;
        }
    }
}