using UnityEngine;

namespace SOD.Event;

public class UiEventBoardMenu : UiJobBoardMenu
{
	public override void OnClick(KAWidget inWidget)
	{
		if (inWidget != null && inWidget.GetUserDataInt() > 0 && inWidget.name == "BtnPrizeCollect")
		{
			inWidget.SetInteractive(isInteractive: false);
			Task task = MissionManager.pInstance.GetTask(inWidget.GetUserDataInt());
			mParentUi.SetSelectedTask(task);
			mParentUi.OnAcceptMission();
		}
	}

	public override void OnTaskComplete(string inTaskName, bool isTrashed)
	{
		if (string.IsNullOrEmpty(inTaskName))
		{
			return;
		}
		KAWidget kAWidget = FindItem(inTaskName);
		if (!(kAWidget != null))
		{
			return;
		}
		PlayJobBoardParticle playJobBoardParticle = kAWidget.gameObject.AddComponent<PlayJobBoardParticle>();
		if (playJobBoardParticle != null)
		{
			ParticleSystem particleSystem = Object.Instantiate(mParentUi._TaskCompleteParticle);
			particleSystem.transform.parent = kAWidget.transform;
			Vector3 localPosition = new Vector3(0f, 0f, -50f);
			particleSystem.transform.localPosition = localPosition;
			particleSystem.transform.localRotation = Quaternion.identity;
			playJobBoardParticle._Particle = particleSystem;
			playJobBoardParticle._Duration = mParentUi._ParticleDuration;
			playJobBoardParticle.Init(base.gameObject, isTrashed);
			playJobBoardParticle.PlayParticle(isPlay: true);
			if (mParentUi._TaskCompleteSfx != null)
			{
				SnChannel.Play(mParentUi._TaskCompleteSfx, "SFX_Pool", inForce: true);
			}
		}
	}

	public override void CompleteTask(string inTaskName, bool isTrash)
	{
		KAWidget kAWidget = FindItem(inTaskName);
		int userDataInt = kAWidget.GetUserDataInt();
		Task task = MissionManager.pInstance.GetTask(userDataInt);
		KAWidget kAWidget2 = kAWidget.FindChildItem("BtnPrizeCollect");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		((KAToggleButton)kAWidget.FindChildItem("MissionState")).SetChecked(isChecked: true);
		KAWidget kAWidget3 = kAWidget.FindChildItem("Required" + task.pData.Objectives.Count.ToString("d2"));
		if (kAWidget3 != null)
		{
			kAWidget3.SetVisibility(inVisible: false);
		}
	}
}
