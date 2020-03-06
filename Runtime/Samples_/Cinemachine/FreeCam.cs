using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Cinemachine;

// TODO : make this a Cinemachine Extension ?
// TODO : make this work with new Input System

namespace Cinemachine.Helpers
{
	/// <summary>
	/// Controls a Virtual Camera's position
	/// </summary>
	[AddComponentMenu("Cinemachine/Motion Input")]
	[RequireComponent(typeof(CinemachineVirtualCamera))]
	public class FreeCam : MonoBehaviour
	{
		[SerializeField] private string _horizontalMotionInput = "Horizontal";
		[SerializeField] private string _forwardMotionInput = "Vertical";
		[SerializeField] private string _upwardMotionInput = "UpDown";
		[SerializeField] private KeyCode _fastKeycode = KeyCode.LeftShift;
		[SerializeField] private KeyCode _fasterKeycode = KeyCode.LeftControl;
		[SerializeField] private float _normalSpeed = 1f;
		[SerializeField] private float _fastSpeed = 5f;
		[SerializeField] private float _fasterSpeed = 20f;

		private float _speed;
		Vector3 _position;

		private CinemachineBrain vBrain;
		private CinemachineVirtualCamera vCam;

		private void Awake()
		{
			vBrain = FindObjectOfType<CinemachineBrain>();
			vCam = GetComponent<CinemachineVirtualCamera>();
		}

		void Update()
		{
			if ((CinemachineVirtualCamera)vBrain.ActiveVirtualCamera != vCam)
				return;
			_speed = Input.GetKey(_fasterKeycode) ? _fasterSpeed : Input.GetKey(_fastKeycode) ? _fastSpeed : _normalSpeed;
			_position.x = Input.GetAxis(_horizontalMotionInput) * Time.deltaTime * _speed;
			_position.z = Input.GetAxis(_forwardMotionInput) * Time.deltaTime * _speed;
			_position.y = Input.GetAxis(_upwardMotionInput) * Time.deltaTime * _speed;
			transform.Translate(_position, vBrain.transform);
		}
	}
}