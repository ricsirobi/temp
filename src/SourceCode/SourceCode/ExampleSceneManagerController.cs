using System;
using System.Collections.Generic;
using CI.WSANative.Advertising;
using CI.WSANative.Device;
using CI.WSANative.Dialogs;
using CI.WSANative.FileStorage;
using CI.WSANative.IAPStore;
using CI.WSANative.Mapping;
using CI.WSANative.Notification;
using CI.WSANative.Pickers;
using CI.WSANative.Security;
using CI.WSANative.Serialisers;
using CI.WSANative.Twitter;
using UnityEngine;

public class ExampleSceneManagerController : MonoBehaviour
{
	public void Start()
	{
	}

	public void CreateDialog()
	{
		WSANativeDialog.ShowDialogWithOptions("This is a title", "This is a message", new List<WSADialogCommand>
		{
			new WSADialogCommand("Yes"),
			new WSADialogCommand("No"),
			new WSADialogCommand("Cancel")
		}, 0, 2, delegate(WSADialogResult result)
		{
			if (result.ButtonPressed == "Yes")
			{
				WSANativeDialog.ShowDialog("Yes Pressed", "Yes was pressed!");
			}
			else if (result.ButtonPressed == "No")
			{
				WSANativeDialog.ShowDialog("No Pressed", "No was pressed!");
			}
			else if (result.ButtonPressed == "Cancel")
			{
				WSANativeDialog.ShowDialog("Cancel Pressed", "Cancel was pressed!");
			}
		});
	}

	public void CreatePopupMenu()
	{
		WSANativePopupMenu.ShowPopupMenu(Screen.width / 2, Screen.height / 2, new List<WSADialogCommand>
		{
			new WSADialogCommand("Option 1"),
			new WSADialogCommand("Option 2"),
			new WSADialogCommand("Option 3"),
			new WSADialogCommand("Option 4"),
			new WSADialogCommand("Option 5"),
			new WSADialogCommand("Option 6")
		}, WSAPopupMenuPlacement.Above, delegate(WSADialogResult result)
		{
			if (result.ButtonPressed == "Yes")
			{
				WSANativeDialog.ShowDialog("Yes Pressed", "Yes was pressed!");
			}
			else if (result.ButtonPressed == "No")
			{
				WSANativeDialog.ShowDialog("No Pressed", "No was pressed!");
			}
			else if (result.ButtonPressed == "Cancel")
			{
				WSANativeDialog.ShowDialog("Cancel Pressed", "Cancel was pressed!");
			}
		});
	}

	public void CreateToastNotification()
	{
		WSANativeNotification.ShowToastNotification("This is a title", "This is a description, This is a description, This is a description", null);
	}

	public void SaveFile()
	{
		CI.WSANative.FileStorage.WSAStorageFile wSAStorageFile = WSANativeStorageLibrary.CreateFile(WSAStorageLibrary.Local, "Test.txt");
		string text = WSANativeSerialisation.SerialiseToXML(new Test());
		wSAStorageFile.WriteText(text);
	}

	public void LoadFile()
	{
		if (WSANativeStorageLibrary.DoesFileExist(WSAStorageLibrary.Local, "Test.txt"))
		{
			WSANativeStorageLibrary.GetFile(WSAStorageLibrary.Local, "Test.txt", delegate(CI.WSANative.FileStorage.WSAStorageFile result)
			{
				WSANativeSerialisation.DeserialiseXML<Test>(result.ReadText());
			});
		}
	}

	public void DeleteFile()
	{
		if (WSANativeStorageLibrary.DoesFileExist(WSAStorageLibrary.Local, "Test.txt"))
		{
			WSANativeStorageLibrary.DeleteFile(WSAStorageLibrary.Local, "Test.txt");
		}
	}

	public void PurchaseProduct()
	{
		WSANativeStore.GetProductListings(delegate(List<WSAProduct> products)
		{
			if (products != null && products.Count > 0)
			{
				WSANativeStore.PurchaseProduct(products[0].Id, delegate(WSAPurchaseResult result)
				{
					if (result.Status == WSAPurchaseResultStatus.Succeeded)
					{
						WSANativeDialog.ShowDialog("Purchased", "YAY");
					}
					else
					{
						WSANativeDialog.ShowDialog("Not Purchased", "NAY");
					}
				});
			}
		});
	}

	public void PurchaseApp()
	{
		WSANativeStore.PurchaseApp(delegate(string response)
		{
			if (!string.IsNullOrEmpty(response))
			{
				WSANativeDialog.ShowDialog("Purchased", response);
			}
			else
			{
				WSANativeDialog.ShowDialog("Not Purchased", "NAY");
			}
		});
	}

	public void ShowFileOpenPicker()
	{
		WSANativeFilePicker.PickSingleFile("Select", WSAPickerViewMode.Thumbnail, WSAPickerLocationId.PicturesLibrary, new string[2] { ".png", ".jpg" }, delegate(CI.WSANative.Pickers.WSAStorageFile result)
		{
			if (result != null)
			{
				result.ReadBytes();
				result.ReadText();
			}
		});
	}

	public void ShowFileSavePicker()
	{
		WSANativeFilePicker.PickSaveFile("Save", ".txt", "Test Text File", WSAPickerLocationId.DocumentsLibrary, new List<KeyValuePair<string, IList<string>>>
		{
			new KeyValuePair<string, IList<string>>("Text Files", new List<string> { ".txt" })
		}, delegate(CI.WSANative.Pickers.WSAStorageFile result)
		{
			if (result != null)
			{
				result.WriteBytes(new byte[2]);
				result.WriteText("Hello World");
			}
		});
	}

	public void ShowFolderPicker()
	{
		WSANativeFolderPicker.PickSingleFolder("Ok", WSAPickerViewMode.List, WSAPickerLocationId.DocumentsLibrary, null, delegate
		{
		});
	}

	public void CreateInterstitialAd()
	{
		WSANativeInterstitialAd.Initialise(WSAInterstitialAdType.Microsoft, "d25517cb-12d4-4699-8bdc-52040c712cab", "11389925");
		WSANativeInterstitialAd.AdReady = (Action<WSAInterstitialAdType>)Delegate.Combine(WSANativeInterstitialAd.AdReady, (Action<WSAInterstitialAdType>)delegate(WSAInterstitialAdType adType)
		{
			if (adType == WSAInterstitialAdType.Microsoft)
			{
				WSANativeInterstitialAd.ShowAd(WSAInterstitialAdType.Microsoft);
			}
		});
		WSANativeInterstitialAd.RequestAd(WSAInterstitialAdType.Microsoft, WSAInterstitialAdVariant.Video);
	}

	public void CreateBannerAd()
	{
		WSANativeBannerAd.Initialise(WSABannerAdType.Microsoft, "3f83fe91-d6be-434d-a0ae-7351c5a997f1", "10865270");
		WSANativeBannerAd.CreatAd(WSABannerAdType.Microsoft, 728, 90, WSAAdVerticalPlacement.Top, WSAAdHorizontalPlacement.Centre);
	}

	public void LaunchMapsApp()
	{
		WSANativeMap.LaunchMapsApp("collection=point.40.726966_-74.006076_Some Business");
	}

	public void CreateMap()
	{
		int x = Screen.width / 2 - 350;
		int y = Screen.height / 2 - 350;
		WSANativeMap.CreateMap(string.Empty, 700, 700, new WSAPosition
		{
			X = x,
			Y = y
		}, new WSAGeoPoint
		{
			Latitude = 50.0,
			Longitude = 0.0
		}, 6, WSAMapInteractionMode.GestureAndControl);
	}

	public void DestroyMap()
	{
		WSANativeMap.DestroyMap();
	}

	public void AddPOI()
	{
		WSANativeMap.AddMapElement("You are here", new WSAGeoPoint
		{
			Latitude = 52.0,
			Longitude = 5.0
		});
	}

	public void ClearMap()
	{
		WSANativeMap.ClearMap();
	}

	public void CenterMap()
	{
		WSANativeMap.CenterMap(new WSAGeoPoint
		{
			Latitude = 52.0,
			Longitude = 5.0
		});
	}

	public void EnableFlashlight()
	{
		WSANativeDevice.EnableFlashlight(new WSANativeColour
		{
			Red = 0,
			Green = 0,
			Blue = byte.MaxValue
		});
	}

	public void DisableFlashlight()
	{
		WSANativeDevice.DisableFlashlight();
	}

	public void CameraCapture()
	{
		WSANativeDevice.CapturePicture(512, 512, delegate
		{
		});
	}

	public void EncryptDecrypt()
	{
		string data = WSANativeSecurity.SymmetricEncrypt("ffffffffffffffffffffffffffffffff", "aaaaaaaaaaaaaaaa", "Tesing123");
		WSANativeSecurity.SymmetricDecrypt("ffffffffffffffffffffffffffffffff", "aaaaaaaaaaaaaaaa", data);
	}

	public void FacebookLogin()
	{
	}

	public void TwitterLogin()
	{
		WSANativeTwitter.Initialise("consumerKey", "consumerSecret", "https://www.twitter.com");
		WSANativeTwitter.Login(delegate(WSATwitterLoginResult result)
		{
			if (result.Success)
			{
				WSANativeTwitter.GetUserDetails(includeEmail: true, delegate(WSATwitterResponse response)
				{
					if (response.Success)
					{
						_ = response.Data;
					}
				});
			}
		});
	}
}
