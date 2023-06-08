using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class CinematicCamera : KAMonoBase
{
	public delegate void Capture(bool flag);

	public Capture _CinematicCapture;

	public CinematicCameraSetting _DefaultCameraSetting;

	public string _ActiveCharacterLayer = "IgnoreGroundRay";

	public string _CulledCharacterLayer = "Marker";

	private static CinematicCamera mInstance;

	private CinematicCameraSetting mCameraSetting;

	private List<Renderer> mBlockingObjects = new List<Renderer>();

	private Camera mMainCamera;

	private Camera mCamera;

	private Transform mTarget;

	private float mDirection = 1f;

	private float mDeltaAngle;

	private bool mStartCapture;

	private Vector3 mInitialPosition;

	private float mTotalAngleToTraverse;

	public static CinematicCamera pInstance => mInstance;

	public bool pCapturing => mStartCapture;

	private void Awake()
	{
		mInstance = this;
		mInitialPosition = base.transform.position;
		mCameraSetting = _DefaultCameraSetting;
		mMainCamera = Camera.main;
		mCamera = GetComponent<Camera>();
		if (mCamera != null)
		{
			mCamera.enabled = false;
		}
	}

	private void SetupCamera()
	{
		if (mTarget != null)
		{
			base.transform.position = mTarget.position + mTarget.forward * mCameraSetting._InitialDistance + mTarget.up * mCameraSetting._InitialHeight;
			base.transform.RotateAround(mTarget.transform.position, Vector3.up, mCameraSetting._InitialCameraAngle);
			Vector3 worldPosition = mTarget.position + mTarget.transform.right * mCameraSetting._FocusOffset.x + mTarget.transform.up * mCameraSetting._FocusOffset.y + mTarget.transform.forward * mCameraSetting._FocusOffset.z;
			base.transform.LookAt(worldPosition);
			float f = mCameraSetting._FinalCameraAngle - mCameraSetting._InitialCameraAngle;
			mDirection = Mathf.Sign(f);
			mTotalAngleToTraverse = Mathf.Abs(f);
			mDeltaAngle = mTotalAngleToTraverse / mCameraSetting._TimeToRotate;
			if (collider != null)
			{
				collider.enabled = true;
			}
		}
	}

	public void StartCapture(Transform target, CinematicCameraSetting cameraSetting = null)
	{
		if (mCamera == null)
		{
			Debug.LogError("No cinematic camera attached");
			return;
		}
		if (cameraSetting != null)
		{
			mCameraSetting = cameraSetting;
		}
		mTarget = target;
		SetupCamera();
		if (mMainCamera != null)
		{
			mMainCamera.enabled = false;
		}
		mCamera.enabled = true;
		mStartCapture = true;
		if (_CinematicCapture != null)
		{
			_CinematicCapture(!mStartCapture);
		}
	}

	public void StartCapture(Character character, CinematicCameraSetting cameraSetting = null)
	{
		SetCharactersLayers(character);
		StartCapture(character.transform, cameraSetting);
	}

	private void SetCharactersLayers(Character activeCharacter)
	{
		int cinematicLayer = LayerMask.NameToLayer(_ActiveCharacterLayer);
		int cinematicLayer2 = LayerMask.NameToLayer(_CulledCharacterLayer);
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit == activeCharacter)
			{
				activeUnit.SetCinematicLayer(cinematicLayer);
			}
			else
			{
				activeUnit.SetCinematicLayer(cinematicLayer2);
			}
		}
	}

	private void ResetCharactersLayers()
	{
		foreach (Character activeUnit in GameManager.pInstance._ActiveUnits)
		{
			if (activeUnit != null)
			{
				activeUnit.ResetCinematicLayers();
			}
		}
	}

	private void Update()
	{
		if (mStartCapture)
		{
			if (mTotalAngleToTraverse > 0f)
			{
				base.transform.RotateAround(mTarget.transform.position, Vector3.up, mDirection * mDeltaAngle * Time.deltaTime);
				mTotalAngleToTraverse -= mDeltaAngle * Time.deltaTime;
			}
			else
			{
				ResetCamera();
			}
		}
	}

	private void OnTriggerEnter(Collider hit)
	{
		DisableBlocker(hit.gameObject);
	}

	private void OnTriggerExit(Collider hit)
	{
		EnableBlocker(hit.gameObject);
	}

	private void DisableBlocker(GameObject blocker)
	{
		Renderer component = blocker.GetComponent<Renderer>();
		if (component != null && component.enabled)
		{
			component.enabled = false;
			mBlockingObjects.Add(component);
		}
		Renderer[] componentsInChildren = blocker.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (renderer.enabled)
			{
				renderer.enabled = false;
				mBlockingObjects.Add(renderer);
			}
		}
	}

	private void EnableBlocker(GameObject blocker)
	{
		Renderer component = blocker.GetComponent<Renderer>();
		if (component != null && mBlockingObjects.Contains(component))
		{
			component.enabled = true;
			mBlockingObjects.Remove(component);
		}
		Renderer[] componentsInChildren = blocker.GetComponentsInChildren<Renderer>();
		if (componentsInChildren == null)
		{
			return;
		}
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			if (mBlockingObjects.Contains(renderer))
			{
				renderer.enabled = true;
				mBlockingObjects.Remove(renderer);
			}
		}
	}

	private void EnablePendingBlockers()
	{
		foreach (Renderer mBlockingObject in mBlockingObjects)
		{
			mBlockingObject.enabled = true;
		}
		mBlockingObjects.Clear();
	}

	private void ResetCamera()
	{
		mStartCapture = false;
		mCameraSetting = _DefaultCameraSetting;
		mCamera.enabled = false;
		ResetCharactersLayers();
		if (mMainCamera != null)
		{
			mMainCamera.enabled = true;
		}
		base.transform.position = mInitialPosition;
		if (collider != null)
		{
			collider.enabled = false;
		}
		EnablePendingBlockers();
		if (_CinematicCapture != null)
		{
			_CinematicCapture(!mStartCapture);
		}
	}
}
