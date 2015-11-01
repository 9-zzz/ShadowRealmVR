/************************************************************************************

Copyright   :   Copyright 2014 Oculus VR, LLC. All Rights reserved.

Licensed under the Oculus VR Rift SDK License Version 3.2 (the "License");
you may not use the Oculus VR Rift SDK except in compliance with the License,
which is provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

You may obtain a copy of the License at

http://www.oculusvr.com/licenses/LICENSE-3.2

Unless required by applicable law or agreed to in writing, the Oculus VR SDK
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

************************************************************************************/

#if !UNITY_5 || UNITY_5_0
#error Oculus Utilities require Unity 5.1 or higher.
#endif

using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using VR = UnityEngine.VR;

/// <summary>
/// Configuration data for Oculus virtual reality.
/// </summary>
public class OVRManager : MonoBehaviour
{
	/// <summary>
	/// Gets the singleton instance.
	/// </summary>
	public static OVRManager instance { get; private set; }
		
	/// <summary>
	/// Gets a reference to the active OVRDisplay
	/// </summary>
	public static OVRDisplay display { get; private set; }

	/// <summary>
	/// Gets a reference to the active OVRTracker
	/// </summary>
	public static OVRTracker tracker { get; private set; }

	/// <summary>
	/// Gets a reference to the active OVRInput
	/// </summary>
	public static OVRInput input { get; private set; }

	private static bool _profileIsCached = false;
	private static OVRProfile _profile;
	/// <summary>
	/// Gets the current profile, which contains information about the user's settings and body dimensions.
	/// </summary>
	public static OVRProfile profile
	{
		get {
			if (!_profileIsCached)
			{
				_profile = new OVRProfile();
				_profile.TriggerLoad();
				
				while (_profile.state == OVRProfile.State.LOADING)
					System.Threading.Thread.Sleep(1);
				
				if (_profile.state != OVRProfile.State.READY)
					Debug.LogWarning("Failed to load profile.");
				
				_profileIsCached = true;
			}

			return _profile;
		}
	}

	/// <summary>
	/// Occurs when an HMD attached.
	/// </summary>
	public static event Action HMDAcquired;

	/// <summary>
	/// Occurs when an HMD detached.
	/// </summary>
	public static event Action HMDLost;

	/// <summary>
	/// Occurs when the tracker gained tracking.
	/// </summary>
	public static event Action TrackingAcquired;

	/// <summary>
	/// Occurs when the tracker lost tracking.
	/// </summary>
	public static event Action TrackingLost;
	
	/// <summary>
	/// Occurs when HSW dismissed.
	/// </summary>
	public static event Action HSWDismissed;
	
	private static bool _isHmdPresentCached = false;
	private static bool _isHmdPresent = false;
	/// <summary>
	/// If true, a head-mounted display is connected and present.
	/// </summary>
	public static bool isHmdPresent
	{
		get {
			if (!_isHmdPresentCached)
			{
				_isHmdPresentCached = true;
				_isHmdPresent = OVRPlugin.hmdPresent;
			}

			return _isHmdPresent;
		}

		private set {
			_isHmdPresentCached = true;
			_isHmdPresent = value;
		}
	}

	private static bool _isHSWDisplayedCached = false;
	private static bool _isHSWDisplayed = false;
	private static bool _wasHSWDisplayed;
	/// <summary>
	/// If true, then the Oculus health and safety warning (HSW) is currently visible.
	/// </summary>
	public static bool isHSWDisplayed
	{
		get {
			if (!isHmdPresent)
				return false;

			if (!_isHSWDisplayedCached)
			{
				_isHSWDisplayedCached = true;
				_isHSWDisplayed = OVRPlugin.hswVisible;
			}

			return _isHSWDisplayed;
		}

		private set {
			_isHSWDisplayedCached = true;
			_isHSWDisplayed = value;
		}
	}
	
	/// <summary>
	/// If the HSW has been visible for the necessary amount of time, this will make it disappear.
	/// </summary>
	public static void DismissHSWDisplay()
	{
		if (!isHmdPresent)
			return;

		OVRPlugin.DismissHSW();
	}

	/// <summary>
	/// If true, chromatic de-aberration will be applied, improving the image at the cost of texture bandwidth.
	/// </summary>
	public bool chromatic
	{
		get {
			if (!isHmdPresent)
				return false;

			return OVRPlugin.chromatic;
		}

		set {
			if (!isHmdPresent)
				return;

			OVRPlugin.chromatic = value;
		}
	}
	
	/// <summary>
	/// If true, both eyes will see the same image, rendered from the center eye pose, saving performance.
	/// </summary>
	public bool monoscopic
	{
		get {
			if (!isHmdPresent)
				return true;

			return OVRPlugin.monoscopic;
		}
		
		set {
			if (!isHmdPresent)
				return;

			OVRPlugin.monoscopic = value;
		}
	}

	/// <summary>
	/// If true, distortion rendering work is submitted a quarter-frame early to avoid pipeline stalls and increase CPU-GPU parallelism.
	/// </summary>
	public bool queueAhead = true;
	
	/// <summary>
	/// Gets the current battery level.
	/// </summary>
	/// <returns><c>battery level in the range [0.0,1.0]</c>
	/// <param name="batteryLevel">Battery level.</param>
	public static float batteryLevel
	{
		get {
			if (!isHmdPresent)
				return 1f;

			return OVRPlugin.batteryLevel;
		}
	}
	
	/// <summary>
	/// Gets the current battery temperature.
	/// </summary>
	/// <returns><c>battery temperature in Celsius</c>
	/// <param name="batteryTemperature">Battery temperature.</param>
	public static float batteryTemperature
	{
		get {
			if (!isHmdPresent)
				return 0f;

			return OVRPlugin.batteryTemperature;
		}
	}
	
	/// <summary>
	/// Gets the current battery status.
	/// </summary>
	/// <returns><c>battery status</c>
	/// <param name="batteryStatus">Battery status.</param>
	public static int batteryStatus
	{
		get {
			if (!isHmdPresent)
				return -1;

			return (int)OVRPlugin.batteryStatus;
		}
	}

	/// <summary>
	/// Gets the current volume level.
	/// </summary>
	/// <returns><c>volume level in the range [0,1].</c>
	public static float volumeLevel
	{
		get {
			if (!isHmdPresent)
				return 0f;

			return OVRPlugin.systemVolume;
		}
	}

	/// <summary>
	/// If true, head tracking will affect the orientation of each OVRCameraRig's cameras.
	/// </summary>
	public bool usePositionTracking = true;

	/// <summary>
	/// If true, each scene load will cause the head pose to reset.
	/// </summary>
	public bool resetTrackerOnLoad = true;

	/// <summary>
	/// True if the current platform supports virtual reality.
	/// </summary>
    public bool isSupportedPlatform { get; private set; }

	private static bool wasHmdPresent = false;
	private static bool wasPositionTracked = false;

	[NonSerialized]
	private static OVRVolumeControl volumeController = null;
	[NonSerialized]
	private Transform volumeControllerTransform = null;

#region Unity Messages

	private void Awake()
	{
		// Only allow one instance at runtime.
		if (instance != null)
		{
			enabled = false;
			DestroyImmediate(this);
			return;
		}

		instance = this;

		System.Version netVersion = OVRPlugin.wrapperVersion;
		System.Version ovrVersion = OVRPlugin.version;

		Debug.Log("Unity v" + Application.unityVersion + ", " +
		          "Oculus Utilities v" + netVersion + ", " +
		          "OVRPlugin v" + ovrVersion + ".");

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
		if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
			Debug.LogWarning("VR rendering requires Direct3D11. Your graphics device: " + SystemInfo.graphicsDeviceType);
#endif

        // Detect whether this platform is a supported platform
        RuntimePlatform currPlatform = Application.platform;
        isSupportedPlatform |= currPlatform == RuntimePlatform.Android;
        //isSupportedPlatform |= currPlatform == RuntimePlatform.LinuxPlayer;
        isSupportedPlatform |= currPlatform == RuntimePlatform.OSXEditor;
        isSupportedPlatform |= currPlatform == RuntimePlatform.OSXPlayer;
        isSupportedPlatform |= currPlatform == RuntimePlatform.WindowsEditor;
        isSupportedPlatform |= currPlatform == RuntimePlatform.WindowsPlayer;
        if (!isSupportedPlatform)
        {
            Debug.LogWarning("This platform is unsupported");
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
		// We want to set up our touchpad messaging system
		OVRTouchpad.Create();

        // Turn off chromatic aberration by default to save texture bandwidth.
        chromatic = false;
#endif

        InitVolumeController();

		if (display == null)
			display = new OVRDisplay();
		if (tracker == null)
			tracker = new OVRTracker();
		if (input == null)
			input = new OVRInput();

		if (resetTrackerOnLoad)
			display.RecenterPose();
	}

	private void OnEnable()
	{
		if (volumeController != null)
		{
			volumeController.UpdatePosition(volumeControllerTransform);
		}
    }

	private void Update()
	{
		tracker.isEnabled = usePositionTracking;

		// Dispatch any events.
		isHmdPresent = OVRPlugin.hmdPresent;

		if (isHmdPresent)
		{
			OVRPlugin.queueAheadFraction = (queueAhead) ? 0.25f : 0f;
		}

		if (HMDLost != null && wasHmdPresent && !isHmdPresent)
			HMDLost();

        if (HMDAcquired != null && !wasHmdPresent && isHmdPresent)
			HMDAcquired();

        wasHmdPresent = isHmdPresent;

		if (TrackingLost != null && wasPositionTracked && !tracker.isPositionTracked)
			TrackingLost();

		if (TrackingAcquired != null && !wasPositionTracked && tracker.isPositionTracked)
			TrackingAcquired();

		wasPositionTracked = tracker.isPositionTracked;

		isHSWDisplayed = OVRPlugin.hswVisible;

		if (isHSWDisplayed && Input.anyKeyDown)
			DismissHSWDisplay();
		
		if (!isHSWDisplayed && _wasHSWDisplayed)
		{
			if (HSWDismissed != null)
				HSWDismissed();
		}
		
		_wasHSWDisplayed = isHSWDisplayed;

		display.Update();
		input.Update();
		
		if (volumeController != null)
		{
			if (volumeControllerTransform == null)
			{
				if (gameObject.GetComponent<OVRCameraRig>() != null)
				{
					volumeControllerTransform = gameObject.GetComponent<OVRCameraRig>().centerEyeAnchor;
				}
			}
			volumeController.UpdatePosition(volumeControllerTransform);
		}
    }

	/// <summary>
	/// Creates a popup dialog that shows when volume changes.
	/// </summary>
	private static void InitVolumeController()
	{
		if (volumeController == null)
		{
			Debug.Log("Creating volume controller...");
			// Create the volume control popup
			GameObject go = GameObject.Instantiate(Resources.Load("OVRVolumeController")) as GameObject;
			if (go != null)
			{
				volumeController = go.GetComponent<OVRVolumeControl>();
			}
			else
			{
				Debug.LogError("Unable to instantiate volume controller");
			}
		}
	}

	/// <summary>
	/// Leaves the application/game and returns to the launcher/dashboard
	/// </summary>
	public void ReturnToLauncher()
	{
		// show the platform UI quit prompt
		OVRManager.PlatformUIConfirmQuit();
	}

#endregion

    public static void PlatformUIConfirmQuit()
	{
		if (!isHmdPresent)
			return;

		OVRPlugin.ShowUI(OVRPlugin.PlatformUI.ConfirmQuit);
    }

    public static void PlatformUIGlobalMenu()
	{
		if (!isHmdPresent)
			return;

		OVRPlugin.ShowUI(OVRPlugin.PlatformUI.GlobalMenu);
    }
}
