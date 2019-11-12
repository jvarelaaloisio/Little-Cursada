using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Player_Body))]
public class Player_Animator : MonoBehaviour, IUpdateable
{
	#region Variables

	#region Constant
	const string CrouchFlag = "Crouch",
	speedX = "SpeedX",
	speedY = "SpeedY",
	speedZ = "SpeedZ",
	animState = "State";
	#endregion

	#region Private
	UpdateManager _uManager;
	Player_Body _body;
	Animator _anim;
	PlayerState _state;
	PlayerState State
	{
		set
		{
			if (_state != value) _anim.SetFloat(animState, (int)value);
			_state = value;
		}
	}
	#endregion

	#endregion

	#region Unity
	void Start()
	{
		try
		{
			_uManager = GameObject.FindObjectOfType<UpdateManager>();
			_uManager.AddItem(this);
		}
		catch (NullReferenceException)
		{
			print(this.name + "update manager not found");
		}
		try
		{
			_anim = GetComponentInChildren<Animator>();
		}
		catch (NullReferenceException)
		{
			print(this.name + "animator not found");
		}
		_body = GetComponent<Player_Body>();
	}
	public void OnUpdate()
	{
		SetSpeedParameter(_body.Velocity);
	}
	#endregion

	#region Public
	public void SetCrouch(bool Value)
	{
		_anim.SetBool(CrouchFlag, Value);
	}

	public void SetSpeedParameter(Vector3 Speed)
	{
		_anim.SetFloat(speedX, Speed.x);
		_anim.SetFloat(speedY, Speed.y);
		_anim.SetFloat(speedZ, Speed.z);
	}

	public void ChangeState(PlayerState newState)
	{
		State = newState;
	}
	#endregion
}
