using DG.Tweening;
using UnityEngine;

public class RotationHelper : MonoBehaviour
{
	public float duration;

	public int rotation;

	private void Start()
	{
		base.transform.DORotate(new Vector3(rotation, 0f, 0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1);
	}
}
