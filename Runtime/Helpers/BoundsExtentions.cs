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
			bounds.min = syncBoundingBox.Min;
			bounds.max = syncBoundingBox.Max;
			return bounds;
		}

		/// <summary>
		/// Converts the Bounds to SyncBoundingBox
		/// </summary>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public static SyncBoundingBox ToSyncBoundingBox (this Bounds bounds)
		{
			return new SyncBoundingBox(bounds.min, bounds.max);
		}

		/// <summary>
		/// Grows the Bounds to fit the SyncBoundingBox
		/// </summary>
		/// <param name="bounds"></param>
		/// <param name="syncBoundingBox"></param>
		public static void Encapsulate(this Bounds bounds, SyncBoundingBox syncBoundingBox)
		{
			bounds.min = Vector3.Min(bounds.min, syncBoundingBox.Min);
			bounds.max = Vector3.Max(bounds.max, syncBoundingBox.Max);
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