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

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Miscellaneous extension methods that any script can use.
/// </summary>
public static class OVRExtensions
{
	/// <summary>
	/// Converts the given world-space transform to an OVRPose in tracking space.
	/// </summary>
	public static OVRPose ToTrackingSpacePose(this Transform transform)
	{
		OVRPose headPose;
		headPose.position = UnityEngine.VR.InputTracking.GetLocalPosition(UnityEngine.VR.VRNode.Head);
		headPose.orientation = UnityEngine.VR.InputTracking.GetLocalRotation(UnityEngine.VR.VRNode.Head);

		var ret = headPose * transform.ToHeadSpacePose();

		return ret;
	}

	/// <summary>
	/// Converts the given world-space transform to an OVRPose in head space.
	/// </summary>
	public static OVRPose ToHeadSpacePose(this Transform transform)
	{
		return Camera.current.transform.ToOVRPose().Inverse() * transform.ToOVRPose();
	}

	internal static OVRPose ToOVRPose(this Transform t, bool isLocal = false)
	{
		OVRPose pose;
		pose.orientation = (isLocal) ? t.localRotation : t.rotation;
		pose.position = (isLocal) ? t.localPosition : t.position;
		return pose;
	}
	
	internal static void FromOVRPose(this Transform t, OVRPose pose, bool isLocal = false)
	{
		if (isLocal)
		{
			t.localRotation = pose.orientation;
			t.localPosition = pose.position;
		}
		else
		{
			t.rotation = pose.orientation;
			t.position = pose.position;
		}
	}

	internal static OVRPose ToOVRPose(this OVRPlugin.Posef p)
	{
		return new OVRPose()
		{
			position = new Vector3(p.Position.x, p.Position.y, -p.Position.z),
			orientation = new Quaternion(-p.Orientation.x, -p.Orientation.y, p.Orientation.z, p.Orientation.w)
		};
	}
	
	internal static OVRTracker.Frustum ToFrustum(this OVRPlugin.Frustumf f)
	{
		return new OVRTracker.Frustum()
		{
			nearZ = f.zNear,
			farZ = f.zFar,
			
			fov = new Vector2()
			{
				x = Mathf.Rad2Deg * f.fovX,
				y = Mathf.Rad2Deg * f.fovY
			}
		};
	}

	internal static OVRPlugin.Vector3f ToVector3f(this Vector3 v)
	{
		return new OVRPlugin.Vector3f() { x = v.x, y = v.y, z = v.z };
	}

	internal static OVRPlugin.Quatf ToQuatf(this Quaternion q)
	{
		return new OVRPlugin.Quatf() { x = q.x, y = q.y, z = q.z, w = q.w };
	}
}

/// <summary>
/// An affine transformation built from a Unity position and orientation.
/// </summary>
public struct OVRPose
{
	/// <summary>
	/// A pose with no translation or rotation.
	/// </summary>
	public static OVRPose identity
	{
		get {
			return new OVRPose()
			{
				position = Vector3.zero,
				orientation = Quaternion.identity
			};
		}
	}

	/// <summary>
	/// The position.
	/// </summary>
	public Vector3 position;

	/// <summary>
	/// The orientation.
	/// </summary>
	public Quaternion orientation;

	/// <summary>
	/// Multiplies two poses.
	/// </summary>
	public static OVRPose operator*(OVRPose lhs, OVRPose rhs)
	{
		var ret = new OVRPose();
		ret.position = lhs.position + lhs.orientation * rhs.position;
		ret.orientation = lhs.orientation * rhs.orientation;
		return ret;
	}

	/// <summary>
	/// Computes the inverse of the given pose.
	/// </summary>
	public OVRPose Inverse()
	{
		OVRPose ret;
		ret.orientation = Quaternion.Inverse(orientation);
		ret.position = ret.orientation * -position;
		return ret;
	}

	/// <summary>
	/// Converts the pose from left- to right-handed or vice-versa.
	/// </summary>
	internal OVRPose flipZ()
	{
		var ret = this;
		ret.position.z = -ret.position.z;
		return ret;
	}

	internal OVRPlugin.Posef ToPosef()
	{
		return new OVRPlugin.Posef()
		{
			Position = position.ToVector3f(),
			Orientation = orientation.ToQuatf()
		};
	}
}
