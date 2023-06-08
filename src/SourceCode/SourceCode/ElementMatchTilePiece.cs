using UnityEngine;

public class ElementMatchTilePiece : TilePiece
{
	public ElementMatchGame.ElementType _ElementType;

	public string _Symbol;

	public LocaleString _NameText;

	public string _AtomicNumber;

	public string _AtomicMass;

	public int _DropRate;

	public override int pWeight
	{
		get
		{
			return _Weight;
		}
		set
		{
			_Weight = value;
		}
	}

	public void SetInfo()
	{
		base.transform.Find("Symbol").gameObject.GetComponent<TextMesh>().text = _Symbol;
	}

	protected override void Die()
	{
		base.Die();
	}
}
