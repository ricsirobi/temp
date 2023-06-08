using UnityEngine;

public class ObReconnectClickable : MonoBehaviour
{
	public string _PropLoaderName = "PfDownTownPropLoader";

	public string _ActiveObjectName = "PfUiSelectPetRide";

	private void Start()
	{
		GameObject activateObject = ((CoPropLoader)GameObject.Find(_PropLoaderName).GetComponent("CoPropLoader")).FindInstance(_ActiveObjectName);
		ObClickable obClickable = (ObClickable)base.gameObject.GetComponent("ObClickable");
		if (obClickable != null)
		{
			obClickable._ActivateObject = activateObject;
		}
		ObProximity obProximity = (ObProximity)base.gameObject.GetComponent("ObProximity");
		if (obProximity != null)
		{
			obProximity._ActivateObject = activateObject;
		}
	}
}
