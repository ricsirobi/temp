using System;

[Serializable]
public class LoginStrings
{
	public LocaleString _InvalidPasswordText = new LocaleString("Password requires at least 6 characters consisting of only alphanumeric characters.");

	public LocaleString _InvalidPlayerNameText = new LocaleString("Username cannot contain special characters.");

	public LocaleString _SpaceInPlayerNameText = new LocaleString("Username cannot contain spaces.");

	public LocaleString _InvalidChildNameText = new LocaleString("Child Name Is not Correct.");

	public LocaleString _InvalidEmailText = new LocaleString("Email address is not in valid format.");

	public LocaleString _ErrorText = new LocaleString("An error occurred !!");

	public LocaleString _DuplicateUserNameText = new LocaleString("Username already exists!");

	public LocaleString _DuplicateEmailText = new LocaleString("Email already used!");

	public LocaleString _InvalidDOBText = new LocaleString("Please enter a valid age.");

	public LocaleString _PasswordRecoverSuccessText = new LocaleString("Your password has been sent to the email address.");

	public LocaleString _PasswordRecoverFailedText = new LocaleString("The Email address provided is invalid.");

	public LocaleString _AccountRecoverSuccessText = new LocaleString("Your username has been sent to the email address.");

	public LocaleString _AccountRecoverFailedText = new LocaleString("Your account could not be recovered, please verify the email is correct");

	public LocaleString _InvalidUsernameText = new LocaleString("Please provide a valid username.");

	public LocaleString _ParentLoginFailedText = new LocaleString("The username and password credentials are invalid. Check and re-enter to login successfully.");

	public LocaleString _NoGuestUserPresentText = new LocaleString("There has been no guest data created on this device.");

	public LocaleString _ConnectivityErrorInitialText = new LocaleString("Could not connect to the server. You must have access to the internet to play this game");

	public LocaleString _InvalidLoginUsernameText = new LocaleString("Please provide a valid username to login.");

	public LocaleString _NoNetworkTitleText = new LocaleString("Connection Error!");

	public LocaleString _NoNetworkRetryText = new LocaleString("Oops, you have lost connection to the server! Please check your connection and tap retry to try again.");

	public LocaleString _NoNetworkRegistrationText = new LocaleString("To register you must be connected to internet! Please check your connection and try again.");

	public LocaleString _NoNetworkPlayText = new LocaleString("To play you must be connected to internet! Please check your connection and try again.");

	public LocaleString _TermsAndConditionsText = new LocaleString("You have to agree to terms and conditions.");

	public LocaleString _WhyRegisterText = new LocaleString("By registering you have the ability to name your Viking and dragon while earning some bonus coins and gems. You will also be able to play your saved game from any computer or device with an internet connection!");

	public LocaleString _WhyRegisterTitleText = new LocaleString("Why Register?");

	public LocaleString _ExitConfirmationText = new LocaleString("Do you want to Quit?");

	public LocaleString _NoNetworkText = new LocaleString("Could not connect to internet. Please check your connection and try again.");

	public LocaleString _PrefetchWarningText = new LocaleString("[Review] Game Will download {X} MB data. Do you want to proceed further?");
}
