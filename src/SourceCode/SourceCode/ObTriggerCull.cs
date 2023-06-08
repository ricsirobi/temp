using UnityEngine;

public class ObTriggerCull : MonoBehaviour
{
	public CullInformation[] _CullInformation;

	private void OnTriggerEnter(Collider collider)
	{
		if (!(collider.transform.root.gameObject == AvAvatar.pObject))
		{
			return;
		}
		for (int i = 0; i < _CullInformation.Length; i++)
		{
			for (int j = 0; j < _CullInformation[i]._CullObjects.Length; j++)
			{
				_CullInformation[i]._CullObjects[j].SetActive(_CullInformation[i]._Activate);
			}
		}
	}
}
