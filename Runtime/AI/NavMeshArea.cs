using System.Collections.Generic;
using UnityEngine.Reflect.Extensions.Rules;

// int waterMask = 1 << NavMesh.GetAreaFromName("water");

namespace UnityEngine.Reflect.Extensions.AI
{
	[CreateAssetMenu(fileName = "New Nav Mesh Area", menuName = "Reflect/AI/NavMesh Area", order = 1)]
	public class NavMeshArea : ScriptableObject
	{
		public int area = 0; // TODO : add a custom inspector to populate an enum from areas if possible
		[Header("Rules")]
		[Tooltip("Filter objects by Metadata Key/Value")]
		public List<SearchCriteria> filters = new List<SearchCriteria> { new SearchCriteria("Category", "Floor") };
	}
}