using UnityEngine;

public class LabSpectrum : MonoBehaviour
{
	public MeshRenderer _Spectrum;

	public MeshRenderer _SpectrumRays;

	public float _WarmingConstant = 500f;

	public Transform _SnapToTransform;

	private LabSpectrumCrucible mSpectrumCrucible;

	public LabCrucible pCrucible => mSpectrumCrucible;

	public void Init(ScientificExperiment inManager)
	{
		mSpectrumCrucible = new LabSpectrumCrucible(inManager, this, _WarmingConstant, _SnapToTransform.position);
	}
}
