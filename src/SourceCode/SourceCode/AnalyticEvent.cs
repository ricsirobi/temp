using System.ComponentModel;

public enum AnalyticEvent
{
	[Description("hatch_egg")]
	HATCH_EGG,
	[Description("meet_headmaster")]
	MEET_HEADMASTER,
	[Description("play_racing")]
	PLAY_RACING,
	[Description("play_flightschool")]
	PLAY_FLIGHTSCHOOL,
	[Description("buttonClicked")]
	BUTTON_CLICKED,
	[Description("sessionEnded")]
	SESSION_ENDED,
	[Description("clickedPlayAsGuest")]
	CLICKED_PLAY_AS_GUEST,
	[Description("registerUser")]
	REGISTER_USER,
	[Description("registerGuest")]
	REGISTER_GUEST,
	[Description("guestChat")]
	GUEST_CHAT,
	[Description("upsellShown")]
	UPSELL_SHOWN,
	[Description("GemsPurchased")]
	GEMS_PURCHASED,
	[Description("MembershipPurchased")]
	MEMBERSHIP_PURCHASED
}
