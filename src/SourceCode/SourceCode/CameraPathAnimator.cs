using UnityEngine;

public class CameraPathAnimator : MonoBehaviour
{
	public enum animationModes
	{
		once,
		loop,
		reverse,
		reverseLoop,
		pingPong,
		still
	}

	public enum orientationModes
	{
		custom,
		target,
		mouselook,
		followpath,
		reverseFollowpath,
		followTransform,
		twoDimentions,
		fixedOrientation,
		none
	}

	public delegate void AnimationStartedEventHandler();

	public delegate void AnimationPausedEventHandler();

	public delegate void AnimationStoppedEventHandler();

	public delegate void AnimationFinishedEventHandler();

	public delegate void AnimationLoopedEventHandler();

	public delegate void AnimationPingPongEventHandler();

	public delegate void AnimationPointReachedEventHandler();

	public delegate void AnimationCustomEventHandler(string eventName);

	public delegate void AnimationPointReachedWithNumberEventHandler(int pointNumber);

	public float minimumCameraSpeed = 0.01f;

	public Transform orientationTarget;

	[SerializeField]
	private CameraPath _cameraPath;

	public bool playOnStart = true;

	public Transform animationObject;

	private Camera animationObjectCamera;

	private bool _isCamera = true;

	private bool _playing;

	public animationModes animationMode;

	[SerializeField]
	private orientationModes _orientationMode;

	public bool smoothOrientationModeChanges;

	public float orientationModeLerpTime = 0.3f;

	private float _orientationModeLerpTimer;

	private orientationModes _previousOrientationMode;

	private float pingPongDirection = 1f;

	public Vector3 fixedOrientaion = Vector3.forward;

	public Vector3 fixedPosition;

	public bool normalised = true;

	[SerializeField]
	private bool _dynamicNormalisation;

	public float editorPercentage;

	[SerializeField]
	private float _pathTime = 10f;

	[SerializeField]
	private float _pathSpeed = 10f;

	private float _percentage;

	private float _lastPercentage;

	public float nearestOffset;

	private float _delayTime;

	public float startPercent;

	public bool animateFOV = true;

	public Vector3 targetModeUp = Vector3.up;

	public float sensitivity = 5f;

	public float minX = -90f;

	public float maxX = 90f;

	private bool _animateSceneObjectInEditor;

	public Vector3 animatedObjectStartPosition;

	public Quaternion animatedObjectStartRotation;

	public virtual float pathSpeed
	{
		get
		{
			return _pathSpeed;
		}
		set
		{
			if (_cameraPath.speedList.listEnabled)
			{
				Debug.LogWarning("Path Speed in Animator component is ignored and overridden by Camera Path speed points.");
			}
			_pathSpeed = Mathf.Max(value, minimumCameraSpeed);
		}
	}

	public virtual float animationTime
	{
		get
		{
			return _pathTime;
		}
		set
		{
			if (animationMode != animationModes.still)
			{
				Debug.LogWarning("Path time is ignored and overridden during animation when not in Animation Mode Still.");
			}
			_pathTime = Mathf.Max(value, 0f);
		}
	}

	public virtual float currentTime => _pathTime * _percentage;

	public virtual bool isPlaying => _playing;

	public virtual float percentage => _percentage;

	public virtual bool pingPongGoingForward => pingPongDirection == 1f;

	public virtual CameraPath cameraPath
	{
		get
		{
			if (!_cameraPath)
			{
				_cameraPath = GetComponent<CameraPath>();
			}
			return _cameraPath;
		}
	}

	public virtual bool dynamicNormalisation
	{
		get
		{
			return _dynamicNormalisation;
		}
		set
		{
			if (value)
			{
				_dynamicNormalisation = true;
				_cameraPath.normalised = false;
			}
			else
			{
				_dynamicNormalisation = false;
			}
		}
	}

	public virtual orientationModes orientationMode
	{
		get
		{
			return _orientationMode;
		}
		set
		{
			if (_orientationMode != value)
			{
				_orientationModeLerpTimer = 0f;
				_previousOrientationMode = _orientationMode;
				_orientationMode = value;
			}
		}
	}

	public virtual bool isCamera
	{
		get
		{
			if (animationObject == null)
			{
				_isCamera = false;
			}
			else
			{
				_isCamera = animationObjectCamera != null;
			}
			return _isCamera;
		}
	}

	public virtual bool animateSceneObjectInEditor
	{
		get
		{
			return _animateSceneObjectInEditor;
		}
		set
		{
			if (value != _animateSceneObjectInEditor)
			{
				_animateSceneObjectInEditor = value;
				if (animationObject != null && animationMode != animationModes.still)
				{
					if (_animateSceneObjectInEditor)
					{
						animatedObjectStartPosition = animationObject.transform.position;
						animatedObjectStartRotation = animationObject.transform.rotation;
					}
					else
					{
						animationObject.transform.position = animatedObjectStartPosition;
						animationObject.transform.rotation = animatedObjectStartRotation;
					}
				}
			}
			_animateSceneObjectInEditor = value;
		}
	}

	protected virtual bool isReversed
	{
		get
		{
			if (animationMode != animationModes.reverse && animationMode != animationModes.reverseLoop)
			{
				return pingPongDirection < 0f;
			}
			return true;
		}
	}

	public event AnimationStartedEventHandler AnimationStartedEvent;

	public event AnimationPausedEventHandler AnimationPausedEvent;

	public event AnimationStoppedEventHandler AnimationStoppedEvent;

	public event AnimationFinishedEventHandler AnimationFinishedEvent;

	public event AnimationLoopedEventHandler AnimationLoopedEvent;

	public event AnimationPingPongEventHandler AnimationPingPongEvent;

	public event AnimationPointReachedEventHandler AnimationPointReachedEvent;

	public event AnimationPointReachedWithNumberEventHandler AnimationPointReachedWithNumberEvent;

	public event AnimationCustomEventHandler AnimationCustomEvent;

	public virtual void Play()
	{
		_playing = true;
		if (!isReversed)
		{
			if (_percentage == 0f)
			{
				if (this.AnimationStartedEvent != null)
				{
					this.AnimationStartedEvent();
				}
				cameraPath.eventList.OnAnimationStart(0f);
			}
		}
		else if (_percentage == 1f)
		{
			if (this.AnimationStartedEvent != null)
			{
				this.AnimationStartedEvent();
			}
			cameraPath.eventList.OnAnimationStart(1f);
		}
		_lastPercentage = _percentage;
	}

	public virtual void Stop()
	{
		_playing = false;
		_percentage = 0f;
		if (this.AnimationStoppedEvent != null)
		{
			this.AnimationStoppedEvent();
		}
	}

	public virtual void Pause()
	{
		_playing = false;
		if (this.AnimationPausedEvent != null)
		{
			this.AnimationPausedEvent();
		}
	}

	public virtual void Seek(float value)
	{
		_percentage = Mathf.Clamp01(value);
		_lastPercentage = _percentage;
		UpdateAnimationTime(advance: false);
		UpdatePointReached();
		bool playing = _playing;
		_playing = true;
		UpdateAnimation();
		_playing = playing;
	}

	public virtual void Reverse()
	{
		switch (animationMode)
		{
		case animationModes.once:
			animationMode = animationModes.reverse;
			break;
		case animationModes.reverse:
			animationMode = animationModes.once;
			break;
		case animationModes.pingPong:
			pingPongDirection = ((pingPongDirection == -1f) ? 1 : (-1));
			break;
		case animationModes.loop:
			animationMode = animationModes.reverseLoop;
			break;
		case animationModes.reverseLoop:
			animationMode = animationModes.loop;
			break;
		}
	}

	public virtual Quaternion GetOrientation(orientationModes mode, float percent, bool ignoreNormalisation)
	{
		Quaternion result = Quaternion.identity;
		switch (mode)
		{
		case orientationModes.custom:
			result = cameraPath.GetPathRotation(percent, ignoreNormalisation);
			break;
		case orientationModes.target:
		{
			Vector3 pathPosition = cameraPath.GetPathPosition(percent);
			Vector3 forward = ((!(orientationTarget != null)) ? Vector3.forward : (orientationTarget.transform.position - pathPosition));
			result = Quaternion.LookRotation(forward, targetModeUp);
			break;
		}
		case orientationModes.followpath:
			result = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
			result *= Quaternion.Euler(base.transform.forward * (0f - cameraPath.GetPathTilt(percent)));
			break;
		case orientationModes.reverseFollowpath:
			result = Quaternion.LookRotation(-cameraPath.GetPathDirection(percent));
			result *= Quaternion.Euler(base.transform.forward * (0f - cameraPath.GetPathTilt(percent)));
			break;
		case orientationModes.mouselook:
			if (!Application.isPlaying)
			{
				result = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
				result *= Quaternion.Euler(base.transform.forward * (0f - cameraPath.GetPathTilt(percent)));
			}
			else
			{
				result = Quaternion.LookRotation(cameraPath.GetPathDirection(percent));
				result *= GetMouseLook();
			}
			break;
		case orientationModes.followTransform:
		{
			if (orientationTarget == null)
			{
				return Quaternion.identity;
			}
			float nearestPoint = cameraPath.GetNearestPoint(orientationTarget.position);
			nearestPoint = Mathf.Clamp01(nearestPoint + nearestOffset);
			Vector3 pathPosition = cameraPath.GetPathPosition(nearestPoint);
			Vector3 forward = orientationTarget.transform.position - pathPosition;
			result = Quaternion.LookRotation(forward);
			break;
		}
		case orientationModes.twoDimentions:
			result = Quaternion.LookRotation(Vector3.forward);
			break;
		case orientationModes.fixedOrientation:
			result = Quaternion.LookRotation(fixedOrientaion);
			break;
		case orientationModes.none:
			result = animationObject.rotation;
			break;
		}
		return result;
	}

	public virtual Quaternion GetAnimatedOrientation(float percent, bool ignoreNormalisation)
	{
		Quaternion quaternion = GetOrientation(_orientationMode, percent, ignoreNormalisation);
		if (smoothOrientationModeChanges && _orientationModeLerpTimer < orientationModeLerpTime)
		{
			Quaternion orientation = GetOrientation(_previousOrientationMode, percent, ignoreNormalisation);
			float t = _orientationModeLerpTimer / orientationModeLerpTime;
			float t2 = Mathf.SmoothStep(0f, 1f, t);
			quaternion = Quaternion.Slerp(orientation, quaternion, t2);
		}
		return quaternion * base.transform.rotation;
	}

	protected virtual void Awake()
	{
		if (animationObject == null)
		{
			_isCamera = false;
		}
		else
		{
			animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
			_isCamera = animationObjectCamera != null;
		}
		if (Camera.allCameras.Length == 0)
		{
			Debug.LogWarning("Warning: There are no cameras in the scene");
			_isCamera = false;
		}
		if (!isReversed)
		{
			_percentage = 0f + startPercent;
		}
		else
		{
			_percentage = 1f - startPercent;
		}
	}

	protected virtual void OnEnable()
	{
		cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
		cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
		if (animationObject != null)
		{
			animationObjectCamera = animationObject.GetComponentInChildren<Camera>();
		}
	}

	protected virtual void Start()
	{
		if (playOnStart)
		{
			Play();
		}
		if (Application.isPlaying && orientationTarget == null && (_orientationMode == orientationModes.followTransform || _orientationMode == orientationModes.target))
		{
			Debug.LogWarning("There has not been an orientation target specified in the Animation component of Camera Path.", base.transform);
		}
	}

	protected virtual void Update()
	{
		if (!isCamera)
		{
			if (_playing)
			{
				UpdateAnimation();
				UpdatePointReached();
				UpdateAnimationTime();
			}
			else if (_cameraPath.nextPath != null && _percentage >= 1f)
			{
				PlayNextAnimation();
			}
		}
	}

	protected virtual void LateUpdate()
	{
		if (isCamera)
		{
			if (_playing)
			{
				UpdateAnimation();
				UpdatePointReached();
				UpdateAnimationTime();
			}
			else if (_cameraPath.nextPath != null && _percentage >= 1f)
			{
				PlayNextAnimation();
			}
		}
	}

	protected virtual void OnDisable()
	{
		CleanUp();
	}

	protected virtual void OnDestroy()
	{
		CleanUp();
	}

	protected virtual void PlayNextAnimation()
	{
		if (_cameraPath.nextPath != null)
		{
			CameraPathAnimator component = _cameraPath.nextPath.GetComponent<CameraPathAnimator>();
			float value = (_cameraPath.interpolateNextPath ? (_percentage % 1f) : 0f);
			component.Seek(value);
			component.Play();
			Stop();
		}
	}

	protected virtual void UpdateAnimation()
	{
		if (animationObject == null)
		{
			Debug.LogError("There is no animation object specified in the Camera Path Animator component. Nothing to animate.\nYou can find this component in the main camera path game object called " + base.gameObject.name + ".");
			Stop();
		}
		else
		{
			if (!_playing)
			{
				return;
			}
			if (animationMode != animationModes.still)
			{
				if (cameraPath.speedList.listEnabled)
				{
					_pathTime = _cameraPath.pathLength / Mathf.Max(cameraPath.GetPathSpeed(_percentage), minimumCameraSpeed);
				}
				else
				{
					_pathTime = _cameraPath.pathLength / Mathf.Max(_pathSpeed * cameraPath.GetPathEase(_percentage), minimumCameraSpeed);
				}
				animationObject.position = cameraPath.GetPathPosition(_percentage);
			}
			if (_orientationMode != orientationModes.none)
			{
				animationObject.rotation = GetAnimatedOrientation(_percentage, ignoreNormalisation: false);
			}
			if (isCamera && _cameraPath.fovList.listEnabled)
			{
				if (!animationObjectCamera.orthographic)
				{
					animationObjectCamera.fieldOfView = _cameraPath.GetPathFOV(_percentage);
				}
				else
				{
					animationObjectCamera.orthographicSize = _cameraPath.GetPathOrthographicSize(_percentage);
				}
			}
			CheckEvents();
		}
	}

	protected virtual void UpdatePointReached()
	{
		if (_percentage == _lastPercentage)
		{
			return;
		}
		if (Mathf.Abs(percentage - _lastPercentage) > 0.999f)
		{
			_lastPercentage = percentage;
			return;
		}
		for (int i = 0; i < cameraPath.realNumberOfPoints; i++)
		{
			CameraPathControlPoint cameraPathControlPoint = cameraPath[i];
			if ((cameraPathControlPoint.percentage >= _lastPercentage && cameraPathControlPoint.percentage <= percentage) || (cameraPathControlPoint.percentage >= percentage && cameraPathControlPoint.percentage <= _lastPercentage))
			{
				if (this.AnimationPointReachedEvent != null)
				{
					this.AnimationPointReachedEvent();
				}
				if (this.AnimationPointReachedWithNumberEvent != null)
				{
					this.AnimationPointReachedWithNumberEvent(i);
				}
			}
		}
		_lastPercentage = percentage;
	}

	protected virtual void UpdateAnimationTime()
	{
		UpdateAnimationTime(advance: true);
	}

	protected virtual void UpdateAnimationTime(bool advance)
	{
		if (_orientationMode == orientationModes.followTransform)
		{
			return;
		}
		if (_delayTime > 0f)
		{
			_delayTime += 0f - Time.deltaTime;
			return;
		}
		if (advance)
		{
			switch (animationMode)
			{
			case animationModes.once:
				if (_percentage >= 1f)
				{
					_playing = false;
					if (this.AnimationFinishedEvent != null)
					{
						this.AnimationFinishedEvent();
					}
				}
				else
				{
					_percentage += Time.deltaTime * (1f / _pathTime);
				}
				break;
			case animationModes.loop:
				if (_percentage >= 1f)
				{
					_percentage = 0f;
					_lastPercentage = 0f;
					if (this.AnimationLoopedEvent != null)
					{
						this.AnimationLoopedEvent();
					}
				}
				_percentage += Time.deltaTime * (1f / _pathTime);
				break;
			case animationModes.reverseLoop:
				if (_percentage <= 0f)
				{
					_percentage = 1f;
					_lastPercentage = 1f;
					if (this.AnimationLoopedEvent != null)
					{
						this.AnimationLoopedEvent();
					}
				}
				_percentage += (0f - Time.deltaTime) * (1f / _pathTime);
				break;
			case animationModes.reverse:
				if (_percentage <= 0f)
				{
					_percentage = 0f;
					_playing = false;
					if (this.AnimationFinishedEvent != null)
					{
						this.AnimationFinishedEvent();
					}
				}
				else
				{
					_percentage += (0f - Time.deltaTime) * (1f / _pathTime);
				}
				break;
			case animationModes.pingPong:
			{
				float num = Time.deltaTime * (1f / _pathTime);
				_percentage += num * pingPongDirection;
				if (_percentage >= 1f)
				{
					_percentage = 1f - num;
					_lastPercentage = 1f;
					pingPongDirection = -1f;
					if (this.AnimationPingPongEvent != null)
					{
						this.AnimationPingPongEvent();
					}
				}
				if (_percentage <= 0f)
				{
					_percentage = num;
					_lastPercentage = 0f;
					pingPongDirection = 1f;
					if (this.AnimationPingPongEvent != null)
					{
						this.AnimationPingPongEvent();
					}
				}
				break;
			}
			case animationModes.still:
				if (_percentage >= 1f)
				{
					_playing = false;
					if (this.AnimationFinishedEvent != null)
					{
						this.AnimationFinishedEvent();
					}
				}
				else
				{
					_percentage += Time.deltaTime * (1f / _pathTime);
				}
				break;
			}
			if (smoothOrientationModeChanges)
			{
				if (_orientationModeLerpTimer < orientationModeLerpTime)
				{
					_orientationModeLerpTimer += Time.deltaTime;
				}
				else
				{
					_orientationModeLerpTimer = orientationModeLerpTime;
				}
			}
		}
		_percentage = Mathf.Clamp01(_percentage);
	}

	protected virtual Quaternion GetMouseLook()
	{
		if (animationObject == null)
		{
			return Quaternion.identity;
		}
		float num = (float)Screen.width / 2f;
		float num2 = (float)Screen.height / 2f;
		float y = (Input.mousePosition.x - num) / (float)Screen.width * 180f;
		return Quaternion.Euler(new Vector3(Mathf.Clamp(((float)Screen.height - Input.mousePosition.y - num2) / (float)Screen.height * 180f, minX, maxX), y, 0f));
	}

	protected virtual void CheckEvents()
	{
		cameraPath.CheckEvents(_percentage);
	}

	protected virtual void CleanUp()
	{
		cameraPath.eventList.CameraPathEventPoint += OnCustomEvent;
		cameraPath.delayList.CameraPathDelayEvent += OnDelayEvent;
	}

	protected virtual void OnDelayEvent(float time)
	{
		if (time > 0f)
		{
			_delayTime = time;
		}
		else
		{
			Pause();
		}
	}

	protected virtual void OnCustomEvent(string eventName)
	{
		if (this.AnimationCustomEvent != null)
		{
			this.AnimationCustomEvent(eventName);
		}
	}
}
