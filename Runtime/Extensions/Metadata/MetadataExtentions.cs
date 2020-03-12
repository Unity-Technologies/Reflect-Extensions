using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions.Rules
{
	/// <summary>
	/// Some Metadata extensions to find matches
	/// </summary>
	public static class MetadataExtentions
	{
		/// <summary>
		/// Returns true if Metadata contains matches all search criterias.
		/// </summary>
		/// <param name="md"></param>
		/// <param name="criterias">A list of Search Criterias</param>
		/// <returns></returns>
		public static bool MatchAllCriterias(this Metadata md, List<SearchCriteria> criterias)
		{
			if (criterias.Count == 0)
				return false; // return false in case of empty criterias

			var parameters = md.GetParameters();
			for (int i = 0; i < criterias.Count; i++)
			{
				if (parameters.ContainsKey(criterias[i].key) && md.parameters.dictionary[criterias[i].key].value == criterias[i].value)
					continue;
				else
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns true if Metadata contains matches any search criterias.
		/// </summary>
		/// <param name="md"></param>
		/// <param name="criterias">A list of Search Criterias</param>
		/// <returns></returns>
		public static bool MatchAnyCriterias(this Metadata md, List<SearchCriteria> criterias)
		{
			if (criterias.Count == 0)
				return false; // return false in case of empty criterias

			var parameters = md.GetParameters();
			for (int i = 0; i < criterias.Count; i++)
			{
				if (parameters.ContainsKey(criterias[i].key) && md.parameters.dictionary[criterias[i].key].value == criterias[i].value)
					return true;
			}
			return false;
		}
	}
}