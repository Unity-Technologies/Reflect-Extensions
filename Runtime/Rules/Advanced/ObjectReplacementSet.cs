
namespace UnityEngine.Reflect.Extensions.Rules.Advanced
{
	[CreateAssetMenu(fileName = "New Object Replacement Set", menuName = "Reflect/Rules/Advanced/Object Replacement Set", order = 1)]
	public class ObjectReplacementSet : ScriptableObject
	{
		[Header("Rules")]
		[Tooltip("Replace Objects by Metadata Key/Value")]
		public Replacement[] replacements = new Replacement[1] {
			new Replacement (new Criteria[1] { new Criteria("Category", "Planting") })
		};
	}

	[System.Serializable]
	public struct Criteria
	{
		public string key;
		public string value;

		public Criteria(string key, string value)
		{
			this.key = key;
			this.value = value;
		}

	}
	[System.Serializable]
	public struct Replacement
	{
		public Criteria[] criterias;
		public GameObject gameObject;
		public bool disableOriginal;
		public bool matchHeight;

		public Replacement(Criteria[] criterias)
		{
			this.criterias = criterias;
			this.gameObject = null;
			this.disableOriginal = true;
			this.matchHeight = false;
		}
	}

	public static class MetadataExtensions
	{
		public static bool MatchAllCriterias(this Metadata mData, Criteria[] criterias)
		{
			if (criterias.Length > 0)
			{
				var parameters = mData.GetParameters();
				for (int i = 0; i < criterias.Length; i++)
				{
					if (parameters.ContainsKey(criterias[i].key) && mData.GetParameter(criterias[i].key) == criterias[i].value)
						continue;
					else
						return false;
				}
				return true;
			}
			return false; // return false in case of empty criterias
		}

		public static bool MatchAnyCriteria(this Metadata mData, Criteria[] criterias)
		{
			if (criterias.Length > 0)
			{
				var parameters = mData.GetParameters();
				for (int i = 0; i < criterias.Length; i++)
				{
					if (parameters.ContainsKey(criterias[i].key) && mData.GetParameter(criterias[i].key) == criterias[i].value)
						return true;
				}
				return false;
			}
			return false; // return false in case of empty criterias
		}
	}
}