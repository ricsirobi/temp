using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DumpAtlasInfo
{
	private static StreamWriter mWriter;

	public static string WriteToFile(string identifier = "")
	{
		string text = (UtPlatform.IsEditor() ? ("Assets/AtlasDump_" + RsResourceManager.pCurrentLevel + identifier + ".txt") : (Application.persistentDataPath + "/AtlasDump_" + RsResourceManager.pCurrentLevel + identifier + ".txt"));
		mWriter = new StreamWriter(text);
		Component[] array = Resources.FindObjectsOfTypeAll(typeof(KAUI)) as KAUI[];
		array = array;
		for (int i = 0; i < array.Length; i++)
		{
			Transform transform = ((KAUI)array[i]).transform;
			UIWidget[] componentsInChildren = transform.GetComponentsInChildren<UIWidget>();
			if (componentsInChildren == null || componentsInChildren.Length == 0)
			{
				continue;
			}
			List<DataHolder> list = new List<DataHolder>();
			List<DataHolder> list2 = new List<DataHolder>();
			UIWidget[] array2 = componentsInChildren;
			foreach (UIWidget uIWidget in array2)
			{
				if (uIWidget == null)
				{
					continue;
				}
				if (uIWidget is UISprite)
				{
					UISprite sprite = uIWidget as UISprite;
					if (!(sprite.atlas == null) && list.Find((DataHolder inlodedAtlas) => inlodedAtlas._Name == sprite.atlas.name) == null)
					{
						string textureMemory = UtMobileUtilities.GetTextureMemory(sprite.atlas.spriteMaterial.mainTexture as Texture2D);
						Texture2D texture2D = sprite.atlas.spriteMaterial.mainTexture as Texture2D;
						TextureFormat inTexFormat = TextureFormat.BGRA32;
						if (texture2D != null)
						{
							inTexFormat = texture2D.format;
						}
						list.Add(new DataHolder(sprite.atlas.name, textureMemory, inTexFormat));
					}
				}
				else
				{
					if (!(uIWidget is UILabel))
					{
						continue;
					}
					UILabel label = uIWidget as UILabel;
					if (!(label.bitmapFont == null) && list2.Find((DataHolder inlodedAtlas) => inlodedAtlas._Name == label.bitmapFont.name) == null)
					{
						Texture2D texture2D2 = label.bitmapFont.material.mainTexture as Texture2D;
						TextureFormat inTexFormat2 = TextureFormat.BGRA32;
						if (texture2D2 != null)
						{
							inTexFormat2 = texture2D2.format;
						}
						string textureMemory2 = UtMobileUtilities.GetTextureMemory(label.bitmapFont.material.mainTexture as Texture2D);
						list2.Add(new DataHolder(label.bitmapFont.name, textureMemory2, inTexFormat2));
					}
				}
			}
			WriteToFile(transform.name, list, list2);
			list.Clear();
			list2.Clear();
		}
		mWriter.Close();
		Debug.Log("Atlas/Font Info written to : " + text);
		return text;
	}

	private static void WriteToFile(string objName, List<DataHolder> atlasNames, List<DataHolder> fontNames)
	{
		mWriter.WriteLine(objName);
		mWriter.WriteLine("\tAtlases : ");
		foreach (DataHolder atlasName in atlasNames)
		{
			mWriter.WriteLine("\t\t" + atlasName._Name + " (" + atlasName._Size + ") " + atlasName._Format);
		}
		mWriter.WriteLine("");
		mWriter.WriteLine("\tFonts : ");
		foreach (DataHolder fontName in fontNames)
		{
			mWriter.WriteLine("\t\t" + fontName._Name + " (" + fontName._Size + ") " + fontName._Format);
		}
		mWriter.WriteLine("");
		mWriter.WriteLine("--------------------------------------------------------");
	}
}
