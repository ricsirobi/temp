public struct UtEntryListHeader
{
	public const byte FLAG_KEYS_AS_STRINGS = 1;

	public const byte FLAG_USERDATA_AS_STRING = 2;

	public uint mEntryCount;

	public uint mRuntimeFlags;

	public byte mEndianFlags;

	public byte mVersion;

	public byte mAttributes;

	public byte mReserved3;
}
