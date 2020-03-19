using System.Collections.Generic;
using Unity.Reflect.Model;
using Unity.Reflect.Data;

namespace UnityEngine.Reflect.Extensions
{
	/// <summary>
	/// Extensions to handle Bounds against SyncBoundingBox types
	/// </summary>
	public static class BoundsExtentions
	{
		/// <summary>
		/// Converts the SyncBoundingBox to Bounds
		/// </summary>
		/// <param name="syncBoundingBox"></param>
		/// <returns></returns>
		public static Bounds ToBounds (this SyncBoundingBox syncBoundingBox)
		{
			Bounds bounds = new Bounds();
			bounds.min = new Vector3 (syncBoundingBox.Min.X, syncBoundingBox.Min.Y, syncBoundingBox.Min.Z);
			bounds.max = new Vector3(syncBoundingBox.Max.X, syncBoundingBox.Max.Y, syncBoundingBox.Max.Z);
            return bounds;
		}

		/// <summary>
		/// Converts the Bounds to SyncBoundingBox
		/// </summary>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public static SyncBoundingBox ToSyncBoundingBox (this Bounds bounds)
		{
			return new SyncBoundingBox(new System.Numerics.Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new System.Numerics.Vector3(bounds.max.x, bounds.max.y, bounds.max.z));
		}

		/// <summary>
		/// Grows the Bounds to fit the SyncBoundingBox
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="syncBoundingBox"></param>
		public static void Encapsulate(this Bounds bounds, SyncBoundingBox syncBoundingBox)
		{
			bounds.min = Vector3.Min(bounds.min, new Vector3(syncBoundingBox.Min.X, syncBoundingBox.Min.Y, syncBoundingBox.Min.Z));
			bounds.max = Vector3.Max(bounds.max, new Vector3(syncBoundingBox.Max.X, syncBoundingBox.Max.Y, syncBoundingBox.Max.Z));
		}

		/// <summary>
		/// Grows the Bounds to fit the SyncBoundingBoxes
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="syncBoundingBoxes"></param>
		public static void Encapsulate(this Bounds bounds, SyncBoundingBox[] syncBoundingBoxes)
		{
			foreach (SyncBoundingBox sbb in syncBoundingBoxes)
				bounds.Encapsulate(sbb);
		}

		/// <summary>
		/// Grows the Bounds to fit the ManifestEntry BoundingBox
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="manifestEntry"></param>
		public static void Encapsulate(this Bounds bounds, ManifestEntry manifestEntry)
		{
			bounds.Encapsulate(manifestEntry.BoundingBox);
		}

		/// <summary>
		/// Grows the Bounds to fit the SyncManifest BoundingBoxes
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="syncInstance"></param>
		public static void Encapsulate(this Bounds bounds, SyncManifest manifest)
		{
			foreach (KeyValuePair<PersistentKey, ManifestEntry> kvp in manifest.Content)
				bounds.Encapsulate(kvp.Value.BoundingBox);
		}

		/// <summary>
		/// Grows the Bounds to fit the SyncInstance BoundingBoxes
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="syncInstance"></param>
		public static void Encapsulate(this Bounds bounds, SyncInstance syncInstance)
		{
			bounds.Encapsulate(syncInstance.Manifest);
		}
	}
}