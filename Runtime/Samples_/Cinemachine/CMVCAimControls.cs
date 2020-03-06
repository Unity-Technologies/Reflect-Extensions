using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

// TODO : make this a Cinemachine Extension ?
// TODO : make this work with new Input System

namespace Cinemachine.Helpers
{
	/// <summary>
	/// Conditions a Virtual Camera's POV Controls and provides Zoom control
	/// </summary>
	[AddComponentMenu("Cinemachine/POV Controls")]
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class CMVCAimControls : MonoBehaviour
	{
		public enum MOUSE_BUTTON { None = -1, Left = 0, Right = 1}

		[SerializeField] private MOUSE_BUTTON aimButton = MOUSE_BUTTON.None;
		[SerializeField] private string zoomInput = "Mouse ScrollWheel";
		[SerializeField] private float zoomSensitivity = 100;
		[SerializeField] private float minAngle = 30;
		[SerializeField] private float maxAngle = 90;

		private CinemachineBrain vBrain;
		private CinemachineVirtualCamera vCam;
		private CinemachinePOV vCamPOV;

		private void Awake()
		{
			vBrain = FindObjectOfType<CinemachineBrain>();
			vCam = GetComponent<CinemachineVirtualCamera>();
			vCamPOV = vCam.GetCinemachineComponent<CinemachinePOV>();
			if (vCamPOV == null)
				Debug.LogWarning("Virtual Camera Aim Type must be set to 'POV'");
		}

		private void Update()
		{
			if (vCamPOV == null || (CinemachineVirtualCamera)vBrain.ActiveVirtualCamera != vCam)
				return;

			if (aimButton == MOUSE_BUTTON.None || Input.GetMouseButton((int)aimButton))
			{
				vCamPOV.m_HorizontalAxis.m_InputAxisValue = Input.GetAxis("Mouse X");
				vCamPOV.m_VerticalAxis.m_InputAxisValue = Input.GetAxis("Mouse Y");
			}
			else
			{
				vCamPOV.m_HorizontalAxis.m_InputAxisValue = 0;
				vCamPOV.m_VerticalAxis.m_InputAxisValue = 0;
			}

			vCam.m_Lens.FieldOfView = Mathf.Clamp(
				vCam.m_Lens.FieldOfView + Input.GetAxis(zoomInput) * Time.deltaTime * zoomSensitivity,
				minAngle,
				maxAngle);
		}
	}
}