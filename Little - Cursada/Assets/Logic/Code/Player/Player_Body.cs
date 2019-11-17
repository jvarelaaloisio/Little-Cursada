﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void BodyEvent();
[RequireComponent(typeof(Rigidbody))]
public class Player_Body : GenericFunctions, IUpdateable
{
	#region Variables

	#region Constants
	const int WATER_LAYER = 4;
	#endregion

	#region Public
	public event BodyEvent PlayerJumpedEvent;
	public event BodyEvent PlayerClimbingEvent;
	public event BodyEvent PlayerLandedEvent;
	#endregion

	#region Serialized
	[Header("Movement")]
	[SerializeField]
	float Speed = 4;
	[SerializeField]
	float _crouchSpeedMultiplier = 1,
		_jumpForce = 6.5f,
		_jumpingAcceleration = 1,
		_FallMultiplier = 1,
		_lowJumpMultiplier = 3,
		_coyoteTime = .25f,
		_glidingDrag = 10,
		_climbSpeed = 1;

	[Header("Audio")]
	[SerializeField]
	AudioClip[] _soundEffects = null;

	[Header("Debug")]
	[SerializeField]
	float _xMinAngle = 5;
	[SerializeField]
	float _xMaxAngle = 92,
		_yMinAngle = 45,
		_yMaxAngle = 90,
		_inTheAirTimeToOff = .005f,
		_colTimeToOff = 0,
		climbTimeToOff = 0.05f;
	#endregion

	#region Private
	UpdateManager _uManager;
	Rigidbody _RB;
	Player_Animator _AnimControl;
	AudioManager _audioManager;
	Timer _colTimer, _coyoteTimer, _jumpTimer, _climbTimer;

	ContactPoint lastContact;
	Vector3 _collisionAngles;
	float CrouchSpeedFactor => _flags[Flag.Crouching] ? _crouchSpeedMultiplier : 1;
	float JumpingAccelerationFactor => _flags[Flag.IsInTheAir] ? _jumpingAcceleration : 1;
	#endregion

	#region Getters
	public Vector3 Position
	{
		get
		{
			return transform.position;
		}
	}
	public Vector3 Velocity
	{
		get
		{
			return _RB.velocity;
		}
	}

	public bool IsInTheAir
	{
		get
		{
			return _flags[Flag.IsInTheAir];
		}
	}
	#endregion

	#region Setters
	public bool PlayerInTheAir
	{
		get
		{
			return _flags[Flag.IsInTheAir];
		}
		set
		{
			if (!value)
			{
				if (_flags[Flag.IsInTheAir])
				{
					//Event
					PlayerLandedEvent?.Invoke();
					_flags[Flag.IsInTheAir] = false;
				}
				if (_flags[Flag.InCoyoteTime])
				{
					_coyoteTimer.GottaCount = false;
					_flags[Flag.InCoyoteTime] = false;
				}
				Glide(false);
			}
			else if (!_flags[Flag.InCoyoteTime] && !_flags[Flag.IsInTheAir])
			{
				_coyoteTimer.GottaCount = true;
				_flags[Flag.InCoyoteTime] = true;
			}
		}
	}
	public bool CollidingWithClimbable
	{
		set
		{
			_flags[Flag.ClimbCollision] = value;
		}
	}
	public bool InputCrouch
	{
		set
		{
			if (value)
			{
				_flags[Flag.Crouching] = true;
				//AnimControl.SetCrouch(true);
			}
			else
			{
				_flags[Flag.Crouching] = false;
			}
		}
	}
	public bool InputClimb
	{
		set
		{
			_flags[Flag.PlayerHoldingClimb] = value;
		}
	}
	public bool InputJump
	{
		set
		{
			_flags[Flag.JumpInput] = true;
		}
	}
	public bool InputGlide
	{
		set
		{
			_flags[Flag.GlideInput] = value;
		}
	}
	#endregion

	#region Flags
	enum Flag
	{
		IsInTheAir,
		JumpInput,
		GlideInput,
		PlayerHoldingClimb,
		Crouching,
		Gliding,
		Colliding,
		ClimbCollision,
		ColCounting,
		WeirdColCounting,
		Climbing,
		InCoyoteTime
	}
	Dictionary<Flag, bool> _flags = new Dictionary<Flag, bool>();

	#endregion

	#region Debug
	public float flash
	{
		set
		{
			Speed *= value;
		}
	}
	#endregion

	#endregion

	#region Unity
	void Start()
	{
		_audioManager = GameObject.Find("AudioManager").GetComponent<AudioManager>();
		try
		{
			_uManager = GameObject.FindObjectOfType<UpdateManager>();
		}
		catch (NullReferenceException)
		{
			print(this.name + "update manager not found");
		}
		if(_uManager != null) _uManager.AddFixedItem(this);
		_RB = GetComponent<Rigidbody>();
		_AnimControl = GetComponent<Player_Animator>();

		SetupFlags();
		InitializeTimers();
	}
	public void OnUpdate()
	{
		ControlJump();
		ControlClimb();
		AccelerateFall();
	}
	#endregion

	#region Private
	/// <summary>
	/// Setups the flags
	/// </summary>
	void SetupFlags()
	{
		foreach (var flag in (Flag[])Enum.GetValues(typeof(Flag)))
		{
			_flags.Add(flag, false);
		}
	}

	/// <summary>
	/// Setups the timers
	/// </summary>
	void InitializeTimers()
	{
		_colTimer = SetupTimer(_colTimeToOff, "Collider Timer");
		_coyoteTimer = SetupTimer(_coyoteTime, "Coyote Timer");
		_jumpTimer = SetupTimer(_inTheAirTimeToOff, "In the Air Timer");
		_climbTimer = SetupTimer(climbTimeToOff, "Climb Off Timer");
	}

	/// <summary>
	/// Event Handler for the timers
	/// </summary>
	protected override void TimerFinishedHandler(string ID)
	{
		switch (ID)
		{
			case "Collider Timer":
			{
				_flags[Flag.Colliding] = false;
				break;
			}
			case "Coyote Timer":
			{
				_flags[Flag.IsInTheAir] = true;
				_flags[Flag.InCoyoteTime] = false;

				//Event
				PlayerJumpedEvent();
				break;
			}
			case "In the Air Timer":
			{
				_flags[Flag.IsInTheAir] = true;
				break;
			}
			case "Climb Off Timer":
			{
				_flags[Flag.Climbing] = false;
				_RB.isKinematic = false;

				//Event
				PlayerJumpedEvent?.Invoke();
				break;
			}
		}
	}

	/// <summary>
	/// This function lets the body know if it should move or if moving would cause trouble
	/// </summary>
	/// <param name="Input">Player's input</param>
	/// <returns></returns>
	bool DecideIfWalk(Vector2 Input)
	{
		//Set Variables
		Vector3 horizontalCollisionNormal = lastContact.normal;
		horizontalCollisionNormal.y = 0;
		Vector3 inputNormal = transform.forward * Input.y + transform.right * Input.x;

		_collisionAngles = new Vector2(Vector3.Angle(inputNormal, horizontalCollisionNormal), Vector3.Angle(transform.up, lastContact.normal));

		//Conditions
		bool _conditionA = (_collisionAngles.y > _yMinAngle && _collisionAngles.y < _yMaxAngle);
		bool _conditionB = (_collisionAngles.x > _xMinAngle && _collisionAngles.x < _xMaxAngle);

		//Decide
		if (!_flags[Flag.Colliding])
		{
			return true;
		}
		return !(_conditionA && _conditionB);
	}

	/// <summary>
	/// Makes the player jump
	/// </summary>
	void ControlJump()
	{
		if (_flags[Flag.JumpInput])
		{
			_flags[Flag.JumpInput] = false;
			if (DecideIfJump())
			{
				//Physics
				Vector3 newVel = _RB.velocity;
				newVel.y = 0;
				_RB.velocity = newVel;
				_RB.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);

				//Event
				PlayerJumpedEvent?.Invoke();
				_jumpTimer.GottaCount = true;

				//Sound
				PlaySound(0);
			}
		}
		else
		{
			Glide(_flags[Flag.GlideInput]);
		}
	}

	/// <summary>
	/// Decides if the player can Jump
	/// </summary>
	/// <param name="HoldingButton">If the player is holding the button or just pressed it</param>
	/// <returns></returns>
	bool DecideIfJump()
	{
		#region OLD
		//print("IN THE AIR: " + _flags[Flag.IsInTheAir]);
		//if (_flags[Flag.IsInTheAir])
		//{
		//	Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1000, 1 << 11);
		//	if (hit.distance < transform.localScale.y / 2 + _jumpRaycastTreshold)
		//	{
		//		print("RAY JUMP");
		//		Debug.DrawRay(transform.position, Vector3.down, Color.green, 1);
		//		return true;
		//	}
		//	else
		//	{
		//		Debug.DrawRay(transform.position, Vector3.down, Color.red, 1);
		//		return false;
		//	}
		//}
		//else
		//{
		//	print("NORMAL JUMP");
		//	Debug.DrawRay(transform.position, Vector3.down, Color.yellow, 1);
		//	return true;
		//} 
		#endregion
		return _flags[Flag.IsInTheAir] ? false : true;
	}

	/// <summary>
	/// Accelerates the velocity of the player while falling to eliminate feather falling effet
	/// </summary>
	void AccelerateFall()
	{
		if (_RB.velocity.y < .5 && _RB.velocity.y > -15)
		{
			_RB.velocity += Vector3.up * Physics2D.gravity.y * (_FallMultiplier - 1) * Time.deltaTime;
		}
	}

	void ControlClimb()
	{
		if (_flags[Flag.PlayerHoldingClimb] && _flags[Flag.ClimbCollision])
		{
			_climbTimer.GottaCount = false;
			if (_flags[Flag.Climbing]) return;
			_flags[Flag.Climbing] = true;
			_RB.isKinematic = true;

			//Event
			PlayerClimbingEvent?.Invoke();
		}
		//-----------------------------------------------------------ACA--------------------------
		else if (_flags[Flag.Climbing])
		{
			if(!_climbTimer.Counting) _climbTimer.GottaCount = true;
			//_flags[Flag.Climbing] = false;
			//_RB.isKinematic = false;

			////Event
			//print("CLIMB");
			//PlayerJumpedEvent?.Invoke();
		}
	}

	void PlaySound(int Index)
	{
		try
		{
			_audioManager.PlayCharacterSound(_soundEffects[Index]);

		}
		catch (NullReferenceException)
		{
			print("(PBODY) AudioManager not found");
		}
	}

	#endregion

	#region Public
	/// <summary>
	/// Sets the Velocity for the Player
	/// </summary>
	/// <param name="Input"></param>
	public void Walk(Vector2 Input)
	{
		Vector3 NewVel;
		if (DecideIfWalk(Input))
		{
			NewVel = (transform.forward * Input.y + transform.right * Input.x) * Speed * CrouchSpeedFactor * JumpingAccelerationFactor + Vector3.up * _RB.velocity.y;
			_RB.velocity = NewVel;
		}
		else
		{
			NewVel = Vector3.zero + Vector3.up * _RB.velocity.y;
		}
		_RB.velocity = NewVel;
	}

	public void Climb(Vector2 Input)
	{
		if (!_flags[Flag.Climbing]) return;
		transform.position += (transform.right * Input.x + transform.up * Input.y) * _climbSpeed * Time.deltaTime;
	}

	/// <summary>
	/// Stops Characters jump to give the user more control
	/// </summary>
	public void StopJump()
	{
		_RB.velocity += Vector3.up * Physics2D.gravity.y * (_lowJumpMultiplier - 1) * Time.deltaTime;
		//COMMENT
		//Glide(false);
	}

	/// <summary>
	/// Turn on and off the gliding
	/// </summary>
	/// <param name="HoldingButton"></param>
	public void Glide(bool HoldingButton)
	{
		if (HoldingButton)
		{
			if (_RB.velocity.y < 0 && _flags[Flag.IsInTheAir] && !_flags[Flag.Gliding])
			{
				_RB.drag = _glidingDrag;
				_flags[Flag.Gliding] = true;
			}
		}
		else
		{
			_RB.drag = 0;
			_flags[Flag.Gliding] = false;
		}
	}
	#endregion

	#region Collisions
	private void OnCollisionStay(Collision collision)
	{
		_flags[Flag.Colliding] = true;
		lastContact = collision.contacts[0];
	}

	private void OnCollisionExit(Collision collision)
	{
		if (_flags[Flag.Colliding])
		{
			_colTimer.GottaCount = true;
		}
	}
	#endregion
}
