# How to use the example scenes

To run the examples, please ensure first that the scenes ("Hub", "Calibration", "SDKDiscovery", "NeuroTagGallery") are properly referenced in the project's Build Settings.
Then run the "Hub" scene. "Calibration", "SDKDiscovery" and "NeuroTagGallery" scenes will be loaded automatically from the Hub scene by clicking on the dedicated buttons.

# VR scenes usage

To run the VR scenes, your project has to be configured to do so. Thus, you have to import the following packages from the Package Manager:

- "XR Plugin Management"
- "XR Interaction Toolkit"
- The specific package for the device you intend to use. (e.g. "Oculus XR Plugin" if you intend to use an Oculus headset).

Please visit Unity's official XR documentation (https://docs.unity3d.com/Manual/configuring-project-for-xr.html) for further details.

# Scriptable Render Pipeline (SRP) example scenes usage

If you intend to use the SRP example scenes and assets, please be sure to configure your project to use "Linear" color space (Project Settings > Player > Other Settings > Color Space). 
The example scenes are not fully HDRP-ready though. But following these few steps, you can make them compatible:
- On the Hub_SRP scene's "Main Camera" :
	- Change the Background Type/Color to "Color"/Black.
	- Change the "Volume Layer Mask" property to "nothing".
- Select NextMindSDK/Examples/Common Resources/Materials/SRP/PlaneMaterial and change its shader to HDRP/Lit
- Select NextMindSDK/Examples/SDKDiscovery/Materials/SRP/CubesMaterial and change its shader to HDRP/Lit

