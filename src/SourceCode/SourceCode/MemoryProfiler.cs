using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

public class MemoryProfiler
{
	private struct ObjMemDef
	{
		public int _Size;

		public bool _IsADependency;

		public string _ObjType;

		public string _Name;

		public ObjMemDef(int Size, string ObjType, string Name, bool IsADependency)
		{
			_Size = Size;
			_ObjType = ObjType;
			_Name = Name;
			_IsADependency = IsADependency;
		}

		public int CompareTo(ref ObjMemDef other)
		{
			if (_Size != other._Size)
			{
				return _Size - other._Size;
			}
			if (_IsADependency != other._IsADependency)
			{
				return (_IsADependency ? 1 : 0) - (other._IsADependency ? 1 : 0);
			}
			int num = string.Compare(_Name, other._Name);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(_ObjType, other._ObjType);
		}
	}

	private enum StatsViewTab
	{
		CURRENT_STATS,
		CURRENT_OBJECTS,
		DIF_STATS,
		DIF_OBJECTS
	}

	private List<KeyValuePair<string, int>> mStats_Current1 = new List<KeyValuePair<string, int>>();

	private Dictionary<string, int> mStats_Current = new Dictionary<string, int>();

	private List<ObjMemDef> mList_Snapshot = new List<ObjMemDef>();

	private List<KeyValuePair<string, int>> mStats_Dif1 = new List<KeyValuePair<string, int>>();

	private Dictionary<string, int> mStats_Dif = new Dictionary<string, int>();

	private List<ObjMemDef> mList_Differences = new List<ObjMemDef>();

	private List<ObjMemDef> mList_LastSnapshot = new List<ObjMemDef>();

	public GameObject _OwnerGO;

	private Vector2 mScrollViewPos_Stats = new Vector2(0f, 0f);

	private StatsViewTab mStatsViewIndex;

	private string mFilter_Text = "";

	private bool mFilter_ShowDependencies = true;

	private bool mCollapsed;

	public static void ForceMemoryCleanUp()
	{
		SpawnPool[] array = Object.FindObjectsOfType(typeof(SpawnPool)) as SpawnPool[];
		foreach (SpawnPool spawnPool in array)
		{
			for (int num = spawnPool._prefabPools.Count - 1; num >= 0; num--)
			{
				PrefabPool prefabPool = spawnPool._prefabPools[num];
				int j = 0;
				for (int count = prefabPool.despawned.Count; j < count; j++)
				{
					if (prefabPool.despawned[j] != null)
					{
						Object.Destroy(prefabPool.despawned[j].gameObject);
					}
				}
				prefabPool._despawned.Clear();
				if (prefabPool.spawned.Count <= 0)
				{
					spawnPool.prefabs._prefabs.Remove(prefabPool.prefab.name);
					spawnPool._prefabPools.RemoveAt(num);
				}
			}
		}
		RsResourceManager.UnloadUnusedAssets(canGCCollect: true);
	}

	public void RenderGUI()
	{
		GUILayoutOption gUILayoutOption = GUILayout.ExpandWidth(expand: false);
		if (mCollapsed)
		{
			if (GUILayout.Button("MemProfiler", gUILayoutOption))
			{
				mCollapsed = false;
			}
			return;
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Close", gUILayoutOption))
		{
			CloseProfiler();
		}
		if (GUILayout.Button("Collapse", gUILayoutOption))
		{
			mCollapsed = true;
		}
		GUILayout.Space(20f);
		if (GUILayout.Button("SnapShot"))
		{
			TakeSnapShot();
		}
		if (GUILayout.Button("Refresh"))
		{
			TakeLightSnapShot();
		}
		GUILayout.Space(20f);
		if (GUILayout.Button(" Log ", gUILayoutOption))
		{
			OutputToLog();
		}
		GUILayout.EndHorizontal();
		RenderGUI_Stats();
	}

	private void CloseProfiler()
	{
		if (_OwnerGO != null)
		{
			Object.Destroy(_OwnerGO);
		}
	}

	private void RenderGUI_StatsViewTab(string Name, StatsViewTab Mode)
	{
		Color backgroundColor = GUI.backgroundColor;
		if (mStatsViewIndex != Mode)
		{
			GUI.backgroundColor = Color.gray;
		}
		if (GUILayout.Button(Name))
		{
			mStatsViewIndex = Mode;
		}
		GUI.backgroundColor = backgroundColor;
	}

	private void RenderGUI_Stats()
	{
		GUI_BeginContents();
		GUILayout.BeginHorizontal();
		RenderGUI_StatsViewTab("Current Stats", StatsViewTab.CURRENT_STATS);
		RenderGUI_StatsViewTab("Current Objects", StatsViewTab.CURRENT_OBJECTS);
		RenderGUI_StatsViewTab("Dif Stats", StatsViewTab.DIF_STATS);
		RenderGUI_StatsViewTab("Dif Objects", StatsViewTab.DIF_OBJECTS);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Filter:", GUILayout.Width(40f));
		mFilter_Text = GUILayout.TextField(mFilter_Text, GUILayout.ExpandWidth(expand: true));
		if (GUILayout.Button("X", GUILayout.Width(20f)))
		{
			mFilter_Text = "";
		}
		mFilter_ShowDependencies = GUILayout.Toggle(mFilter_ShowDependencies, "Dependencies", GUILayout.ExpandWidth(expand: false));
		GUILayout.EndHorizontal();
		switch (mStatsViewIndex)
		{
		case StatsViewTab.CURRENT_STATS:
			RenderGUI_Stats(mStats_Current1);
			break;
		case StatsViewTab.CURRENT_OBJECTS:
			RenderGUI_List(mList_Snapshot);
			break;
		case StatsViewTab.DIF_STATS:
			RenderGUI_Stats(mStats_Dif1);
			break;
		case StatsViewTab.DIF_OBJECTS:
			RenderGUI_List(mList_Differences);
			break;
		}
		GUI_EndContents();
	}

	private void RenderGUI_List(List<ObjMemDef> mListDef)
	{
		mScrollViewPos_Stats = GUILayout.BeginScrollView(mScrollViewPos_Stats, GUILayout.ExpandHeight(expand: true));
		float num = GUI.skin.button.lineHeight + (float)GUI.skin.button.margin.top + (float)GUI.skin.button.padding.top + (float)GUI.skin.button.padding.bottom;
		int num2 = (int)(mScrollViewPos_Stats.y / num);
		int num3 = num2 + (int)((float)Screen.height / num);
		float num4 = 0f;
		int num5 = 0;
		TextAnchor alignment = GUI.skin.label.alignment;
		GUI.skin.label.alignment = TextAnchor.MiddleRight;
		int i = 0;
		for (int count = mListDef.Count; i < count; i++)
		{
			ObjMemDef objMemDef = mListDef[i];
			if ((!mFilter_ShowDependencies && objMemDef._IsADependency) || (!string.IsNullOrEmpty(mFilter_Text) && !objMemDef._Name.Contains(mFilter_Text) && !objMemDef._ObjType.Contains(mFilter_Text)))
			{
				continue;
			}
			if (num5 > 0 && (num5 < num2 || num5 > num3))
			{
				num4 += num;
			}
			else
			{
				if (num4 > 0f)
				{
					GUILayout.Space(num4);
					num4 = 0f;
				}
				GUILayout.BeginHorizontal();
				GUILayout.Label((objMemDef._Size < 1048576) ? (objMemDef._Size / 1024 + " Kb") : ((objMemDef._Size / 1048576).ToString("0.00") + " Mb"), GUILayout.Width(80f));
				if (GUILayout.Button(objMemDef._ObjType.ToString(), GUILayout.Width(200f)))
				{
					mFilter_Text = objMemDef._ObjType;
				}
				GUILayout.Button(objMemDef._Name);
				GUILayout.EndHorizontal();
			}
			num5++;
		}
		GUI.skin.label.alignment = alignment;
		if (num4 > 0f)
		{
			GUILayout.Space(num4);
		}
		GUILayout.EndScrollView();
	}

	private void RenderGUI_Stats(List<KeyValuePair<string, int>> mStats)
	{
		mScrollViewPos_Stats = GUILayout.BeginScrollView(mScrollViewPos_Stats, GUILayout.ExpandHeight(expand: true));
		TextAnchor alignment = GUI.skin.label.alignment;
		GUI.skin.label.alignment = TextAnchor.MiddleRight;
		int i = 0;
		for (int count = mStats.Count; i < count; i++)
		{
			KeyValuePair<string, int> keyValuePair = mStats[i];
			GUILayout.BeginHorizontal();
			GUILayout.Label((keyValuePair.Value < 1048576) ? (keyValuePair.Value / 1024 + " Kb") : ((keyValuePair.Value / 1048576).ToString("0.00") + " Mb"), GUILayout.Width(80f));
			if (GUILayout.Button(keyValuePair.Key))
			{
				mFilter_Text = keyValuePair.Key;
				switch (mStatsViewIndex)
				{
				case StatsViewTab.CURRENT_STATS:
					mStatsViewIndex = StatsViewTab.CURRENT_OBJECTS;
					break;
				case StatsViewTab.DIF_STATS:
					mStatsViewIndex = StatsViewTab.DIF_OBJECTS;
					break;
				}
			}
			GUILayout.EndHorizontal();
		}
		GUI.skin.label.alignment = alignment;
		GUILayout.EndScrollView();
	}

	public void GUI_BeginContents()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Space(4f);
		GUILayout.BeginHorizontal("RL Background", GUILayout.ExpandHeight(expand: true));
		GUILayout.BeginVertical();
		GUILayout.Space(2f);
	}

	public void GUI_EndContents()
	{
		GUILayout.Space(3f);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
		GUILayout.EndHorizontal();
		GUILayout.Space(3f);
	}

	public void TakeSnapShot()
	{
		TakeLightSnapShot();
		mList_LastSnapshot.Clear();
		mList_LastSnapshot.Capacity = mList_Snapshot.Count;
		int i = 0;
		for (int count = mList_Snapshot.Count; i < count; i++)
		{
			mList_LastSnapshot.Add(mList_Snapshot[i]);
		}
	}

	private int IndexOfObjInArray(ref Object[] Objs, Object Obj)
	{
		int i = 0;
		for (int num = Objs.Length; i < num; i++)
		{
			if (Objs[i] == Obj)
			{
				return i;
			}
		}
		return -1;
	}

	public void TakeLightSnapShot()
	{
		Dictionary<string, List<ObjMemDef>> LastSnapshot = new Dictionary<string, List<ObjMemDef>>();
		int i = 0;
		for (int count = mList_LastSnapshot.Count; i < count; i++)
		{
			AddLastSnapShotElement(ref LastSnapshot, mList_LastSnapshot[i]);
		}
		mList_Differences.Clear();
		mList_Snapshot.Clear();
		int num = 0;
		mStats_Current.Clear();
		mStats_Dif.Clear();
		Object[] Objs = Resources.FindObjectsOfTypeAll(typeof(Object));
		int j = 0;
		for (int num2 = Objs.Length; j < num2; j++)
		{
			Object @object = Objs[j];
			int num3 = (int)Profiler.GetRuntimeMemorySizeLong(@object);
			num += num3;
			string text = @object.GetType().ToString();
			if (text.StartsWith("UnityEngine."))
			{
				text = text.Substring(12);
			}
			int value = 0;
			mStats_Current.TryGetValue(text, out value);
			mStats_Current[text] = value + num3;
			ObjMemDef objMemDef = new ObjMemDef(num3, text, @object.name, HasADependantInTheList(@object, ref Objs));
			mList_Snapshot.Add(objMemDef);
			if (!RemoveLastSnapShotElement(ref LastSnapshot, objMemDef))
			{
				mList_Differences.Add(objMemDef);
				value = 0;
				mStats_Dif.TryGetValue(text, out value);
				mStats_Dif[text] = value + num3;
			}
		}
		mStats_Dif1.Clear();
		mStats_Current1.Clear();
		foreach (KeyValuePair<string, int> item in mStats_Dif)
		{
			mStats_Dif1.Add(item);
		}
		mStats_Dif1.Sort((KeyValuePair<string, int> v1, KeyValuePair<string, int> v2) => v2.Value - v1.Value);
		foreach (KeyValuePair<string, int> item2 in mStats_Current)
		{
			mStats_Current1.Add(item2);
		}
		mStats_Current1.Sort((KeyValuePair<string, int> v1, KeyValuePair<string, int> v2) => v2.Value - v1.Value);
		mStats_Dif.Clear();
		mStats_Current.Clear();
		mList_Snapshot.Sort((ObjMemDef p1, ObjMemDef p2) => p2.CompareTo(ref p1));
		mList_Differences.Sort((ObjMemDef p1, ObjMemDef p2) => p2.CompareTo(ref p1));
	}

	private bool HasADependantInTheList(Object Obj, ref Object[] Objs)
	{
		GameObject gameObject = Obj as GameObject;
		if (gameObject == null)
		{
			return true;
		}
		return gameObject.transform.parent != null;
	}

	private void AddLastSnapShotElement(ref Dictionary<string, List<ObjMemDef>> LastSnapshot, ObjMemDef ObjDef)
	{
		List<ObjMemDef> value = null;
		if (!LastSnapshot.TryGetValue(ObjDef._Name, out value))
		{
			value = new List<ObjMemDef>();
			LastSnapshot[ObjDef._Name] = value;
		}
		value.Add(ObjDef);
	}

	private bool RemoveLastSnapShotElement(ref Dictionary<string, List<ObjMemDef>> LastSnapshot, ObjMemDef ObjDef)
	{
		List<ObjMemDef> value = null;
		if (!LastSnapshot.TryGetValue(ObjDef._Name, out value))
		{
			return false;
		}
		int num = value.FindIndex((ObjMemDef p) => ObjDef._ObjType == p._ObjType);
		if (num < 0)
		{
			return false;
		}
		value.RemoveAt(num);
		return true;
	}

	private void OutputToLog()
	{
	}
}
