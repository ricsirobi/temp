using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

namespace Xsolla;

public abstract class BaseWWWRequest
{
	public Action<int, object[]> ObjectsRecived;

	private List<WWW> prepared;

	private int type;

	public BaseWWWRequest(int newType)
	{
		type = newType;
		prepared = new List<WWW>();
	}

	public virtual BaseWWWRequest Prepare(bool isSandbox, Dictionary<string, object> requestParams)
	{
		WWWForm wWWForm = new WWWForm();
		foreach (KeyValuePair<string, object> requestParam in requestParams)
		{
			string value = ((requestParam.Value != null) ? requestParam.Value.ToString() : "");
			wWWForm.AddField(requestParam.Key, value);
		}
		WWW item = new WWW(GetDomain(isSandbox) + GetMethod(), wWWForm);
		prepared.Add(item);
		return this;
	}

	public virtual BaseWWWRequest Prepare(bool isSandbox, string method, Dictionary<string, object> requestParams)
	{
		WWWForm wWWForm = new WWWForm();
		foreach (KeyValuePair<string, object> requestParam in requestParams)
		{
			string value = ((requestParam.Value != null) ? requestParam.Value.ToString() : "");
			wWWForm.AddField(requestParam.Key, value);
		}
		WWW item = new WWW(GetDomain(isSandbox) + method, wWWForm);
		prepared.Add(item);
		return this;
	}

	public virtual IEnumerator Execute()
	{
		foreach (WWW www in prepared)
		{
			yield return www;
			if (www.error == null)
			{
				JSONNode jSONNode = JSON.Parse(www.text);
				if (!jSONNode.AsObject.ContainsKey("error"))
				{
					ObjectsRecived(type, ParseResult(jSONNode));
				}
				else
				{
					Logger.Log("ErRoR");
				}
			}
			else
			{
				Logger.Log("ErRoR");
			}
		}
	}

	protected abstract object[] ParseResult(JSONNode text);

	protected abstract string GetMethod();

	private void OnObjectsRecieved(int type, object[] os)
	{
		if (ObjectsRecived != null)
		{
			ObjectsRecived(type, os);
		}
	}

	private string GetDomain(bool isSandbox)
	{
		if (!isSandbox)
		{
			return "https://secure.xsolla.com";
		}
		return "https://sandbox-secure.xsolla.com";
	}
}
