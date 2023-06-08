using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class UIObjectiveTreeView : KAUITreeListMenu
{
	[SerializeField]
	private KAWidget m_TemplateSubObjective;

	public void Populate()
	{
		mTreeGroupData.Clear();
		ClearItems();
		List<Objective> objectives = GameManager.pInstance.GetHeaderObjectives().FindAll((Objective x) => x._IsMandatory);
		UpdateTreeData(objectives, null, isOptional: false);
		List<Objective> objectives2 = GameManager.pInstance.GetFinalObjectives().FindAll((Objective x) => !x._IsMandatory);
		UpdateTreeData(objectives2, null, isOptional: true);
		PopulateTreeList();
	}

	private void UpdateTreeData(List<Objective> objectives, KAUITreeListItemData parentNode, bool isOptional)
	{
		if (objectives == null || objectives.Count < 1)
		{
			return;
		}
		for (int i = 0; i < objectives.Count; i++)
		{
			KAUITreeListItemData kAUITreeListItemData = new KAUITreeListItemData(parentNode, (parentNode == null) ? "Objective" : "SubObjective", new ObjectiveData(objectives[i]));
			if (parentNode == null)
			{
				mTreeGroupData.Add(kAUITreeListItemData);
			}
			else
			{
				parentNode.AddChild(kAUITreeListItemData);
			}
			if (objectives[i].HasChildObjectives() && !isOptional)
			{
				List<Objective> objectives2 = objectives[i]._ChildObjectives.FindAll((Objective x) => x._IsMandatory);
				UpdateTreeData(objectives2, kAUITreeListItemData, isOptional: false);
			}
		}
	}

	protected override KAWidget CreateWidgetFromData(KAUITreeListItemData inItem)
	{
		Objective objective = ((ObjectiveData)inItem.pUserData)._Objective;
		if (objective == null)
		{
			return null;
		}
		KAWidget kAWidget = DuplicateWidget(GetTemplateItem(inItem._Name));
		kAWidget.name = inItem._Name;
		kAWidget.SetUserData(inItem);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtName");
		string finalObjectiveStr = GameManager.pInstance.GetSceneData().GetFinalObjectiveStr(objective);
		kAWidget2.SetText((objective.pHiddenStatus == ObjectiveHiddenStatus.UNHIDDEN) ? finalObjectiveStr : GameManager.pInstance._HUD._HUDStrings._HiddenText.GetLocalizedString());
		kAWidget2.GetLabel().color = (objective._IsMandatory ? GameManager.pInstance._TxtColorMandatoryObjective : GameManager.pInstance._TxtColorOptionalObjective);
		kAWidget.FindChildItem("CompletedSprite").SetVisibility(objective.pObjectiveStatus == ObjectiveStatus.COMPLETED);
		kAWidget.FindChildItem("FailedSprite").SetVisibility(objective.pObjectiveStatus != ObjectiveStatus.COMPLETED);
		kAWidget.SetVisibility(inVisible: true);
		return kAWidget;
	}

	protected override KAWidget GetTemplateItem(string itemName)
	{
		if (!itemName.Equals("Objective"))
		{
			return m_TemplateSubObjective;
		}
		return _Template;
	}

	protected override void IndentTreeNode(KAUITreeListItemData inItemData, float inParentIndent)
	{
		KAWidget item = inItemData._Item;
		if (item != null)
		{
			_DefaultGrid.padding = new Vector2(inParentIndent + _GroupChildItemOffsetX, _DefaultGrid.padding.y);
			item.SetPosition(inParentIndent + _GroupChildItemOffsetX, item.GetPosition().y);
		}
		foreach (KAUITreeListItemData child in inItemData._ChildList)
		{
			IndentTreeNode(child, (inItemData.pParent == null) ? inParentIndent : (inParentIndent + _GroupChildItemOffsetX));
		}
	}
}
