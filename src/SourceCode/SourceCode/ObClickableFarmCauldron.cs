using System.Collections.Generic;
using UnityEngine;

public class ObClickableFarmCauldron : ObClickableCreateInstance
{
	public List<Renderer> _SkipRenderers;

	private bool mMessageSent;

	private Vector3 mPreviousMousePosition = Vector3.zero;

	public override void OnActivate()
	{
		if (!(MyRoomsIntMain.pInstance != null) || !MyRoomsIntMain.pInstance.pIsBuildMode)
		{
			base.OnActivate();
		}
	}

	public override void Update()
	{
		base.Update();
		if (base.pMousePressed && !mMessageSent && mPreviousMousePosition != KAInput.mousePosition)
		{
			mMessageSent = true;
			if (_MessageObject != null)
			{
				_MessageObject.SendMessage("OnDragStart", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		mPreviousMousePosition = KAInput.mousePosition;
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
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if ((farmManager != null && farmManager.pBuilder.pDragObject != null && (GameObject)farmManager.pBuilder.pDragObject != base.gameObject) || KAUI.GetGlobalMouseOverItem() != null)
		{
			return;
		}
		base.OnMouseDown();
		if (base.pMousePressed)
		{
			mMessageSent = false;
			if ((bool)_MessageObject)
			{
				_MessageObject.SendMessage("OnPress", base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
			UnHighlight();
		}
	}

	public override void ProcessMouseUp()
	{
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (farmManager != null && farmManager.pBuilder.pDragObject != null && (GameObject)farmManager.pBuilder.pDragObject != base.gameObject)
		{
			return;
		}
		base.ProcessMouseUp();
		mMessageSent = false;
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

	public override void Highlight()
	{
		if (!(MyRoomsIntMain.pInstance == null))
		{
			ProcessHighlight();
		}
	}

	private void ProcessHighlight()
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
		FarmManager farmManager = MyRoomsIntMain.pInstance as FarmManager;
		if (farmManager != null && farmManager.pIsBuildMode && farmManager.pBuilder != null)
		{
			if (base.gameObject != farmManager.pBuilder.pSelectedObject)
			{
				ProcessUnHighlight();
			}
		}
		else
		{
			ProcessUnHighlight();
		}
	}

	private void ProcessUnHighlight()
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
