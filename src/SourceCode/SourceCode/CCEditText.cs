using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class CCEditText : MonoBehaviour
{
	public enum Type
	{
		AMEX,
		ENROUTE,
		JSB15,
		DINNERS_CLUB_CARTE_BLANCHE,
		DINERS_CLUB_INTERNATIONAL,
		VISA,
		VISA_ELECTRON,
		MASTERCARD,
		DISCOVER,
		JSB16,
		MAESTRO,
		LASER,
		WRONG
	}

	public class CardType
	{
		public Type type;

		public Regex pattern;

		public int[] length;

		public CardType(Type mType, Regex pattern, int[] length)
		{
			type = mType;
			this.pattern = pattern;
			this.length = length;
		}
	}

	private List<CardType> mTypes;

	private Type currentType = Type.WRONG;

	private string mask;

	private bool doCorrect = true;

	private InputField inputField;

	private string text;

	public string getMask()
	{
		return mask;
	}

	private void Awake()
	{
		inputField = GetComponent<InputField>();
		inputField.onValueChanged.AddListener(delegate
		{
			Correct();
		});
		InputField obj = inputField;
		obj.onValidateInput = (InputField.OnValidateInput)Delegate.Combine(obj.onValidateInput, new InputField.OnValidateInput(ValidateInput));
	}

	private char ValidateInput(string text, int charIndex, char addedChar)
	{
		if (int.TryParse(addedChar.ToString(), out var _) || addedChar.Equals(' '))
		{
			return addedChar;
		}
		return '\0';
	}

	public void Correct()
	{
		if (doCorrect)
		{
			text = inputField.text;
			chooseMask(text);
		}
	}

	public void updateMask(string mask)
	{
		this.mask = mask;
		List<int> list = new List<int>();
		string text = mask;
		if (mask != null)
		{
			while (text.LastIndexOf(' ') != -1)
			{
				int num = text.LastIndexOf(' ');
				text = text.Remove(num, 1);
				list.Add(num);
			}
			this.text = this.text.Replace(" ", "");
			int num2 = list.Count - 1;
			while (num2 > -1 && list[num2] < this.text.Length && this.text[list[num2]] != ' ')
			{
				this.text = this.text.Insert(list[num2], " ");
				num2--;
			}
			doCorrect = false;
			inputField.text = this.text;
			inputField.MoveTextEnd(shift: false);
			doCorrect = true;
		}
	}

	private void chooseMask(string s)
	{
		currentType = getType(s);
		switch (currentType)
		{
		case Type.AMEX:
		case Type.ENROUTE:
		case Type.JSB15:
			updateMask("#### ###### #####");
			break;
		case Type.DINNERS_CLUB_CARTE_BLANCHE:
		case Type.DINERS_CLUB_INTERNATIONAL:
			updateMask("#### #### #### ##");
			break;
		case Type.VISA:
		case Type.VISA_ELECTRON:
		case Type.MASTERCARD:
		case Type.JSB16:
		case Type.MAESTRO:
			updateMask("#### #### #### ####");
			break;
		default:
			updateMask("#### #### #### #### ###");
			break;
		}
	}

	public bool IsMaestro()
	{
		return getCurrentType() == Type.MAESTRO;
	}

	private List<CardType> getTypes()
	{
		if (mTypes == null)
		{
			mTypes = new List<CardType>();
			mTypes.Add(new CardType(Type.AMEX, new Regex("^3[47]"), new int[1] { 15 }));
			mTypes.Add(new CardType(Type.DINNERS_CLUB_CARTE_BLANCHE, new Regex("^30[0-5]"), new int[1] { 14 }));
			mTypes.Add(new CardType(Type.DINERS_CLUB_INTERNATIONAL, new Regex("^36"), new int[1] { 14 }));
			mTypes.Add(new CardType(Type.JSB16, new Regex("^3"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.JSB15, new Regex("^(2131|1800)"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.LASER, new Regex("^(6304|670[69]|6771)"), new int[4] { 16, 17, 18, 19 }));
			mTypes.Add(new CardType(Type.VISA_ELECTRON, new Regex("^(4026|417500|4508|4844|491(3|7))"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.VISA, new Regex("^4"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.MASTERCARD, new Regex("^5[1-5]"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.MAESTRO, new Regex("^(5018|5020|5038|6304|6759|676[1-3])"), new int[8] { 12, 13, 14, 15, 16, 17, 18, 19 }));
			mTypes.Add(new CardType(Type.DISCOVER, new Regex("^(6011|622(12[6-9]|1[3-9][0-9]|[2-8][0-9]{2}|9[0-1][0-9]|92[0-5]|64[4-9])|65)"), new int[1] { 16 }));
			mTypes.Add(new CardType(Type.ENROUTE, new Regex("^(2(014|149))"), new int[1] { 15 }));
		}
		return mTypes;
	}

	private Type getType(string cardNumber)
	{
		foreach (CardType type in getTypes())
		{
			if (type.pattern.Matches(cardNumber).Count > 0)
			{
				return type.type;
			}
		}
		return Type.WRONG;
	}

	public Type getCurrentType()
	{
		return currentType;
	}
}
