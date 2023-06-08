using System.Collections.Generic;
using Microsoft.Applications.Events.DataModels;

namespace Microsoft.Applications.Events;

internal static class MiniBond
{
	public static void Serialize(CompactBinaryProtocolWriter writer, Ingest value, bool isBase)
	{
		writer.WriteStructBegin(null, isBase);
		if (value.time != 0L)
		{
			writer.WriteFieldBegin(17, 1);
			writer.WriteInt64(value.time);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.clientIp))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.clientIp);
			writer.WriteFieldEnd();
		}
		if (value.auth != 0L)
		{
			writer.WriteFieldBegin(17, 3);
			writer.WriteInt64(value.auth);
			writer.WriteFieldEnd();
		}
		if (value.quality != 0L)
		{
			writer.WriteFieldBegin(17, 4);
			writer.WriteInt64(value.quality);
			writer.WriteFieldEnd();
		}
		if (value.uploadTime != 0L)
		{
			writer.WriteFieldBegin(17, 5);
			writer.WriteInt64(value.uploadTime);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.userAgent))
		{
			writer.WriteFieldBegin(9, 6);
			writer.WriteString(value.userAgent);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.client))
		{
			writer.WriteFieldBegin(9, 7);
			writer.WriteString(value.client);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, User value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.id))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.id);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.localId))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.localId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.authId))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.authId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.locale))
		{
			writer.WriteFieldBegin(9, 4);
			writer.WriteString(value.locale);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Device value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.id))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.id);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.localId))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.localId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.authId))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.authId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.authSecId))
		{
			writer.WriteFieldBegin(9, 4);
			writer.WriteString(value.authSecId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.deviceClass))
		{
			writer.WriteFieldBegin(9, 5);
			writer.WriteString(value.deviceClass);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.orgId))
		{
			writer.WriteFieldBegin(9, 6);
			writer.WriteString(value.orgId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.orgAuthId))
		{
			writer.WriteFieldBegin(9, 7);
			writer.WriteString(value.orgAuthId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.make))
		{
			writer.WriteFieldBegin(9, 8);
			writer.WriteString(value.make);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.model))
		{
			writer.WriteFieldBegin(9, 9);
			writer.WriteString(value.model);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Os value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.locale))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.locale);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.expId))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.expId);
			writer.WriteFieldEnd();
		}
		if (value.bootId != 0)
		{
			writer.WriteFieldBegin(16, 3);
			writer.WriteInt32(value.bootId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.name))
		{
			writer.WriteFieldBegin(9, 4);
			writer.WriteString(value.name);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.ver))
		{
			writer.WriteFieldBegin(9, 5);
			writer.WriteString(value.ver);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, App value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.expId))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.expId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.userId))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.userId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.env))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.env);
			writer.WriteFieldEnd();
		}
		if (value.asId != 0)
		{
			writer.WriteFieldBegin(16, 4);
			writer.WriteInt32(value.asId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.id))
		{
			writer.WriteFieldBegin(9, 5);
			writer.WriteString(value.id);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.ver))
		{
			writer.WriteFieldBegin(9, 6);
			writer.WriteString(value.ver);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.locale))
		{
			writer.WriteFieldBegin(9, 7);
			writer.WriteString(value.locale);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.name))
		{
			writer.WriteFieldBegin(9, 8);
			writer.WriteString(value.name);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Utc value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.stId))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.stId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.aId))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.aId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.raId))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.raId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.op))
		{
			writer.WriteFieldBegin(9, 4);
			writer.WriteString(value.op);
			writer.WriteFieldEnd();
		}
		if (value.cat != 0L)
		{
			writer.WriteFieldBegin(17, 5);
			writer.WriteInt64(value.cat);
			writer.WriteFieldEnd();
		}
		if (value.flags != 0L)
		{
			writer.WriteFieldBegin(17, 6);
			writer.WriteInt64(value.flags);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.sqmId))
		{
			writer.WriteFieldBegin(9, 7);
			writer.WriteString(value.sqmId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.mon))
		{
			writer.WriteFieldBegin(9, 9);
			writer.WriteString(value.mon);
			writer.WriteFieldEnd();
		}
		if (value.cpId != 0)
		{
			writer.WriteFieldBegin(16, 10);
			writer.WriteInt32(value.cpId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.bSeq))
		{
			writer.WriteFieldBegin(9, 11);
			writer.WriteString(value.bSeq);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.epoch))
		{
			writer.WriteFieldBegin(9, 12);
			writer.WriteString(value.epoch);
			writer.WriteFieldEnd();
		}
		if (value.seq != 0L)
		{
			writer.WriteFieldBegin(17, 13);
			writer.WriteInt64(value.seq);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Xbl value, bool isBase)
	{
		if (value.claims != null && value.claims.Count != 0)
		{
			writer.WriteFieldBegin(13, 5);
			writer.WriteMapContainerBegin((ushort)value.claims.Count, 9, 9);
			foreach (KeyValuePair<string, string> claim in value.claims)
			{
				writer.WriteString(claim.Key);
				writer.WriteString(claim.Value);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.nbf))
		{
			writer.WriteFieldBegin(9, 10);
			writer.WriteString(value.nbf);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.exp))
		{
			writer.WriteFieldBegin(9, 20);
			writer.WriteString(value.exp);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.sbx))
		{
			writer.WriteFieldBegin(9, 30);
			writer.WriteString(value.sbx);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.dty))
		{
			writer.WriteFieldBegin(9, 40);
			writer.WriteString(value.dty);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.did))
		{
			writer.WriteFieldBegin(9, 50);
			writer.WriteString(value.did);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.xid))
		{
			writer.WriteFieldBegin(9, 60);
			writer.WriteString(value.xid);
			writer.WriteFieldEnd();
		}
		if (value.uts != 0L)
		{
			writer.WriteFieldBegin(6, 70);
			writer.WriteUInt64(value.uts);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.pid))
		{
			writer.WriteFieldBegin(9, 80);
			writer.WriteString(value.pid);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.dvr))
		{
			writer.WriteFieldBegin(9, 90);
			writer.WriteString(value.dvr);
			writer.WriteFieldEnd();
		}
		if (value.tid != 0)
		{
			writer.WriteFieldBegin(5, 100);
			writer.WriteUInt32(value.tid);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.tvr))
		{
			writer.WriteFieldBegin(9, 110);
			writer.WriteString(value.tvr);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.sty))
		{
			writer.WriteFieldBegin(9, 120);
			writer.WriteString(value.sty);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.sid))
		{
			writer.WriteFieldBegin(9, 130);
			writer.WriteString(value.sid);
			writer.WriteFieldEnd();
		}
		if (value.eid != 0)
		{
			writer.WriteFieldBegin(17, 140);
			writer.WriteInt64(value.eid.Value);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.ip))
		{
			writer.WriteFieldBegin(9, 150);
			writer.WriteString(value.ip);
			writer.WriteFieldEnd();
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Javascript value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.libVer))
		{
			writer.WriteFieldBegin(9, 10);
			writer.WriteString(value.libVer);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.osName))
		{
			writer.WriteFieldBegin(9, 15);
			writer.WriteString(value.osName);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.browser))
		{
			writer.WriteFieldBegin(9, 20);
			writer.WriteString(value.browser);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.browserVersion))
		{
			writer.WriteFieldBegin(9, 21);
			writer.WriteString(value.browserVersion);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.platform))
		{
			writer.WriteFieldBegin(9, 25);
			writer.WriteString(value.platform);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.make))
		{
			writer.WriteFieldBegin(9, 30);
			writer.WriteString(value.make);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.model))
		{
			writer.WriteFieldBegin(9, 35);
			writer.WriteString(value.model);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.screenSize))
		{
			writer.WriteFieldBegin(9, 40);
			writer.WriteString(value.screenSize);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.mc1Id))
		{
			writer.WriteFieldBegin(9, 50);
			writer.WriteString(value.mc1Id);
			writer.WriteFieldEnd();
		}
		if (value.mc1Lu != 0L)
		{
			writer.WriteFieldBegin(6, 60);
			writer.WriteUInt64(value.mc1Lu);
			writer.WriteFieldEnd();
		}
		if (value.isMc1New)
		{
			writer.WriteFieldBegin(2, 70);
			writer.WriteBool(value.isMc1New);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.ms0))
		{
			writer.WriteFieldBegin(9, 80);
			writer.WriteString(value.ms0);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.anid))
		{
			writer.WriteFieldBegin(9, 90);
			writer.WriteString(value.anid);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.a))
		{
			writer.WriteFieldBegin(9, 100);
			writer.WriteString(value.a);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.msResearch))
		{
			writer.WriteFieldBegin(9, 110);
			writer.WriteString(value.msResearch);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.csrvc))
		{
			writer.WriteFieldBegin(9, 120);
			writer.WriteString(value.csrvc);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.rtCell))
		{
			writer.WriteFieldBegin(9, 130);
			writer.WriteString(value.rtCell);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.rtEndAction))
		{
			writer.WriteFieldBegin(9, 140);
			writer.WriteString(value.rtEndAction);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.rtPermId))
		{
			writer.WriteFieldBegin(9, 150);
			writer.WriteString(value.rtPermId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.r))
		{
			writer.WriteFieldBegin(9, 160);
			writer.WriteString(value.r);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.wtFpc))
		{
			writer.WriteFieldBegin(9, 170);
			writer.WriteString(value.wtFpc);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.omniId))
		{
			writer.WriteFieldBegin(9, 180);
			writer.WriteString(value.omniId);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.gsfxSession))
		{
			writer.WriteFieldBegin(9, 190);
			writer.WriteString(value.gsfxSession);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.domain))
		{
			writer.WriteFieldBegin(9, 200);
			writer.WriteString(value.domain);
			writer.WriteFieldEnd();
		}
		if (!string.IsNullOrEmpty(value.dnt))
		{
			writer.WriteFieldBegin(9, 999);
			writer.WriteString(value.dnt);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 999);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Protocol value, bool isBase)
	{
		if (value.metadataCrc != 0)
		{
			writer.WriteFieldBegin(16, 1);
			writer.WriteInt32(value.metadataCrc);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(16, 1);
		}
		if (value.ticketKeys != null && value.ticketKeys.Count != 0)
		{
			writer.WriteFieldBegin(11, 2);
			writer.WriteContainerBegin((ushort)value.ticketKeys.Count, 11);
			foreach (List<string> ticketKey in value.ticketKeys)
			{
				writer.WriteContainerBegin((ushort)ticketKey.Count, 9);
				foreach (string item in ticketKey)
				{
					writer.WriteString(item);
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 2);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Receipts value, bool isBase)
	{
		if (value.originalTime != 0L)
		{
			writer.WriteFieldBegin(17, 1);
			writer.WriteInt64(value.originalTime);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(17, 1);
		}
		if (value.uploadTime != 0L)
		{
			writer.WriteFieldBegin(17, 2);
			writer.WriteInt64(value.uploadTime);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(17, 2);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Net value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.provider))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.provider);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 1);
		}
		if (!string.IsNullOrEmpty(value.cost))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.cost);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 2);
		}
		if (!string.IsNullOrEmpty(value.type))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.type);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 3);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Loc value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.id))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.id);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 1);
		}
		if (!string.IsNullOrEmpty(value.country))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.country);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 2);
		}
		if (!string.IsNullOrEmpty(value.timezone))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.timezone);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 3);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Sdk value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.libVer))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.libVer);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 1);
		}
		if (!string.IsNullOrEmpty(value.epoch))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.epoch);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 2);
		}
		if (value.seq != 0L)
		{
			writer.WriteFieldBegin(17, 3);
			writer.WriteInt64(value.seq);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 3);
		}
		if (!string.IsNullOrEmpty(value.installId))
		{
			writer.WriteFieldBegin(9, 4);
			writer.WriteString(value.installId);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 4);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, PII value, bool isBase)
	{
		if (value.Kind != 0)
		{
			writer.WriteFieldBegin(16, 1);
			writer.WriteInt32((int)value.Kind);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(16, 1);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, CustomerContent value, bool isBase)
	{
		if (value.Kind != 0)
		{
			writer.WriteFieldBegin(16, 1);
			writer.WriteInt32((int)value.Kind);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(16, 1);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Attributes value, bool isBase)
	{
		if (value.pii != null)
		{
			writer.WriteFieldBegin(11, 1);
			writer.WriteContainerBegin((ushort)value.pii.Count, 10);
			foreach (PII item in value.pii)
			{
				Serialize(writer, item, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 1);
		}
		if (value.customerContent != null)
		{
			writer.WriteFieldBegin(11, 2);
			writer.WriteContainerBegin((ushort)value.customerContent.Count, 10);
			foreach (CustomerContent item2 in value.customerContent)
			{
				Serialize(writer, item2, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 2);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Value value, bool isBase)
	{
		if (value.type != ValueKind.ValueString)
		{
			writer.WriteFieldBegin(16, 1);
			writer.WriteInt32((int)value.type);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(16, 1);
		}
		if (value.attributes != null)
		{
			writer.WriteFieldBegin(11, 2);
			writer.WriteContainerBegin((ushort)value.attributes.Count, 10);
			foreach (Attributes attribute in value.attributes)
			{
				Serialize(writer, attribute, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 2);
		}
		if (!string.IsNullOrEmpty(value.stringValue))
		{
			writer.WriteFieldBegin(9, 3);
			writer.WriteString(value.stringValue);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 3);
		}
		if (value.longValue != 0L)
		{
			writer.WriteFieldBegin(17, 4);
			writer.WriteInt64(value.longValue);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(17, 4);
		}
		if (value.doubleValue != 0.0)
		{
			writer.WriteFieldBegin(8, 5);
			writer.WriteDouble(value.doubleValue);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(8, 5);
		}
		if (value.guidValue != null && value.guidValue.Count != 0)
		{
			writer.WriteFieldBegin(11, 6);
			writer.WriteContainerBegin((ushort)value.guidValue.Count, 11);
			foreach (List<byte> item in value.guidValue)
			{
				writer.WriteContainerBegin((ushort)item.Count, 3);
				foreach (byte item2 in item)
				{
					writer.WriteUInt8(item2);
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 6);
		}
		if (value.stringArray != null && value.stringArray.Count != 0)
		{
			writer.WriteFieldBegin(11, 10);
			writer.WriteContainerBegin((ushort)value.stringArray.Count, 11);
			foreach (List<string> item3 in value.stringArray)
			{
				writer.WriteContainerBegin((ushort)item3.Count, 9);
				foreach (string item4 in item3)
				{
					writer.WriteString(item4);
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 10);
		}
		if (value.longArray != null && value.longArray.Count != 0)
		{
			writer.WriteFieldBegin(11, 11);
			writer.WriteContainerBegin((ushort)value.longArray.Count, 11);
			foreach (List<long> item5 in value.longArray)
			{
				writer.WriteContainerBegin((ushort)item5.Count, 17);
				foreach (long item6 in item5)
				{
					writer.WriteInt64(item6);
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 11);
		}
		if (value.doubleArray != null && value.doubleArray.Count != 0)
		{
			writer.WriteFieldBegin(11, 12);
			writer.WriteContainerBegin((ushort)value.doubleArray.Count, 11);
			foreach (List<double> item7 in value.doubleArray)
			{
				writer.WriteContainerBegin((ushort)item7.Count, 8);
				foreach (double item8 in item7)
				{
					writer.WriteDouble(item8);
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 12);
		}
		if (value.guidArray != null && value.guidArray.Count != 0)
		{
			writer.WriteFieldBegin(11, 13);
			writer.WriteContainerBegin((ushort)value.guidArray.Count, 11);
			foreach (List<List<long>> item9 in value.guidArray)
			{
				writer.WriteContainerBegin((ushort)item9.Count, 11);
				foreach (List<long> item10 in item9)
				{
					writer.WriteContainerBegin((ushort)item10.Count, 17);
					foreach (long item11 in item10)
					{
						writer.WriteInt64(item11);
					}
					writer.WriteContainerEnd();
				}
				writer.WriteContainerEnd();
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 13);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, Data value, bool isBase)
	{
		if (value.properties != null && value.properties.Count != 0)
		{
			writer.WriteFieldBegin(13, 1);
			writer.WriteMapContainerBegin((ushort)value.properties.Count, 9, 10);
			foreach (KeyValuePair<string, Value> property in value.properties)
			{
				writer.WriteString(property.Key);
				Serialize(writer, property.Value, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(13, 1);
		}
		writer.WriteStructEnd(isBase);
	}

	public static void Serialize(CompactBinaryProtocolWriter writer, CsEvent value, bool isBase)
	{
		if (!string.IsNullOrEmpty(value.ver))
		{
			writer.WriteFieldBegin(9, 1);
			writer.WriteString(value.ver);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 1);
		}
		if (!string.IsNullOrEmpty(value.name))
		{
			writer.WriteFieldBegin(9, 2);
			writer.WriteString(value.name);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 2);
		}
		if (value.time != 0L)
		{
			writer.WriteFieldBegin(17, 3);
			writer.WriteInt64(value.time);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(17, 3);
		}
		if (value.popSample != 100.0)
		{
			writer.WriteFieldBegin(8, 4);
			writer.WriteDouble(value.popSample);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(8, 4);
		}
		if (!string.IsNullOrEmpty(value.iKey))
		{
			writer.WriteFieldBegin(9, 5);
			writer.WriteString(value.iKey);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 5);
		}
		if (value.flags != 0L)
		{
			writer.WriteFieldBegin(17, 6);
			writer.WriteInt64(value.flags);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(17, 6);
		}
		if (!string.IsNullOrEmpty(value.cV))
		{
			writer.WriteFieldBegin(9, 7);
			writer.WriteString(value.cV);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 7);
		}
		if (value.extIngest != null && value.extIngest.Count != 0)
		{
			writer.WriteFieldBegin(11, 20);
			writer.WriteContainerBegin((ushort)value.extIngest.Count, 10);
			foreach (Ingest item in value.extIngest)
			{
				Serialize(writer, item, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 20);
		}
		if (value.extProtocol != null)
		{
			writer.WriteFieldBegin(11, 21);
			writer.WriteContainerBegin((ushort)value.extProtocol.Count, 10);
			foreach (Protocol item2 in value.extProtocol)
			{
				Serialize(writer, item2, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 21);
		}
		if (value.extUser != null && value.extUser.Count != 0)
		{
			writer.WriteFieldBegin(11, 22);
			writer.WriteContainerBegin((ushort)value.extUser.Count, 10);
			foreach (User item3 in value.extUser)
			{
				Serialize(writer, item3, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 22);
		}
		if (value.extDevice != null && value.extDevice.Count != 0)
		{
			writer.WriteFieldBegin(11, 23);
			writer.WriteContainerBegin((ushort)value.extDevice.Count, 10);
			foreach (Device item4 in value.extDevice)
			{
				Serialize(writer, item4, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 23);
		}
		if (value.extOs != null && value.extOs.Count != 0)
		{
			writer.WriteFieldBegin(11, 24);
			writer.WriteContainerBegin((ushort)value.extOs.Count, 10);
			foreach (Os extO in value.extOs)
			{
				Serialize(writer, extO, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 24);
		}
		if (value.extApp != null && value.extApp.Count != 0)
		{
			writer.WriteFieldBegin(11, 25);
			writer.WriteContainerBegin((ushort)value.extApp.Count, 10);
			foreach (App item5 in value.extApp)
			{
				Serialize(writer, item5, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 25);
		}
		if (value.extUtc != null && value.extUtc.Count != 0)
		{
			writer.WriteFieldBegin(11, 26);
			writer.WriteContainerBegin((ushort)value.extUtc.Count, 10);
			foreach (Utc item6 in value.extUtc)
			{
				Serialize(writer, item6, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 26);
		}
		if (value.extXbl != null && value.extXbl.Count != 0)
		{
			writer.WriteFieldBegin(11, 27);
			writer.WriteContainerBegin((ushort)value.extXbl.Count, 10);
			foreach (Xbl item7 in value.extXbl)
			{
				Serialize(writer, item7, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 27);
		}
		if (value.extJavascript != null && value.extJavascript.Count != 0)
		{
			writer.WriteFieldBegin(11, 28);
			writer.WriteContainerBegin((ushort)value.extJavascript.Count, 10);
			foreach (Javascript item8 in value.extJavascript)
			{
				Serialize(writer, item8, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 28);
		}
		if (value.extReceipts != null && value.extReceipts.Count != 0)
		{
			writer.WriteFieldBegin(11, 29);
			writer.WriteContainerBegin((ushort)value.extReceipts.Count, 10);
			foreach (Receipts extReceipt in value.extReceipts)
			{
				Serialize(writer, extReceipt, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 29);
		}
		if (value.extNet != null && value.extNet.Count != 0)
		{
			writer.WriteFieldBegin(11, 31);
			writer.WriteContainerBegin((ushort)value.extNet.Count, 10);
			foreach (Net item9 in value.extNet)
			{
				Serialize(writer, item9, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 31);
		}
		if (value.extSdk != null && value.extSdk.Count != 0)
		{
			writer.WriteFieldBegin(11, 32);
			writer.WriteContainerBegin((ushort)value.extSdk.Count, 10);
			foreach (Sdk item10 in value.extSdk)
			{
				Serialize(writer, item10, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 32);
		}
		if (value.extLoc != null && value.extLoc.Count != 0)
		{
			writer.WriteFieldBegin(11, 33);
			writer.WriteContainerBegin((ushort)value.extLoc.Count, 10);
			foreach (Loc item11 in value.extLoc)
			{
				Serialize(writer, item11, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 33);
		}
		if (value.ext != null && value.ext.Count != 0)
		{
			writer.WriteFieldBegin(11, 41);
			writer.WriteContainerBegin((ushort)value.ext.Count, 10);
			foreach (Data item12 in value.ext)
			{
				Serialize(writer, item12, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 41);
		}
		if (value.tags != null && value.tags.Count != 0)
		{
			writer.WriteFieldBegin(13, 51);
			writer.WriteMapContainerBegin((ushort)value.tags.Count, 9, 9);
			foreach (KeyValuePair<string, string> tag in value.tags)
			{
				writer.WriteString(tag.Key);
				writer.WriteString(tag.Value);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(13, 51);
		}
		if (!string.IsNullOrEmpty(value.baseType))
		{
			writer.WriteFieldBegin(9, 60);
			writer.WriteString(value.baseType);
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(9, 60);
		}
		if (value.baseData != null && value.baseData.Count != 0)
		{
			writer.WriteFieldBegin(11, 61);
			writer.WriteContainerBegin((ushort)value.baseData.Count, 10);
			foreach (Data baseDatum in value.baseData)
			{
				Serialize(writer, baseDatum, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 61);
		}
		if (value.data != null && value.data.Count != 0)
		{
			writer.WriteFieldBegin(11, 70);
			writer.WriteContainerBegin((ushort)value.data.Count, 10);
			foreach (Data datum in value.data)
			{
				Serialize(writer, datum, isBase: false);
			}
			writer.WriteContainerEnd();
			writer.WriteFieldEnd();
		}
		else
		{
			writer.WriteFieldOmitted(11, 70);
		}
		writer.WriteStructEnd(isBase);
	}
}
