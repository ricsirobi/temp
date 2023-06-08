using System;

public class ObSwitch : ObSwitchBase
{
	[Serializable]
	public class SwitchConfigAnd
	{
		public string _SwitchName;

		public bool _SwitchTargetState;

		private bool mCurrentState;

		public bool pCurrentState
		{
			get
			{
				return mCurrentState;
			}
			set
			{
				mCurrentState = value;
			}
		}
	}

	[Serializable]
	public class SwitchConfigOR
	{
		public SwitchConfigAnd[] _ConfigAndSets;
	}

	public SwitchConfigOR[] _ConfigOrSets;

	public void OnSwitchOn(string switchName)
	{
		SwitchConfigOR[] configOrSets = _ConfigOrSets;
		for (int i = 0; i < configOrSets.Length; i++)
		{
			SwitchConfigAnd[] configAndSets = configOrSets[i]._ConfigAndSets;
			foreach (SwitchConfigAnd switchConfigAnd in configAndSets)
			{
				if (!switchConfigAnd.pCurrentState && switchConfigAnd._SwitchName.Equals(switchName))
				{
					switchConfigAnd.pCurrentState = true;
					break;
				}
			}
		}
		Validate();
	}

	public void OnSwitchOff(string switchName)
	{
		SwitchConfigOR[] configOrSets = _ConfigOrSets;
		for (int i = 0; i < configOrSets.Length; i++)
		{
			SwitchConfigAnd[] configAndSets = configOrSets[i]._ConfigAndSets;
			foreach (SwitchConfigAnd switchConfigAnd in configAndSets)
			{
				if (switchConfigAnd.pCurrentState && switchConfigAnd._SwitchName.Equals(switchName))
				{
					switchConfigAnd.pCurrentState = false;
					break;
				}
			}
		}
		Validate();
	}

	private void Validate()
	{
		bool flag = false;
		SwitchConfigOR[] configOrSets = _ConfigOrSets;
		foreach (SwitchConfigOR obj in configOrSets)
		{
			bool flag2 = true;
			SwitchConfigAnd[] configAndSets = obj._ConfigAndSets;
			foreach (SwitchConfigAnd switchConfigAnd in configAndSets)
			{
				if (switchConfigAnd.pCurrentState != switchConfigAnd._SwitchTargetState)
				{
					flag2 = false;
					break;
				}
			}
			if (flag2)
			{
				flag = true;
				break;
			}
		}
		if (!mSwitchOn && flag)
		{
			SwitchOn();
		}
		else if (mSwitchOn && !flag)
		{
			SwitchOff();
		}
	}
}
