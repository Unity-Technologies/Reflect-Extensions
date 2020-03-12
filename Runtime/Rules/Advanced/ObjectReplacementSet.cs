using System.Collections.Generic;

namespace UnityEngine.Reflect.Extensions.Rules.Advanced
{
	[CreateAssetMenu(fileName = "New Object Replacement Set", menuName = "Reflect/Rules/Advanced/Object Replacement Set", order = 1)]
	public class ObjectReplacementSet : ScriptableObject
	{
		[Header("Rules")]
		[Tooltip("Replace Objects by Metadata Key/Value")]
		public Replacement[] replacements = new Replacement[1] {
			new Replacement (new List<SearchCriteria>() { new SearchCriteria ("Category", "Planting") })
		};
	}

	[System.Serializable]
	public struct Replacement
	{
		public List<SearchCriteria> criterias;
		public GameObject gameObject;
		public bool disableOriginal;
		public bool matchHeight;

		public Replacement(List<SearchCriteria> criterias)
		{
			this.criterias = criterias;
			this.gameObject = null;
			this.disableOriginal = true;
			this.matchHeight = false;
		}
	}
}