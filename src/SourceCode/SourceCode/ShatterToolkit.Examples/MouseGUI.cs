using ShatterToolkit.Helpers;
using UnityEngine;

namespace ShatterToolkit.Examples;

[RequireComponent(typeof(MouseForce), typeof(MouseSplit), typeof(MouseShatter))]
public class MouseGUI : MonoBehaviour
{
	public int defaultSelection;

	protected MouseForce mouseForce;

	protected MouseSplit mouseSplit;

	protected MouseShatter mouseShatter;

	protected int toolbarSelection;

	protected string[] toolbarLabels = new string[3] { "Mouse force (Click and drag)", "Mouse split (Click and drag, release)", "Mouse shatter (Click)" };

	public void Awake()
	{
		mouseForce = GetComponent<MouseForce>();
		mouseSplit = GetComponent<MouseSplit>();
		mouseShatter = GetComponent<MouseShatter>();
		toolbarSelection = defaultSelection;
		SelectTool();
	}

	public void OnGUI()
	{
		toolbarSelection = GUI.Toolbar(new Rect(10f, 10f, Screen.width - 20, 20f), toolbarSelection, toolbarLabels);
		if (GUI.changed)
		{
			SelectTool();
		}
	}

	protected void SelectTool()
	{
		mouseForce.enabled = false;
		mouseSplit.enabled = false;
		mouseShatter.enabled = false;
		if (toolbarSelection == 0)
		{
			mouseForce.enabled = true;
		}
		else if (toolbarSelection == 1)
		{
			mouseSplit.enabled = true;
		}
		else if (toolbarSelection == 2)
		{
			mouseShatter.enabled = true;
		}
	}
}
