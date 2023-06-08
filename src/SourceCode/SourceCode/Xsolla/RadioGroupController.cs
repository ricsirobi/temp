using System.Collections.Generic;
using UnityEngine;

namespace Xsolla;

public class RadioGroupController : MonoBehaviour
{
	public List<RadioButton> radioButtons;

	private int prevSelected;

	private bool isUpdated;

	public RadioGroupController()
	{
		radioButtons = new List<RadioButton>();
	}

	public void AddButton(RadioButton rb)
	{
		radioButtons.Add(rb);
	}

	public void SetButtons(List<GameObject> objects)
	{
		foreach (GameObject @object in objects)
		{
			radioButtons.Add(@object.GetComponent<RadioButton>());
		}
	}

	public void SelectItem(RadioButton.RadioType pType)
	{
		foreach (RadioButton radioButton in radioButtons)
		{
			if (radioButton.getType() != pType)
			{
				radioButton.Deselect();
			}
			else
			{
				radioButton.Select();
			}
		}
	}

	public void SelectItem(int pPosition)
	{
		if (prevSelected >= 0)
		{
			radioButtons[prevSelected].Deselect();
		}
		radioButtons[pPosition].Select();
		prevSelected = pPosition;
		isUpdated = false;
	}

	private void Update()
	{
		if (isUpdated)
		{
			return;
		}
		foreach (RadioButton radioButton in radioButtons)
		{
			radioButton.Deselect();
		}
		radioButtons[prevSelected].Select();
		isUpdated = true;
	}
}
