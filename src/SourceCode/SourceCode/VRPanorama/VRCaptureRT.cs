using UnityEngine;
using UnityEngine.Rendering;

namespace VRPanorama;

[RequireComponent(typeof(AudioListener))]
public class VRCaptureRT : MonoBehaviour
{
	public enum VRModeList
	{
		EquidistantSBS,
		EquidistantStereo,
		EquidistantMono
	}

	public enum VRFormatList
	{
		JPG,
		PNG
	}

	public int cubemapSize = 128;

	public bool oneFacePerFrame;

	public Camera cubeCam;

	private RenderTexture rtex;

	private RenderTexture rtexr;

	public bool captureAudio;

	private float remainingTime;

	private int minutesRemain;

	private int secondsRemain;

	public bool alignPanoramaWithHorizont = true;

	private Material VRAA;

	private RenderTexture unfilteredRt;

	private GameObject renderHead;

	private GameObject rig;

	private GameObject cam;

	private RenderTexture flTex;

	private RenderTexture frTex;

	private RenderTexture llTex;

	private RenderTexture lrTex;

	private RenderTexture rlTex;

	private RenderTexture rrTex;

	private RenderTexture dlTex;

	private RenderTexture drTex;

	private RenderTexture blTex;

	private RenderTexture brTex;

	private RenderTexture tlTex;

	private RenderTexture trTex;

	private RenderTexture tlxTex;

	private RenderTexture trxTex;

	private RenderTexture dlxTex;

	private RenderTexture drxTex;

	private Material FL;

	private Material FR;

	private Material LL;

	private Material LR;

	private Material RL;

	private Material RR;

	private Material DL;

	private Material DR;

	private Material BL;

	private Material BR;

	private Material TL;

	private Material TR;

	private Material DLX;

	private Material DRX;

	private Material TLX;

	private Material TRX;

	private RenderTexture rt;

	private Texture2D screenShot;

	private Camera camLL;

	private Camera camRL;

	private Camera camTL;

	private Camera camBL;

	private Camera camFL;

	private Camera camDL;

	private Camera camLR;

	private Camera camRR;

	private Camera camTR;

	private Camera camBR;

	private Camera camFR;

	private Camera camDR;

	private Camera camDRX;

	private Camera camDLX;

	private Camera camTRX;

	private Camera camTLX;

	private GameObject renderPanorama;

	private GameObject cloneCamLL;

	private GameObject cloneCamRL;

	private GameObject cloneCamTL;

	private GameObject cloneCamBL;

	private GameObject cloneCamFL;

	private GameObject cloneCamDL;

	private GameObject cloneCamLR;

	private GameObject cloneCamRR;

	private GameObject cloneCamTR;

	private GameObject cloneCamBR;

	private GameObject cloneCamFR;

	private GameObject cloneCamDR;

	private GameObject cloneCamDRX;

	private GameObject cloneCamDLX;

	private GameObject cloneCamTRX;

	private GameObject cloneCamTLX;

	private GameObject camll;

	private GameObject camrl;

	private GameObject camfl;

	private GameObject camtl;

	private GameObject cambl;

	private GameObject camdl;

	[Header("VR Panorama Renderer")]
	public VRModeList panoramaType;

	public VRFormatList ImageFormatType;

	[Tooltip("Store PNG sequence in this folder.")]
	public string Folder = "VR_Sequence";

	[Tooltip("Sequence framerate")]
	public int FPS = 25;

	[Tooltip("Sequence resolution")]
	public int resolution = 2048;

	public int resolutionH = 1080;

	public int NumberOfFramesToRender;

	public int renderFromFrame;

	[VRPanorama]
	public string sequenceLength;

	public float IPDistance = 0.066f;

	public float EnvironmentDistance = 5f;

	public bool openDestinationFolder = true;

	[Header("Make H.264/mp4 Movie")]
	public bool encodeToMp4 = true;

	public int Mp4Bitrate = 12000;

	[Header("RenderTime/Quality Optimization")]
	[Range(1f, 16f)]
	public int renderQuality = 8;

	public string formatString;

	private int qualityTemp;

	public bool mailme;

	public string _mailto = "name@domain.com";

	public string _pass;

	public string _mailfrom = "name@gmail.com";

	public string _prefix = "img";

	[VRPanorama]
	public string RenderInfo = " ";

	public int bufferSize;

	public int numBuffers;

	public bool recOutput;

	private void Awake()
	{
	}

	private void Start()
	{
		PreparePano();
		RenderPano();
	}

	public void PreparePano()
	{
		qualityTemp = resolution / 32 * renderQuality;
		GameObject obj = (GameObject)Object.Instantiate(Resources.Load("Rig"));
		obj.name = "Rig";
		obj.transform.SetParent(base.transform, worldPositionStays: false);
		cam = base.gameObject;
		cam.GetComponent<Camera>().fieldOfView = 100f;
		cam.GetComponent<Camera>().renderingPath = RenderingPath.DeferredShading;
		float num = 0f - IPDistance;
		camll = GameObject.Find("Rig/Left");
		camrl = GameObject.Find("Rig/Right");
		camfl = GameObject.Find("Rig/Front");
		camtl = GameObject.Find("Rig/Top");
		cambl = GameObject.Find("Rig/Back");
		camdl = GameObject.Find("Rig/Down");
		cloneCamLL = Object.Instantiate(cam);
		Object.Destroy(cloneCamLL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamLL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamLL.GetComponent(typeof(AudioListener)));
		cloneCamRL = Object.Instantiate(cam);
		Object.Destroy(cloneCamRL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamRL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamRL.GetComponent(typeof(AudioListener)));
		cloneCamTL = Object.Instantiate(cam);
		Object.Destroy(cloneCamTL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamTL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamTL.GetComponent(typeof(AudioListener)));
		cloneCamBL = Object.Instantiate(cam);
		Object.Destroy(cloneCamBL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamBL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamBL.GetComponent(typeof(AudioListener)));
		cloneCamFL = Object.Instantiate(cam);
		Object.Destroy(cloneCamFL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamFL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamFL.GetComponent(typeof(AudioListener)));
		cloneCamDL = Object.Instantiate(cam);
		Object.Destroy(cloneCamDL.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamDL.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamDL.GetComponent(typeof(AudioListener)));
		cloneCamLR = Object.Instantiate(cam);
		Object.Destroy(cloneCamLR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamLR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamLR.GetComponent(typeof(AudioListener)));
		cloneCamRR = Object.Instantiate(cam);
		Object.Destroy(cloneCamRR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamRR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamRR.GetComponent(typeof(AudioListener)));
		cloneCamTR = Object.Instantiate(cam);
		Object.Destroy(cloneCamTR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamTR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamTR.GetComponent(typeof(AudioListener)));
		cloneCamBR = Object.Instantiate(cam);
		Object.Destroy(cloneCamBR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamBR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamBR.GetComponent(typeof(AudioListener)));
		cloneCamFR = Object.Instantiate(cam);
		Object.Destroy(cloneCamFR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamFR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamFR.GetComponent(typeof(AudioListener)));
		cloneCamDR = Object.Instantiate(cam);
		Object.Destroy(cloneCamDR.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamDR.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamDR.GetComponent(typeof(AudioListener)));
		cloneCamDRX = Object.Instantiate(cam);
		Object.Destroy(cloneCamDRX.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamDRX.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamDRX.GetComponent(typeof(AudioListener)));
		cloneCamDLX = Object.Instantiate(cam);
		Object.Destroy(cloneCamDLX.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamDLX.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamDLX.GetComponent(typeof(AudioListener)));
		cloneCamTRX = Object.Instantiate(cam);
		Object.Destroy(cloneCamTRX.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamTRX.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamTRX.GetComponent(typeof(AudioListener)));
		cloneCamTLX = Object.Instantiate(cam);
		Object.Destroy(cloneCamTLX.GetComponent(typeof(Animator)));
		Object.Destroy(cloneCamTLX.GetComponent(typeof(VRCaptureRT)));
		Object.Destroy(cloneCamTLX.GetComponent(typeof(AudioListener)));
		camLL = cloneCamLL.GetComponent<Camera>();
		camRL = cloneCamRL.GetComponent<Camera>();
		camTL = cloneCamTL.GetComponent<Camera>();
		camBL = cloneCamBL.GetComponent<Camera>();
		camFL = cloneCamFL.GetComponent<Camera>();
		camDL = cloneCamDL.GetComponent<Camera>();
		camLR = cloneCamLR.GetComponent<Camera>();
		camRR = cloneCamRR.GetComponent<Camera>();
		camTR = cloneCamTR.GetComponent<Camera>();
		camBR = cloneCamBR.GetComponent<Camera>();
		camFR = cloneCamFR.GetComponent<Camera>();
		camDR = cloneCamDR.GetComponent<Camera>();
		camDRX = cloneCamDRX.GetComponent<Camera>();
		camDLX = cloneCamDLX.GetComponent<Camera>();
		camTRX = cloneCamTRX.GetComponent<Camera>();
		camTLX = cloneCamTLX.GetComponent<Camera>();
		cloneCamLL.transform.SetParent(camll.transform, worldPositionStays: false);
		cloneCamRL.transform.SetParent(camrl.transform, worldPositionStays: false);
		cloneCamTL.transform.SetParent(camtl.transform, worldPositionStays: false);
		cloneCamBL.transform.SetParent(cambl.transform, worldPositionStays: false);
		cloneCamFL.transform.SetParent(camfl.transform, worldPositionStays: false);
		cloneCamDL.transform.SetParent(camdl.transform, worldPositionStays: false);
		cloneCamTLX.transform.SetParent(camtl.transform, worldPositionStays: false);
		cloneCamDLX.transform.SetParent(camdl.transform, worldPositionStays: false);
		if (panoramaType == VRModeList.EquidistantMono)
		{
			num = 0f;
		}
		Vector3 vector = new Vector3(num, 0f, 0f);
		Vector3 vector2 = new Vector3(0f, num, 0f);
		cloneCamLL.transform.localPosition = -vector / 2f;
		cloneCamRL.transform.localPosition = -vector / 2f;
		cloneCamTL.transform.localPosition = -vector / 2f * -1f;
		cloneCamBL.transform.localPosition = -vector / 2f;
		cloneCamFL.transform.localPosition = -vector / 2f;
		cloneCamDL.transform.localPosition = -vector / 2f;
		cloneCamLR.transform.SetParent(camll.transform, worldPositionStays: false);
		cloneCamLR.transform.localPosition = vector / 2f;
		cloneCamRR.transform.SetParent(camrl.transform, worldPositionStays: false);
		cloneCamRR.transform.localPosition = vector / 2f;
		cloneCamTR.transform.SetParent(camtl.transform, worldPositionStays: false);
		cloneCamTR.transform.localPosition = vector / 2f * -1f;
		cloneCamBR.transform.SetParent(cambl.transform, worldPositionStays: false);
		cloneCamBR.transform.localPosition = vector / 2f;
		cloneCamFR.transform.SetParent(camfl.transform, worldPositionStays: false);
		cloneCamFR.transform.localPosition = vector / 2f;
		cloneCamDR.transform.SetParent(camdl.transform, worldPositionStays: false);
		cloneCamDR.transform.localPosition = vector / 2f;
		cloneCamDLX.transform.localPosition = -vector2 / 2f;
		cloneCamTLX.transform.localPosition = -vector2 / 2f;
		cloneCamTRX.transform.SetParent(camtl.transform, worldPositionStays: false);
		cloneCamTRX.transform.localPosition = vector2 / 2f;
		cloneCamDRX.transform.SetParent(camdl.transform, worldPositionStays: false);
		cloneCamDRX.transform.localPosition = vector2 / 2f;
		renderHead = (GameObject)Object.Instantiate(Resources.Load("360RenderHead"));
		renderHead.hideFlags = HideFlags.HideInHierarchy;
		if (panoramaType == VRModeList.EquidistantStereo)
		{
			renderPanorama = (GameObject)Object.Instantiate(Resources.Load("360UnwrappedRT"));
		}
		if (panoramaType == VRModeList.EquidistantMono)
		{
			renderPanorama = (GameObject)Object.Instantiate(Resources.Load("360UnwrappedRTMono"));
		}
		if (panoramaType == VRModeList.EquidistantSBS)
		{
			renderPanorama = (GameObject)Object.Instantiate(Resources.Load("360UnwrappedRTSBS"));
		}
		renderPanorama.hideFlags = HideFlags.HideInHierarchy;
		cloneCamFL.transform.LookAt(camfl.transform.position + camfl.transform.forward * EnvironmentDistance, camfl.transform.up);
		cloneCamFR.transform.LookAt(camfl.transform.position + camfl.transform.forward * EnvironmentDistance, camfl.transform.up);
		cloneCamLL.transform.LookAt(camll.transform.position + camll.transform.forward * EnvironmentDistance, camll.transform.up);
		cloneCamLR.transform.LookAt(camll.transform.position + camll.transform.forward * EnvironmentDistance, camll.transform.up);
		cloneCamRL.transform.LookAt(camrl.transform.position + camrl.transform.forward * EnvironmentDistance, camrl.transform.up);
		cloneCamRR.transform.LookAt(camrl.transform.position + camrl.transform.forward * EnvironmentDistance, camrl.transform.up);
		cloneCamBL.transform.LookAt(cambl.transform.position + cambl.transform.forward * EnvironmentDistance, cambl.transform.up);
		cloneCamBR.transform.LookAt(cambl.transform.position + cambl.transform.forward * EnvironmentDistance, cambl.transform.up);
		cloneCamTL.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamTR.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamDL.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamDR.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamTLX.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamTRX.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamDLX.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamDRX.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
	}

	public void RenderPano()
	{
		if (panoramaType == VRModeList.EquidistantStereo || panoramaType == VRModeList.EquidistantSBS)
		{
			screenShot = new Texture2D(resolution, resolution, TextureFormat.RGB24, mipChain: false);
		}
		else
		{
			screenShot = new Texture2D(resolution, resolution / 2, TextureFormat.RGB24, mipChain: false);
		}
		float value = resolution;
		flTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		llTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		rlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		dlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		blTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		tlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		FL = Resources.Load("RTs/Materials/FL") as Material;
		FR = Resources.Load("RTs/Materials/FR") as Material;
		LL = Resources.Load("RTs/Materials/LL") as Material;
		LR = Resources.Load("RTs/Materials/LR") as Material;
		RL = Resources.Load("RTs/Materials/RL") as Material;
		RR = Resources.Load("RTs/Materials/RR") as Material;
		DL = Resources.Load("RTs/Materials/DL") as Material;
		DR = Resources.Load("RTs/Materials/DR") as Material;
		BL = Resources.Load("RTs/Materials/BL") as Material;
		BR = Resources.Load("RTs/Materials/BR") as Material;
		TL = Resources.Load("RTs/Materials/TL") as Material;
		TR = Resources.Load("RTs/Materials/TR") as Material;
		camLL.targetTexture = llTex;
		camRL.targetTexture = rlTex;
		camTL.targetTexture = tlTex;
		camBL.targetTexture = blTex;
		camFL.targetTexture = flTex;
		camDL.targetTexture = dlTex;
		FL.SetFloat("_U", value);
		FR.SetFloat("_U", value);
		LL.SetFloat("_U", value);
		LR.SetFloat("_U", value);
		RL.SetFloat("_U", value);
		RR.SetFloat("_U", value);
		DL.SetFloat("_U", value);
		DR.SetFloat("_U", value);
		BL.SetFloat("_U", value);
		BR.SetFloat("_U", value);
		TL.SetFloat("_U", value);
		TR.SetFloat("_U", value);
		if (panoramaType == VRModeList.EquidistantStereo || panoramaType == VRModeList.EquidistantSBS)
		{
			dlxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			drxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			tlxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			trxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			frTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			lrTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			rrTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			drTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			brTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			trTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			FL.SetTexture("_Main", flTex);
			FR.SetTexture("_Main", frTex);
			LL.SetTexture("_Main", llTex);
			LR.SetTexture("_Main", lrTex);
			RL.SetTexture("_Main", rlTex);
			RR.SetTexture("_Main", rrTex);
			DL.SetTexture("_Main", dlTex);
			DR.SetTexture("_Main", drTex);
			BL.SetTexture("_Main", blTex);
			BR.SetTexture("_Main", brTex);
			TL.SetTexture("_Main", tlTex);
			TR.SetTexture("_Main", trTex);
			TL.SetTexture("_MainR", trTex);
			TR.SetTexture("_MainR", tlTex);
			DL.SetTexture("_MainR", drTex);
			DR.SetTexture("_MainR", dlTex);
			TL.SetTexture("_MainX", trxTex);
			TR.SetTexture("_MainX", tlxTex);
			TL.SetTexture("_MainRX", tlxTex);
			TR.SetTexture("_MainRX", trxTex);
			DL.SetTexture("_MainX", dlxTex);
			DR.SetTexture("_MainX", drxTex);
			DL.SetTexture("_MainRX", drxTex);
			DR.SetTexture("_MainRX", dlxTex);
			camLR.targetTexture = lrTex;
			camRR.targetTexture = rrTex;
			camTR.targetTexture = trTex;
			camBR.targetTexture = brTex;
			camFR.targetTexture = frTex;
			camDR.targetTexture = drTex;
			camDRX.targetTexture = drxTex;
			camDLX.targetTexture = dlxTex;
			camTRX.targetTexture = trxTex;
			camTLX.targetTexture = tlxTex;
		}
		else
		{
			FL.SetTexture("_Main", flTex);
			FR.SetTexture("_Main", flTex);
			LL.SetTexture("_Main", llTex);
			LR.SetTexture("_Main", llTex);
			RL.SetTexture("_Main", rlTex);
			RR.SetTexture("_Main", rlTex);
			DL.SetTexture("_Main", dlTex);
			DR.SetTexture("_Main", dlTex);
			BL.SetTexture("_Main", blTex);
			BR.SetTexture("_Main", blTex);
			TL.SetTexture("_Main", tlTex);
			TR.SetTexture("_Main", tlTex);
			TL.SetTexture("_MainR", tlTex);
			DL.SetTexture("_MainR", dlTex);
			TL.SetTexture("_MainX", tlTex);
			TL.SetTexture("_MainRX", tlTex);
			DL.SetTexture("_MainX", dlTex);
			DL.SetTexture("_MainRX", dlTex);
			TR.SetTexture("_MainR", tlTex);
			DR.SetTexture("_MainR", dlTex);
			TR.SetTexture("_MainX", tlTex);
			TR.SetTexture("_MainRX", tlTex);
			DR.SetTexture("_MainX", dlTex);
			DR.SetTexture("_MainRX", dlTex);
		}
		if (Application.isPlaying)
		{
			Time.captureFramerate = FPS;
		}
	}

	public void RenderPanoRT()
	{
		if (panoramaType == VRModeList.EquidistantStereo || panoramaType == VRModeList.EquidistantSBS)
		{
			screenShot = new Texture2D(resolution, resolution, TextureFormat.RGB24, mipChain: false);
		}
		else
		{
			screenShot = new Texture2D(resolution, resolution / 2, TextureFormat.RGB24, mipChain: false);
		}
		float value = resolution;
		flTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		llTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		rlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		dlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		blTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		tlTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
		FL = Resources.Load("RTs/Materials/FL") as Material;
		FR = Resources.Load("RTs/Materials/FR") as Material;
		LL = Resources.Load("RTs/Materials/LL") as Material;
		LR = Resources.Load("RTs/Materials/LR") as Material;
		RL = Resources.Load("RTs/Materials/RL") as Material;
		RR = Resources.Load("RTs/Materials/RR") as Material;
		DL = Resources.Load("RTs/Materials/DL") as Material;
		DR = Resources.Load("RTs/Materials/DR") as Material;
		BL = Resources.Load("RTs/Materials/BL") as Material;
		BR = Resources.Load("RTs/Materials/BR") as Material;
		TL = Resources.Load("RTs/Materials/TL") as Material;
		TR = Resources.Load("RTs/Materials/TR") as Material;
		camLL.targetTexture = llTex;
		camRL.targetTexture = rlTex;
		camTL.targetTexture = tlTex;
		camBL.targetTexture = blTex;
		camFL.targetTexture = flTex;
		camDL.targetTexture = dlTex;
		FL.SetFloat("_U", value);
		FR.SetFloat("_U", value);
		LL.SetFloat("_U", value);
		LR.SetFloat("_U", value);
		RL.SetFloat("_U", value);
		RR.SetFloat("_U", value);
		DL.SetFloat("_U", value);
		DR.SetFloat("_U", value);
		BL.SetFloat("_U", value);
		BR.SetFloat("_U", value);
		TL.SetFloat("_U", value);
		TR.SetFloat("_U", value);
		if (panoramaType == VRModeList.EquidistantStereo || panoramaType == VRModeList.EquidistantSBS)
		{
			dlxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			drxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			tlxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			trxTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			frTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			lrTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			rrTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			drTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			brTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			trTex = RenderTexture.GetTemporary(qualityTemp, qualityTemp, 0);
			FL.SetTexture("_Main", flTex);
			FR.SetTexture("_Main", frTex);
			LL.SetTexture("_Main", llTex);
			LR.SetTexture("_Main", lrTex);
			RL.SetTexture("_Main", rlTex);
			RR.SetTexture("_Main", rrTex);
			DL.SetTexture("_Main", dlTex);
			DR.SetTexture("_Main", drTex);
			BL.SetTexture("_Main", blTex);
			BR.SetTexture("_Main", brTex);
			TL.SetTexture("_Main", tlTex);
			TR.SetTexture("_Main", trTex);
			TL.SetTexture("_MainR", trTex);
			TR.SetTexture("_MainR", tlTex);
			DL.SetTexture("_MainR", drTex);
			DR.SetTexture("_MainR", dlTex);
			TL.SetTexture("_MainX", trxTex);
			TR.SetTexture("_MainX", tlxTex);
			TL.SetTexture("_MainRX", tlxTex);
			TR.SetTexture("_MainRX", trxTex);
			DL.SetTexture("_MainX", dlxTex);
			DR.SetTexture("_MainX", drxTex);
			DL.SetTexture("_MainRX", drxTex);
			DR.SetTexture("_MainRX", dlxTex);
			camLR.targetTexture = lrTex;
			camRR.targetTexture = rrTex;
			camTR.targetTexture = trTex;
			camBR.targetTexture = brTex;
			camFR.targetTexture = frTex;
			camDR.targetTexture = drTex;
			camDRX.targetTexture = drxTex;
			camDLX.targetTexture = dlxTex;
			camTRX.targetTexture = trxTex;
			camTLX.targetTexture = tlxTex;
		}
		else
		{
			FL.SetTexture("_Main", flTex);
			FR.SetTexture("_Main", flTex);
			LL.SetTexture("_Main", llTex);
			LR.SetTexture("_Main", llTex);
			RL.SetTexture("_Main", rlTex);
			RR.SetTexture("_Main", rlTex);
			DL.SetTexture("_Main", dlTex);
			DR.SetTexture("_Main", dlTex);
			BL.SetTexture("_Main", blTex);
			BR.SetTexture("_Main", blTex);
			TL.SetTexture("_Main", tlTex);
			TR.SetTexture("_Main", tlTex);
			TL.SetTexture("_MainR", tlTex);
			DL.SetTexture("_MainR", dlTex);
			TL.SetTexture("_MainX", tlTex);
			TL.SetTexture("_MainRX", tlTex);
			DL.SetTexture("_MainX", dlTex);
			DL.SetTexture("_MainRX", dlTex);
			TR.SetTexture("_MainR", tlTex);
			DR.SetTexture("_MainR", dlTex);
			TR.SetTexture("_MainX", tlTex);
			TR.SetTexture("_MainRX", tlTex);
			DR.SetTexture("_MainX", dlTex);
			DR.SetTexture("_MainRX", dlTex);
		}
	}

	private void Update()
	{
	}

	private void LateUpdate()
	{
		cloneCamFL.transform.LookAt(camfl.transform.position + camfl.transform.forward * EnvironmentDistance, camfl.transform.up);
		cloneCamFR.transform.LookAt(camfl.transform.position + camfl.transform.forward * EnvironmentDistance, camfl.transform.up);
		cloneCamLL.transform.LookAt(camll.transform.position + camll.transform.forward * EnvironmentDistance, camll.transform.up);
		cloneCamLR.transform.LookAt(camll.transform.position + camll.transform.forward * EnvironmentDistance, camll.transform.up);
		cloneCamRL.transform.LookAt(camrl.transform.position + camrl.transform.forward * EnvironmentDistance, camrl.transform.up);
		cloneCamRR.transform.LookAt(camrl.transform.position + camrl.transform.forward * EnvironmentDistance, camrl.transform.up);
		cloneCamBL.transform.LookAt(cambl.transform.position + cambl.transform.forward * EnvironmentDistance, cambl.transform.up);
		cloneCamBR.transform.LookAt(cambl.transform.position + cambl.transform.forward * EnvironmentDistance, cambl.transform.up);
		cloneCamTL.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamTR.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamDL.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamDR.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamTLX.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamTRX.transform.LookAt(camtl.transform.position + camtl.transform.forward * EnvironmentDistance, camtl.transform.up);
		cloneCamDLX.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		cloneCamDRX.transform.LookAt(camdl.transform.position + camdl.transform.forward * EnvironmentDistance, camdl.transform.up);
		if (captureAudio)
		{
			_ = Time.timeSinceLevelLoad;
			_ = (float)(NumberOfFramesToRender / FPS);
			return;
		}
		if (alignPanoramaWithHorizont)
		{
			Vector3 euler = new Vector3(0f, base.gameObject.transform.rotation.eulerAngles.y, 0f);
			base.gameObject.transform.rotation = Quaternion.Euler(euler);
		}
		UpdateCubemapL(renderHead);
		if (panoramaType == VRModeList.EquidistantStereo || panoramaType == VRModeList.EquidistantSBS)
		{
			UpdateCubemapR(GameObject.Find("RcubemapRender"));
		}
		RenderVRPanorama();
	}

	public void RenderVRPanorama()
	{
		SaveScreenshot();
	}

	public void RenderVideo()
	{
	}

	public void SaveScreenshot()
	{
	}

	public Texture2D GetScreenshot(bool eye)
	{
		_ = panoramaType;
		_ = 1;
		return screenShot;
	}

	public void SendEmail()
	{
	}

	private void OnDrawGizmos()
	{
	}

	public Texture2D GetVideoScreenshot()
	{
		Camera main = Camera.main;
		main.targetTexture = unfilteredRt;
		main.Render();
		RenderTexture.active = unfilteredRt;
		VRAA.mainTexture = unfilteredRt;
		VRAA.SetInt("_U", resolution * 2);
		VRAA.SetInt("_V", resolutionH * 2);
		Graphics.Blit(unfilteredRt, rt, VRAA, -1);
		screenShot.ReadPixels(new Rect(0f, 0f, resolution, resolutionH), 0, 0);
		return screenShot;
	}

	public void VideoRenderPrepare()
	{
		VRAA = Resources.Load("Materials/VRAA") as Material;
		unfilteredRt = new RenderTexture(resolution * 2, resolutionH * 2, 0, RenderTextureFormat.DefaultHDR, RenderTextureReadWrite.sRGB);
		rt = new RenderTexture(resolution, resolutionH, 0, RenderTextureFormat.Default, RenderTextureReadWrite.sRGB);
		screenShot = new Texture2D(resolution, resolutionH, TextureFormat.RGB24, mipChain: false);
	}

	public void SaveScreenshotVideo()
	{
	}

	public void DestroyPano()
	{
	}

	public void RenderStaticVRPanorama()
	{
		SaveScreenshot();
	}

	public void CounterPost()
	{
	}

	public void UpdateCubemapL(GameObject gObject)
	{
		if (!rtex)
		{
			rtex = new RenderTexture(resolution / 4, resolution / 4, 0);
			rtex.dimension = TextureDimension.Cube;
			rtex.hideFlags = HideFlags.HideAndDontSave;
			rtex.autoGenerateMips = false;
			gObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", rtex);
		}
		Camera component = gObject.GetComponent<Camera>();
		component.transform.position = gObject.transform.position;
		component.RenderToCubemap(rtex, 63);
	}

	public void UpdateCubemapR(GameObject gObject)
	{
		if (!rtexr)
		{
			rtexr = new RenderTexture(resolution / 4, resolution / 4, 0);
			rtexr.dimension = TextureDimension.Cube;
			rtex.hideFlags = HideFlags.HideAndDontSave;
			rtexr.autoGenerateMips = false;
			gObject.GetComponent<Renderer>().sharedMaterial.SetTexture("_Cube", rtexr);
		}
		Camera component = gObject.GetComponent<Camera>();
		component.transform.position = gObject.transform.position;
		component.RenderToCubemap(rtexr, 63);
	}

	private void StartWriting(string name)
	{
	}

	private void OnAudioFilterRead(float[] data, int channels)
	{
		_ = recOutput;
	}
}
