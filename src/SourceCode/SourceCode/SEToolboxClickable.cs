using UnityEngine;

public class SEToolboxClickable : SEClickable
{
	public enum ToolState
	{
		CLOSED,
		OPENING,
		OPENED,
		CLOSING
	}

	public UiScienceExperiment _MainUI;

	public bool _OpenEnabled = true;

	private ToolState mState = ToolState.CLOSING;

	private float mTimer;

	private Animation pToolbox
	{
		get
		{
			if (_MainUI != null && _MainUI._Manager != null && _MainUI._Manager._Toolbox != null)
			{
				return _MainUI._Manager._Toolbox.GetComponent<Animation>();
			}
			return null;
		}
	}

	public ToolState pState => mState;

	public override void ProcessMouseUp()
	{
		if (mState == ToolState.CLOSED && _OpenEnabled)
		{
			SetState(ToolState.OPENING);
		}
	}

	public override void Update()
	{
		base.Update();
		switch (mState)
		{
		case ToolState.OPENING:
			mTimer = Mathf.Max(0f, mTimer - Time.deltaTime);
			if (mTimer <= 0f && SetState(ToolState.OPENED))
			{
				base.ProcessMouseUp();
			}
			break;
		case ToolState.CLOSING:
			mTimer = Mathf.Max(0f, mTimer - Time.deltaTime);
			if (mTimer <= 0f)
			{
				SetState(ToolState.CLOSED);
			}
			break;
		}
	}

	public bool SetState(ToolState inState, bool inTarget = true)
	{
		if (mState == inState || pToolbox == null)
		{
			return false;
		}
		switch (inState)
		{
		case ToolState.CLOSED:
			pToolbox.CrossFade("CloseIdle");
			collider.enabled = true;
			break;
		case ToolState.OPENING:
			pToolbox.CrossFade("Open");
			mTimer = pToolbox["Open"].length;
			collider.enabled = false;
			break;
		case ToolState.OPENED:
			pToolbox.CrossFade("OpenIdle");
			collider.enabled = false;
			break;
		case ToolState.CLOSING:
			pToolbox.CrossFade("Close");
			mTimer = pToolbox["CloseIdle"].length;
			collider.enabled = false;
			break;
		}
		mState = inState;
		return true;
	}

	public void Close()
	{
		if (mState == ToolState.OPENING || mState == ToolState.OPENED)
		{
			SetState(ToolState.CLOSING);
		}
	}
}
