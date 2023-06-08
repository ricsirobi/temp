using UnityEngine;

public class ObClickable : KAMonoBase
{
	public delegate void ActivatedEventHandler(GameObject go);

	public delegate void ObjectClickedDelegate(GameObject go);

	public bool _Draw;

	public bool _Active = true;

	public bool _UseGlobalActive = true;

	public bool _AvatarWalkTo = true;

	public string _LoadLevel = "";

	public string _StartMarker = "";

	public GameObject _ActivateObject;

	public GameObject _MessageObject;

	public const string _HighlightShader = "KAHighlight";

	public Material _HighlightMaterial;

	public string _MouseOverAnim = "";

	public string _MouseExitAnim = "";

	public string _RollOverCursorName = "";

	public float _Range;

	public float _RangeAngle = 20f;

	public Vector3 _RangeOffset = new Vector3(0f, 0f, 0f);

	public Vector3 _Offset = new Vector3(0f, 0f, 0f);

	public SnSound _RolloverSound;

	public SnRandomSound _RolloverRandomSound;

	public bool _RolloverStopOnMouseExit = true;

	public float _RolloverReplayDelay;

	public float _RolloverTime = 1f;

	public SnSound _ClickSound;

	public bool _FirstOnly;

	public bool _StopVOPoolOnClick = true;

	protected bool mMouseOver;

	private bool mMousePressed;

	public ObjectClickedDelegate _ObjectClickedCallback;

	protected Shader[][] mShaders;

	private static bool mGlobalActive = true;

	private float mRolloverTime = -1f;

	private SnChannel mRolloverSoundChannel;

	private AudioClip mRolloverAudioClip;

	private float mRolloverReplayDelayTimer;

	protected bool mHighlighted;

	private static GameObject mMouseOverObject = null;

	private float mLastUpdateTime;

	public bool pMouseOver => mMouseOver;

	public bool pMousePressed => mMousePressed;

	public static GameObject pMouseOverObject
	{
		get
		{
			return mMouseOverObject;
		}
		set
		{
			mMouseOverObject = value;
		}
	}

	public static bool pGlobalActive
	{
		get
		{
			return mGlobalActive;
		}
		set
		{
			mGlobalActive = value;
		}
	}

	private static event ActivatedEventHandler OnActivated;

	public virtual bool IsActive()
	{
		if (!UICursorManager.IsCursorHidden() && !(KAUI._GlobalExclusiveUI != null))
		{
			if ((!_UseGlobalActive || pGlobalActive) && _Active)
			{
				if (UIDragObject.pMouseOverObject != null)
				{
					return !UIDragObject.pMouseOverObject.activeInHierarchy;
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public virtual void ProcessMouseUp()
	{
		if (_ObjectClickedCallback != null)
		{
			_ObjectClickedCallback(base.gameObject);
		}
		if ((bool)_MessageObject)
		{
			_MessageObject.SendMessage("OnClick", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		if (_AvatarWalkTo && AvAvatar.pObject != null)
		{
			AvAvatar.pObject.SendMessage("OnClick", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
		if (_ClickSound != null && _ClickSound._AudioClip != null)
		{
			_ClickSound.Play();
		}
		if (WithinRange())
		{
			if (_StopVOPoolOnClick)
			{
				SnChannel.StopPool("VO_Pool");
			}
			SendMessage("OnActivate", null, SendMessageOptions.DontRequireReceiver);
			KAInput.ResetInputAxes();
		}
	}

	public virtual void OnMouseUp()
	{
		if (!UtMobileUtilities.IsMultiTouch())
		{
			mMousePressed = false;
			if (mMouseOver && IsActive() && KAUI.GetGlobalMouseOverItem() == null)
			{
				ProcessMouseUp();
			}
		}
	}

	public virtual void OnMouseDrag()
	{
		if (!UtMobileUtilities.IsMultiTouch() && mMousePressed && mMouseOver && IsActive() && (bool)_MessageObject)
		{
			_MessageObject.SendMessage("OnDrag", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void OnMouseDown()
	{
		if (!UtMobileUtilities.IsMultiTouch())
		{
			if (KAInput.pInstance.IsTouchInput())
			{
				OnMouseEnter();
			}
			if (mMouseOver && IsActive())
			{
				mMousePressed = true;
			}
		}
	}

	public virtual void OnActivate()
	{
		if (ObClickable.OnActivated != null)
		{
			ObClickable.OnActivated(base.gameObject);
		}
		if (mRolloverSoundChannel != null)
		{
			if (mRolloverSoundChannel.pAudioSource.clip == mRolloverAudioClip)
			{
				mRolloverSoundChannel.Stop();
			}
			mRolloverSoundChannel = null;
		}
		mRolloverTime = -1f;
		if (_LoadLevel.Length > 0)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			AvAvatar.SetActive(inActive: false);
			if (_StartMarker != "")
			{
				AvAvatar.pStartLocation = _StartMarker;
			}
			RsResourceManager.LoadLevel(_LoadLevel);
		}
		else if (_ActivateObject != null)
		{
			_ActivateObject.SetActive(value: true);
		}
	}

	public virtual void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float num = 0.25f;
		if (realtimeSinceStartup - mLastUpdateTime < num)
		{
			return;
		}
		mLastUpdateTime = realtimeSinceStartup + Random.value * 0.1f * num;
		if (mMouseOver && (!IsActive() || (!_AvatarWalkTo && !WithinRange())))
		{
			mMouseOver = false;
			mMouseOverObject = null;
			ResetObject();
			mRolloverTime = -1f;
		}
		if (mRolloverTime > 0f)
		{
			mRolloverTime -= Time.deltaTime;
			if (mRolloverTime < 0f)
			{
				if (!_RolloverStopOnMouseExit && mRolloverSoundChannel != null && mRolloverSoundChannel.pAudioSource.clip == mRolloverAudioClip)
				{
					mRolloverSoundChannel.Stop();
				}
				if (_RolloverSound != null && _RolloverSound._AudioClip != null)
				{
					mRolloverSoundChannel = _RolloverSound.Play();
				}
				else
				{
					mRolloverSoundChannel = _RolloverRandomSound.Play();
				}
				mRolloverTime = -1f;
				if (mRolloverSoundChannel != null)
				{
					mRolloverAudioClip = mRolloverSoundChannel.pAudioSource.clip;
				}
				mRolloverReplayDelayTimer = _RolloverReplayDelay;
			}
		}
		if (mRolloverReplayDelayTimer > 0f)
		{
			mRolloverReplayDelayTimer -= Time.deltaTime;
		}
	}

	public virtual bool WithinRange()
	{
		if (AvAvatar.pObject == null)
		{
			return false;
		}
		if (_Range != 0f && (AvAvatar.position - (base.transform.position + base.transform.TransformDirection(_RangeOffset))).magnitude > _Range)
		{
			return false;
		}
		return true;
	}

	public void GeometryUpdated()
	{
		if (!mMouseOver)
		{
			mShaders = null;
		}
	}

	public virtual void Highlight()
	{
		if (!(_HighlightMaterial != null) || mHighlighted)
		{
			return;
		}
		mHighlighted = true;
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		if (mShaders == null || mShaders.Length != componentsInChildren.Length)
		{
			mShaders = new Shader[componentsInChildren.Length][];
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (_FirstOnly && i != 0)
			{
				continue;
			}
			if (mShaders[i] == null || mShaders[i].Length != renderer.materials.Length)
			{
				mShaders[i] = new Shader[renderer.materials.Length];
			}
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				mShaders[i][j] = renderer.materials[j].shader;
				renderer.materials[j].shader = Shader.Find("KAHighlight");
				if (_HighlightMaterial.HasProperty("_RimColor"))
				{
					renderer.materials[j].SetColor("_RimColor", _HighlightMaterial.GetColor("_RimColor"));
				}
				if (_HighlightMaterial.HasProperty("_RimPower"))
				{
					renderer.materials[j].SetFloat("_RimPower", _HighlightMaterial.GetFloat("_RimPower"));
				}
			}
		}
	}

	public virtual void ProcessMouseEnter()
	{
		mMouseOver = true;
		mMouseOverObject = base.gameObject;
		if (base.animation != null && _MouseOverAnim.Length > 0 && base.animation[_MouseOverAnim] != null && (base.animation.IsPlaying(_MouseExitAnim) || !base.animation.isPlaying))
		{
			base.animation.wrapMode = WrapMode.Loop;
			base.animation.CrossFade(_MouseOverAnim, 0.2f);
		}
		if (_RollOverCursorName.Length > 0)
		{
			UICursorManager.SetCursor(_RollOverCursorName, showHideSystemCursor: true);
		}
		if (UICursorManager.MouseMoved())
		{
			UICursorManager.SetHoverAutoHide(t: false);
		}
		if ((_RolloverSound != null && _RolloverSound._AudioClip != null) || (_RolloverRandomSound != null && _RolloverRandomSound._ClipList != null && _RolloverRandomSound._ClipList.Length != 0))
		{
			mRolloverTime = Mathf.Max(_RolloverTime, mRolloverReplayDelayTimer);
		}
		Highlight();
	}

	public virtual void OnMouseEnter()
	{
		if (!UtMobileUtilities.IsMultiTouch() && Application.isPlaying && base.enabled && (KAInput.pInstance.IsTouchInput() || !KAInput.GetMouseButton(0)) && IsActive() && !mMouseOver && (!(KAUI.GetGlobalMouseOverItem() != null) || (KAInput.pInstance.IsTouchInput() && !KAInput.GetMouseButton(0))) && (_AvatarWalkTo || WithinRange()))
		{
			ProcessMouseEnter();
		}
	}

	public virtual void OnMouseOver()
	{
		if (!UtMobileUtilities.IsMultiTouch())
		{
			OnMouseEnter();
		}
	}

	public virtual void ProcessMouseExit()
	{
		mMouseOver = false;
		mMouseOverObject = null;
		ResetObject();
	}

	public virtual void OnMouseExit()
	{
		if (!UtMobileUtilities.IsMultiTouch() && Application.isPlaying && base.enabled && IsActive() && mMouseOver)
		{
			ProcessMouseExit();
		}
	}

	public virtual void UnHighlight()
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
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (_FirstOnly && i != 0)
			{
				continue;
			}
			for (int j = 0; j < renderer.materials.Length; j++)
			{
				if (i < mShaders.Length && j < mShaders[i].Length)
				{
					renderer.materials[j].shader = mShaders[i][j];
				}
			}
		}
	}

	public virtual void ResetObject()
	{
		if (base.animation != null && _MouseExitAnim.Length > 0 && base.animation[_MouseExitAnim] != null && (base.animation.IsPlaying(_MouseOverAnim) || !base.animation.isPlaying))
		{
			base.animation.wrapMode = WrapMode.ClampForever;
			base.animation.CrossFade(_MouseExitAnim, 0.2f);
		}
		UICursorManager.SetHoverAutoHide(t: true);
		if (_RollOverCursorName.Length > 0)
		{
			KAUICursorManager.SetDefaultCursor();
		}
		if (_RolloverStopOnMouseExit && mRolloverSoundChannel != null)
		{
			if (mRolloverSoundChannel.pAudioSource.clip == mRolloverAudioClip)
			{
				mRolloverSoundChannel.Stop();
			}
			mRolloverSoundChannel = null;
		}
		mRolloverTime = -1f;
		UnHighlight();
	}

	private void OnDrawGizmos()
	{
		if (_Draw)
		{
			if (_Range != 0f)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(base.transform.position + base.transform.TransformDirection(_RangeOffset), _Range);
			}
			if (_Offset.magnitude != 0f)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireCube(base.transform.position + base.transform.TransformDirection(_Offset), new Vector3(0.2f, 0.2f, 0.2f));
			}
		}
	}

	public virtual Component[] GetRenderers()
	{
		return GetComponentsInChildren(typeof(Renderer));
	}

	public static void AddActivatedEventHandler(ActivatedEventHandler objectclicked)
	{
		OnActivated += objectclicked;
	}

	public static void RemoveActivatedEventHandler(ActivatedEventHandler objectclicked)
	{
		OnActivated -= objectclicked;
	}
}
