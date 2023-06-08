using System.Collections.Generic;
using JSGames;
using JSGames.UI;
using UnityEngine;

public class MyRoomItemClickable : ObClickable
{
	protected bool messageSent;

	protected bool mDisableMouseOverNotInBuildMode;

	protected string mRollOverCursorName = "";

	protected Vector3 mPreviousMousePosition = Vector3.zero;

	public Renderer[] _HighlightRenderers;

	public List<Renderer> _SkipRenderers;

	public override Component[] GetRenderers()
	{
		return _HighlightRenderers;
	}

	public virtual void Start()
	{
		mDisableMouseOverNotInBuildMode = true;
		if (_HighlightMaterial == null && MyRoomsIntMain.pInstance != null)
		{
			_HighlightMaterial = MyRoomsIntMain.pInstance._HighlightMaterial;
		}
		mRollOverCursorName = _RollOverCursorName;
		Highlight();
	}

	public override void Update()
	{
		base.Update();
		if (base.pMousePressed && !messageSent && mPreviousMousePosition != KAInput.mousePosition)
		{
			messageSent = true;
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnDragStart", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		mPreviousMousePosition = KAInput.mousePosition;
	}

	public override void ProcessMouseEnter()
	{
		if (mDisableMouseOverNotInBuildMode)
		{
			_RollOverCursorName = (MyRoomsIntMain.pInstance.pIsBuildMode ? mRollOverCursorName : "");
		}
		base.ProcessMouseEnter();
	}

	public override void OnMouseExit()
	{
		if (!mMouseOver || !base.pMousePressed)
		{
			base.OnMouseExit();
		}
	}

	public override void OnMouseDown()
	{
		if (Singleton<UIManager>.pInstance.GetGlobalMouseOverItem(0) != null)
		{
			return;
		}
		base.OnMouseDown();
		if (base.pMousePressed)
		{
			messageSent = false;
			if ((bool)_MessageObject)
			{
				_MessageObject.SendMessage("OnPress", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			UnHighlight();
		}
	}

	public override void ProcessMouseUp()
	{
		base.ProcessMouseUp();
		messageSent = false;
		if ((bool)_MessageObject)
		{
			_MessageObject.SendMessage("OnRelease", null, SendMessageOptions.DontRequireReceiver);
		}
		if ((!(ObContextSensitive.pExclusiveUI != null) || !(ObContextSensitive.pExclusiveUI != null) || !(ObContextSensitive.pExclusiveUI.pContextSensitiveObj.gameObject == base.gameObject)) && !WithinRange())
		{
			if (_StopVOPoolOnClick)
			{
				SnChannel.StopPool("VO_Pool");
			}
			KAInput.ResetInputAxes();
			SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override bool WithinRange()
	{
		if (AvAvatar.pObject == null)
		{
			return false;
		}
		if ((AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_RangeOffset))).magnitude > _Range)
		{
			return false;
		}
		return true;
	}

	public override void Highlight()
	{
		if (!(MyRoomsIntMain.pInstance == null) && (MyRoomsIntMain.pInstance.pIsBuildMode || !mDisableMouseOverNotInBuildMode))
		{
			ProcessHighlight();
		}
	}

	protected void ProcessHighlight()
	{
		if (!(_HighlightMaterial != null) || mHighlighted)
		{
			return;
		}
		mHighlighted = true;
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		List<Renderer> list = new List<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer item in array)
		{
			if (_SkipRenderers == null || !_SkipRenderers.Contains(item))
			{
				list.Add(item);
			}
		}
		if (mShaders == null || mShaders.Length != list.Count)
		{
			mShaders = new Shader[list.Count][];
		}
		for (int j = 0; j < list.Count; j++)
		{
			Renderer renderer = list[j];
			if (_FirstOnly && j != 0)
			{
				continue;
			}
			if (mShaders[j] == null || mShaders[j].Length != renderer.materials.Length)
			{
				mShaders[j] = new Shader[renderer.materials.Length];
			}
			for (int k = 0; k < renderer.materials.Length; k++)
			{
				mShaders[j][k] = renderer.materials[k].shader;
				renderer.materials[k].shader = Shader.Find("KAHighlight");
				if (_HighlightMaterial.HasProperty("_RimColor"))
				{
					renderer.materials[k].SetColor("_RimColor", _HighlightMaterial.GetColor("_RimColor"));
				}
				if (_HighlightMaterial.HasProperty("_RimPower"))
				{
					renderer.materials[k].SetFloat("_RimPower", _HighlightMaterial.GetFloat("_RimPower"));
				}
			}
		}
	}

	public override void UnHighlight()
	{
		if (MyRoomsIntMain.pInstance != null && MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			if (base.gameObject != MyRoomsIntMain.pInstance._UiMyRoomsInt._MyRoomBuilder.pSelectedObject)
			{
				ProcessUnHighlight();
			}
		}
		else
		{
			ProcessUnHighlight();
		}
	}

	protected void ProcessUnHighlight()
	{
		if (!mHighlighted)
		{
			return;
		}
		mHighlighted = false;
		if (!(_HighlightMaterial != null) || mShaders == null)
		{
			return;
		}
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		List<Renderer> list = new List<Renderer>();
		Renderer[] array = componentsInChildren;
		foreach (Renderer item in array)
		{
			if (_SkipRenderers == null || !_SkipRenderers.Contains(item))
			{
				list.Add(item);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			Renderer renderer = list[j];
			if (_FirstOnly && j != 0)
			{
				continue;
			}
			for (int k = 0; k < renderer.materials.Length; k++)
			{
				if (j < mShaders.Length && k < mShaders[j].Length)
				{
					renderer.materials[k].shader = mShaders[j][k];
				}
			}
		}
	}
}
