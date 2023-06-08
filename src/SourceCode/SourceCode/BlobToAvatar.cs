using UnityEngine;
using UnityEngine.Rendering;

public class BlobToAvatar : MonoBehaviour
{
	private int lastLevel;

	private Projector projector;

	private void Start()
	{
		base.transform.parent = GameObject.Find(AvAvatar.GetAvatarPrefabName()).transform;
		projector = base.transform.GetComponent(typeof(Projector)) as Projector;
		lastLevel = QualitySettings.GetQualityLevel();
		GenerateShadow();
	}

	private void Update()
	{
		if (QualitySettings.GetQualityLevel() != lastLevel)
		{
			GenerateShadow();
		}
	}

	private void GenerateShadow()
	{
		if (QualitySettings.GetQualityLevel() < UtUtilities.GetQualityByName("High"))
		{
			Component[] componentsInChildren = base.transform.parent.GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Renderer)componentsInChildren[i]).shadowCastingMode = ShadowCastingMode.Off;
			}
			projector.enabled = true;
		}
		else
		{
			Component[] componentsInChildren = base.transform.parent.GetComponentsInChildren(typeof(Renderer));
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Renderer)componentsInChildren[i]).shadowCastingMode = ShadowCastingMode.On;
			}
			projector.enabled = false;
		}
		lastLevel = QualitySettings.GetQualityLevel();
	}
}
