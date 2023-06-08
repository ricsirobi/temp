using UnityEngine;

public class FlyCamera : MonoBehaviour
{
	public float _MouseRange2 = 90000f;

	public float mainSpeed = 100f;

	public float shiftAdd = 250f;

	public float maxShift = 1000f;

	public float camSens = 0.25f;

	private Vector3 lastMouse = new Vector3(255f, 255f, 255f);

	private float totalRun = 1f;

	private float mNoProcess;

	private Vector3 mCenterOfScreen = Vector3.zero;

	private void Start()
	{
		mCenterOfScreen = new Vector3((float)Screen.width * 0.5f, (float)Screen.height * 0.5f, 0f);
		Application.targetFrameRate = 30;
		KAInput.Init();
		if (UtPlatform.IsMobile())
		{
			mainSpeed = 10f;
			camSens = 0.3f;
		}
	}

	private void Update()
	{
		if (mNoProcess > Time.time || (UtPlatform.IsMobile() && Input.touchCount == 0))
		{
			return;
		}
		Vector3 vector = Vector3.zero;
		if (KAUI.GetGlobalMouseOverItem() == null)
		{
			if ((mCenterOfScreen - Input.mousePosition).sqrMagnitude < _MouseRange2)
			{
				lastMouse = Input.mousePosition - lastMouse;
				lastMouse = new Vector3((0f - lastMouse.y) * camSens, lastMouse.x * camSens, 0f);
				lastMouse = new Vector3(base.transform.eulerAngles.x + lastMouse.x, base.transform.eulerAngles.y + lastMouse.y, 0f);
				base.transform.eulerAngles = lastMouse;
			}
			lastMouse = Input.mousePosition;
		}
		if (UtPlatform.IsMobile())
		{
			if (KAUI.GetGlobalMouseOverItem() != null)
			{
				vector = GetBaseInput();
			}
		}
		else
		{
			vector = GetBaseInput();
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			totalRun += Time.deltaTime;
			vector = vector * totalRun * shiftAdd;
			vector.x = Mathf.Clamp(vector.x, 0f - maxShift, maxShift);
			vector.y = Mathf.Clamp(vector.y, 0f - maxShift, maxShift);
			vector.z = Mathf.Clamp(vector.z, 0f - maxShift, maxShift);
		}
		else
		{
			totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 100f);
			vector *= mainSpeed;
		}
		vector *= Time.deltaTime;
		Vector3 position = base.transform.position;
		if (Input.GetKey(KeyCode.Space))
		{
			base.transform.Translate(vector);
			position.x = base.transform.position.x;
			position.z = base.transform.position.z;
			base.transform.position = position;
		}
		else
		{
			base.transform.Translate(vector);
		}
	}

	private Vector3 GetBaseInput()
	{
		Vector3 result = default(Vector3);
		if (KAInput.GetAxis("Vertical") > 0f)
		{
			result += new Vector3(0f, 0f, 1f);
		}
		if (KAInput.GetAxis("Vertical") < 0f)
		{
			result += new Vector3(0f, 0f, -1f);
		}
		if (KAInput.GetAxis("Horizontal") < 0f)
		{
			result += new Vector3(-1f, 0f, 0f);
		}
		if (KAInput.GetAxis("Horizontal") > 0f)
		{
			result += new Vector3(1f, 0f, 0f);
		}
		return result;
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(Screen.height - 100, 10f, 300f, 500f));
		GUILayout.Box("mouse: " + Input.mousePosition.ToString() + " Quality: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
		GUILayout.Label(" Cam sensitive: ");
		camSens = GUILayout.HorizontalSlider(camSens, 0.1f, 3f);
		GUILayout.Label(" Cam Speed: ");
		mainSpeed = GUILayout.HorizontalSlider(mainSpeed, 3f, 2000f);
		GUILayout.EndArea();
	}
}
