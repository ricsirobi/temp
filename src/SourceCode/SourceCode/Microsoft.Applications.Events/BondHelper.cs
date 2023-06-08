using System.IO;
using Microsoft.Applications.Events.DataModels;
using UnityEngine;

namespace Microsoft.Applications.Events;

internal static class BondHelper
{
	public static void Serialize(CsEvent objToSerialize, MemoryStream outStream)
	{
		try
		{
			CompactBinaryProtocolWriter compactBinaryProtocolWriter = new CompactBinaryProtocolWriter();
			MiniBond.Serialize(compactBinaryProtocolWriter, objToSerialize, isBase: false);
			outStream.Write(compactBinaryProtocolWriter.Data.ToArray(), 0, compactBinaryProtocolWriter.Data.Count);
		}
		catch
		{
			Debug.LogError("Failed to serialize");
		}
	}
}
