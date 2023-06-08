namespace SquadTactics;

public class UiCharacterStatus : KAUI
{
	private UiCharacterStatusMenu mMenu;

	protected override void Awake()
	{
		base.Awake();
		mMenu = (UiCharacterStatusMenu)GetMenu("SquadTactics.UiCharacterStatusMenu");
	}

	public void SetupStatMenu(Character character)
	{
		mMenu.ClearItems();
		if (character.pIsIncapacitated && character.pCurrentRevivalCountdown <= 0)
		{
			SetVisibility(inVisible: false);
		}
		else if (character.pActiveStatusEffects.Count > 0)
		{
			mMenu.SetupStatMenu(character);
			SetVisibility(inVisible: true);
		}
		else
		{
			OnClick(FindItem("BtnStatusClose"));
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnClick(inWidget);
			if (inWidget.name == "BtnStatusClose")
			{
				SetVisibility(inVisible: false);
				GameManager.pInstance._HUD.pCharacterInfoUI.OnCloseStat(this);
			}
		}
	}
}
