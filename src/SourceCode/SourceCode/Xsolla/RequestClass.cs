using System.Collections;

namespace Xsolla;

public class RequestClass : IEnumerable, IEnumerator
{
	private string _textrequest;

	private bool _error;

	private string _texterror;

	private string _url;

	private int[] ints = new int[4] { 12, 13, 1, 4 };

	private int _index;

	public object Current => ints[_index];

	public string TextRequest
	{
		get
		{
			return _textrequest;
		}
		set
		{
			_textrequest = value;
		}
	}

	public bool HasError
	{
		get
		{
			return _error;
		}
		set
		{
			_error = value;
		}
	}

	public string ErrorText
	{
		get
		{
			if (!_error)
			{
				return "";
			}
			return _texterror;
		}
		set
		{
			_texterror = value;
		}
	}

	public string Url
	{
		get
		{
			return _url;
		}
		set
		{
			_url = value;
		}
	}

	public bool MoveNext()
	{
		if (_index == ints.Length - 1)
		{
			Reset();
			return false;
		}
		_index++;
		return true;
	}

	public void Reset()
	{
		_index = -1;
	}

	public IEnumerator GetEnumerator()
	{
		return this;
	}

	public RequestClass(string pRequest, string pUrl, bool pError = false, string pErrorText = "")
	{
		_textrequest = pRequest;
		_url = pUrl;
		_error = pError;
		_texterror = pErrorText;
	}

	public RequestClass()
	{
	}
}
