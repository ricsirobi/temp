using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Zendesk.UI;

namespace UnityEngine.UI.Extensions;

[ExecuteInEditMode]
public class ZendeskTextPic : Text, IPointerClickHandler, IEventSystemHandler, IPointerExitHandler, IPointerEnterHandler, ISelectHandler
{
	[Serializable]
	public struct IconName
	{
		public string name;

		public Sprite sprite;

		public Vector2 offset;

		public Vector2 scale;
	}

	[Serializable]
	public class HrefClickEvent : UnityEvent<string>
	{
	}

	[Serializable]
	public class HrefInfo
	{
		public int startIndex;

		public int endIndex;

		public string name;

		public readonly List<Rect> boxes = new List<Rect>();
	}

	public IconName[] inspectorIconList;

	[Tooltip("Global scaling factor for all images")]
	public float ImageScalingFactor = 1f;

	public string hyperlinkColor = "#00a656";

	public Vector2 imageOffset = Vector2.zero;

	public bool isCreating_m_HrefInfos = true;

	[SerializeField]
	private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

	private readonly List<Image> m_ImagesPool = new List<Image>();

	private readonly List<GameObject> culled_ImagesPool = new List<GameObject>();

	private bool clearImages;

	private Object thisLock = new Object();

	private readonly List<int> m_ImagesVertexIndex = new List<int>();

	private static readonly Regex s_Regex = new Regex("<quad name=(.+?) size=(\\d*\\.?\\d+%?) width=(\\d*\\.?\\d+%?) />", RegexOptions.Singleline);

	private static readonly Regex s_HrefRegex = new Regex("<a href=([^>\\n\\s]+)>(.*?)(</a>)", RegexOptions.Singleline);

	private string fixedString;

	private bool updateQuad;

	private string m_OutputText;

	private Button button;

	private bool selected;

	private List<Vector2> positions = new List<Vector2>();

	private string previousText = "";

	private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

	private static readonly StringBuilder s_TextBuilder = new StringBuilder();

	private MatchCollection matches;

	private MatchCollection href_matches;

	private MatchCollection removeCharacters;

	private int picIndex;

	private int vertIndex;

	private bool usesNewRendering;

	private static readonly Regex remove_Regex = new Regex("<b>|</b>|<i>|</i>|<size=.*?>|</size>|<color=.*?>|</color>|<material=.*?>|</material>|<quad name=(.+?) size=(\\d*\\.?\\d+%?) width=(\\d*\\.?\\d+%?) />|<a href=([^>\\n\\s]+)>|</a>|\\s", RegexOptions.Singleline);

	private List<int> indexes = new List<int>();

	private int charactersRemoved;

	private int startCharactersRemoved;

	private int endCharactersRemoved;

	private int count;

	private int indexText;

	private string originalText;

	private UIVertex vert;

	private Vector2 lp;

	private ZendeskLinkHandler zendeskLinkHandler;

	public HrefClickEvent onHrefClick
	{
		get
		{
			return m_OnHrefClick;
		}
		set
		{
			m_OnHrefClick = value;
		}
	}

	public bool Selected
	{
		get
		{
			return selected;
		}
		set
		{
			selected = value;
		}
	}

	public void ResetIconList()
	{
		Reset_m_HrefInfos();
		base.Start();
	}

	protected void UpdateQuadImage()
	{
		m_OutputText = GetOutputText();
		matches = s_Regex.Matches(m_OutputText);
		if (matches != null && matches.Count > 0)
		{
			for (int i = 0; i < matches.Count; i++)
			{
				m_ImagesPool.RemoveAll((Image image) => image == null);
				if (m_ImagesPool.Count == 0)
				{
					GetComponentsInChildren(includeInactive: true, m_ImagesPool);
				}
				if (matches.Count > m_ImagesPool.Count)
				{
					GameObject gameObject = DefaultControls.CreateImage(default(DefaultControls.Resources));
					gameObject.layer = base.gameObject.layer;
					RectTransform rectTransform = gameObject.transform as RectTransform;
					if ((bool)rectTransform)
					{
						rectTransform.SetParent(base.rectTransform);
						rectTransform.anchoredPosition3D = Vector3.zero;
						rectTransform.localRotation = Quaternion.identity;
						rectTransform.localScale = Vector3.one;
					}
					m_ImagesPool.Add(gameObject.GetComponent<Image>());
				}
				string value = matches[i].Groups[1].Value;
				Image image2 = m_ImagesPool[i];
				_ = Vector2.zero;
				if ((image2.sprite == null || image2.sprite.name != value) && inspectorIconList != null && inspectorIconList.Length != 0)
				{
					for (int j = 0; j < inspectorIconList.Length; j++)
					{
						if (inspectorIconList[j].name == value)
						{
							image2.sprite = inspectorIconList[j].sprite;
							image2.preserveAspect = true;
							image2.rectTransform.sizeDelta = new Vector2((float)base.fontSize * ImageScalingFactor * inspectorIconList[j].scale.x, (float)base.fontSize * ImageScalingFactor * inspectorIconList[j].scale.y);
							_ = ref inspectorIconList[j];
							break;
						}
					}
				}
				image2.color = new Color32(7, 167, 51, byte.MaxValue);
				image2.enabled = true;
				if (positions.Count > 0 && i < positions.Count)
				{
					image2.rectTransform.anchorMax = new Vector2(0f, 0f);
					image2.rectTransform.anchorMin = new Vector2(0f, 0f);
					image2.rectTransform.anchoredPosition = new Vector2(positions[i].x, positions[i].y - 9f);
				}
			}
		}
		else
		{
			for (int num = m_ImagesPool.Count - 1; num > 0; num--)
			{
				if ((bool)m_ImagesPool[num] && !culled_ImagesPool.Contains(m_ImagesPool[num].gameObject))
				{
					culled_ImagesPool.Add(m_ImagesPool[num].gameObject);
					m_ImagesPool.Remove(m_ImagesPool[num]);
				}
			}
		}
		for (int num2 = m_ImagesPool.Count - 1; num2 >= matches.Count; num2--)
		{
			if (num2 >= 0 && m_ImagesPool.Count > 0 && (bool)m_ImagesPool[num2] && !culled_ImagesPool.Contains(m_ImagesPool[num2].gameObject))
			{
				culled_ImagesPool.Add(m_ImagesPool[num2].gameObject);
				m_ImagesPool.Remove(m_ImagesPool[num2]);
			}
		}
		if (culled_ImagesPool.Count > 0)
		{
			clearImages = true;
		}
	}

	private void Reset_m_HrefInfos()
	{
		previousText = text;
		m_HrefInfos.Clear();
		isCreating_m_HrefInfos = true;
	}

	protected string GetOutputText()
	{
		s_TextBuilder.Length = 0;
		indexText = 0;
		fixedString = text;
		if (inspectorIconList != null && inspectorIconList.Length != 0)
		{
			for (int i = 0; i < inspectorIconList.Length; i++)
			{
				if (!string.IsNullOrEmpty(inspectorIconList[i].name))
				{
					fixedString = fixedString.Replace(inspectorIconList[i].name, "<quad name=" + inspectorIconList[i].name + " size=" + base.fontSize + " width=1 />");
				}
			}
		}
		count = 0;
		href_matches = s_HrefRegex.Matches(fixedString);
		if (href_matches != null && href_matches.Count > 0)
		{
			for (int j = 0; j < href_matches.Count; j++)
			{
				s_TextBuilder.Append(fixedString.Substring(indexText, href_matches[j].Index - indexText));
				s_TextBuilder.Append("<color=" + hyperlinkColor + ">");
				System.Text.RegularExpressions.Group group = href_matches[j].Groups[1];
				if (isCreating_m_HrefInfos)
				{
					HrefInfo item = new HrefInfo
					{
						startIndex = (usesNewRendering ? s_TextBuilder.Length : (s_TextBuilder.Length * 4)),
						endIndex = (usesNewRendering ? (s_TextBuilder.Length + href_matches[j].Groups[2].Length - 1) : ((s_TextBuilder.Length + href_matches[j].Groups[2].Length - 1) * 4 + 3)),
						name = group.Value
					};
					m_HrefInfos.Add(item);
				}
				else if (count <= m_HrefInfos.Count - 1)
				{
					m_HrefInfos[count].startIndex = (usesNewRendering ? s_TextBuilder.Length : (s_TextBuilder.Length * 4));
					m_HrefInfos[count].endIndex = (usesNewRendering ? (s_TextBuilder.Length + href_matches[j].Groups[2].Length - 1) : ((s_TextBuilder.Length + href_matches[j].Groups[2].Length - 1) * 4 + 3));
					count++;
				}
				s_TextBuilder.Append(href_matches[j].Groups[2].Value);
				s_TextBuilder.Append("</color>");
				indexText = href_matches[j].Index + href_matches[j].Length;
			}
		}
		if (isCreating_m_HrefInfos)
		{
			isCreating_m_HrefInfos = false;
		}
		s_TextBuilder.Append(fixedString.Substring(indexText, fixedString.Length - indexText));
		m_OutputText = s_TextBuilder.ToString();
		m_ImagesVertexIndex.Clear();
		matches = s_Regex.Matches(m_OutputText);
		href_matches = s_HrefRegex.Matches(m_OutputText);
		indexes.Clear();
		for (int k = 0; k < matches.Count; k++)
		{
			indexes.Add(matches[k].Index);
		}
		if (matches != null && matches.Count > 0)
		{
			for (int l = 0; l < matches.Count; l++)
			{
				picIndex = matches[l].Index;
				if (usesNewRendering)
				{
					charactersRemoved = 0;
					removeCharacters = remove_Regex.Matches(m_OutputText);
					for (int m = 0; m < removeCharacters.Count; m++)
					{
						if (removeCharacters[m].Index < picIndex && !indexes.Contains(removeCharacters[m].Index))
						{
							charactersRemoved += removeCharacters[m].Length;
						}
					}
					for (int n = 0; n < l; n++)
					{
						charactersRemoved += matches[n].Length - 1;
					}
					picIndex -= charactersRemoved;
				}
				vertIndex = picIndex * 4 + 3;
				m_ImagesVertexIndex.Add(vertIndex);
			}
		}
		if (usesNewRendering && m_HrefInfos != null && m_HrefInfos.Count > 0)
		{
			for (int num = 0; num < m_HrefInfos.Count; num++)
			{
				startCharactersRemoved = 0;
				endCharactersRemoved = 0;
				removeCharacters = remove_Regex.Matches(m_OutputText);
				for (int num2 = 0; num2 < removeCharacters.Count; num2++)
				{
					if (removeCharacters[num2].Index < m_HrefInfos[num].startIndex && !indexes.Contains(removeCharacters[num2].Index))
					{
						startCharactersRemoved += removeCharacters[num2].Length;
					}
					else if (removeCharacters[num2].Index < m_HrefInfos[num].startIndex && indexes.Contains(removeCharacters[num2].Index))
					{
						startCharactersRemoved += removeCharacters[num2].Length - 1;
					}
					if (removeCharacters[num2].Index < m_HrefInfos[num].endIndex && !indexes.Contains(removeCharacters[num2].Index))
					{
						endCharactersRemoved += removeCharacters[num2].Length;
					}
					else if (removeCharacters[num2].Index < m_HrefInfos[num].endIndex && indexes.Contains(removeCharacters[num2].Index))
					{
						endCharactersRemoved += removeCharacters[num2].Length - 1;
					}
				}
				m_HrefInfos[num].startIndex -= startCharactersRemoved;
				m_HrefInfos[num].startIndex = m_HrefInfos[num].startIndex * 4;
				m_HrefInfos[num].endIndex -= endCharactersRemoved;
				m_HrefInfos[num].endIndex = m_HrefInfos[num].endIndex * 4 + 3;
			}
		}
		return m_OutputText;
	}

	public virtual void OnHrefClick(string href)
	{
		zendeskLinkHandler.HandleLink(href);
	}

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
		originalText = m_Text;
		m_Text = GetOutputText();
		base.OnPopulateMesh(toFill);
		m_DisableFontTextureRebuiltCallback = true;
		m_Text = originalText;
		positions.Clear();
		vert = default(UIVertex);
		for (int i = 0; i < m_ImagesVertexIndex.Count; i++)
		{
			int num = m_ImagesVertexIndex[i];
			if (num < toFill.currentVertCount)
			{
				toFill.PopulateUIVertex(ref vert, num);
				positions.Add(new Vector2(vert.position.x + (float)(base.fontSize / 2), vert.position.y + (float)(base.fontSize / 2)) + imageOffset);
				toFill.PopulateUIVertex(ref vert, num - 3);
				Vector3 position = vert.position;
				int num2 = num;
				int num3 = num - 3;
				while (num2 > num3)
				{
					toFill.PopulateUIVertex(ref vert, num);
					vert.position = position;
					toFill.SetUIVertex(vert, num2);
					num2--;
				}
			}
		}
		for (int j = 0; j < m_HrefInfos.Count; j++)
		{
			m_HrefInfos[j].boxes.Clear();
			if (m_HrefInfos[j].startIndex >= toFill.currentVertCount)
			{
				continue;
			}
			toFill.PopulateUIVertex(ref vert, m_HrefInfos[j].startIndex);
			Vector3 position2 = vert.position;
			Bounds bounds = new Bounds(position2, Vector3.zero);
			int k = m_HrefInfos[j].startIndex;
			for (int endIndex = m_HrefInfos[j].endIndex; k < endIndex && k < toFill.currentVertCount; k++)
			{
				toFill.PopulateUIVertex(ref vert, k);
				position2 = vert.position;
				if (position2.x < bounds.min.x)
				{
					m_HrefInfos[j].boxes.Add(new Rect(bounds.min, bounds.size));
					bounds = new Bounds(position2, Vector3.zero);
				}
				else
				{
					bounds.Encapsulate(position2);
				}
			}
			m_HrefInfos[j].boxes.Add(new Rect(bounds.min, bounds.size));
		}
		updateQuad = true;
		m_DisableFontTextureRebuiltCallback = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(base.rectTransform, eventData.position, eventData.pressEventCamera, out lp);
		for (int i = 0; i < m_HrefInfos.Count; i++)
		{
			for (int j = 0; j < m_HrefInfos[i].boxes.Count; j++)
			{
				if (m_HrefInfos[i].boxes[j].Contains(lp))
				{
					m_OnHrefClick.Invoke(m_HrefInfos[i].name);
					return;
				}
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		selected = true;
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		for (int i = 0; i < m_ImagesPool.Count; i++)
		{
			if (button != null && button.isActiveAndEnabled)
			{
				m_ImagesPool[i].color = new Color32(7, 167, 51, byte.MaxValue);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		selected = false;
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		for (int i = 0; i < m_ImagesPool.Count; i++)
		{
			if (button != null && button.isActiveAndEnabled)
			{
				m_ImagesPool[i].color = new Color32(7, 167, 51, byte.MaxValue);
			}
			else
			{
				m_ImagesPool[i].color = new Color32(7, 167, 51, byte.MaxValue);
			}
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
		selected = true;
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		for (int i = 0; i < m_ImagesPool.Count; i++)
		{
			if (button != null && button.isActiveAndEnabled)
			{
				m_ImagesPool[i].color = new Color32(7, 167, 51, byte.MaxValue);
			}
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		selected = false;
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		for (int i = 0; i < m_ImagesPool.Count; i++)
		{
			if (button != null && button.isActiveAndEnabled)
			{
				m_ImagesPool[i].color = new Color32(7, 167, 51, byte.MaxValue);
			}
		}
	}

	public override void SetVerticesDirty()
	{
		base.SetVerticesDirty();
		updateQuad = true;
	}

	protected override void OnEnable()
	{
		usesNewRendering = false;
		if (Application.unityVersion.StartsWith("2018.4."))
		{
			if (!char.IsDigit(Application.unityVersion[8]))
			{
				if (Convert.ToInt32(Application.unityVersion[7].ToString()) > 4)
				{
					usesNewRendering = true;
				}
			}
			else
			{
				usesNewRendering = true;
			}
		}
		else
		{
			usesNewRendering = true;
		}
		base.OnEnable();
		base.supportRichText = true;
		base.alignByGeometry = false;
		if (m_ImagesPool.Count >= 1)
		{
			for (int i = 0; i < m_ImagesPool.Count; i++)
			{
				if (m_ImagesPool[i] != null)
				{
					m_ImagesPool[i].enabled = true;
				}
			}
		}
		updateQuad = true;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		base.gameObject.GetComponent<ZendeskTextPic>().onHrefClick.RemoveListener(OnHrefClick);
		if (m_ImagesPool.Count < 1)
		{
			return;
		}
		for (int i = 0; i < m_ImagesPool.Count; i++)
		{
			if (m_ImagesPool[i] != null)
			{
				m_ImagesPool[i].enabled = false;
			}
		}
	}

	private new void Start()
	{
		button = GetComponent<Button>();
		ResetIconList();
		base.gameObject.GetComponent<ZendeskTextPic>().onHrefClick.AddListener(OnHrefClick);
	}

	public void init(string text, ZendeskLinkHandler zendeskLinkHandler)
	{
		this.zendeskLinkHandler = zendeskLinkHandler;
		this.text = RemoveLineBreak(text);
	}

	public string RemoveLineBreak(string text)
	{
		while (text.ToLower().EndsWith("\n"))
		{
			text = text.Remove(text.Length - 1);
		}
		return text;
	}

	private void LateUpdate()
	{
		if (previousText != text)
		{
			Reset_m_HrefInfos();
			updateQuad = true;
		}
		lock (thisLock)
		{
			if (updateQuad)
			{
				UpdateQuadImage();
				updateQuad = false;
			}
			if (clearImages)
			{
				for (int i = 0; i < culled_ImagesPool.Count; i++)
				{
					Object.DestroyImmediate(culled_ImagesPool[i]);
				}
				culled_ImagesPool.Clear();
				clearImages = false;
			}
		}
	}
}
