using System;
using System.Collections.Generic;

[Serializable]
public class TimeBrackets
{
	public List<float> _Time = new List<float>(4);

	public List<string> _Grade = new List<string>(4) { "A", "B", "C", "D" };
}
