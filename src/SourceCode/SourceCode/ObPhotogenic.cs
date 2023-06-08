using System.Collections.Generic;
using UnityEngine;

public class ObPhotogenic : MonoBehaviour
{
	public string _Name = "";

	public bool _IsInView;

	public static List<ObPhotogenic> _ObjectList = new List<ObPhotogenic>();

	private void Start()
	{
		if (_Name.Length == 0)
		{
			_Name = base.transform.root.name;
		}
	}

	public void ClearList()
	{
		_ObjectList.Clear();
	}

	private void OnBecameVisible()
	{
		_IsInView = true;
	}

	private void OnBecameInvisible()
	{
		_IsInView = false;
	}

	private void OnEnable()
	{
		_ObjectList.Add(this);
	}

	private void OnDisable()
	{
		_ObjectList.Remove(this);
	}
}
