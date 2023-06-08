using UnityEngine;
using UnityEngine.UI;

namespace Xsolla;

public class ElementTableController : MonoBehaviour
{
	public Text _title;

	public GameObject _container;

	public void InitScreen(XsollaFormElement pElem)
	{
		_title.text = pElem.GetTableOptions()._head[0];
		foreach (string item in pElem.GetTableOptions()._body)
		{
			GameObject obj = Object.Instantiate(Resources.Load("Prefabs/SimpleView/_PaymentFormElements/ContainerTableItem")) as GameObject;
			obj.GetComponentInChildren<Text>().text = item;
			obj.transform.SetParent(_container.transform);
		}
	}
}
