#define EAGER_EVALUATION
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// TODO : make this a Cinemachine Extension ?

namespace Cinemachine.Helpers
{
	/// <summary>
	/// Exposes a Virtual Camera's TrackedDolly Path Position
	/// </summary>
	[AddComponentMenu ("Cinemachine/TrackedDolly PathPosition")]
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class CMTDPosition : MonoBehaviour
	{
		private CinemachineTrackedDolly trackedDolly;

		public CinemachineTrackedDolly TrackedDolly
		{
			get
			{
#if !EAGER_EVALUATION
				if (trackedDolly == null)
					trackedDolly = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>();
				if (TrackedDolly == null)
					Debug.LogWarning("Virtual Camera Body Type must be set to 'Tracked Dolly'");
#endif
				return trackedDolly;
			}
		}

#if EAGER_EVALUATION
		private void Awake()
		{
			trackedDolly = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineTrackedDolly>();
			if (TrackedDolly == null)
				Debug.LogWarning("Virtual Camera Body Type must be set to 'Tracked Dolly'");
		}
#endif

		public float PathPosition
		{
			get => (float)TrackedDolly?.m_PathPosition;
			set
			{
				if (TrackedDolly == null)
					return;
				TrackedDolly.m_PathPosition = value;
			}
		}
	}
}