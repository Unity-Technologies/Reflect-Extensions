using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions
{
	/// <summary>
	/// Some Metadata extensions to find matches
	/// </summary>
	public static class MetadataExtentions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="md"></param>
		/// <param name="criterias"></param>
		/// <returns></returns>
		public static bool MatchAllCriterias(this Metadata md, List<SearchCriteria> criterias)
		{
			foreach (SearchCriteria criteria in criterias)
			{
				if (md.parameters.dictionary.ContainsKey(criteria.key) && md.parameters.dictionary[criteria.key].value == criteria.value)
					continue;
				else
					return false;
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="md"></param>
		/// <param name="criterias"></param>
		/// <returns></returns>
		public static bool MatchAnyCriterias(this Metadata md, List<SearchCriteria> criterias)
		{
			foreach (SearchCriteria criteria in criterias)
			{
				if (md.parameters.dictionary.ContainsKey(criteria.key) && md.parameters.dictionary[criteria.key].value == criteria.value)
					return true;
				else
					continue;
			}
			return false;
		}
	}
}