using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public abstract class MOBAEntity : MonoBehaviour
{
	public string _UniqueName = "host_chosen";

	public int _TeamID;

	public int _EntityID = -1;

	public bool _Resident;

	protected bool mInitialized;

	protected Vector3 mOrigPosition = Vector3.zero;

	protected Quaternion mOrigRotation = Quaternion.identity;

	[HideInInspector]
	public bool mLimbo = true;

	[HideInInspector]
	public bool mForceReplication;

	[HideInInspector]
	public bool mRequestDestroy;

	private float mLastReplicationTimer;

	private const float mEntityTimeout = 30f;

	private float mFullReplicationFreq = 10f;

	private float mFullReplicationLimit;

	private Dictionary<string, RVar> mRVars = new Dictionary<string, RVar>();

	public bool pGameActive => MOBASDManager.pState == 1;

	protected virtual void Awake()
	{
		mFullReplicationFreq = Random.Range(8f, 10f);
		if (_UniqueName == "host_chosen" && _Resident)
		{
			UtDebug.LogError("MOBA Error: resident MOBA objects must be given a unique name");
			return;
		}
		mOrigPosition = base.transform.localPosition;
		mOrigRotation = base.transform.localRotation;
		base.gameObject.name = _UniqueName;
	}

	public virtual void Init()
	{
		base.gameObject.name = _UniqueName;
		InitVars();
	}

	public string GetReplicationString()
	{
		string text = "";
		if (mLimbo || mRVars == null || mRVars.Count == 0)
		{
			return text;
		}
		bool flag = mForceReplication;
		mForceReplication = false;
		foreach (KeyValuePair<string, RVar> mRVar in mRVars)
		{
			string outString = "";
			RVar value = mRVar.Value;
			object value2 = value.field.GetValue(value.isStatic ? null : this);
			if (value2 is float num)
			{
				if (flag || Mathf.Abs((float)value.lastValue - num) > value.threshold)
				{
					AppendMsgStream(ref outString, value.name, num);
				}
			}
			else if (value2 is int num2)
			{
				if (flag || (float)Mathf.Abs((int)value.lastValue - num2) > value.threshold)
				{
					AppendMsgStream(ref outString, value.name, num2);
				}
			}
			else if (value2 is bool flag2)
			{
				if (flag || (bool)value.lastValue != flag2)
				{
					AppendMsgStream(ref outString, value.name, flag2);
				}
			}
			else if (value2 is Vector3 vector && (flag || ((Vector3)value.lastValue - vector).magnitude > value.threshold))
			{
				AppendMsgStream(ref outString, value.name, vector);
			}
			if (value2 is string)
			{
				string text2 = (string)value2;
				if (flag || (string)value.lastValue != text2)
				{
					AppendMsgStream(ref outString, value.name, text2);
				}
			}
			if (!string.IsNullOrEmpty(outString))
			{
				value.lastValue = value2;
				text += outString;
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			text = ":" + _UniqueName + ":" + _EntityID + text;
		}
		return text;
	}

	public int ApplyReplicationString(string[] repString, int startIndex)
	{
		mLimbo = false;
		mLastReplicationTimer = 0f;
		if (!mInitialized)
		{
			Init();
		}
		int num = startIndex;
		if (repString.Length > num)
		{
			_UniqueName = repString[num];
			num += 2;
		}
		while (repString.Length > num && mRVars.ContainsKey(repString[num]))
		{
			RVar rVar = mRVars[repString[num]];
			Object obj = (rVar.isStatic ? null : this);
			object value = rVar.field.GetValue(obj);
			if (value is float)
			{
				if (float.TryParse(repString[num + 1], out var result))
				{
					rVar.field.SetValue(obj, result);
				}
				num += 2;
			}
			else if (value is int)
			{
				if (int.TryParse(repString[num + 1], out var result2))
				{
					rVar.field.SetValue(obj, result2);
				}
				num += 2;
			}
			else if (value is bool)
			{
				if (bool.TryParse(repString[num + 1], out var result3))
				{
					rVar.field.SetValue(obj, result3);
				}
				num += 2;
			}
			else if (value is Vector3)
			{
				Vector3 zero = Vector3.zero;
				if (float.TryParse(repString[num + 1], out zero.x) && float.TryParse(repString[num + 2], out zero.y) && float.TryParse(repString[num + 3], out zero.z))
				{
					rVar.field.SetValue(obj, zero);
				}
				num += 4;
			}
			else if (value is string)
			{
				rVar.field.SetValue(obj, repString[num + 1]);
				num += 2;
			}
			OnVariableChanged(rVar.name);
		}
		return num;
	}

	private void AppendMsgStream(ref string outString, string varName, float v)
	{
		outString = outString + ":" + varName + ":" + v;
	}

	private void AppendMsgStream(ref string outString, string varName, int v)
	{
		outString = outString + ":" + varName + ":" + v;
	}

	private void AppendMsgStream(ref string outString, string varName, bool v)
	{
		outString = outString + ":" + varName + ":" + v;
	}

	private void AppendMsgStream(ref string outString, string varName, Vector3 v)
	{
		outString = outString + ":" + varName + ":" + v.x + ":" + v.y + ":" + v.z;
	}

	private void AppendMsgStream(ref string outString, string varName, string v)
	{
		outString = outString + ":" + varName + ":" + v;
	}

	protected void Update()
	{
		mLastReplicationTimer += Time.deltaTime;
		if (!_Resident && !mLimbo && !MOBAManager.mIsAuthority && mLastReplicationTimer > 30f)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		if (!mLimbo && !mRequestDestroy)
		{
			EntityUpdate(MOBAManager.mIsAuthority);
		}
		if (!mLimbo)
		{
			mFullReplicationLimit += Time.deltaTime;
			if (mFullReplicationLimit > mFullReplicationFreq)
			{
				mForceReplication = true;
				mFullReplicationLimit = 0f;
			}
		}
	}

	protected abstract void EntityUpdate(bool isAuthority);

	public virtual void EntityReset(bool isAuthority)
	{
		if (mRVars != null)
		{
			foreach (KeyValuePair<string, RVar> mRVar in mRVars)
			{
				RVar value = mRVar.Value;
				value.field.SetValue(value.isStatic ? null : this, value.origValue);
			}
		}
		base.gameObject.transform.localPosition = mOrigPosition;
		base.gameObject.transform.localRotation = mOrigRotation;
	}

	protected virtual void OnVariableChanged(string varName)
	{
	}

	public virtual void OnGainAuthority()
	{
	}

	protected void RequestDestroy()
	{
		if (!_Resident)
		{
			mRequestDestroy = true;
		}
	}

	private void InitVars()
	{
		if (mInitialized)
		{
			return;
		}
		mInitialized = true;
		mRVars.Clear();
		FieldInfo[] fieldsInfo = GetType().GetFieldsInfo(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fieldsInfo)
		{
			if (!fieldInfo.IsDefined(typeof(Replicated), inherit: true))
			{
				continue;
			}
			float threshold = 0f;
			object[] customAttributes = fieldInfo.GetCustomAttributes(inherit: false);
			foreach (object obj in customAttributes)
			{
				if (obj is Replicated)
				{
					threshold = ((Replicated)obj).threshold;
					break;
				}
			}
			RVar rVar = new RVar();
			rVar.name = fieldInfo.Name;
			rVar.threshold = threshold;
			rVar.lastValue = fieldInfo.GetValue(this);
			rVar.origValue = fieldInfo.GetValue(this);
			rVar.isStatic = false;
			rVar.field = fieldInfo;
			mRVars.Add(rVar.name, rVar);
		}
		fieldsInfo = GetType().GetFieldsInfo(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo2 in fieldsInfo)
		{
			if (!fieldInfo2.IsDefined(typeof(Replicated), inherit: true))
			{
				continue;
			}
			float threshold2 = 0f;
			object[] customAttributes = fieldInfo2.GetCustomAttributes(inherit: false);
			foreach (object obj2 in customAttributes)
			{
				if (obj2 is Replicated)
				{
					threshold2 = ((Replicated)obj2).threshold;
					break;
				}
			}
			RVar rVar2 = new RVar();
			rVar2.name = fieldInfo2.Name;
			rVar2.threshold = threshold2;
			rVar2.lastValue = fieldInfo2.GetValue(null);
			rVar2.origValue = fieldInfo2.GetValue(null);
			rVar2.isStatic = true;
			rVar2.field = fieldInfo2;
			mRVars.Add(rVar2.name, rVar2);
		}
	}
}
