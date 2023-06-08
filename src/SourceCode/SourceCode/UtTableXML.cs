using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

public class UtTableXML
{
	private enum ParseMode
	{
		NONE,
		SCHEMA_ID,
		FIELD_ID,
		FIELD_TYPE,
		TABLE_ID,
		TABLE_PROP,
		RECORDS
	}

	public class Field
	{
		public int index;

		public int arraySize = -1;

		public string name = "";

		public UtType type;

		public bool isArray;

		public Field(int index, string name, UtType type)
		{
			this.index = index;
			this.name = name;
			this.type = type;
			switch (type)
			{
			case UtType.ULONG_ARRAY:
			case UtType.SLONG_ARRAY:
			case UtType.FLOAT_ARRAY:
			case UtType.STRING_ARRAY:
			case UtType.VECTOR3D_ARRAY:
			case UtType.IDSTRING_ARRAY:
			case UtType.KEYSTRING_ARRAY:
				isArray = true;
				break;
			}
		}
	}

	public class RecordValue
	{
		public int field = -1;

		public int arrayIdx = -1;

		public string value = "";

		public RecordValue(int field, string value)
		{
			this.field = field;
			this.value = value;
		}

		public RecordValue(int field, int arrayIdx, string value)
		{
			this.field = field;
			this.arrayIdx = arrayIdx;
			this.value = value;
		}
	}

	private interface ParseNode
	{
		ParseNode Add(XmlReader reader);

		void SetData(XmlReader reader);

		void End(XmlReader reader);
	}

	private class Root : ParseNode
	{
		public UtTableXML parser;

		public Root(UtTableXML parser)
		{
			this.parser = parser;
		}

		public UtTableXML GetParser()
		{
			return parser;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Workbook")
			{
				return new Workbook(this);
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
		}
	}

	private class Workbook : ParseNode
	{
		public Root parent;

		public Workbook(Root parent)
		{
			this.parent = parent;
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Worksheet")
			{
				return new Worksheet(this);
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
		}
	}

	private class Worksheet : ParseNode
	{
		public Workbook parent;

		public Worksheet(Workbook parent)
		{
			this.parent = parent;
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Table")
			{
				return new Table(this);
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
		}
	}

	private class Table : ParseNode
	{
		public Worksheet parent;

		public int rowIndex;

		public Table(Worksheet parent)
		{
			this.parent = parent;
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Row")
			{
				bool isEmptyElement = reader.IsEmptyElement;
				rowIndex++;
				while (reader.MoveToNextAttribute())
				{
					if (reader.Name == "ss:Index")
					{
						rowIndex = Convert.ToInt32(reader.Value);
					}
				}
				if (isEmptyElement)
				{
					return null;
				}
				return new Row(this, rowIndex);
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
		}
	}

	private class Row : ParseNode
	{
		public Table parent;

		public int index;

		public int cellIndex;

		public string propKey = "";

		public bool commentFound;

		public List<RecordValue> recValues;

		public Row(Table parent, int index)
		{
			this.parent = parent;
			this.index = index;
			UtTableXML parser = GetParser();
			if (parser != null && parser.mode == ParseMode.RECORDS && parser.tableValid)
			{
				recValues = new List<RecordValue>();
			}
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Cell")
			{
				bool isEmptyElement = reader.IsEmptyElement;
				cellIndex++;
				while (reader.MoveToNextAttribute())
				{
					if (reader.Name == "ss:Index")
					{
						cellIndex = Convert.ToInt32(reader.Value);
					}
				}
				if (isEmptyElement)
				{
					return null;
				}
				int field = -1;
				UtTableXML parser = GetParser();
				if (parser != null)
				{
					field = parser.FindField(cellIndex);
				}
				return new Cell(this, cellIndex, field, "");
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
			UtTableXML parser = GetParser();
			switch (parser.mode)
			{
			case ParseMode.SCHEMA_ID:
				parser.mode = ParseMode.NONE;
				parser.schema.Clear();
				break;
			case ParseMode.FIELD_ID:
				parser.mode = ParseMode.NONE;
				break;
			case ParseMode.FIELD_TYPE:
				parser.mode = ParseMode.NONE;
				break;
			case ParseMode.TABLE_ID:
				parser.mode = ParseMode.NONE;
				break;
			case ParseMode.TABLE_PROP:
				parser.mode = ParseMode.NONE;
				break;
			case ParseMode.RECORDS:
				if (!parser.tableValid || recValues == null || recValues.Count <= 0)
				{
					break;
				}
				parser.OnRecordBegin();
				{
					foreach (RecordValue recValue in recValues)
					{
						parser.OnFieldValue(parser.schema[recValue.field], recValue.value);
					}
					break;
				}
			}
		}
	}

	private class Cell : ParseNode
	{
		public Row parent;

		public int index;

		public int field = -1;

		public string data = "";

		public Cell(Row parent, int index, int field, string data)
		{
			this.parent = parent;
			this.index = index;
			this.field = field;
			this.data = data;
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			if (reader.Name == "Data")
			{
				return new Data(this);
			}
			return null;
		}

		public void SetData(XmlReader reader)
		{
		}

		public void End(XmlReader reader)
		{
			if (parent.commentFound)
			{
				return;
			}
			if (data.StartsWith("//") && data != "//-Prop")
			{
				parent.commentFound = true;
				return;
			}
			UtTableXML parser = GetParser();
			switch (parser.mode)
			{
			case ParseMode.NONE:
			{
				if (index == 1 && parser.modeMap.TryGetValue(data, out var value4))
				{
					parser.mode = value4;
				}
				break;
			}
			case ParseMode.FIELD_ID:
				if (data != "")
				{
					Field item = new Field(index, data, UtType.NONE);
					parser.schema.Add(item);
				}
				break;
			case ParseMode.FIELD_TYPE:
			{
				if (!(data != "") || field == -1 || !parser.typeMap.TryGetValue(data, out var value3))
				{
					break;
				}
				parser.schema[field].type = value3;
				switch (value3)
				{
				case UtType.ULONG_ARRAY:
				case UtType.SLONG_ARRAY:
				case UtType.FLOAT_ARRAY:
				case UtType.STRING_ARRAY:
				case UtType.VECTOR3D_ARRAY:
				case UtType.IDSTRING_ARRAY:
				case UtType.KEYSTRING_ARRAY:
					parser.schema[field].isArray = true;
					break;
				}
				if (parser.schema[field].isArray)
				{
					int num = field + 1;
					if (num < parser.schema.Count)
					{
						parser.schema[field].arraySize = parser.schema[num].index - parser.schema[field].index;
					}
					else
					{
						parser.schema[field].arraySize = -1;
					}
				}
				break;
			}
			case ParseMode.TABLE_ID:
				if (index == 2)
				{
					parser.tableValid = parser.OnTableBegin(data, parser.schema) == 0;
				}
				break;
			case ParseMode.TABLE_PROP:
				if (parser.tableValid)
				{
					if (index == 2 && parent != null && data != "")
					{
						parent.propKey = data.ToUpper();
					}
					if (index == 3 && parent != null)
					{
						parser.OnPropertyValue(parent.propKey, data);
					}
				}
				break;
			case ParseMode.RECORDS:
			{
				ParseMode value2;
				if (parser.tableValid)
				{
					ParseMode value;
					if (field != -1)
					{
						if (parent.recValues != null)
						{
							if (parser.schema[field].isArray)
							{
								int arrayIdx = index - parser.schema[field].index;
								parent.recValues.Add(new RecordValue(field, arrayIdx, data));
							}
							else
							{
								parent.recValues.Add(new RecordValue(field, data));
							}
						}
					}
					else if (index == 1 && data != "" && parser.modeMap.TryGetValue(data, out value))
					{
						parser.OnTableEnd();
						parser.tableValid = false;
						parser.mode = value;
					}
				}
				else if (index == 1 && data != "" && parser.modeMap.TryGetValue(data, out value2))
				{
					parser.mode = value2;
				}
				break;
			}
			case ParseMode.SCHEMA_ID:
				break;
			}
		}
	}

	private class Data : ParseNode
	{
		public Cell parent;

		public Data(Cell parent)
		{
			this.parent = parent;
		}

		public UtTableXML GetParser()
		{
			if (parent != null)
			{
				return parent.GetParser();
			}
			return null;
		}

		public ParseNode Add(XmlReader reader)
		{
			return null;
		}

		public void SetData(XmlReader reader)
		{
			if (parent != null)
			{
				parent.data = reader.Value;
			}
		}

		public void End(XmlReader reader)
		{
		}
	}

	private const string commentMark = "//";

	private const string propTag = "//-Prop";

	private Dictionary<string, ParseMode> modeMap;

	private Dictionary<string, UtType> typeMap;

	private List<Field> schema;

	private Stack<ParseNode> parserStack;

	private XmlReader reader;

	private ParseMode mode;

	private ParseNode root;

	private bool tableValid;

	public UtTableXML()
	{
		typeMap = new Dictionary<string, UtType>();
		typeMap["int"] = UtType.SLONG;
		typeMap["uint"] = UtType.ULONG;
		typeMap["float"] = UtType.FLOAT;
		typeMap["vector"] = UtType.VECTOR3D;
		typeMap["string"] = UtType.STRING;
		typeMap["idstring"] = UtType.IDSTRING;
		typeMap["key"] = UtType.KEYSTRING;
		typeMap["[int]"] = UtType.SLONG_ARRAY;
		typeMap["[uint]"] = UtType.ULONG_ARRAY;
		typeMap["[float]"] = UtType.FLOAT_ARRAY;
		typeMap["[vector]"] = UtType.VECTOR3D_ARRAY;
		typeMap["[string]"] = UtType.STRING_ARRAY;
		typeMap["[idstring]"] = UtType.IDSTRING_ARRAY;
		typeMap["[key]"] = UtType.KEYSTRING_ARRAY;
		modeMap = new Dictionary<string, ParseMode>();
		modeMap["Schema ID"] = ParseMode.SCHEMA_ID;
		modeMap["Field ID"] = ParseMode.FIELD_ID;
		modeMap["Field Type"] = ParseMode.FIELD_TYPE;
		modeMap["Table ID"] = ParseMode.TABLE_ID;
		modeMap["//-Prop"] = ParseMode.TABLE_PROP;
		modeMap["Records"] = ParseMode.RECORDS;
		modeMap["EndRecords"] = ParseMode.NONE;
	}

	public int FindField(int cellIndex)
	{
		for (int i = 0; i < schema.Count; i++)
		{
			if (schema[i].index == cellIndex)
			{
				return i;
			}
			if (schema[i].index < cellIndex && schema[i].isArray)
			{
				int arraySize = schema[i].arraySize;
				if (arraySize == -1)
				{
					return i;
				}
				if (cellIndex - schema[i].index < arraySize)
				{
					return i;
				}
			}
		}
		return -1;
	}

	public string[] ParseVector(string inValue)
	{
		return inValue.TrimStart("["[0]).TrimEnd("]"[0]).Split(","[0]);
	}

	public virtual int OnTableBegin(string id, List<Field> schema)
	{
		return 0;
	}

	public virtual int OnPropertyValue(string key, string value)
	{
		return 0;
	}

	public virtual int OnRecordBegin()
	{
		return 0;
	}

	public virtual int OnFieldValue(Field field, string value)
	{
		return 0;
	}

	public virtual int OnTableEnd()
	{
		return 0;
	}

	public bool LoadString(string fileData)
	{
		using StringReader stringReader = new StringReader(fileData);
		if (stringReader == null)
		{
			return false;
		}
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.CloseInput = true;
		using (reader = XmlReader.Create(stringReader, xmlReaderSettings))
		{
			if (reader == null)
			{
				return false;
			}
			Parse(reader);
			reader = null;
			return true;
		}
	}

	public bool LoadFile(string fileName)
	{
		using StreamReader streamReader = new StreamReader(File.OpenRead(fileName));
		if (streamReader == null)
		{
			return false;
		}
		XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
		xmlReaderSettings.CloseInput = true;
		using (reader = XmlReader.Create(streamReader, xmlReaderSettings))
		{
			if (reader == null)
			{
				return false;
			}
			Parse(reader);
			reader = null;
			return true;
		}
	}

	private void CreateRootNode()
	{
		root = new Root(this);
		parserStack.Push(root);
	}

	private void Parse(XmlReader reader)
	{
		parserStack = new Stack<ParseNode>();
		schema = new List<Field>();
		mode = ParseMode.NONE;
		tableValid = false;
		CreateRootNode();
		while (reader.Read())
		{
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
			{
				ParseNode parseNode = parserStack.Peek().Add(reader);
				if (parseNode != null)
				{
					parserStack.Push(parseNode);
				}
				else if (!reader.IsEmptyElement)
				{
					reader.Skip();
				}
				break;
			}
			case XmlNodeType.Text:
				parserStack.Peek().SetData(reader);
				break;
			case XmlNodeType.EndElement:
				parserStack.Peek().End(reader);
				parserStack.Pop();
				break;
			}
		}
		schema = null;
		parserStack = null;
	}
}
