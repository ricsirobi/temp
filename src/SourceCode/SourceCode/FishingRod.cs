using UnityEngine;

public class FishingRod : MonoBehaviour
{
	public float _ReelMax;

	public float _ReelDrag = 1f;

	public float _RodPower;

	public float _FloatSpeedModifier = 1f;

	public float _BaitLoseModifier = 1f;

	public float _LineSnapModifier = 1f;

	public float _RightInterpolationModifier = 1f;

	public bool _AllowDrag = true;

	public float _DragSpeed = 2f;

	public Transform _LineStart;

	[HideInInspector]
	public Transform _FloatObject;

	[HideInInspector]
	public Transform _Fish;

	[HideInInspector]
	public bool _DrawLine;

	private LineRenderer lineRenderer;

	private void Start()
	{
		lineRenderer = base.gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Diffuse"));
		lineRenderer.startColor = Color.black;
		lineRenderer.endColor = Color.black;
		lineRenderer.startWidth = 0.02f;
		lineRenderer.endWidth = 0.02f;
		lineRenderer.positionCount = 3;
		if (_LineStart != null)
		{
			lineRenderer.SetPosition(0, _LineStart.position);
			lineRenderer.SetPosition(1, _LineStart.position);
			lineRenderer.SetPosition(2, _LineStart.position);
		}
	}

	public void SnapLine()
	{
	}

	public void TakeRodDamage()
	{
	}

	public void ConsumeBait()
	{
	}

	public void NibbleBait()
	{
	}

	public void LineSetVisible(bool visible)
	{
		_DrawLine = visible;
		lineRenderer.enabled = visible;
	}

	public void Update()
	{
		if (_DrawLine && lineRenderer != null)
		{
			if (_LineStart != null)
			{
				lineRenderer.SetPosition(0, _LineStart.position);
			}
			if (_FloatObject != null)
			{
				lineRenderer.SetPosition(1, _FloatObject.position);
			}
			if (null != _Fish && _Fish.Find("MainRoot/Root/Head") != null)
			{
				lineRenderer.SetPosition(2, _Fish.Find("MainRoot/Root/Head").position);
			}
			else if (_FloatObject != null)
			{
				lineRenderer.SetPosition(2, _FloatObject.position);
			}
		}
	}
}
