namespace Zendesk.UI;

public sealed class ZendeskScreen
{
	public static readonly ZendeskScreen MyTickets = new ZendeskScreen(Screen.MyTickets, ScreenType.Support);

	public static readonly ZendeskScreen CreateTicket = new ZendeskScreen(Screen.CreateTicket, ScreenType.Support);

	public static readonly ZendeskScreen TicketResponse = new ZendeskScreen(Screen.TicketResponse, ScreenType.Support);

	public static readonly ZendeskScreen MyHelpCenterCategories = new ZendeskScreen(Screen.MyHelpCenterCategories, ScreenType.HelpCenter);

	public static readonly ZendeskScreen MyHelpCenter = new ZendeskScreen(Screen.MyHelpCenter, ScreenType.HelpCenter);

	public static readonly ZendeskScreen ArticleListScreen = new ZendeskScreen(Screen.ArticleList, ScreenType.HelpCenter);

	public static readonly ZendeskScreen ArticleScreen = new ZendeskScreen(Screen.Article, ScreenType.HelpCenter);

	public static readonly ZendeskScreen Empty = new ZendeskScreen(Screen.Empty, ScreenType.None);

	public Screen Screen { get; private set; }

	public ScreenType ScreenType { get; private set; }

	private ZendeskScreen(Screen screen, ScreenType screenType)
	{
		ScreenType = screenType;
		Screen = screen;
	}
}
