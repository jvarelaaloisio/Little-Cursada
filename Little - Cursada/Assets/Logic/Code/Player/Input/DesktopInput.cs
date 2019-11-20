using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopInput : MonoBehaviour, IPlayerInput
{
	#region Public
	public bool ReadClimbInput()
	{
		return Input.GetButton("Climb");
	}

	public Vector2 ReadHorInput()
	{
		return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	public bool ReadJumpInput()
	{
		return Input.GetButton("Jump");
	}
	#endregion
}
