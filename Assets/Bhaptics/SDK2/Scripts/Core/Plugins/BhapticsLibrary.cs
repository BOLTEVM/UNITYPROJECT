using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bhaptics.SDK2
{
    public class BhapticsLibrary
    {
        private static readonly object Lock = new object();
        private static readonly List<HapticDevice> EmptyDevices = new List<HapticDevice>();

        private static AndroidHaptic android = null;
        private static bool _initialized = false;
        private static bool isAvailable = false;
        private static bool isAvailableChecked = false;





        public static bool IsBhapticsAvailable(bool isAutoRunPlayer)
        {
            if (isAvailableChecked)
            {
                return isAvailable;
            }

            return IsBhapticsAvailableForce(isAutoRunPlayer);
        }

        public static bool IsBhapticsAvailableForce(bool isAutoRunPlayer)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (android == null)
                {
                    Debug.LogErrorFormat("[bHaptics] IsBhapticsAvailable() android object not initialized.");
                    isAvailable = false;
                    return isAvailable;
                }

                android.RefreshPairing();
                isAvailable = android.CheckBhapticsAvailable();
                isAvailableChecked = true;
                return isAvailable;
            }


            if (!bhaptics_library.isPlayerInstalled())
            {
                isAvailable = false;
                isAvailableChecked = true;
                return isAvailable;
            }

            if (!bhaptics_library.isPlayerRunning() && isAutoRunPlayer)
            {
                Debug.LogFormat("[bHaptics] bHaptics Player(PC) is not running, so try launch it.");
                bhaptics_library.launchPlayer(true);
            }

            isAvailable = true;
            isAvailableChecked = true;
            return isAvailable;
        }



        public static bool Initialize(string appId, string apiKey, string json)
        {
            lock (Lock)
            {
                if (_initialized)
                {
                    return false;
                }
                _initialized = true;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android == null)
                {
                    Debug.Log("[bHaptics] BhapticsLibrary - Initialize ");
                    android = new AndroidHaptic();
                    android.Initialize(appId, apiKey, json);
                    _initialized = true;
                    return true;
                }

                return false;
            }

            if (bhaptics_library.wsIsConnected())
            {
                Debug.Log("[bHaptics] BhapticsLibrary - connection already opened");
                //return false;       // NOTE-230117      Temporary comment out for IL2CPP
            }

            Debug.LogFormat("[bHaptics] BhapticsLibrary - Initialize() {0} {1}", apiKey, appId);
            return bhaptics_library.registryAndInit(apiKey, appId, json);
        }

        public static void Destroy()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    android.Dispose();
                    android = null;
                }

                return;
            }

            Debug.LogFormat("[bHaptics] Destroy()");
            bhaptics_library.wsClose();

            _initialized = false;
        }

        public static bool IsConnect(PositionType type)
        {
            if (!isAvailable)
            {
                return false;
            }

            return GetConnectedDevices(type).Count > 0;
        }

        public static int Play(string eventId)
        {
            if (!isAvailable)
            {
                return -1;
            }

            if (eventId == null || eventId.Equals(string.Empty))
            {
                return -1;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.Play(eventId);
                }

                return -1;
            }

            return bhaptics_library.play(eventId);
        }

        public static int PlayParam(string eventId, float intensity, float duration, float angleX, float offsetY)
        {
            if (!isAvailable)
            {
                return -1;
            }

            if (eventId == null || eventId.Equals(string.Empty))
            {
                return -1;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.PlayParam(eventId, intensity, duration, angleX, offsetY);
                }

                return -1;
            }

            return bhaptics_library.playPosParam(eventId, 0, intensity, duration, angleX, offsetY);
        }

        public static int PlayMotors(int position, int[] motors, int durationMillis)
        {
            if (!isAvailable)
            {
                return -1;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.PlayMotors(position, motors, durationMillis);
                }

                return -1;
            }

            return bhaptics_library.playDot(position, durationMillis, motors, motors.Length);
        }

        public static int PlayWaveform(PositionType positionType, int[] motorValues, GlovePlayTime[] playTimeValues, GloveShapeValue[] shapeValues)
        {
            if (!isAvailable)
            {
                return -1;
            }

            if (motorValues.Length != 6 || playTimeValues.Length != 6 || shapeValues.Length != 6)
            {
                Debug.LogError("[bHaptics] BhapticsLibrary - PlayWaveform() 'motorValues, playTimeValues, shapeValues' necessarily require 6 values each.");
                return -1;
            }


            var playTimes = new int[playTimeValues.Length];
            var shapeVals = new int[shapeValues.Length];

            for (int i = 0; i < playTimes.Length; i++)
            {
                playTimes[i] = (int)playTimeValues[i];
            }
            for (int i = 0; i < shapeVals.Length; i++)
            {
                shapeVals[i] = (int)shapeValues[i];
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.PlayGlove((int)positionType, motorValues, playTimes, shapeVals);
                }
                return -1;
            }
            return bhaptics_library.playGlove((int)positionType, motorValues, playTimes, shapeVals, 6);
        }

        public static bool StopByEventId(string eventId)
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.StopByEventId(eventId);
                }

                return false;
            }

            return bhaptics_library.stopByEventId(eventId);
        }

        public static bool StopInt(int requestId)
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.StopByRequestId(requestId);
                }

                return false;
            }

            return bhaptics_library.stop(requestId);
        }

        public static bool StopAll()
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.Stop();
                }

                return false;
            }

            return bhaptics_library.stopAll();
        }

        public static bool IsPlaying()
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.IsPlaying();
                }

                return false;
            }

            return bhaptics_library.isPlaying();
        }
        public static bool IsPlayingByEventId(string eventId)
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.IsPlayingByEventId(eventId);
                }

                return false;
            }

            return bhaptics_library.isPlayingByEventId(eventId);
        }

        public static bool IsPlayingByRequestId(int requestId)
        {
            if (!isAvailable)
            {
                return false;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.IsPlayingByRequestId(requestId);
                }

                return false;
            }

            return bhaptics_library.isPlayingByRequestId(requestId);
        }

        public static List<HapticDevice> GetDevices()
        {
            if (!isAvailable)
            {
                return EmptyDevices;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    return android.GetDevices();
                }

                return EmptyDevices;
            }

            return bhaptics_library.GetDevices();
        }

        public static List<HapticDevice> GetConnectedDevices(PositionType pos)
        {
            if (!isAvailable)
            {
                return EmptyDevices;
            }

            var pairedDeviceList = new List<HapticDevice>();
            var devices = GetDevices();
            foreach (var device in devices)
            {
                if (device.IsPaired && device.Position == pos && device.IsConnected)
                {
                    pairedDeviceList.Add(device);
                }
            }

            return pairedDeviceList;
        }

        public static List<HapticDevice> GetPairedDevices(PositionType pos)
        {
            if (!isAvailable)
            {
                return EmptyDevices;
            }

            var res = new List<HapticDevice>();
            var devices = GetDevices();
            foreach (var device in devices)
            {
                if (device.IsPaired && device.Position == pos)
                {
                    res.Add(device);
                }
            }

            return res;
        }

        public static void Ping(PositionType pos)
        {
            if (!isAvailable)
            {
                return;
            }

            var currentDevices = GetConnectedDevices(pos);

            foreach (var device in currentDevices)
            {
                Ping(device);
            }
        }

        public static void Ping(HapticDevice targetDevice)
        {
            if (!isAvailable)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    android.Ping(targetDevice.Address);
                }

                return;
            }

            bhaptics_library.ping(targetDevice.Address);

        }

        public static void PingAll()
        {
            if (!isAvailable)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    android.PingAll();
                }

                return;
            }

            bhaptics_library.pingAll();
        }

        public static void TogglePosition(HapticDevice targetDevice)
        {
            if (!isAvailable)
            {
                return;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                if (android != null)
                {
                    android.TogglePosition(targetDevice.Address);
                }

                return;
            }

            bhaptics_library.swapPosition(targetDevice.Address);
        }

        public static void OnApplicationFocus()
        {
            IsBhapticsAvailableForce(false);
        }

        public static void OnApplicationPause()
        {
            StopAll();
        }

        public static void OnApplicationQuit()
        {
            Destroy();
        }

#if UNITY_EDITOR
        public static List<MappingMetaData> EditorGetEventList(string appId, string apiKey, int lastVersion, out int status)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                status = 0;
                return new List<MappingMetaData>();
            }

            var res = bhaptics_library.EditorGetEventList(appId, apiKey, lastVersion, out int code);
            status = code;
            return res;
        }

        public static string EditorGetSettings(string appId, string apiKey, int lastVersion, out int status)
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                status = 0;
                return "";
            }

            var bytes = bhaptics_library.EditorGetSettings(appId, apiKey, lastVersion, out int code);
            Debug.LogFormat("EditorGetSettings {0} {1}", code, bytes);
            status = code;
            return bytes;
        }

        public static bool EditorReInitialize(string appId, string apiKey, string json)
        {
            lock (Lock)
            {
                _initialized = true;
            }

            if (Application.platform == RuntimePlatform.Android)
            {
                return false;
            }

            Debug.LogFormat("[bHaptics] BhapticsLibrary - ReInitialize() {0} {1}", apiKey, appId);
            return bhaptics_library.reInitMessage(apiKey, appId, json);
        }
#endif
    }
}
