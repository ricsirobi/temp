using System;

namespace Xsolla;

public class CCPayment : XsollaPaymentImpl
{
	public delegate void OnNextStepRequired(XsollaForm form);

	public delegate void OnPaymentSuccess(XsollaStatus paymentStatus);

	public string _cardNumber;

	public string _cardExpMonth;

	public string _cardExpYear;

	public string _cardCvv;

	public string _cardZip;

	public string _cardHolder;

	public event OnNextStepRequired NextStepRecieved;

	public event OnPaymentSuccess PaymentSuccessRecieved;

	public CCPayment()
	{
	}

	public CCPayment(string accessToken, string cardNumber, string cardExpMonth, string cardExpYear, string cardCvv, string cardZip, string cardHolder)
	{
		_accessToken = accessToken;
		_cardNumber = cardNumber;
		_cardExpMonth = cardExpMonth;
		_cardExpYear = cardExpYear;
		_cardCvv = cardCvv;
		_cardZip = cardZip;
		_cardHolder = cardHolder;
	}

	public void SetParams(string cardNumber, string cardExpMonth, string cardExpYear, string cardCvv, string cardZip, string cardHolder)
	{
		_cardNumber = cardNumber;
		_cardExpMonth = cardExpMonth;
		_cardExpYear = cardExpYear;
		_cardCvv = cardCvv;
		_cardZip = cardZip;
		_cardHolder = cardHolder;
	}

	public void InitPaystation()
	{
		FormReceived = (Action<XsollaForm>)Delegate.Combine(FormReceived, new Action<XsollaForm>(OnFormReceived));
		StatusReceived = (Action<XsollaStatus, XsollaForm>)Delegate.Combine(StatusReceived, new Action<XsollaStatus, XsollaForm>(OnStatusReceived));
		ErrorReceived = (Action<XsollaError>)Delegate.Combine(ErrorReceived, new Action<XsollaError>(OnErrorReceived));
		StartPaymentWithoutUtils(XsollaWallet.Factory.CreateWallet(_accessToken));
	}

	public new void OnFormReceived(XsollaForm form)
	{
		switch (form.GetCurrentCommand())
		{
		case XsollaForm.CurrentCommand.FORM:
		{
			XsollaError error = form.GetError();
			if (!form.IsValidPaymentSystem())
			{
				OnErrorReceived(XsollaError.GetUnsuportedError());
			}
			else if (error == null)
			{
				form.UpdateElement("card_number", _cardNumber);
				form.UpdateElement("card_year", _cardExpYear);
				form.UpdateElement("card_month", _cardExpMonth);
				form.UpdateElement("cvv", _cardCvv);
				form.UpdateElement("zip", _cardZip);
				form.UpdateElement("cardholdername", _cardHolder);
				NextStep(form.GetXpsMap());
			}
			else
			{
				OnErrorReceived(error);
			}
			break;
		}
		case XsollaForm.CurrentCommand.CHECK:
			if (form.GetItem("zip") != null)
			{
				form.UpdateElement("zip", _cardZip);
				NextStep(form.GetXpsMap());
			}
			else
			{
				OnNextStepRecieved(form);
			}
			break;
		case XsollaForm.CurrentCommand.CREATE:
		case XsollaForm.CurrentCommand.STATUS:
		case XsollaForm.CurrentCommand.CHECKOUT:
		case XsollaForm.CurrentCommand.ACCOUNT:
			break;
		}
	}

	public void OnStatusReceived(XsollaStatus status)
	{
		OnPaymentSuccessRecieved(status);
	}

	public new void OnErrorReceived(XsollaError error)
	{
	}

	protected virtual void OnNextStepRecieved(XsollaForm form)
	{
		if (this.NextStepRecieved != null)
		{
			this.NextStepRecieved(form);
		}
	}

	protected virtual void OnPaymentSuccessRecieved(XsollaStatus status)
	{
		if (this.PaymentSuccessRecieved != null)
		{
			this.PaymentSuccessRecieved(status);
		}
	}
}
