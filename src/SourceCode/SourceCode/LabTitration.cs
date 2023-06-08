public class LabTitration : KAMonoBase
{
	private LabTitrationCrucible mCrucible;

	public LabCrucible pCrucible => mCrucible;

	public void Init(ScientificExperiment inManager)
	{
		mCrucible = new LabTitrationCrucible(inManager, this);
	}
}
