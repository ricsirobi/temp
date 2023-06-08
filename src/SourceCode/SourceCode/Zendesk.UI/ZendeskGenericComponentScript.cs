using UnityEngine;
using UnityEngine.UI;

namespace Zendesk.UI;

public class ZendeskGenericComponentScript : MonoBehaviour
{
	[HideInInspector]
	public long idCustomField;

	public Text labelComponent;
}
