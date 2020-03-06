using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace UnityEngine.Reflect.Extensions.AI
{
	/// <summary>
	/// NavMeshDisplay.
	/// Renders the NAVMesh.
	/// Requires a Camera.
	/// </summary>
	[AddComponentMenu("Reflect/AI/NavMesh Display")]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	public class NavMeshDisplay : MonoBehaviour
	{
		//[SerializeField] Color navMeshDisplayColor = new Color (0, .5f, 1f, .5f);
		[SerializeField] Gradient navMeshDisplayColors = new Gradient();
		Color[] navMeshDisplayColorsTable = default;

		NavMeshTriangulation triangulation;
		Material material;
		int numberOfAreas;

		private void Reset()
		{
			navMeshDisplayColors = new Gradient();
			navMeshDisplayColors.colorKeys = new GradientColorKey[2] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.blue, 1f) };
			navMeshDisplayColors.alphaKeys = new GradientAlphaKey[2] { new GradientAlphaKey(0.2f, 0f), new GradientAlphaKey(0.2f, 1f) };
		}

		private void Awake()
		{
			material = new Material(Shader.Find("Hidden/Internal-Colored"));
			material.hideFlags = HideFlags.HideAndDontSave;
			triangulation = NavMesh.CalculateTriangulation();
		}

		private void Start()
		{
			OnReflectNavMeshUpdated();
			if (ReflectNavMeshBuilder.instance != null)
				ReflectNavMeshBuilder.instance.onNavMeshUpdated += OnReflectNavMeshUpdated;
		}

		private void OnReflectNavMeshUpdated()
		{
			triangulation = NavMesh.CalculateTriangulation();

			if (triangulation.vertices.Length == 0)
				return;

			numberOfAreas = triangulation.indices.Length / 3;
			navMeshDisplayColorsTable = new Color[numberOfAreas];
			for (int i = 0; i < numberOfAreas; i++)
				navMeshDisplayColorsTable[i] = navMeshDisplayColors.Evaluate(Mathf.InverseLerp(0, numberOfAreas, i));
			Debug.Log(numberOfAreas);
		}
		
		//private void OnRenderObject() // TODO : doesn't z-test nicely
		void OnPostRender()
		{
			if (triangulation.vertices.Length == 0 || material == null)
				return;

			GL.PushMatrix();

			material.SetPass(0);
			GL.Begin(GL.TRIANGLES);

			for (int i = 0; i < triangulation.indices.Length; i += 3)
			{
				var triangleIndex = i / 3;
				var i1 = triangulation.indices[i];
				var i2 = triangulation.indices[i + 1];
				var i3 = triangulation.indices[i + 2];
				var p1 = triangulation.vertices[i1];
				var p2 = triangulation.vertices[i2];
				var p3 = triangulation.vertices[i3];
				var areaIndex = triangulation.areas[triangleIndex];
				GL.Color(navMeshDisplayColorsTable[areaIndex]);
				//GL.Color(navMeshDisplayColor);
				GL.Vertex(p1);
				GL.Vertex(p2);
				GL.Vertex(p3);
			}
			GL.End();

			GL.PopMatrix();
		}
	}
}