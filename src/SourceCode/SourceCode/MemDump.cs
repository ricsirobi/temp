using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public class MemDump
{
	public class MemDumpData
	{
		public string _Name = "";

		public string _Info = "";

		public int _Size;
	}

	private static bool mSortBySize;

	private static StreamWriter fOut;

	private static List<MemDumpData> mTextures;

	private static List<MemDumpData> mMeshes;

	private static List<MemDumpData> mAudioClip;

	private static List<MemDumpData> mMaterials;

	private static List<MemDumpData> mAnimationClips;

	public static string WriteToFile(string identifier = "")
	{
		mTextures = new List<MemDumpData>();
		mMeshes = new List<MemDumpData>();
		mAudioClip = new List<MemDumpData>();
		mMaterials = new List<MemDumpData>();
		mAnimationClips = new List<MemDumpData>();
		int num = 0;
		string text = (UtPlatform.IsEditor() ? ("Assets/MemDump_" + RsResourceManager.pCurrentLevel + identifier + ".txt") : (Application.persistentDataPath + "/MemDump_" + RsResourceManager.pCurrentLevel + identifier + ".txt"));
		fOut = new StreamWriter(text);
		fOut.WriteLine("Platform : " + Application.platform);
		fOut.WriteLine("Time : " + DateTime.Now);
		fOut.WriteLine("");
		fOut.WriteLine("---------------------------------------------------------------");
		int num2 = 0;
		num2 = Enlist<Texture>(mTextures);
		fOut.WriteLine("Total Memory of all Textures : " + GetFormattedSize(num2));
		num += num2;
		num2 = Enlist<AudioClip>(mAudioClip);
		fOut.WriteLine("Total Memory of all AudioClip : " + GetFormattedSize(num2));
		num += num2;
		num2 = Enlist<Mesh>(mMeshes);
		fOut.WriteLine("Total Memory of all Meshes : " + GetFormattedSize(num2));
		num += num2;
		num2 = Enlist<Material>(mMaterials);
		fOut.WriteLine("Total Memory of all Materials : " + GetFormattedSize(num2));
		num += num2;
		num2 = Enlist<AnimationClip>(mAnimationClips);
		fOut.WriteLine("Total Memory of all AnimationClips : " + GetFormattedSize(num2));
		num += num2;
		fOut.WriteLine("---------------------------------------------------------------");
		fOut.WriteLine("Used Heap Size : " + GetFormattedSize(Profiler.usedHeapSizeLong));
		fOut.WriteLine("---------------------------------------------------------------");
		fOut.WriteLine("Total Memory of all Assets in Memory : " + GetFormattedSize(num));
		if (mSortBySize)
		{
			mTextures.Sort(SortBySize);
			mMeshes.Sort(SortBySize);
			mAudioClip.Sort(SortBySize);
			mMaterials.Sort(SortBySize);
			mAnimationClips.Sort(SortBySize);
		}
		else
		{
			mTextures.Sort(SortByName);
			mMeshes.Sort(SortByName);
			mAudioClip.Sort(SortByName);
			mMaterials.Sort(SortByName);
			mAnimationClips.Sort(SortByName);
		}
		Dump("Textures", mTextures);
		Dump("Meshes", mMeshes);
		Dump("AudioClip", mAudioClip);
		Dump("Materials", mMaterials);
		Dump("AnimationClips", mAnimationClips);
		DumpResources();
		DumpAtlasData();
		DumpUiTextureData();
		DumpFontData();
		fOut.Close();
		Debug.Log("Memory usage dumped to : " + text);
		return text;
	}

	private static int SortBySize(MemDumpData data1, MemDumpData data2)
	{
		return data2._Size.CompareTo(data1._Size);
	}

	private static int SortByName(MemDumpData data1, MemDumpData data2)
	{
		return data2._Name.CompareTo(data1._Name);
	}

	private static string GetFormattedSize(double inSize)
	{
		string text = " Bytes";
		if (inSize >= 1024.0)
		{
			inSize /= 1024.0;
			text = " KB";
			if (inSize >= 1024.0)
			{
				inSize /= 1024.0;
				text = " MB";
			}
		}
		return inSize.ToString("0.00") + text;
	}

	private static int Enlist<TYPE>(List<MemDumpData> inData) where TYPE : class
	{
		TYPE[] obj = Resources.FindObjectsOfTypeAll(typeof(TYPE)) as TYPE[];
		int num = 0;
		TYPE[] array = obj;
		foreach (TYPE val in array)
		{
			string text = "";
			int num2 = (int)Profiler.GetRuntimeMemorySizeLong(val as UnityEngine.Object);
			if (val is Texture)
			{
				Texture texture = val as Texture;
				if (texture != null)
				{
					text = texture.width + "x" + texture.height;
					num2 = UtMobileUtilities.GetTextureMemoryInt(val as Texture);
					Texture2D texture2D = val as Texture2D;
					if (texture2D != null)
					{
						text = text + " (" + texture2D.format.ToString() + ")";
					}
				}
			}
			else if (val is Mesh)
			{
				Mesh mesh = val as Mesh;
				if (mesh != null)
				{
					text = "Vertices " + mesh.vertexCount + ", Trianles " + mesh.triangles.Length;
				}
			}
			else if (val is Material)
			{
				Material material = val as Material;
				if (material != null)
				{
					text = "Shader -> " + material.shader.name;
				}
			}
			num += num2;
			MemDumpData memDumpData = new MemDumpData();
			memDumpData._Size = num2;
			memDumpData._Name = (val as UnityEngine.Object).name;
			memDumpData._Info = text;
			inData.Add(memDumpData);
		}
		return num;
	}

	private static void Dump(string tag, List<MemDumpData> inData)
	{
		int num = 0;
		fOut.WriteLine("");
		fOut.WriteLine("");
		fOut.WriteLine("======================================================");
		fOut.WriteLine("======================================================");
		fOut.WriteLine("Total " + tag + " : " + inData.Count);
		foreach (MemDumpData inDatum in inData)
		{
			fOut.WriteLine("{0,-35}({1,5}) : {2,10:F}", inDatum._Name, inDatum._Info, UtMobileUtilities.FormatBytes(inDatum._Size));
			num += inDatum._Size;
		}
		fOut.WriteLine("------------------------------------------------------");
		fOut.WriteLine("Total Memory : " + UtMobileUtilities.FormatBytes(num));
	}

	private static void DumpResources()
	{
		List<string> resourcesList = RsResourceManager.GetResourcesList();
		fOut.WriteLine("--------------------------Bundles Loaded By RsResourceManager-------------------------------");
		fOut.WriteLine("Total Bundles : " + resourcesList.Count);
		foreach (string item in resourcesList)
		{
			fOut.WriteLine(item);
		}
		fOut.WriteLine("--------------------------------------------------------------------------------------------");
	}

	private static void DumpAtlasData()
	{
		fOut.WriteLine("--------------------------Loaded Atlas-------------------------------");
		foreach (ImageReferenceData pAtlasDatum in AtlasManager.pAtlasData)
		{
			fOut.WriteLine(pAtlasDatum.pRefName + " : " + pAtlasDatum.pRefCount);
		}
		fOut.WriteLine("--------------------------------------------------------------------------------------------");
	}

	private static void DumpUiTextureData()
	{
		fOut.WriteLine("--------------------------Loaded UiTextures-------------------------------");
		foreach (ImageReferenceData pTextureDatum in AtlasManager.pTextureData)
		{
			fOut.WriteLine(pTextureDatum.pRefName + " : " + pTextureDatum.pRefCount);
		}
		fOut.WriteLine("--------------------------------------------------------------------------------------------");
	}

	private static void DumpFontData()
	{
		fOut.WriteLine("--------------------------Loaded Fonts-------------------------------");
		foreach (FontManager.FontData pFontDatum in FontManager.pFontData)
		{
			fOut.WriteLine(pFontDatum.pFontName + " : " + pFontDatum.pRefCount);
		}
		fOut.WriteLine("--------------------------------------------------------------------------------------------");
	}
}
