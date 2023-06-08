using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialStepHandler
{
	public delegate void OnTutorialStepProgress(float completed, float max);

	private delegate void ProcessGOActions(GameObject go, InteractiveTutGameObject actionDetails);

	protected InteractiveTutStep mTutStep;

	protected List<GameObject> mDisabledGameObjects = new List<GameObject>();

	public OnTutorialStepProgress _StepProgressCallback;

	public InteractiveTutStep pTutStep
	{
		get
		{
			return mTutStep;
		}
		set
		{
			mTutStep = value;
		}
	}

	public static TutorialStepHandler InitTutorialStepHandler(InteractiveTutStep tutStepDef)
	{
		TutorialStepHandler tutorialStepHandler = null;
		switch (tutStepDef._StepDetails._StepType)
		{
		case InteractiveTutStepTypes.STEP_TYPE_NON_INTERACTIVE:
			tutorialStepHandler = new TutorialStepHandler();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_UI_CLICK:
			tutorialStepHandler = new TutorialStepHandlerUiClick();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_GAMEOBJECT_CLICK:
			tutorialStepHandler = new TutorialStepHandlerObjClick();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_TIMED_STEP:
			tutorialStepHandler = new TutorialStepHandlerTimed();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_PROXIMITY_CHECK:
			tutorialStepHandler = new TutorialStepHandlerProximity();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_ASYNC:
			tutorialStepHandler = new TutorialStepHandlerAsync();
			break;
		case InteractiveTutStepTypes.STEP_TYPE_CUSTOM:
			tutorialStepHandler = new TutorialStepHandlerCustom();
			break;
		}
		tutorialStepHandler.mTutStep = tutStepDef;
		return tutorialStepHandler;
	}

	public virtual void SetupTutorialStep()
	{
		ProcessInterface(mTutStep._Interfaces);
		ProcessPlayer(mTutStep._PlayerControls);
		ProcessGameObject(mTutStep._GameObjectControls);
	}

	public virtual void StepUpdate()
	{
	}

	public virtual void StepLateUpdate()
	{
	}

	public virtual void FinishTutorialStep()
	{
		ProcessInterface(mTutStep._StepEndInterfaces);
		ProcessPlayer(mTutStep._StepEndPlayerControls);
		ProcessGameObject(mTutStep._StepEndGameObjectControls);
		ProcessStepRewards();
	}

	protected virtual void ProcessStepRewards()
	{
		InteractiveTutReward[] stepRewards = mTutStep._StepRewards;
		foreach (InteractiveTutReward interactiveTutReward in stepRewards)
		{
			if (interactiveTutReward._Type == InteractiveTutRewardTypes.GAME_CURRENCY)
			{
				Money.AddMoney(interactiveTutReward._Count, bForceUpdate: true);
			}
		}
	}

	protected virtual void ProcessInterface(InteractiveTutInterface[] inInterfaces)
	{
		if (inInterfaces == null)
		{
			return;
		}
		foreach (InteractiveTutInterface interactiveTutInterface in inInterfaces)
		{
			if (interactiveTutInterface._Interface.Length != 0)
			{
				KAUI kAUI = ResolveInterface(interactiveTutInterface._Interface);
				if (kAUI != null)
				{
					ProcessKAUInterface(kAUI, interactiveTutInterface);
				}
				else
				{
					UtDebug.Log("Couldn't find either a UiInterface or a KAUI. Skipping interface: " + interactiveTutInterface._Interface);
				}
			}
		}
	}

	protected virtual void ProcessKAUInterface(KAUI uiinterface, InteractiveTutInterface tutInterface)
	{
		if (!(uiinterface != null) || tutInterface == null)
		{
			return;
		}
		switch (tutInterface._Action)
		{
		case InteractiveTutActions.DISABLE:
			uiinterface.SetState(KAUIState.DISABLED);
			break;
		case InteractiveTutActions.NOT_INTERACTIVE:
			uiinterface.SetState(KAUIState.NOT_INTERACTIVE);
			break;
		case InteractiveTutActions.ENABLE:
			uiinterface.SetState(KAUIState.INTERACTIVE);
			break;
		case InteractiveTutActions.INVISIBLE:
			uiinterface.SetVisibility(inVisible: false);
			break;
		case InteractiveTutActions.VISIBLE:
			uiinterface.SetVisibility(inVisible: true);
			break;
		case InteractiveTutActions.BLINK_ON:
			InteractiveTutManager._BlinkEvent = (BlinkTextEvent)Delegate.Combine(InteractiveTutManager._BlinkEvent, new BlinkTextEvent(uiinterface.SetVisibility));
			break;
		case InteractiveTutActions.BLINK_OFF:
			InteractiveTutManager._BlinkEvent = (BlinkTextEvent)Delegate.Remove(InteractiveTutManager._BlinkEvent, new BlinkTextEvent(uiinterface.SetVisibility));
			uiinterface.SetVisibility(inVisible: true);
			break;
		case InteractiveTutActions.ENABLE_ALL_ITEMS:
		{
			for (int l = 0; l < uiinterface.GetItemCount(); l++)
			{
				uiinterface.FindItemAt(l).SetState(KAUIState.INTERACTIVE);
			}
			break;
		}
		case InteractiveTutActions.DISABLE_ALL_ITEMS:
		{
			for (int k = 0; k < uiinterface.GetItemCount(); k++)
			{
				uiinterface.FindItemAt(k).SetState(KAUIState.DISABLED);
			}
			break;
		}
		case InteractiveTutActions.SHOW_ALL_ITEMS:
		{
			for (int j = 0; j < uiinterface.GetItemCount(); j++)
			{
				uiinterface.FindItemAt(j).SetVisibility(inVisible: true);
			}
			break;
		}
		case InteractiveTutActions.HIDE_ALL_ITEMS:
		{
			for (int i = 0; i < uiinterface.GetItemCount(); i++)
			{
				uiinterface.FindItemAt(i).SetVisibility(inVisible: false);
			}
			break;
		}
		}
		InteractiveTutItem[] items = tutInterface._Items;
		foreach (InteractiveTutItem interactiveTutItem in items)
		{
			KAWidget kAWidget = null;
			if (!tutInterface._MenuInterface)
			{
				string[] array = interactiveTutItem._Button.Split('/');
				if (array.Length > 1)
				{
					KAWidget kAWidget2 = uiinterface.FindItem(array[0]);
					if (kAWidget2 != null)
					{
						kAWidget = kAWidget2.FindChildItem(array[1]);
					}
				}
				else
				{
					kAWidget = uiinterface.FindItem(interactiveTutItem._Button);
				}
			}
			else
			{
				int result = 0;
				if (int.TryParse(interactiveTutItem._Button, out result))
				{
					kAWidget = uiinterface.FindItemAt(result);
				}
			}
			if (kAWidget != null)
			{
				switch (interactiveTutItem._Action)
				{
				case InteractiveTutActions.DISABLE:
					kAWidget.SetState(KAUIState.DISABLED);
					break;
				case InteractiveTutActions.ENABLE:
					kAWidget.SetState(KAUIState.INTERACTIVE);
					break;
				case InteractiveTutActions.NOT_INTERACTIVE:
					kAWidget.SetState(KAUIState.NOT_INTERACTIVE);
					break;
				case InteractiveTutActions.INVISIBLE:
					kAWidget.SetVisibility(inVisible: false);
					break;
				case InteractiveTutActions.VISIBLE:
					kAWidget.SetVisibility(inVisible: true);
					break;
				case InteractiveTutActions.FLASH:
					kAWidget.PlayAnim("Flash");
					break;
				case InteractiveTutActions.STOP_FLASH:
					kAWidget.PlayAnim("Normal");
					break;
				case InteractiveTutActions.SHOW:
					kAWidget.PlayAnim("Normal");
					break;
				case InteractiveTutActions.BLINK_ON:
					InteractiveTutManager._BlinkEvent = (BlinkTextEvent)Delegate.Combine(InteractiveTutManager._BlinkEvent, new BlinkTextEvent(kAWidget.SetVisibility));
					break;
				case InteractiveTutActions.BLINK_OFF:
					InteractiveTutManager._BlinkEvent = (BlinkTextEvent)Delegate.Remove(InteractiveTutManager._BlinkEvent, new BlinkTextEvent(kAWidget.SetVisibility));
					kAWidget.SetVisibility(inVisible: true);
					break;
				}
			}
		}
	}

	protected virtual void ProcessPlayer(InteractiveTutPlayerActions[] inPlayerActions)
	{
		if (inPlayerActions == null)
		{
			return;
		}
		for (int i = 0; i < inPlayerActions.Length; i++)
		{
			switch (inPlayerActions[i])
			{
			case InteractiveTutPlayerActions.ENABLE:
				AvAvatar.pObject.SetActive(value: true);
				break;
			case InteractiveTutPlayerActions.DISABLE:
				AvAvatar.pObject.SetActive(value: false);
				break;
			case InteractiveTutPlayerActions.VISIBLE:
				AvAvatar.pObject.GetComponent<Renderer>().enabled = true;
				break;
			case InteractiveTutPlayerActions.INVISIBLE:
				AvAvatar.pObject.GetComponent<Renderer>().enabled = false;
				break;
			case InteractiveTutPlayerActions.ENABLE_CONTROL:
				AvAvatar.EnableAllInputs(inActive: true);
				AvAvatar.pState = AvAvatarState.IDLE;
				break;
			case InteractiveTutPlayerActions.DISABLE_CONTROL:
				AvAvatar.EnableAllInputs(inActive: false);
				AvAvatar.pState = AvAvatarState.PAUSED;
				break;
			}
		}
	}

	protected virtual void ProcessGameObject(InteractiveTutGameObject[] inGO)
	{
		ProcessGOActions processGOActions = delegate(GameObject go, InteractiveTutGameObject action)
		{
			switch (action._Action)
			{
			case InteractiveTutGameObjectActions.ENABLE:
				go.SetActive(value: true);
				mDisabledGameObjects.Remove(go);
				break;
			case InteractiveTutGameObjectActions.DISABLE:
				go.SetActive(value: false);
				if (!mDisabledGameObjects.Exists((GameObject a) => a == go))
				{
					mDisabledGameObjects.Add(go);
				}
				break;
			case InteractiveTutGameObjectActions.VISIBLE:
				go.GetComponent<Renderer>().enabled = true;
				break;
			case InteractiveTutGameObjectActions.INVISIBLE:
				go.GetComponent<Renderer>().enabled = false;
				break;
			case InteractiveTutGameObjectActions.START_ANIM:
				if (!string.IsNullOrEmpty(action._AnimName))
				{
					go.GetComponent<Animation>().Play(action._AnimName);
				}
				break;
			case InteractiveTutGameObjectActions.STOP_ANIM:
				if (!string.IsNullOrEmpty(action._AnimName))
				{
					go.GetComponent<Animation>().Stop(action._AnimName);
				}
				else
				{
					go.GetComponent<Animation>().Stop();
				}
				break;
			case InteractiveTutGameObjectActions.ENABLE_COMPONENT:
			{
				MonoBehaviour monoBehaviour2 = go.GetComponent(action._ComponentName) as MonoBehaviour;
				if (monoBehaviour2 != null)
				{
					monoBehaviour2.enabled = true;
				}
				break;
			}
			case InteractiveTutGameObjectActions.DISABLE_COMPONENT:
			{
				MonoBehaviour monoBehaviour = go.GetComponent(action._ComponentName) as MonoBehaviour;
				if (monoBehaviour != null)
				{
					monoBehaviour.enabled = false;
				}
				break;
			}
			case InteractiveTutGameObjectActions.NONE:
				break;
			}
		};
		if (inGO == null)
		{
			return;
		}
		foreach (InteractiveTutGameObject interactiveTutGameObject in inGO)
		{
			string[] array;
			if (!string.IsNullOrEmpty(interactiveTutGameObject._GameObjectNames))
			{
				array = interactiveTutGameObject._GameObjectNames.Split(default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char));
				foreach (string text in array)
				{
					GameObject gameObject = GameObject.Find(text);
					if (gameObject != null)
					{
						processGOActions(gameObject, interactiveTutGameObject);
					}
					else
					{
						if (mDisabledGameObjects.Count <= 0)
						{
							continue;
						}
						foreach (GameObject mDisabledGameObject in mDisabledGameObjects)
						{
							if (mDisabledGameObject.name == text)
							{
								processGOActions(mDisabledGameObject, interactiveTutGameObject);
								break;
							}
						}
					}
				}
			}
			if (string.IsNullOrEmpty(interactiveTutGameObject._GameObjectTags))
			{
				continue;
			}
			array = interactiveTutGameObject._GameObjectTags.Split(default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char), default(char));
			for (int j = 0; j < array.Length; j++)
			{
				GameObject[] array2 = GameObject.FindGameObjectsWithTag(array[j]);
				if (array2 != null)
				{
					GameObject[] array3 = array2;
					foreach (GameObject go2 in array3)
					{
						processGOActions(go2, interactiveTutGameObject);
					}
				}
			}
		}
	}

	public static KAUI ResolveInterface(string interfaceID)
	{
		if (interfaceID == null)
		{
			return null;
		}
		int num = interfaceID.LastIndexOf('/');
		string text = ((num != -1) ? interfaceID.Substring(0, num) : interfaceID);
		string text2 = ((num != -1) ? interfaceID.Substring(num + 1, interfaceID.Length - (num + 1)) : "");
		GameObject gameObject = GameObject.Find(text);
		if (gameObject == null)
		{
			UtDebug.LogWarning("GameObject not found " + text);
			return null;
		}
		KAUI kAUI = null;
		if (!string.IsNullOrEmpty(text2))
		{
			kAUI = gameObject.GetComponent(text2) as KAUI;
			if (kAUI == null)
			{
				Type type = Type.GetType(text2);
				if (type != null)
				{
					kAUI = gameObject.GetComponentInChildren(type) as KAUI;
				}
			}
		}
		if (kAUI == null)
		{
			kAUI = gameObject.GetComponent<KAUI>();
		}
		if (kAUI == null)
		{
			kAUI = gameObject.GetComponentInChildren<KAUI>();
		}
		return kAUI;
	}
}
