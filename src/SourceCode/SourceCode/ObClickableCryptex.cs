using UnityEngine;

public class ObClickableCryptex : ObClickableCreateInstance
{
	public Animator _AnimatorToPlay;

	public string _MecanimParamater;

	public string _GameLevel = "";

	public override void OnCreateInstance(GameObject inObject)
	{
		UiCryptex component = inObject.GetComponent<UiCryptex>();
		if (component != null)
		{
			component.CreateCryptexObject();
			component._GameLevel = _GameLevel;
		}
		CryptexGameManager componentInChildren = inObject.GetComponentInChildren<CryptexGameManager>();
		if (componentInChildren != null)
		{
			componentInChildren.pClickableCryptex = this;
		}
	}
}
