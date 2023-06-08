namespace Xsolla;

public static class ValidatorFactory
{
	public enum ValidatorType
	{
		EMPTY,
		MONTH,
		YEAR,
		CVV,
		CREDIT_CARD
	}

	public static IValidator GetByType(ValidatorType type)
	{
		return type switch
		{
			ValidatorType.EMPTY => new ValidatorEmpty(), 
			ValidatorType.MONTH => new ValidatorMonth(), 
			ValidatorType.YEAR => new ValidatorYear(), 
			ValidatorType.CVV => new ValidatorCvv(), 
			ValidatorType.CREDIT_CARD => new ValidatorCreditCard(), 
			_ => new ValidatorEmpty(), 
		};
	}
}
