public enum TouchID
{
	NONE = 0,
	SINGLE_TAP = 1,
	DOUBLE_TAP = 2,
	SWIPE_LEFT = 4,
	SWIPE_RIGHT = 8,
	SWIPE_UP = 0x10,
	SWIPE_DOWN = 0x20,
	SWIPE_DIGONAL_TOP_LEFT = 0x40,
	SWIPE_DIGONAL_TOP_RIGHT = 0x80,
	SWIPE_DIGONAL_BOTTOM_LEFT = 0x100,
	SWIPE_DIGONAL_BOTTOM_RIGHT = 0x200,
	ROTATE_LEFT = 0x400,
	ROTATE_RIGHT = 0x800,
	ZOOM_IN = 0x1000,
	ZOOM_OUT = 0x2000
}
