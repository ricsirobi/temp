using UnityEngine;
using UnityEngine.UI;

namespace SquadTactics;

public class UiFloatingTip : MonoBehaviour
{
	public Text _Text;

	public void Initialize(string text, Color color)
	{
		_Text.text = text;
		_Text.color = color;
	}
}
