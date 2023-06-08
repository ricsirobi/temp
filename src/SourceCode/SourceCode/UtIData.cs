public interface UtIData
{
	UtStatus pLastStatus { get; }

	void Clear();

	UtContainer AddContainer(UtContainer inParentContainer, string inNewContainerName);

	UtStatus DeleteContainer(UtContainer inParentContainer, string inContainerName);

	UtContainer AppendContainer(UtContainer inParentContainer, UtData inData);

	UtData CopyContainer(UtContainer inContainer);

	UtContainer GetContainer(string inPathStr);

	UtContainer GetContainer(UtContainer inParentContainer, string inSubPathStr);

	string GetContainerName(UtContainer inContainer);

	string GetContainerPath(UtContainer inContainer);

	UtStatus SetValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue);

	UtStatus SetValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue, int inRecordIdx);

	UtStatus SetArrayElementValue<TYPE>(UtContainer inContainer, string inKey, TYPE inValue, int inRecordIdx, int inArrayIdx);

	TYPE GetValue<TYPE>(UtContainer inContainer, string inKey);

	TYPE GetValue<TYPE>(UtContainer inContainer, string inKey, int inRecordIdx);

	TYPE GetArrayElementValue<TYPE>(UtContainer inContainer, string inKey, int inRecordIdx, int inArrayIdx);

	int GetArrayElementCount(UtContainer inContainer, string inKey, int recordIdx);

	bool HasKey(UtContainer inContainer, string inKey);

	UtType GetKeyType(UtContainer inContainer, string inKey);

	int GetRecordCount(UtContainer inContainer);

	int AddRecord(UtContainer inContainer);

	int FindRecord<TYPE>(UtContainer inContainer, int inStartRecIdx, string inKey, TYPE inValue);

	UtStatus CopyRecord(UtContainer inSrcContainer, int inSrcRecordIdx, UtData inDstData, UtContainer inDstContainer, int inDstRecordIdx);

	UtStatus RemoveRecord(UtContainer inContainer, int inRecordIdx);

	UtStatus ReadFromResource(byte[] inResource, bool inReadOnly);

	UtStatus WriteToResource(out byte[] outResource);
}
