namespace UnityEngine.Reflect.Extensions
{
	/// <summary>
	/// A Key Value Pair struct to search Metadata
	/// </summary>
	[System.Serializable]
	public struct SearchCriteria
	{
		public string key;
		public string value;

		public SearchCriteria(string key, string value)
		{
			this.key = key;
			this.value = value;
		}
	}
}