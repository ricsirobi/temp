using System.Collections.Generic;
using UnityEngine;

public class UiAnimStyle : MonoBehaviour
{
	public enum AnimStyle
	{
		SLIDE,
		CASCADE_FALL,
		FALL_THOUGH,
		PEEK_IN
	}

	public float _Duration = 1f;

	protected float mTime;

	protected int mIndex;

	protected bool mDone;

	protected CompactUI mCompactUI;

	protected KAWidget[] pChildList => mCompactUI._ChildList;

	public List<Vector3> pChildInitialPos => mCompactUI.pChildInitialPos;

	public List<Vector3> pChildTargetDirectionList => mCompactUI.pChildTargetDirectionList;

	protected virtual void Awake()
	{
		mCompactUI = base.gameObject.GetComponent<CompactUI>();
		if (mCompactUI == null)
		{
			base.enabled = false;
			Debug.LogError("CompactUI script not found under : " + base.name);
		}
	}

	protected virtual void Start()
	{
		if (pChildList.Length != 0)
		{
			mTime = _Duration / (float)pChildList.Length;
		}
		for (int i = 0; i < pChildList.Length; i++)
		{
			pChildList[i].OnMoveToDone += OnWidgetMoveToDone;
		}
		base.enabled = false;
	}

	protected virtual void Update()
	{
	}

	protected virtual void OnWidgetMoveToDone(KAWidget inWidget)
	{
	}

	protected virtual void OnDestroy()
	{
		KAWidget[] array = pChildList;
		foreach (KAWidget kAWidget in array)
		{
			if (kAWidget != null)
			{
				kAWidget.OnMoveToDone -= OnWidgetMoveToDone;
			}
		}
	}

	public virtual void StartEffect()
	{
		base.enabled = true;
		mDone = false;
	}

	public virtual void EndEffect()
	{
		OnEffectDone();
	}

	protected virtual void OnEffectDone()
	{
		base.enabled = false;
		mDone = true;
		if (mCompactUI != null)
		{
			mCompactUI.OnEffectDone();
		}
	}
}
