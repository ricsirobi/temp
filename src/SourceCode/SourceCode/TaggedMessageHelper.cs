using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TaggedMessageHelper
{
	public MessageInfo Message;

	public Dictionary<string, string> MemberMessage;

	public Dictionary<string, string> NonMemberMessage;

	public Dictionary<string, string> MemberImageUrl;

	public Dictionary<string, string> NonMemberImageUrl;

	public Dictionary<string, string> MemberLinkUrl;

	public Dictionary<string, string> NonMemberLinkUrl;

	public Dictionary<string, string> MemberAudioUrl;

	public Dictionary<string, string> NonMemberAudioUrl;

	private static Regex matchKeyValue = new Regex("\\[{2}(.*?)\\]{2}=\\[{2}(.*?)\\]{2}(\\&{2})?");

	private static Regex matchWord = new Regex("\\[{2}(.*?)\\]{2}");

	public string SubType
	{
		get
		{
			string result = string.Empty;
			if (MemberMessage != null && MemberMessage.ContainsKey("SubType"))
			{
				result = MemberMessage["SubType"];
			}
			return result;
		}
	}

	public int? MessageID => Message.MessageID;

	public TaggedMessageHelper(MessageInfo messageInfo)
	{
		Message = messageInfo;
		MemberMessage = Match(messageInfo.MemberMessage);
		NonMemberMessage = Match(messageInfo.NonMemberMessage);
		MemberImageUrl = Match(messageInfo.MemberImageUrl);
		NonMemberImageUrl = Match(messageInfo.NonMemberImageUrl);
		MemberLinkUrl = Match(messageInfo.MemberLinkUrl);
		NonMemberLinkUrl = Match(messageInfo.NonMemberLinkUrl);
		MemberAudioUrl = Match(messageInfo.MemberAudioUrl);
		NonMemberAudioUrl = Match(messageInfo.NonMemberAudioUrl);
	}

	public static Dictionary<string, string> Match(string stringToParse)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (string.IsNullOrEmpty(stringToParse))
		{
			return dictionary;
		}
		foreach (Match item in matchKeyValue.Matches(stringToParse))
		{
			MatchCollection matchCollection = matchWord.Matches(item.Value);
			if (matchCollection.Count == 2)
			{
				string value = matchCollection[0].Value;
				value = value.Substring(2, value.Length - 4);
				string value2 = matchCollection[1].Value;
				value2 = value2.Substring(2, value2.Length - 4);
				dictionary.Add(value, value2);
			}
		}
		return dictionary;
	}
}
