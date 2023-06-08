using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using SimpleJSON;

namespace Xsolla;

public class XsollaFormElement : IParseble
{
	public class Option
	{
		private string value;

		private string label;

		private Option()
		{
		}

		public Option(string value, string label)
		{
			this.value = value;
			this.label = label;
		}

		public string GetValue()
		{
			return value;
		}

		public void SetValue(string value)
		{
			this.value = value;
		}

		public string GetLabel()
		{
			return label;
		}

		public void SetLabel(string label)
		{
			this.label = label;
		}

		public override string ToString()
		{
			return string.Format("[Option]\n value= " + value + "\n label= " + label);
		}
	}

	public class TableOptions : IParseble
	{
		public List<string> _head;

		public List<string> _body;

		public TableOptions()
		{
			_head = new List<string>();
			_body = new List<string>();
		}

		public IParseble Parse(JSONNode rootNode)
		{
			foreach (JSONNode item in rootNode["head"].AsArray)
			{
				_head.Add(item.AsArray[0]);
			}
			foreach (JSONNode item2 in rootNode["body"].AsArray)
			{
				_body.Add(item2.AsArray[0]);
			}
			return this;
		}
	}

	public const int TYPE_UNKNOWN = -1;

	public const int TYPE_HIDDEN = 0;

	public const int TYPE_TEXT = 1;

	public const int TYPE_SELECT = 2;

	public const int TYPE_VISIBLE = 3;

	public const int TYPE_TABLE = 4;

	public const int TYPE_CHECK = 5;

	public const int TYPE_HTML = 6;

	public const int TYPE_LABEL = 7;

	private string name;

	private string title;

	private string type;

	private string example;

	private string value;

	private TableOptions tableOptions;

	private List<Option> options;

	private bool isMandatory;

	private bool isReadonly;

	private bool isVisible;

	private bool isPakets;

	private string tooltip;

	private string javascript;

	public string GetName()
	{
		return name;
	}

	public string GetTitle()
	{
		return GetFormatString(title);
	}

	public string GetValue()
	{
		return value;
	}

	public string GetExample()
	{
		if ("null".Equals(example))
		{
			return title;
		}
		return example;
	}

	public List<Option> GetOptions()
	{
		return options;
	}

	public TableOptions GetTableOptions()
	{
		return tableOptions;
	}

	public bool IsVisible()
	{
		return isVisible;
	}

	public bool IsReadOnly()
	{
		return isReadonly;
	}

	public bool IsPackets()
	{
		return isPakets;
	}

	public int GetElementType()
	{
		return type switch
		{
			"hidden" => 0, 
			"text" => 1, 
			"select" => 2, 
			"isVisible" => 3, 
			"table" => 4, 
			"check" => 5, 
			"html" => 6, 
			"label" => 7, 
			_ => -1, 
		};
	}

	public void SetValue(string value)
	{
		this.value = value;
	}

	private string GetFormatString(string pStr)
	{
		if (pStr != null)
		{
			return Regex.Replace(pStr, "<[^>]*>", string.Empty);
		}
		return string.Empty;
	}

	private List<Option> ParseOptions(JSONNode optionsNode)
	{
		return new List<Option>();
	}

	private void SetName(string name)
	{
		this.name = name;
	}

	private void SetTitle(string title)
	{
		this.title = title;
	}

	private void SetType(string type)
	{
		this.type = type;
	}

	private void SetExample(string example)
	{
		this.example = example;
	}

	private void SetOptions(JSONNode optionsNode)
	{
		List<Option> list = new List<Option>(optionsNode.Count);
		IEnumerator<JSONNode> enumerator = optionsNode.Childs.GetEnumerator();
		while (enumerator.MoveNext())
		{
			JSONNode current = enumerator.Current;
			list.Add(new Option(current["value"], current["label"]));
		}
		if (GetElementType() == 4)
		{
			tableOptions = new TableOptions();
			tableOptions.Parse(optionsNode);
		}
		options = list;
	}

	private void SetMandatory(bool isMandatory)
	{
		this.isMandatory = isMandatory;
	}

	private void SetReadonly(bool isReadonly)
	{
		this.isReadonly = isReadonly;
	}

	private void SetVisible(bool isVisible)
	{
		this.isVisible = isVisible;
	}

	private void SetPakets(bool isPakets)
	{
		this.isPakets = isPakets;
	}

	private void SetTooltip(string tooltip)
	{
		this.tooltip = tooltip;
	}

	private void SetJavascript(string javascript)
	{
		this.javascript = javascript;
	}

	public IParseble Parse(JSONNode obj)
	{
		SetName(obj["name"]);
		SetTitle(obj["title"]);
		SetType(obj["type"]);
		SetExample(obj["example"]);
		SetValue(obj["value"]);
		SetOptions(obj["options"]);
		SetMandatory(obj["isMandatory"].AsInt > 0);
		SetReadonly(obj["isReadonly"].AsInt > 0);
		SetVisible(obj["isVisible"].AsInt > 0);
		SetPakets(obj["isPakets"].AsInt > 0);
		SetTooltip(obj["tooltip"]);
		SetJavascript(obj["javascript"]);
		return this;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Option option in options)
		{
			stringBuilder.Append("\t/").Append(option.ToString());
		}
		return string.Format("[XsollaFormElement]\n name= " + name + "\n title= " + title + "\n type=" + type + "\n options=" + stringBuilder.ToString() + "\n isMandatory=" + isMandatory + "\n isReadonly=" + isReadonly + "\n isVisible=" + isVisible + "\n tooltip=" + tooltip + "\n javascript=" + javascript);
	}
}
