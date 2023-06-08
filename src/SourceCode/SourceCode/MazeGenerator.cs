using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : KAMonoBase
{
	public MazeSet[] _MazeSets;

	[Tooltip("Exit Marker Name")]
	public string _ExitMarker;

	private MazeSet mSelectedSet;

	private MazeLayout mSelectedLayout;

	private MazeRoom[] mSelectedRooms;

	private bool mIsValidMazeExit;

	private List<string> mAssetList;

	private List<GameObject> mMazePieces;

	private readonly string EXIT = "MarkerExit";

	private int RandomWeighted(int[] inWeights)
	{
		int num = 0;
		foreach (int num2 in inWeights)
		{
			num += num2;
		}
		int num3 = 0;
		int j = 0;
		int num4 = Random.Range(0, num + 1);
		for (; j < inWeights.Length; j++)
		{
			num3 += inWeights[j];
			if (num3 >= num4)
			{
				break;
			}
		}
		return j;
	}

	private int[] RandomOrderWeighted(int[] inWeights)
	{
		int[] array = new int[inWeights.Length];
		for (int i = 0; i < inWeights.Length; i++)
		{
			array[i] = i;
		}
		List<int> list = new List<int>(inWeights.Length);
		list.AddRange(inWeights);
		for (int j = 0; j < inWeights.Length; j++)
		{
			int[] array2 = new int[list.Count];
			list.CopyTo(array2);
			int num = RandomWeighted(array2);
			if (num != 0)
			{
				int num2 = array[0];
				array[0] = array[j + num];
				array[j + num] = num2;
				list[num] = list[0];
			}
			list.RemoveAt(0);
		}
		return array;
	}

	private void SelectRooms()
	{
		if (mSelectedLayout._MazeRooms.Length != 0)
		{
			if (mSelectedLayout._Random)
			{
				UtUtilities.Shuffle(mSelectedLayout._MazeRooms);
			}
			mSelectedRooms = mSelectedLayout._MazeRooms;
		}
		else
		{
			mSelectedRooms = null;
		}
	}

	private TYPE SelectMazeObject<TYPE>(TYPE[] inArray) where TYPE : MazeInfo
	{
		TYPE result = null;
		if (inArray.Length != 0)
		{
			if (inArray.Length > 1)
			{
				int[] array = new int[inArray.Length];
				for (int i = 0; i < inArray.Length; i++)
				{
					int num = inArray[i]._Weightage;
					if (num < 1)
					{
						num = 1;
					}
					array[i] = num;
				}
				return inArray[RandomWeighted(array)];
			}
			return inArray[0];
		}
		return result;
	}

	private void ArrangeMaze()
	{
		if (string.IsNullOrEmpty(_ExitMarker))
		{
			_ExitMarker = EXIT;
		}
		mMazePieces[0].transform.position = Vector3.zero;
		for (int i = 1; i < mMazePieces.Count; i++)
		{
			mMazePieces[i].transform.position = mMazePieces[i - 1].transform.Find(_ExitMarker).position;
		}
	}

	private bool GenerateMaze()
	{
		bool result = false;
		mSelectedSet = SelectMazeObject(_MazeSets);
		if (mSelectedSet != null)
		{
			mSelectedLayout = SelectMazeObject(mSelectedSet._Layouts);
			if (mSelectedLayout != null)
			{
				SelectRooms();
				if (mSelectedRooms != null)
				{
					if (mAssetList == null)
					{
						mAssetList = new List<string>();
					}
					else
					{
						mAssetList.Clear();
					}
					if (mMazePieces == null)
					{
						mMazePieces = new List<GameObject>();
					}
					else
					{
						mMazePieces.Clear();
					}
					mAssetList.Add(mSelectedSet._Entrance);
					MazeRoom[] array = mSelectedRooms;
					foreach (MazeRoom mazeRoom in array)
					{
						MazeRoom.Variation variation = SelectMazeObject(mazeRoom._RoomVersions);
						mAssetList.Add(variation._BundlePath);
					}
					mAssetList.Add(mSelectedSet._Exit);
					new RsAssetLoader().Load(mAssetList.ToArray(), null, OnAssetLoad);
					result = true;
				}
			}
		}
		return result;
	}

	private void OnAssetLoad(RsAssetLoader inLoader, RsResourceLoadEvent inEvent, float inProgress, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			return;
		}
		for (int i = 0; i < mAssetList.Count; i++)
		{
			GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromBundle(mAssetList[i], typeof(GameObject));
			if (gameObject != null)
			{
				mMazePieces.Add(Object.Instantiate(gameObject));
			}
		}
		ArrangeMaze();
	}

	private void OnEnable()
	{
		GenerateMaze();
	}
}
