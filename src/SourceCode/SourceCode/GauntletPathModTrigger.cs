using UnityEngine;

public class GauntletPathModTrigger : MonoBehaviour
{
	public GauntletPathModData[] _AlternatePaths;

	public void OnTriggerEnter(Collider collider)
	{
		GauntletController component = collider.gameObject.GetComponent<GauntletController>();
		if (component != null)
		{
			int num = _AlternatePaths.Length;
			if (num > 0)
			{
				int num2 = Random.Range(0, num);
				GauntletPathModData gauntletPathModData = _AlternatePaths[num2];
				component.ChangeSplinePath(base.transform.position, gauntletPathModData._SplineObject, gauntletPathModData._NodeIndex);
			}
		}
	}
}
