using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class KAInput : KAMonoBase
{
	private static KAInput mInstance;

	private static int mResetInputAxesFrameCount;

	public KAInputMode pInputMode;

	private List<GameInput> mRegInputs = new List<GameInput>();

	private List<UiJoystick> mJoystickList = new List<UiJoystick>();

	private List<JoyStickPos> mJoystickDownloadIds = new List<JoyStickPos>();

	private bool mAreInputsHidden = true;

	private bool mHideJoystickOnCreation = true;

	private KAInputState mState;

	private GameInput mLastAddedInput;

	private bool mLevelLoading;

	public Vector3 pPrevMousePos = new Vector3(0f, 0f, 0f);

	public bool pMouseMoved;

	public bool pAreInputsHidden => mAreInputsHidden;

	public bool pIsReady => mState == KAInputState.READY;

	public static KAInput pInstance
	{
		get
		{
			if (!Application.isPlaying)
			{
				return null;
			}
			return mInstance;
		}
	}

	public static bool pHideJoystickOnCreation
	{
		get
		{
			return pInstance.mHideJoystickOnCreation;
		}
		set
		{
			pInstance.mHideJoystickOnCreation = value;
		}
	}

	public static bool anyKey => Input.anyKey;

	public static string inputString => Input.inputString;

	public static string compositionString => Input.compositionString;

	public static Vector2 compositionCursorPos
	{
		get
		{
			return Input.compositionCursorPos;
		}
		set
		{
			Input.compositionCursorPos = value;
		}
	}

	public static Vector3 acceleration => Input.acceleration;

	public static IMECompositionMode imeCompositionMode
	{
		get
		{
			return Input.imeCompositionMode;
		}
		set
		{
			Input.imeCompositionMode = value;
		}
	}

	public static int touchCount => Input.touchCount;

	public static Touch[] touches => Input.touches;

	public static Vector3 mousePosition => Input.mousePosition;

	public static DeviceOrientation deviceOrientation => Input.deviceOrientation;

	public static bool multiTouchEnabled
	{
		get
		{
			return Input.multiTouchEnabled;
		}
		set
		{
			Input.multiTouchEnabled = value;
		}
	}

	public bool IsPointerValid(PointerEventData eventData)
	{
		if (eventData == null || eventData.button != 0)
		{
			return false;
		}
		return true;
	}

	public void ShowInputs(bool inShow)
	{
		mAreInputsHidden = !inShow;
		if (UtPlatform.IsMobile() || UtPlatform.IsWSA())
		{
			foreach (UiJoystick mJoystick in mJoystickList)
			{
				mJoystick.SetVisibility(inShow);
			}
		}
		foreach (GameInput mRegInput in mRegInputs)
		{
			mRegInput.ShowInput(inShow);
		}
	}

	public static void Init()
	{
		if (mInstance == null)
		{
			GameObject obj = new GameObject("InputManager");
			obj.AddComponent<KAInput>();
			UnityEngine.Object.DontDestroyOnLoad(obj);
		}
	}

	public static void LoadInput()
	{
		if (!mInstance.pIsReady)
		{
			LoadInputManager();
		}
	}

	public static void LoadInputManager()
	{
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfInputManager");
		if (gameObject != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
			if (gameObject2 != null)
			{
				gameObject2.name = "PfInputManager";
			}
		}
	}

	protected virtual void Awake()
	{
		if (mInstance == null)
		{
			mState = KAInputState.INIT;
			mInstance = this;
			pInputMode = ((!UtPlatform.IsMobile()) ? KAInputMode.MOUSE : KAInputMode.TOUCH);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void Update()
	{
		if (RsResourceManager.pLevelLoading)
		{
			mLevelLoading = true;
		}
		if (mLevelLoading && !RsResourceManager.pLevelLoading)
		{
			mLevelLoading = false;
			ValidateInputs();
		}
		if (!mLevelLoading)
		{
			for (int i = 0; i < mRegInputs.Count; i++)
			{
				mRegInputs[i].Update();
			}
			DetermineInputMode();
		}
	}

	public void LateUpdate()
	{
		if (mLevelLoading)
		{
			return;
		}
		int count = mRegInputs.Count;
		for (int i = 0; i < count; i++)
		{
			mRegInputs[i].LateUpdate();
		}
		if (mJoystickList.Count <= 0 || mJoystickDownloadIds.Count != 0)
		{
			return;
		}
		int num = 0;
		for (int j = 0; j < count; j++)
		{
			GameInput gameInput = mRegInputs[j];
			int count2 = gameInput.pModifiers.Count;
			for (int k = 0; k < count2; k++)
			{
				InputInfo inputInfo = gameInput.pModifiers[k];
				if (inputInfo._Type == InputType.JOYSTICK && num < mJoystickList.Count && mJoystickList[num] == null)
				{
					mJoystickList.RemoveAt(num);
					mJoystickDownloadIds.Remove(inputInfo._Joystick);
					CreateJoyStick(inputInfo._Joystick, inputInfo._JoystickPath, inputInfo._JoystickPrefabName);
					num++;
				}
			}
		}
	}

	public static int CreateInputType(params InputType[] inArgs)
	{
		int num = 0;
		foreach (InputType inputType in inArgs)
		{
			num |= (int)inputType;
		}
		return num;
	}

	public static int CreateAxisFromEnum(params Axis[] inArgs)
	{
		int num = 0;
		foreach (Axis axis in inArgs)
		{
			num |= (int)axis;
		}
		return num;
	}

	public static int CreateTouchIdFromEnum(params TouchID[] inArgs)
	{
		int num = 0;
		foreach (TouchID touchID in inArgs)
		{
			num |= (int)touchID;
		}
		return num;
	}

	protected GameInput GetInputAtIdx(int inIdx)
	{
		GameInput result = null;
		foreach (GameInput mRegInput in mRegInputs)
		{
			if (mRegInput._Idx == inIdx)
			{
				result = mRegInput;
				break;
			}
		}
		return result;
	}

	public static bool Remove(int inIdx)
	{
		if (pInstance == null)
		{
			return false;
		}
		GameInput inputAtIdx = pInstance.GetInputAtIdx(inIdx);
		if (inputAtIdx != null)
		{
			pInstance.mRegInputs.Remove(inputAtIdx);
		}
		return inputAtIdx != null;
	}

	public static void ClearAll()
	{
		if (pInstance == null)
		{
			return;
		}
		foreach (GameInput mRegInput in pInstance.mRegInputs)
		{
			mRegInput.Clear();
		}
		pInstance.mRegInputs.Clear();
	}

	public static void AddScenes(SceneGroup _scenes)
	{
		if (!(pInstance == null))
		{
			pInstance.mLastAddedInput.AddScenes(_scenes);
		}
	}

	public static void AddPlatforms(PlatformGroup inPlatforms)
	{
		if (!(pInstance == null))
		{
			pInstance.mLastAddedInput.AddPlatforms(inPlatforms);
		}
	}

	public static void AddInput(string inStrId, float inDefaultVal)
	{
		if (!(pInstance == null))
		{
			GameInput inputForString = pInstance.GetInputForString(inStrId);
			if (inputForString != null)
			{
				inputForString.Clear();
				pInstance.mRegInputs.Remove(inputForString);
				UtDebug.LogWarning("@@@ Input " + inStrId + " is already defined! Clearing old one. ");
			}
			pInstance.mLastAddedInput = new GameInput(inStrId, inDefaultVal);
			pInstance.mRegInputs.Add(pInstance.mLastAddedInput);
		}
	}

	public static void AddModifier(float inMinVal, float inMaxVal, InputType inType, KeyCode inKeyCode, int inAxis, int inTouchIds, float inStep = 0f)
	{
		if (!(pInstance == null))
		{
			if (pInstance.mLastAddedInput == null)
			{
				Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			}
			else
			{
				pInstance.mLastAddedInput.AddModifier(inMinVal, inMaxVal, inType, inKeyCode, inAxis, inTouchIds, inStep);
			}
		}
	}

	public static void AddModifier(float inMinVal, float inMaxVal, string inKeyName, int inAxis, float inStep = 0f)
	{
		if (!(pInstance == null))
		{
			if (pInstance.mLastAddedInput == null)
			{
				Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			}
			else
			{
				pInstance.mLastAddedInput.AddModifier(inMinVal, inMaxVal, InputType.KEYBOARD, inKeyName, inAxis, inStep);
			}
		}
	}

	public static void AddModifier(float inMinVal, float inMaxVal, int inAxis, JoyStickPos inJoyStickPos, float inStep, string inAssetPath, string inAssetName)
	{
		if (pInstance == null || (!UtPlatform.IsMobile() && !UtPlatform.IsWSA() && !Application.isEditor))
		{
			return;
		}
		if (pInstance.mLastAddedInput == null)
		{
			Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			return;
		}
		if (!pInstance.IsJoystickPresent(inJoyStickPos))
		{
			pInstance.CreateJoyStick(inJoyStickPos, inAssetPath, inAssetName);
		}
		pInstance.mLastAddedInput.AddModifier(inJoyStickPos, inMinVal, inMaxVal, inAxis, inStep, inAssetPath, inAssetName);
	}

	public static void AddModifier(float inMinVal, float inMaxVal, KAUIPadButtons inUI, string inButtonName, float inStep = 0f)
	{
		if (!(pInstance == null))
		{
			if (pInstance.mLastAddedInput == null)
			{
				Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			}
			else
			{
				pInstance.mLastAddedInput.AddModifier(inMinVal, inMaxVal, inUI, inButtonName, inStep);
			}
		}
	}

	public static void AddModifier(float inMinVal, float inMaxVal, string inUIName, string inButtonName, float inStep)
	{
		if (!(pInstance == null))
		{
			if (pInstance.mLastAddedInput == null)
			{
				Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			}
			else
			{
				pInstance.mLastAddedInput.AddModifier(inMinVal, inMaxVal, inUIName, inButtonName, inStep);
			}
		}
	}

	public static void AddModifier(string inName, float inMinVal, float inMaxVal, InputType inType, int inAxis, float inStep = 0f)
	{
		if (!(pInstance == null))
		{
			if (pInstance.mLastAddedInput == null)
			{
				Debug.LogError("===================== Assert mLastAddedInput not good ====================");
			}
			else
			{
				pInstance.mLastAddedInput.AddModifier(inMinVal, inMaxVal, inType, inAxis, inName, inStep);
			}
		}
	}

	public bool GetInputTypeState(string inInputName, InputType inType)
	{
		return GetInputForString(inInputName)?.GetInputTypeState(inType) ?? false;
	}

	public static UiJoystick GetJoyStick(JoyStickPos inPos)
	{
		if (pInstance != null)
		{
			int count = pInstance.mJoystickList.Count;
			for (int i = 0; i < count; i++)
			{
				UiJoystick uiJoystick = pInstance.mJoystickList[i];
				if (uiJoystick != null && uiJoystick.pPos == inPos)
				{
					return uiJoystick;
				}
			}
		}
		return null;
	}

	public static void ShowJoystick(JoyStickPos inPos, bool inShow)
	{
		if (!(pInstance == null))
		{
			UiJoystick joyStick = GetJoyStick(inPos);
			if (joyStick != null)
			{
				joyStick.SetVisibility(inShow);
			}
		}
	}

	public static Vector2 GetJoyStickVal(JoyStickPos inPos)
	{
		if (pInstance == null)
		{
			return Vector2.zero;
		}
		Vector2 result = Vector2.zero;
		UiJoystick joyStick = GetJoyStick(inPos);
		if (joyStick != null)
		{
			result = joyStick._Position;
		}
		return result;
	}

	protected bool IsJoystickPresent(JoyStickPos inPos)
	{
		bool result = false;
		foreach (UiJoystick mJoystick in mJoystickList)
		{
			if (mJoystick != null && mJoystick.pPos == inPos)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void CreateJoyStick(JoyStickPos inPos, string inAssetPath, string inAssetName)
	{
		if (!mJoystickDownloadIds.Contains(inPos))
		{
			mState = KAInputState.LOADING;
			mJoystickDownloadIds.Add(inPos);
			if (!JoystickPresentInScene(inAssetName))
			{
				RsResourceManager.LoadAssetFromBundle(inAssetPath, inAssetName, OnPrefabLoaded, typeof(GameObject));
			}
		}
	}

	private bool JoystickPresentInScene(string inAssetName)
	{
		GameObject gameObject = GameObject.Find(inAssetName);
		if (gameObject != null)
		{
			AddJoystickToList(gameObject);
			return true;
		}
		return false;
	}

	public void OnPrefabLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			int num = inURL.LastIndexOf("/");
			if (num != -1)
			{
				RsResourceManager.SetDontDestroy(inURL.Substring(0, num), inDontDestroy: true);
			}
			GameObject gameObject = (GameObject)inObject;
			if (gameObject != null)
			{
				GameObject inGO = UnityEngine.Object.Instantiate(gameObject);
				AddJoystickToList(inGO);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			if (mJoystickDownloadIds.Count == 0)
			{
				mState = KAInputState.READY;
			}
			Debug.LogError(" @@ Joystick load failed !!!");
			break;
		}
	}

	private void AddJoystickToList(GameObject inGO)
	{
		JoyStickPos pPos = mJoystickDownloadIds[0];
		UiJoystick component = inGO.GetComponent<UiJoystick>();
		component.pPos = pPos;
		mJoystickList.Add(component);
		component.SetVisibility(!mHideJoystickOnCreation);
		mJoystickDownloadIds.RemoveAt(0);
		if (mJoystickDownloadIds.Count == 0)
		{
			mState = KAInputState.READY;
		}
	}

	public int GetIndex(int inInputID)
	{
		int result = -1;
		for (int i = 0; i < mRegInputs.Count; i++)
		{
			if (mRegInputs[i]._Idx == inInputID)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public GameInput GetInputForID(int inInputID)
	{
		GameInput result = null;
		for (int i = 0; i < mRegInputs.Count; i++)
		{
			GameInput gameInput = mRegInputs[i];
			if (gameInput._Idx == inInputID)
			{
				result = gameInput;
				break;
			}
		}
		return result;
	}

	public GameInput GetInputForString(string inInputName)
	{
		GameInput result = null;
		int i = 0;
		for (int count = mRegInputs.Count; i < count; i++)
		{
			GameInput gameInput = mRegInputs[i];
			if (string.Compare(gameInput._Name, inInputName, StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = gameInput;
				break;
			}
		}
		return result;
	}

	public static bool IsPressed(int inIdx)
	{
		if (pInstance == null)
		{
			return false;
		}
		return pInstance.GetInputForID(inIdx)?.IsPressed() ?? false;
	}

	public static bool IsPressed(string inInputName)
	{
		if (pInstance == null)
		{
			return false;
		}
		return pInstance.GetInputForString(inInputName)?.IsPressed() ?? false;
	}

	public static bool IsUp(int inIdx)
	{
		if (pInstance == null)
		{
			return false;
		}
		return pInstance.GetInputForID(inIdx)?.IsUp() ?? false;
	}

	public static bool IsDown(int inIdx)
	{
		if (pInstance == null)
		{
			return false;
		}
		return pInstance.GetInputForID(inIdx)?.IsDown() ?? false;
	}

	public static float GetValue(int inIdx)
	{
		if (pInstance == null)
		{
			return 0f;
		}
		return pInstance.GetInputForID(inIdx)?.GetValue() ?? 0f;
	}

	public static float GetValue(string inInputName)
	{
		if (pInstance == null)
		{
			return 0f;
		}
		return pInstance.GetInputForString(inInputName)?.GetValue() ?? 0f;
	}

	public static bool GetKey(string inName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inName)?.IsPressed() ?? Input.GetKey(inName);
		}
		return false;
	}

	public static bool GetKey(KeyCode inKeyCode)
	{
		return Input.GetKey(inKeyCode);
	}

	public static bool GetKeyUp(string inName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inName)?.IsUp() ?? Input.GetKeyUp(inName);
		}
		return false;
	}

	public static bool GetKeyUp(KeyCode inKeyCode)
	{
		return Input.GetKeyUp(inKeyCode);
	}

	public static bool GetKeyDown(KeyCode inKeyCode)
	{
		return Input.GetKeyDown(inKeyCode);
	}

	public static bool GetKeyDown(string inName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inName)?.IsDown() ?? Input.GetKeyDown(inName);
		}
		return false;
	}

	public static bool GetButtonDown(string inButtonName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inButtonName)?.IsDown() ?? Input.GetButtonDown(inButtonName);
		}
		return false;
	}

	public static bool GetButtonUp(string inButtonName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inButtonName)?.IsUp() ?? Input.GetButtonUp(inButtonName);
		}
		return false;
	}

	public static bool GetButton(string inButtonName)
	{
		if (pInstance != null)
		{
			return pInstance.GetInputForString(inButtonName)?.IsPressed() ?? Input.GetButton(inButtonName);
		}
		return false;
	}

	public static bool GetMouseButtonDown(int inButton)
	{
		if (pInstance.IsTouchInput() && mResetInputAxesFrameCount == Time.frameCount)
		{
			return false;
		}
		return Input.GetMouseButtonDown(inButton);
	}

	public static bool GetMouseButtonUp(int inButton)
	{
		if (pInstance.IsTouchInput() && mResetInputAxesFrameCount == Time.frameCount)
		{
			return false;
		}
		return Input.GetMouseButtonUp(inButton);
	}

	public static bool GetMouseButton(int inButton)
	{
		if (pInstance.IsTouchInput() && mResetInputAxesFrameCount == Time.frameCount)
		{
			return false;
		}
		return Input.GetMouseButton(inButton);
	}

	public static float GetAxis(string inAxisName)
	{
		if (pInstance != null)
		{
			GameInput inputForString = pInstance.GetInputForString(inAxisName);
			if (inputForString != null)
			{
				return inputForString.GetValue();
			}
		}
		return 0f;
	}

	public static float GetAxisRaw(string inAxisName)
	{
		return Input.GetAxisRaw(inAxisName);
	}

	public static void ResetInputAxes()
	{
		if (mResetInputAxesFrameCount != Time.frameCount)
		{
			if (KAUIManager.pInstance != null)
			{
				KAUIManager.pInstance._IsMouseButtonUp = Input.GetMouseButtonUp(0);
			}
			mResetInputAxesFrameCount = Time.frameCount;
			Input.ResetInputAxes();
		}
	}

	public static Touch GetTouch(int inIndex)
	{
		return Input.GetTouch(inIndex);
	}

	public void ShowTweak(bool inShow)
	{
		GameObject gameObject = GameObject.Find("PfInputManager");
		if (inShow)
		{
			if (gameObject == null)
			{
				LoadInputManager();
			}
			else
			{
				gameObject.GetComponent<KAInputManager>().enabled = true;
			}
		}
		else if (gameObject != null)
		{
			KAInputManager component = gameObject.GetComponent<KAInputManager>();
			if (component != null)
			{
				component.enabled = false;
			}
		}
	}

	public void SetCalibration(float x, float y, float z, InputType inType)
	{
		foreach (GameInput mRegInput in mRegInputs)
		{
			mRegInput.pModifiers.Find((InputInfo i) => i._Type == inType)?.pProcessor.SetCalibration(x, y, z);
		}
	}

	public void CalibrateInput(InputType inType)
	{
		foreach (GameInput mRegInput in mRegInputs)
		{
			mRegInput.pModifiers.Find((InputInfo i) => i._Type == inType)?.pProcessor.Calibrate();
		}
	}

	public void EnableInputType(string inInputName, InputType inType, bool inEnable)
	{
		GameInput inputForString = GetInputForString(inInputName);
		if (inputForString != null)
		{
			if (inEnable)
			{
				inputForString.EnableInputType(inType);
			}
			else
			{
				inputForString.DisableInputType(inType);
			}
		}
	}

	public void SetState(KAInputState inState)
	{
		if (mState != KAInputState.LOADING)
		{
			mState = inState;
		}
		if (!mLevelLoading)
		{
			ValidateInputs();
		}
	}

	private void ValidateInputs()
	{
		if (mState != KAInputState.READY)
		{
			return;
		}
		foreach (GameInput mRegInput in mRegInputs)
		{
			mRegInput.ValidateInputsForScene(RsResourceManager.pCurrentLevel.ToLower());
			mRegInput.ValidateInputsForPlatform();
		}
	}

	private void DetermineInputMode()
	{
		pMouseMoved = Input.mousePosition.x != pPrevMousePos.x || Input.mousePosition.y != pPrevMousePos.y;
		if (pMouseMoved || Input.GetMouseButtonDown(0))
		{
			if (Input.touchCount == 0)
			{
				pInputMode = KAInputMode.MOUSE;
			}
			else
			{
				pInputMode = KAInputMode.TOUCH;
			}
			pPrevMousePos = Input.mousePosition;
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		pPrevMousePos = Input.mousePosition;
	}

	public bool IsTouchInput()
	{
		if (Application.isEditor)
		{
			if (UtPlatform.IsMobile())
			{
				return true;
			}
			return false;
		}
		if (pInstance.pInputMode == KAInputMode.MOUSE)
		{
			return false;
		}
		return true;
	}
}
