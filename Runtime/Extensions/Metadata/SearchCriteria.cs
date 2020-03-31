namespace UnityEngine.Reflect.Extensions.Rules
{
	/// <summary>
	/// A Key Value Pair struct to search Metadata
	/// </summary>
	[System.Serializable]
	public struct SearchCriteria
	{
		public string key;
		public string value;

		/// <summary>
		/// A Metadata Search Pattern object
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public SearchCriteria(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}
}