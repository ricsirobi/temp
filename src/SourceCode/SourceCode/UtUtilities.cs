using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Rendering;

public class UtUtilities
{
	public const float FEET_TO_METERS = 0.3048f;

	public const float DISTANCE_FOR_STOP_UPDATE = 100f;

	public const float IOS_VERSION_THIRTEEN = 13f;

	public static bool _ConnectedToInternet = true;

	private static int mRCL = -1;

	private static string mSysLanguage = "";

	private static List<Type> mAllClassList = null;

	private const int TAB_SIZE = 20;

	[DllImport("wininet.dll")]
	private static extern bool InternetGetConnectedState(out int Description, int ReservedValue);

	public static bool IsKeyboardAttached()
	{
		return true;
	}

	public static void LoadLevel(string level = null)
	{
		string savedScene = ProductData.GetSavedScene();
		if (!string.IsNullOrEmpty(level))
		{
			RsResourceManager.LoadLevel(level);
		}
		else if (!string.IsNullOrEmpty(savedScene))
		{
			RsResourceManager.LoadLevel(savedScene);
		}
		else
		{
			RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
		}
	}

	public static UnityEngine.Component GetComponentInParent(Type inType, GameObject inObject)
	{
		if (inObject == null)
		{
			Debug.LogError(" Object referance is not set for type : " + inType);
		}
		UnityEngine.Component component = null;
		Transform parent = inObject.transform.parent;
		while (component == null && parent != null)
		{
			UnityEngine.Component component2 = parent.GetComponent(inType);
			if (component2 != null)
			{
				component = component2;
				break;
			}
			parent = parent.transform.parent;
		}
		return component;
	}

	public static GameObject[] GetChildObjectList(GameObject obj)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in obj.transform)
		{
			list.Add(item.gameObject);
		}
		return list.ToArray();
	}

	public static Transform FindChildTransform(GameObject obj, string tname)
	{
		return FindChildTransform(obj, tname, inactive: false);
	}

	public static Transform FindChildTransform(GameObject obj, string tname, bool inactive)
	{
		if (obj == null)
		{
			return null;
		}
		if (obj.name == tname)
		{
			return obj.transform;
		}
		foreach (Transform item in obj.transform)
		{
			if (inactive || item.gameObject.activeSelf)
			{
				Transform transform2 = FindChildTransform(item.gameObject, tname, inactive);
				if (transform2 != null)
				{
					return transform2;
				}
			}
		}
		return null;
	}

	public static Transform FindChildTransformContaining(GameObject obj, string tname)
	{
		if (obj.name.Contains(tname))
		{
			return obj.transform;
		}
		foreach (Transform item in obj.transform)
		{
			if (item.name.Contains(tname))
			{
				return item;
			}
			Transform transform2 = FindChildTransformContaining(item.gameObject, tname);
			if (transform2 != null)
			{
				return transform2;
			}
		}
		return null;
	}

	public static void FindChildTransformsContaining(GameObject obj, string tname, ref List<Transform> outList)
	{
		if (obj.name.Contains(tname))
		{
			outList.Add(obj.transform);
		}
		foreach (Transform item in obj.transform)
		{
			if (item.childCount > 0)
			{
				FindChildTransformsContaining(item.gameObject, tname, ref outList);
			}
			else if (item.name.Contains(tname))
			{
				outList.Add(item);
			}
		}
	}

	public static void SetChildrenActive(GameObject obj, bool active, bool recursive = false, string inObjectName = "")
	{
		foreach (Transform item in obj.transform)
		{
			if (string.IsNullOrEmpty(inObjectName) || item.name.Contains(inObjectName))
			{
				item.gameObject.SetActive(active);
				if (recursive)
				{
					SetChildrenActive(item.gameObject, active, recursive, inObjectName);
				}
			}
		}
	}

	public static void SetObjectSelfLit(Transform obj, bool t)
	{
		Renderer[] componentsInChildren = obj.gameObject.GetComponentsInChildren<Renderer>();
		Shader shader = null;
		shader = ((!t) ? Shader.Find("Diffuse") : Shader.Find("Self-Illumin/Diffuse"));
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			Material[] materials = array[i].materials;
			for (int j = 0; j < materials.Length; j++)
			{
				materials[j].shader = shader;
			}
		}
	}

	public static void SetObjectCastShadow(Transform obj, bool t)
	{
		Renderer[] componentsInChildren = obj.gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].shadowCastingMode = (t ? ShadowCastingMode.On : ShadowCastingMode.Off);
		}
	}

	public static void SetObjectReceiveShadow(Transform obj, bool t)
	{
		Renderer[] componentsInChildren = obj.gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].receiveShadows = t;
		}
	}

	public static Vector3 MoveAlongVector(Vector3 vs, Vector3 ve, float dist)
	{
		Vector3 vector = new Vector3(0f, 0f, 0f);
		vector = ve - vs;
		if (dist > vector.magnitude - 0.001f || vector.magnitude < 0.001f)
		{
			return ve;
		}
		vector *= dist / vector.magnitude;
		return vector + vs;
	}

	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;
		foreach (Transform item in obj.transform)
		{
			SetLayerRecursively(item.gameObject, newLayer);
		}
	}

	public static int FindMaterialIdx(GameObject obj, string matName)
	{
		Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			int num = componentInChildren.materials.Length;
			for (int i = 0; i < num; i++)
			{
				if (componentInChildren.materials[i].name.Contains(matName))
				{
					return i;
				}
			}
		}
		return -1;
	}

	public static Shader GetMaterialShader(GameObject obj, int idx)
	{
		Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			return componentInChildren.materials[idx].shader;
		}
		Debug.LogError("Missing Shader on material " + idx);
		return null;
	}

	public static void SetMaterialShader(GameObject obj, int idx, string shaderName)
	{
		Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
		if (componentInChildren != null)
		{
			Shader shader = Shader.Find(shaderName);
			if (shader != null)
			{
				componentInChildren.materials[idx].shader = shader;
			}
			else
			{
				Debug.LogError("Missing Shader " + shaderName);
			}
		}
	}

	public static Texture GetObjectTexture(GameObject obj, int idx)
	{
		return obj.GetComponentInChildren<Renderer>().materials[idx].mainTexture;
	}

	public static Texture SetObjectTexture(GameObject obj, string partName, int idx, Texture t)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.transform.name == partName)
			{
				Texture mainTexture = renderer.materials[idx].mainTexture;
				renderer.materials[idx].mainTexture = t;
				return mainTexture;
			}
		}
		Debug.LogError(partName + " not found in object " + obj.name + " for setting texture " + t.name);
		return null;
	}

	public static void SetObjectTexture(GameObject obj, int idx, Texture t)
	{
		Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
		if (componentInChildren != null && idx < componentInChildren.materials.Length)
		{
			componentInChildren.materials[idx].mainTexture = t;
		}
	}

	public static void SetObjectTexture(GameObject obj, int idx, string propertyName, Texture t)
	{
		Renderer componentInChildren = obj.GetComponentInChildren<Renderer>();
		if (componentInChildren != null && propertyName != null && componentInChildren.materials[idx].HasProperty(propertyName))
		{
			componentInChildren.materials[idx].SetTexture(propertyName, t);
		}
	}

	public static void SetObjectVisibility(GameObject obj, bool t)
	{
		obj.GetComponentInChildren<Renderer>().enabled = t;
	}

	public static int[] GenerateShuffledInts(int size)
	{
		return GenerateShuffledInts(size, size, size);
	}

	public static int[] GenerateShuffledInts(int size, int valRange, int times)
	{
		int[] array = new int[size];
		for (int i = 0; i < size; i++)
		{
			array[i] = i % valRange;
		}
		for (int i = 0; i < times; i++)
		{
			int num = UnityEngine.Random.Range(0, size);
			int num2 = UnityEngine.Random.Range(0, size);
			if (num != num2)
			{
				int num3 = array[num];
				array[num] = array[num2];
				array[num2] = num3;
			}
		}
		return array;
	}

	public static void Shuffle<TYPE>(TYPE[] array)
	{
		for (int i = 0; i < array.Length; i++)
		{
			int num = UnityEngine.Random.Range(0, array.Length);
			int num2 = UnityEngine.Random.Range(0, array.Length);
			if (num != num2)
			{
				TYPE val = array[num];
				array[num] = array[num2];
				array[num2] = val;
			}
		}
	}

	public static void Shuffle<TYPE>(List<TYPE> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			int num = UnityEngine.Random.Range(0, list.Count);
			int num2 = UnityEngine.Random.Range(0, list.Count);
			if (num != num2)
			{
				TYPE value = list[num];
				list[num] = list[num2];
				list[num2] = value;
			}
		}
	}

	public static bool IsInView(Camera cam, GameObject obj)
	{
		Vector3 position = obj.transform.position;
		Collider component = obj.GetComponent<Collider>();
		if (component != null)
		{
			position += component.bounds.center;
		}
		Vector3 vector = cam.WorldToViewportPoint(position);
		if (vector.z > cam.farClipPlane || vector.z < cam.nearClipPlane)
		{
			return false;
		}
		if (vector.x > 1f || vector.x < 0f)
		{
			return false;
		}
		if (vector.y > 1f || vector.y < 0f)
		{
			return false;
		}
		return true;
	}

	public static List<string> FindObjectsInCameraView(Camera cam)
	{
		List<string> list = new List<string>();
		GameObject[] array = GameObject.FindGameObjectsWithTag("Photoable");
		foreach (GameObject gameObject in array)
		{
			if (IsInView(cam, gameObject))
			{
				list.Add(gameObject.name);
			}
		}
		return list;
	}

	public static void HideObject(GameObject obj, bool t)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = !t;
		}
	}

	public static int GetGroundRayCheckLayers()
	{
		int num = -1;
		if (mRCL == -1)
		{
			mRCL = ~((1 << LayerMask.NameToLayer("Avatar")) | (1 << LayerMask.NameToLayer("Ignore Raycast")) | (1 << LayerMask.NameToLayer("Collectibles")) | (1 << LayerMask.NameToLayer("IgnoreGroundRay")) | (1 << LayerMask.NameToLayer("MMOAvatar")) | (1 << LayerMask.NameToLayer("CameraCLD")) | (1 << LayerMask.NameToLayer("2DNGUI")) | (1 << LayerMask.NameToLayer("3DNGUI")) | (1 << LayerMask.NameToLayer("LoadScreen")));
		}
		if (AvAvatar.pAvatarCam != null)
		{
			return AvAvatar.pAvatarCam.GetComponent<Camera>().cullingMask & mRCL;
		}
		return num & mRCL;
	}

	public static Collider GetGroundHeight(Vector3 sp, float dist, out float groundHeight, out Vector3 normal)
	{
		if (Physics.Raycast(new Ray(sp, new Vector3(0f, -1f, 0f)), out var hitInfo, dist, GetGroundRayCheckLayers()))
		{
			groundHeight = hitInfo.point.y;
			normal = hitInfo.normal;
			return hitInfo.collider;
		}
		groundHeight = float.NegativeInfinity;
		normal = Vector3.zero;
		return null;
	}

	public static Collider GetGroundHeight(Vector3 sp, float dist, out float groundHeight)
	{
		if (Physics.Raycast(new Ray(sp, new Vector3(0f, -1f, 0f)), out var hitInfo, dist, GetGroundRayCheckLayers()))
		{
			groundHeight = hitInfo.point.y;
			return hitInfo.collider;
		}
		groundHeight = float.NegativeInfinity;
		return null;
	}

	public static float GetGroundHeight(Vector3 sp, float dist)
	{
		if (Physics.Raycast(new Ray(sp, new Vector3(0f, -1f, 0f)), out var hitInfo, dist, GetGroundRayCheckLayers()))
		{
			return hitInfo.point.y;
		}
		return float.NegativeInfinity;
	}

	public static float GetGroundHeightNT(Vector3 sp, float dist)
	{
		if (Physics.Raycast(new Ray(sp, new Vector3(0f, -1f, 0f)), out var hitInfo, dist, GetGroundRayCheckLayers()) && !hitInfo.collider.isTrigger)
		{
			return hitInfo.point.y;
		}
		return float.NegativeInfinity;
	}

	public static bool FindPosNextToObject(out Vector3 outPos, GameObject inDest, float inDist)
	{
		float ioStartOffset = 0f;
		return FindPosNextToObject(out outPos, inDest, inDist, 100f, ref ioStartOffset, 45f, 10f, 0f);
	}

	public static bool FindPosNextToObject(out Vector3 outPos, GameObject inDest, float inDist, float inHeight, ref float ioStartOffset, float inHowOften, float inMaxHeightDiff, float inDestOffset)
	{
		Vector3 position = inDest.transform.position;
		Vector3 forward = inDest.transform.forward;
		Vector3 vector = forward;
		Matrix4x4 identity = Matrix4x4.identity;
		if (ioStartOffset != 0f)
		{
			identity.SetTRS(Vector3.zero, Quaternion.AngleAxis(ioStartOffset, Vector3.up), Vector3.one);
			vector = identity.MultiplyVector(forward);
		}
		identity.SetTRS(Vector3.zero, Quaternion.AngleAxis(inHowOften, Vector3.up), Vector3.one);
		Vector3 vector2 = position + vector * inDist;
		vector2.y += inDestOffset;
		outPos = Vector3.zero;
		bool result = false;
		float num = 0f;
		while (num < 360f)
		{
			bool flag = false;
			Vector3 origin = new Vector3(vector2.x, vector2.y + inHeight, vector2.z);
			Vector3 vector3 = new Vector3(0f, -10000f, 0f);
			if (Physics.Raycast(new Ray(origin, new Vector3(0f, -1f, 0f)), out var hitInfo, float.PositiveInfinity, GetGroundRayCheckLayers()))
			{
				if (hitInfo.point.y > vector3.y && Mathf.Abs(hitInfo.point.y - position.y) <= inMaxHeightDiff)
				{
					vector3 = hitInfo.point;
					result = true;
				}
				else if (hitInfo.point.y - position.y > inMaxHeightDiff)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				outPos = vector3;
				ioStartOffset += num;
				break;
			}
			num += inHowOften;
			vector = identity.MultiplyVector(vector);
			vector2 = position + vector * inDist;
			vector2.y += inDestOffset;
		}
		return result;
	}

	public static void TeleportObjectToObject(GameObject inObject, GameObject inDestObject, float randomOffset, bool useEffect)
	{
		TeleportObjectToPosition(inObject, inDestObject.transform.position, inDestObject.transform.rotation, randomOffset, useEffect);
	}

	public static void TeleportObjectToPosition(GameObject inObject, Vector3 position, Quaternion rotation, float randomOffset, bool useEffect, GameObject fx = null)
	{
		Vector3 position2 = inObject.transform.position;
		if (randomOffset > 0f)
		{
			float num = UnityEngine.Random.Range(0, 24) * 15;
			float num2 = Mathf.Cos(num * (MathF.PI / 180f)) * randomOffset;
			float num3 = Mathf.Sin(num * (MathF.PI / 180f)) * randomOffset;
			position.x += num2;
			position.z += num3;
		}
		inObject.transform.position = position;
		inObject.transform.rotation = rotation;
		if (!useEffect)
		{
			return;
		}
		if ((double)(position2 - position).sqrMagnitude > 0.2)
		{
			if (fx != null)
			{
				TeleportFx.PlayAt(position2, inPlaySound: true, fx);
			}
			else
			{
				TeleportFx.PlayAt(position2);
			}
		}
		if (fx != null)
		{
			TeleportFx.PlayAt(position, inPlaySound: true, fx);
		}
		else
		{
			TeleportFx.PlayAt(position);
		}
	}

	public static bool BelowMinimumLOD(Vector3 pos)
	{
		return BelowMinimumLOD(pos, GetQualityByName("Very Low"));
	}

	public static bool BelowMinimumLOD(Vector3 pos, int qlevel)
	{
		bool result = false;
		if ((QualitySettings.GetQualityLevel() < qlevel || GrFPS._IsBelowMinimum) && AvAvatar.pAvatarCam != null && AvAvatar.pAvatarCam.activeInHierarchy && AvAvatar.pAvatarCam.GetComponent<Camera>().enabled && Vector3.Distance(pos, AvAvatar.AvatarCamPosition) > 100f)
		{
			result = true;
		}
		return result;
	}

	public static int FloorToInt(float val, int digits)
	{
		return Mathf.FloorToInt((float)Math.Round(val, digits));
	}

	public static Vector3 Rotate2DPoint(Vector3 inPosition, Vector3 inPivot, float inAngle)
	{
		Vector3 zero = Vector3.zero;
		inPivot.y = 0f;
		Vector3 vector = inPosition - inPivot;
		zero.x = vector.x * Mathf.Cos(inAngle * (MathF.PI / 180f)) - vector.z * Mathf.Sin(inAngle * (MathF.PI / 180f));
		zero.z = vector.z * Mathf.Cos(inAngle * (MathF.PI / 180f)) + vector.x * Mathf.Sin(inAngle * (MathF.PI / 180f));
		return zero + inPivot;
	}

	public static string GetLocaleLanguage()
	{
		if (!string.IsNullOrEmpty(mSysLanguage))
		{
			return mSysLanguage;
		}
		SetLocaleLanguage("en-US");
		return "en-US";
	}

	public static void SetLocaleLanguage(string locale)
	{
		mSysLanguage = ProductConfig.GetLocale(locale);
	}

	public static bool IsLocaleRTL()
	{
		return GetLocaleLanguage() == "ar-EG";
	}

	public static bool IsConnectedToWWW()
	{
		return _ConnectedToInternet & (Application.internetReachability != NetworkReachability.NotReachable);
	}

	public static void DestroyObjectOfType(GameObject inGameObj, Type inType, bool inDestroyObjToo)
	{
		int num = 0;
		UtDebug.Log(" @@@@@@@@@ Try cleanup of type : " + inType);
		UnityEngine.Component[] componentsInChildren = inGameObj.gameObject.GetComponentsInChildren(inType, includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		for (num = 0; num < componentsInChildren.Length; num++)
		{
			UnityEngine.Component component = componentsInChildren[num];
			if (!(component != null))
			{
				continue;
			}
			if (inType == typeof(Renderer))
			{
				Renderer renderer = (Renderer)component;
				if (renderer.materials != null)
				{
					for (int i = 0; i < renderer.materials.Length; i++)
					{
						UtDebug.Log(" Remove material : " + i);
						if ((bool)renderer.materials[i])
						{
							UnityEngine.Object.Destroy(renderer.materials[i]);
						}
					}
				}
			}
			UtDebug.Log(" ======== removing : " + component.name);
			UnityEngine.Object.Destroy(component);
			component = null;
		}
		GC.Collect();
		if (inDestroyObjToo)
		{
			UnityEngine.Object.Destroy(inGameObj);
		}
	}

	public static int GetQualityByName(string qualityName)
	{
		string[] names = QualitySettings.names;
		for (int i = 0; i < names.Length; i++)
		{
			if (qualityName.Equals(names[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static string GetTimerString(int tn)
	{
		int num = tn / 60;
		int num2 = tn % 60;
		return num + ":" + num2.ToString("d2");
	}

	public static void AddAnimationClipsFromBundle(GameObject inGO, AssetBundle inAnimBundle)
	{
		if (inAnimBundle == null || inGO == null)
		{
			Debug.LogError("!!!!!!!!  UtUtilities::AddAnimation ==> No valid params are passed  ");
			return;
		}
		Animation component = inGO.GetComponent<Animation>();
		if (component == null)
		{
			Debug.Log("  UtUtilities::AddAnimation ==> no animation Component attached to : " + inGO.name);
			return;
		}
		UnityEngine.Object[] array = inAnimBundle.LoadAllAssets(typeof(AnimationClip));
		for (int i = 0; i < array.Length; i++)
		{
			AnimationClip animationClip = (AnimationClip)array[i];
			component.AddClip(animationClip, animationClip.name);
		}
	}

	public static float GetUniformScreenScale(float inStdHorRes, float inStdVerRes, float inMinScale)
	{
		float num = 1f;
		float num2 = (float)Screen.width / (float)Screen.height;
		float num3 = inStdHorRes / inStdVerRes;
		num = ((!(num2 > num3)) ? ((float)Screen.width / inStdHorRes) : ((float)Screen.height / inStdVerRes));
		return (num < inMinScale) ? inMinScale : num;
	}

	public static void DumpDict(Dictionary<string, object> inDict)
	{
		foreach (KeyValuePair<string, object> item in inDict)
		{
			Debug.Log("---------- Data received from client Key: " + item.Key + " = " + item.Value.ToString());
		}
	}

	public static void DumpChangedVariableKeys(List<object> inObject)
	{
		foreach (object item in inObject)
		{
			Debug.LogWarning("@@@@@ ChangedKey: " + item.ToString());
		}
	}

	public static string SerializeToXml(object inData, bool noNamespace = false)
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		XmlSerializer xmlSerializer = new XmlSerializer(inData.GetType());
		if (noNamespace)
		{
			XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
			xmlSerializerNamespaces.Add("", "");
			xmlSerializer.Serialize(stringWriter, inData, xmlSerializerNamespaces);
		}
		else
		{
			xmlSerializer.Serialize(stringWriter, inData);
		}
		string text = stringWriter.ToString();
		string[] array = text.Split('\n');
		text = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].Contains("xsi:nil"))
			{
				text += array[i];
			}
		}
		return text;
	}

	public static TYPE DeserializeFromXml<TYPE>(string inData)
	{
		if (string.IsNullOrEmpty(inData))
		{
			Debug.LogError("Deserialize error inData is null!");
			return default(TYPE);
		}
		using StringReader textReader = new StringReader(inData);
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(TYPE));
		try
		{
			UtDebug.Log("*****Deserialize from xml\n" + inData);
			return (TYPE)xmlSerializer.Deserialize(textReader);
		}
		catch (Exception ex)
		{
			Debug.LogError("Deserialize error = " + ex.ToString() + " \n " + inData);
			return default(TYPE);
		}
	}

	public static TYPE DecryptDeserializeFromXml<TYPE>(string inData)
	{
		inData = TripleDES.DecryptUnicode(inData, WsWebService.pSecret);
		return DeserializeFromXml<TYPE>(inData);
	}

	public static object DeserializeFromXml(string inData, Type inType)
	{
		if (string.IsNullOrEmpty(inData))
		{
			Debug.LogError("Deserialize error inData is null!\n Data Type : " + inType);
			return GetDefault(inType);
		}
		using StringReader textReader = new StringReader(inData);
		XmlSerializer xmlSerializer = new XmlSerializer(inType);
		try
		{
			UtDebug.Log("*****Deserialize from xml\n" + inData);
			return xmlSerializer.Deserialize(textReader);
		}
		catch (Exception ex)
		{
			Debug.LogError("Deserialize error = " + ex.ToString() + " \n " + inData + "\n Data Type : " + inType);
			return GetDefault(inType);
		}
	}

	public static object GetDefault(Type type)
	{
		if (type == null || !type.IsValueType || type == typeof(void))
		{
			return null;
		}
		return Activator.CreateInstance(type);
	}

	public static void CreateOrOpenTable(Type inClass)
	{
		string text = inClass.ToString();
		UtDebug.Log(" inClass : " + text);
	}

	public static Rect RectScale(Rect inRect, float inScaleVal)
	{
		inRect.x *= inScaleVal;
		inRect.y *= inScaleVal;
		inRect.width *= inScaleVal;
		inRect.height *= inScaleVal;
		return inRect;
	}

	public static Rect RectSizeScale(Rect inRect, float inScaleVal)
	{
		inRect.width *= inScaleVal;
		inRect.height *= inScaleVal;
		return inRect;
	}

	public static List<Type> GetAllClasses()
	{
		if (mAllClassList == null)
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			List<Type> list = new List<Type>();
			Type[] types = executingAssembly.GetTypes();
			foreach (Type item in types)
			{
				list.Add(item);
			}
			mAllClassList = new List<Type>();
			mAllClassList = list;
		}
		return mAllClassList;
	}

	public static Type GetType(string inType)
	{
		List<Type> allClasses = GetAllClasses();
		Type result = null;
		foreach (Type item in allClasses)
		{
			if (item.ToString() == inType)
			{
				result = item;
			}
		}
		return result;
	}

	public static object ConvertFrom(string inText, Type inType)
	{
		return TypeDescriptor.GetConverter(inType).ConvertFromString(null, CultureInfo.InvariantCulture, inText);
	}

	public static string ProcessSendObject<TYPE>(TYPE inObject) where TYPE : class
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		using MemoryStream memoryStream = new MemoryStream();
		using StreamWriter textWriter = new StreamWriter(memoryStream, uTF8Encoding);
		new XmlSerializer(typeof(TYPE)).Serialize(textWriter, inObject);
		string @string = uTF8Encoding.GetString(memoryStream.ToArray(), 0, (int)memoryStream.Length);
		string[] array = @string.Split('\n');
		@string = string.Empty;
		for (int i = 0; i < array.Length; i++)
		{
			if (!array[i].Contains("xsi:nil"))
			{
				array[i] = array[i].Replace("ArrayOfInt1", "ArrayOfInt");
				@string += array[i];
			}
		}
		return @string;
	}

	public static string RemoveCharFromStr(string inStr, char[] inBadChars)
	{
		return string.Concat(inStr.Split(inBadChars, StringSplitOptions.RemoveEmptyEntries));
	}

	public static string SerializeToString(object obj)
	{
		Type type = obj.GetType();
		StringWriter stringWriter = new StringWriter();
		new XmlSerializer(type).Serialize(stringWriter, obj);
		return stringWriter.ToString();
	}

	public static string GetStringRepresentation(Color c)
	{
		return c.r + "," + c.g + "," + c.b + "," + c.a;
	}

	public static bool GetColorFromString(string s, out Color cOut)
	{
		string[] array = s.Split(',');
		cOut = new Color(0f, 0f, 0f, 1f);
		float result;
		float result2;
		float result3;
		if (array.Length > 3)
		{
			if (float.TryParse(array[0], out result) && float.TryParse(array[1], out result2) && float.TryParse(array[2], out result3) && float.TryParse(array[3], out var result4))
			{
				cOut = new Color(result, result2, result3, result4);
				return true;
			}
		}
		else if (array.Length > 2 && float.TryParse(array[0], out result) && float.TryParse(array[1], out result2) && float.TryParse(array[2], out result3))
		{
			cOut = new Color(result, result2, result3, 1f);
			return true;
		}
		return false;
	}

	public static string GetFieldInfo(object inInstance)
	{
		string text = "";
		FieldInfo[] fields = inInstance.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			text = text + fieldInfo.Name + " " + fieldInfo.GetValue(inInstance).ToString() + ", ";
		}
		return text;
	}

	public static void DrawPublicProperties(object inObj, int inIndentLevel = 0)
	{
		FieldInfo[] allPublicPropeties = GetAllPublicPropeties(inObj);
		foreach (FieldInfo fieldInfo in allPublicPropeties)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(HideInInspector), inherit: false);
			if (customAttributes == null || customAttributes.Length == 0)
			{
				DrawField(fieldInfo, inObj, inIndentLevel);
			}
		}
	}

	public static void DrawField(FieldInfo inType, object inObj, int inIndentLevel = 0)
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(20 * inIndentLevel);
		GUILayout.Label(inType.Name);
		GUILayout.Label(inType.FieldType.ToString());
		object value = inType.GetValue(inObj);
		string text = "";
		if (value != null)
		{
			text = value.ToString();
		}
		object obj = GUILayout.TextField(text, GUILayout.Width(200f));
		if (text != obj.ToString())
		{
			object value2 = ConvertFrom(obj.ToString(), inType.FieldType);
			inType.SetValue(inObj, value2);
		}
		GUILayout.EndHorizontal();
	}

	public static FieldInfo[] GetAllPublicPropeties(object inObj)
	{
		return inObj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
	}

	public static void RemoveAllAnimationClips(Animation inAnim)
	{
		if (!(inAnim != null))
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (AnimationState item in inAnim)
		{
			list.Add(item.name);
		}
		foreach (string item2 in list)
		{
			inAnim.RemoveClip(item2);
		}
	}

	public static string GetAccountToken(string login, string pass)
	{
		string plaintext = SerializeToString(new ParentLoginData
		{
			UserName = login,
			Password = pass,
			Locale = "en-US"
		});
		WWWForm wWWForm = new WWWForm();
		wWWForm.AddField("apiKey", ProductConfig.pApiKey);
		wWWForm.AddField("parentLoginData", TripleDES.EncryptUnicode(plaintext, ProductConfig.pSecret));
		WWW wWW = new WWW(ProductConfig.pInstance.AuthenticationServerV3URL + "/LoginParent", wWWForm);
		while (!wWW.isDone)
		{
		}
		return (DeserializeFromXml(TripleDES.DecryptUnicode(DeserializeFromXml(wWW.text, typeof(string)) as string, WsWebService.pSecret), typeof(ParentLoginInfo)) as ParentLoginInfo).ApiToken;
	}

	public static T Clone<T>(T inSource) where T : new()
	{
		if (inSource == null)
		{
			return default(T);
		}
		return DeserializeFromXml<T>(SerializeToXml(inSource));
	}

	public static string GetKAUIColorString(Color c)
	{
		return "[" + HexUtil.FloatToHex(Mathf.Clamp01(c.r) * 255f) + HexUtil.FloatToHex(Mathf.Clamp01(c.g) * 255f) + HexUtil.FloatToHex(Mathf.Clamp01(c.b) * 255f) + "]";
	}

	public static float GetScaleProportion(Renderer renderer, float screenX, float screenY, Camera renderCam, Transform marker)
	{
		Vector3 vector = renderCam.WorldToScreenPoint(marker.transform.position);
		float num = Mathf.Max(screenX, screenY);
		Vector3 a = renderCam.ScreenToWorldPoint(vector + new Vector3(0f, (0f - num) / 2f, 0f));
		Vector3 b = renderCam.ScreenToWorldPoint(vector + new Vector3(0f, num / 2f, 0f));
		float num2 = Vector3.Distance(a, b);
		num = Vector3.Distance(renderer.bounds.max, renderer.bounds.min);
		return num2 / num;
	}

	public static int GetTextureResolution()
	{
		return ProductConfig.GetPlatformSettings()?.DownloadTextureSize ?? (-1);
	}

	public static Vector3 ClosestPointOnLine(Vector3 v1, Vector3 v2, Vector3 point)
	{
		Vector3 vector = v2 - v1;
		float magnitude = vector.magnitude;
		if (magnitude > 0f)
		{
			vector *= 1f / magnitude;
		}
		float num = Mathf.Clamp(Vector3.Dot(vector, point - v1), 0f, magnitude);
		return v1 + num * vector;
	}

	public static string GetImageURL(string inURL)
	{
		int textureResolution = GetTextureResolution();
		if (textureResolution == -1)
		{
			return inURL;
		}
		inURL = inURL + ";width=" + textureResolution + ";height=" + textureResolution + ";";
		return inURL;
	}

	public static GameObject AttachObject(GameObject parentObj, GameObject objRef, string attachBone, Vector3 offset, Vector3 rotation)
	{
		if ((bool)objRef && !string.IsNullOrEmpty(attachBone))
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(objRef);
			Transform transform = FindChildTransform(parentObj, attachBone);
			if ((bool)gameObject && (bool)transform)
			{
				gameObject.transform.parent = transform.transform;
				gameObject.transform.localPosition = offset;
				gameObject.transform.localRotation = Quaternion.Euler(rotation);
			}
			return gameObject;
		}
		return null;
	}

	public static void DetachObject(GameObject parentObj, string name)
	{
		List<Transform> outList = new List<Transform>();
		FindChildTransformsContaining(parentObj, name, ref outList);
		foreach (Transform item in outList)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
	}

	public static bool IsValidEmail(string inEmailId)
	{
		string input = inEmailId.Trim();
		return new Regex("\\b\\w+(?:[-+.']\\w+)*@\\w+(?:[-.]\\w+)*\\.\\w{2,4}(?:[-.]\\w{2,4})*\\b").IsMatch(input);
	}

	public static void Vector3FromString(string inString, ref Vector3 inDefaultPos)
	{
		inString = inString.Replace("(", "");
		inString = inString.Replace(")", "");
		string[] array = inString.Split(',');
		if (array.Length >= 1)
		{
			inDefaultPos.x = float.Parse(array[0]);
		}
		if (array.Length >= 2)
		{
			inDefaultPos.y = float.Parse(array[1]);
		}
		if (array.Length >= 3)
		{
			inDefaultPos.z = float.Parse(array[2]);
		}
	}

	public static UIAnchor.Side GetAnchorSide(Vector3 inPosition)
	{
		inPosition = GetScreenPosition(inPosition);
		UIAnchor.Side result = UIAnchor.Side.Center;
		Vector3[] array = new Vector3[9];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = GetAnchorPosition((UIAnchor.Side)i);
		}
		float num = float.PositiveInfinity;
		for (int j = 0; j < array.Length; j++)
		{
			float num2 = Vector3.Distance(inPosition, array[j]);
			if (num2 < num)
			{
				num = num2;
				result = (UIAnchor.Side)j;
			}
		}
		return result;
	}

	public static Vector3 GetScreenPosition(Vector3 inWorldPos)
	{
		if (KAUIManager.pInstance != null)
		{
			return KAUIManager.pInstance.camera.WorldToScreenPoint(inWorldPos);
		}
		return Vector3.zero;
	}

	public static Vector3 GetWorldPosition(Vector3 inScreenPos)
	{
		if (KAUIManager.pInstance != null)
		{
			return Camera.main.ScreenToWorldPoint(inScreenPos);
		}
		return Vector3.zero;
	}

	public static Vector3 GetAnchorPosition(UIAnchor.Side side)
	{
		Rect pixelRect = KAUIManager.pInstance.camera.pixelRect;
		float x = (pixelRect.xMin + pixelRect.xMax) * 0.5f;
		float y = (pixelRect.yMin + pixelRect.yMax) * 0.5f;
		Vector3 result = new Vector3(x, y, 0f);
		switch (side)
		{
		case UIAnchor.Side.TopRight:
		case UIAnchor.Side.Right:
		case UIAnchor.Side.BottomRight:
			result.x = pixelRect.xMax;
			break;
		case UIAnchor.Side.Top:
		case UIAnchor.Side.Bottom:
		case UIAnchor.Side.Center:
			result.x = x;
			break;
		default:
			result.x = pixelRect.xMin;
			break;
		}
		switch (side)
		{
		case UIAnchor.Side.TopLeft:
		case UIAnchor.Side.Top:
		case UIAnchor.Side.TopRight:
			result.y = pixelRect.yMax;
			break;
		case UIAnchor.Side.Left:
		case UIAnchor.Side.Right:
		case UIAnchor.Side.Center:
			result.y = y;
			break;
		default:
			result.y = pixelRect.yMin;
			break;
		}
		result.x = Mathf.RoundToInt(result.x);
		result.y = Mathf.RoundToInt(result.y);
		return result;
	}

	public static void AttachToAnchor(GameObject uiObject, GameObject inObject, UIAnchor.Side anchorSide, bool createIfNotFound = false)
	{
		if (uiObject == null || inObject == null)
		{
			return;
		}
		UIAnchor[] componentsInChildren = uiObject.GetComponentsInChildren<UIAnchor>(includeInactive: true);
		KAWidget component = inObject.GetComponent<KAWidget>();
		if (componentsInChildren == null)
		{
			return;
		}
		UIAnchor[] array = componentsInChildren;
		foreach (UIAnchor uIAnchor in array)
		{
			if (uIAnchor != null && uIAnchor.side == anchorSide && uIAnchor.transform.parent == uiObject.transform)
			{
				inObject.transform.parent = uIAnchor.transform;
				if (component != null)
				{
					component.pAnchor = uIAnchor;
				}
				return;
			}
		}
		if (createIfNotFound)
		{
			GameObject gameObject = new GameObject();
			gameObject.layer = uiObject.layer;
			gameObject.name = "Anchor-" + anchorSide;
			gameObject.transform.parent = uiObject.transform;
			UIAnchor uIAnchor2 = gameObject.AddComponent(typeof(UIAnchor)) as UIAnchor;
			uIAnchor2.side = anchorSide;
			inObject.transform.parent = gameObject.transform;
			if (component != null)
			{
				component.pAnchor = uIAnchor2;
			}
		}
	}

	public static void SetToScreenPosition(Vector3 inPos, GameObject inObject)
	{
		if (KAUIManager.pInstance != null && inObject != null)
		{
			Vector2 localPosition = GetLocalPosition(KAUIManager.pInstance.camera.ScreenToWorldPoint(new Vector2(inPos.x, (float)Screen.height - inPos.y)), inObject);
			inObject.transform.localPosition = new Vector3(localPosition.x, localPosition.y, inPos.z);
		}
	}

	public static Vector2 GetLocalPosition(Vector2 pos, GameObject inObject)
	{
		Transform parent = inObject.transform.parent;
		Vector2 result = pos;
		while (parent != null)
		{
			result -= new Vector2(parent.localPosition.x, parent.localPosition.y);
			parent = parent.parent;
		}
		return result;
	}

	public static bool IsSameList(int[] inList1, int[] inList2)
	{
		if (inList1 == null || inList2 == null || inList1.Length != inList2.Length)
		{
			return false;
		}
		foreach (int num in inList1)
		{
			bool flag = false;
			foreach (int num2 in inList2)
			{
				if (num == num2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public static bool OnServerError(string inErrorText, string inURL, ref int inRetryCount)
	{
		if (inErrorText.StartsWith("Could not resolve host"))
		{
			_ConnectedToInternet = false;
		}
		if (GameDataConfig.pInstance == null || GameDataConfig.pInstance.ServerErrorMessages == null || GameDataConfig.pInstance.ServerErrorMessages.Errors == null)
		{
			return false;
		}
		if (GameDataConfig.pInstance.ServerErrorMessages.ExcludeUrl != null && !string.IsNullOrEmpty(Array.Find(GameDataConfig.pInstance.ServerErrorMessages.ExcludeUrl, (string element) => Regex.IsMatch(inURL, element, RegexOptions.IgnoreCase))))
		{
			return false;
		}
		ServerErrorMessageGroupInfo serverErrorMessageGroupInfo = null;
		ServerErrorMessageGroupInfo[] errors = GameDataConfig.pInstance.ServerErrorMessages.Errors;
		foreach (ServerErrorMessageGroupInfo serverErrorMessageGroupInfo2 in errors)
		{
			if (!string.IsNullOrEmpty(Array.Find(serverErrorMessageGroupInfo2.ErrorTexts, (string element) => inErrorText.IndexOf(element, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				serverErrorMessageGroupInfo = serverErrorMessageGroupInfo2;
				break;
			}
		}
		if (serverErrorMessageGroupInfo != null)
		{
			if (serverErrorMessageGroupInfo.MaxRetries.HasValue && serverErrorMessageGroupInfo.MaxRetries.Value > inRetryCount)
			{
				inRetryCount++;
				return true;
			}
			ShowServerError(serverErrorMessageGroupInfo);
		}
		return false;
	}

	public static void ShowGenericValidationError()
	{
		string inErrorText = "Data Validation Failed";
		if (GameDataConfig.pInstance == null || GameDataConfig.pInstance.ServerErrorMessages == null || GameDataConfig.pInstance.ServerErrorMessages.Errors == null)
		{
			return;
		}
		ServerErrorMessageGroupInfo inErrorMessageInfo = null;
		ServerErrorMessageGroupInfo[] errors = GameDataConfig.pInstance.ServerErrorMessages.Errors;
		foreach (ServerErrorMessageGroupInfo serverErrorMessageGroupInfo in errors)
		{
			if (!string.IsNullOrEmpty(Array.Find(serverErrorMessageGroupInfo.ErrorTexts, (string element) => inErrorText.IndexOf(element, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				inErrorMessageInfo = serverErrorMessageGroupInfo;
				break;
			}
		}
		ShowServerError(inErrorMessageInfo);
	}

	public static void ShowServerError(ServerErrorMessageGroupInfo inErrorMessageInfo, OnUiErrorActionEventHandler errorActionHandler = null, bool destroyOnClick = false)
	{
		if (inErrorMessageInfo != null)
		{
			ShowServerError(inErrorMessageInfo.ErrorMessageText, errorActionHandler, destroyOnClick);
		}
	}

	public static void ShowServerError(LocaleString errorText, OnUiErrorActionEventHandler errorActionHandler = null, bool destroyOnClick = false)
	{
		UiErrorHandler uiErrorHandler = UiErrorHandler.ShowErrorUI(UiErrorHandler.ErrorMessageType.CRITICAL_ERROR);
		if (uiErrorHandler != null)
		{
			uiErrorHandler._DestroyOnClick = destroyOnClick;
			if (errorText != null)
			{
				uiErrorHandler.SetText(errorText.GetLocalizedString());
			}
			if (errorActionHandler != null)
			{
				uiErrorHandler.pOnUiErrorActionEventHandler = errorActionHandler;
			}
		}
	}

	public static void ShowServerError(string errorText, OnUiErrorActionEventHandler errorActionHandler = null)
	{
		ServerErrorMessageGroupInfo serverErrorMessageGroupInfo = null;
		ServerErrorMessageGroupInfo[] errors = GameDataConfig.pInstance.ServerErrorMessages.Errors;
		foreach (ServerErrorMessageGroupInfo serverErrorMessageGroupInfo2 in errors)
		{
			if (!string.IsNullOrEmpty(Array.Find(serverErrorMessageGroupInfo2.ErrorTexts, (string element) => errorText.IndexOf(element, StringComparison.OrdinalIgnoreCase) >= 0)))
			{
				serverErrorMessageGroupInfo = serverErrorMessageGroupInfo2;
				break;
			}
		}
		if (serverErrorMessageGroupInfo != null)
		{
			ShowServerError(serverErrorMessageGroupInfo, errorActionHandler);
		}
	}

	public static DateTime IntToDate(int inDateInt)
	{
		return new DateTime(inDateInt & 0x7FFFF, (inDateInt >> 23) & 0xF, (inDateInt >> 27) & 0x1F);
	}

	public static int DateToInt(DateTime inDate)
	{
		return 0 | ((inDate.Day & 0x1F) << 27) | ((inDate.Month & 0xF) << 23) | (inDate.Year & 0x7FFFF);
	}

	public static bool GetVector3FromString(string inVector, out Vector3 vOut)
	{
		string[] array = inVector.Split(',');
		vOut = Vector3.zero;
		if (array.Length > 2 && float.TryParse(array[0], out var result) && float.TryParse(array[1], out var result2) && float.TryParse(array[2], out var result3))
		{
			vOut = new Vector3(result, result2, result3);
			return true;
		}
		return false;
	}

	public static void ReAssignShader(GameObject obj)
	{
		if (obj == null)
		{
			return;
		}
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (renderer.materials == null || renderer.materials.Length == 0)
			{
				continue;
			}
			Material[] materials = renderer.materials;
			foreach (Material material in materials)
			{
				Shader shader = Shader.Find(material.shader.name);
				if (shader != null)
				{
					material.shader = shader;
				}
			}
		}
	}

	public static void SetOcclusionCulling(bool inEnable)
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			allCameras[i].useOcclusionCulling = inEnable;
		}
	}

	public static void SetRealTimeShadowDisabled(bool disable = true)
	{
		Light[] array = UnityEngine.Object.FindObjectsOfType<Light>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].shadows = ((!disable) ? LightShadows.Soft : LightShadows.None);
		}
	}

	public static CultureInfo GetCultureInfo(string locale)
	{
		return CultureInfo.GetCultureInfo(locale);
	}

	public static float WrapNumber(float numberToWrap, float maxAllowed)
	{
		return (numberToWrap % maxAllowed + maxAllowed) % maxAllowed;
	}

	public static string JsonSerializeObject(object inObject, Formatting inFormatting = Formatting.None)
	{
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
		return JsonConvert.SerializeObject(inObject, inFormatting, jsonSerializerSettings);
	}

	public static DateTime GetDefaultPSTTime(DateTime dateTime)
	{
		DateTime result = dateTime.AddHours(-7.0);
		if (!result.IsDaylightSavingTime())
		{
			result = result.AddHours(-1.0);
		}
		return result;
	}

	public static DateTime GetPSTTimeFromUTC(DateTime dateTime)
	{
		string[] array = new string[4] { "America/Los_Angeles", "Pacific Daylight Time", "Pacific Standard Time", "PST8PDT" };
		ReadOnlyCollection<TimeZoneInfo> systemTimeZones = TimeZoneInfo.GetSystemTimeZones();
		TimeZoneInfo timeZoneInfo = null;
		if (systemTimeZones != null && systemTimeZones.Count > 0)
		{
			foreach (TimeZoneInfo item in systemTimeZones)
			{
				string[] array2 = array;
				foreach (string text in array2)
				{
					if (item.Id.ToLower().Contains(text.ToLower()))
					{
						timeZoneInfo = item;
						break;
					}
				}
			}
			foreach (TimeZoneInfo item2 in systemTimeZones)
			{
				if (array.Contains(item2.Id))
				{
					timeZoneInfo = item2;
					break;
				}
			}
			if (timeZoneInfo != null)
			{
				return TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo);
			}
		}
		return GetDefaultPSTTime(dateTime);
	}

	public static string GetFormattedTime(TimeSpan time, string dayText, string hourText, string minuteText, string secondText)
	{
		int days = time.Days;
		int hours = time.Hours;
		int minutes = time.Minutes;
		int seconds = time.Seconds;
		string result = "0" + dayText + "0" + hourText + "0" + minuteText;
		if (days >= 1)
		{
			result = days.ToString("d2") + dayText + hours.ToString("d2") + hourText + minutes.ToString("d2") + minuteText;
		}
		else if (hours > 0)
		{
			result = hours.ToString("d2") + hourText + minutes.ToString("d2") + minuteText + seconds.ToString("d2") + secondText;
		}
		else if (minutes > 0 || seconds > 0)
		{
			result = minutes.ToString("d2") + minuteText + seconds.ToString("d2") + secondText;
		}
		return result;
	}

	public static float GetOSVersion()
	{
		return 0f;
	}

	public static bool IsIOSLowerThanThirteen()
	{
		if (!UtPlatform.IsiOS())
		{
			return false;
		}
		return GetOSVersion() < 13f;
	}

	public static void CopyCameraParameters(Camera sourceCamera, Camera destinationCamera)
	{
		if (sourceCamera != null && destinationCamera != null)
		{
			destinationCamera.CopyFrom(sourceCamera);
		}
	}
}
