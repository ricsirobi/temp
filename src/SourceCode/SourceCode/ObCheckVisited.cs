using UnityEngine;

public class ObCheckVisited : MonoBehaviour
{
	public string _StarModuleName = "";

	private bool mVisited;

	private bool mChecked;

	public void UpdateStatus()
	{
		mChecked = true;
		mVisited = true;
		if (_StarModuleName.Length > 0)
		{
			mVisited = SceneData.GetGoldStars(_StarModuleName) > 0;
		}
	}

	public void OnPDAActivate()
	{
		OnActivate();
	}

	public void OnActivate()
	{
		UtDebug.Log(base.name + "OnActivate called");
		if (_StarModuleName.Length > 0 && !mVisited)
		{
			if (!mChecked)
			{
				UpdateStatus();
			}
			if (!mVisited)
			{
				SceneData.AddGoldStar(_StarModuleName);
			}
		}
	}

	public bool IsVisited()
	{
		if (!mChecked)
		{
			UpdateStatus();
		}
		return mVisited;
	}
}
