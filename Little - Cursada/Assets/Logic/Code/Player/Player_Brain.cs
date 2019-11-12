using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Player_Body))]
[RequireComponent(typeof(Damage_Handler))]
[RequireComponent(typeof(Player_Animator))]
public class Player_Brain : GenericFunctions, IUpdateable
{
	#region Variables

	#region Public
	public bool GodMode;
	#endregion

	#region Serialized
	Vector3 origin;
	[SerializeField]
	[Range(0, 5)]
	float cameraFollowTime = 0.5f;
	#endregion

	#region Private
	GameManager _manager;
	UpdateManager _uManager;
	Player_Animator _animControl;
	Player_Body _body;
	Damage_Handler _damageHandler;
	Timer _followCameraTimer;
	Vector2 _movInput;

	#region Flags
	enum Flag
	{
		IS_DEAD,
		IS_JUMPING,
		IS_CLIMBING,
		IS_GLIDING,
		IS_GROUND,
		IS_HIT,

		//this gotta be the last flag
		PIVOT
	}
	bool[] _flags = new bool[(int)Flag.PIVOT];
	#endregion

	#region States
	[SerializeField]
	PlayerState _state;
	#endregion

	#endregion

	#endregion

	#region Unity
	void Start()
	{
		origin = transform.position;
		origin.y += 2;
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
			_manager = GameObject.FindObjectOfType<GameManager>();
		}
		catch (NullReferenceException)
		{
			print(this.name + "game manager not found");
		}
		SetupVariables();
		SetupHandlers();
	}
	public void OnUpdate()
	{
		_state = ControlStates(_state);
		_animControl.ChangeState(_state);
		switch (_state)
		{
			case PlayerState.WALKING:
			{
				ReadWalkingStateInput();
				break;
			}
			case PlayerState.JUMPING:
			{
				ReadJumpingStateInput();
				CheckIfJumpStops();
				break;
			}
			case PlayerState.CLIMBING:
			{
				ReadClimbingStateInput();
				ControlClimbInput();
				break;
			}
			case PlayerState.GOT_HIT:
			{
				break;
			}
			case PlayerState.DEAD:
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
				break;
			}
		}
	}
	#endregion

	#region Public
	public Vector3 GivePostion()
	{
		return _body.Position;
	}
	#endregion

	#region Private

	/// <summary>
	/// Called in the start to prepeare this script
	/// </summary>
	void SetupVariables()
	{
		_body = GetComponent<Player_Body>();
		_damageHandler = GetComponent<Damage_Handler>();
		_animControl = GetComponent<Player_Animator>();
		_followCameraTimer = SetupTimer(cameraFollowTime, "Follow Camera Timer");
	}

	#region Input
	/// <summary>
	/// reads input from the movement axises
	/// </summary>
	Vector2 ReadHorInput()
	{
		return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
	}

	/// <summary>
	/// Tells the body to change the velocity in the horizontal plane
	/// </summary>
	void ControlHorMovement()
	{
		_movInput = ReadHorInput();
		if (_movInput == Vector2.zero) return;
		_body.Walk(_movInput);
		UpdateForward();
	}

	/// <summary>
	/// reads input to tell the body if the player wants to walk
	/// </summary>
	void ControlCrouchInput()
	{
		//_body.InputCrouch = Input.GetButton("Crouch");
	}

	/// <summary>
	/// reads input to tell the body if the player wants to climb
	/// </summary>
	void ControlClimbInput()
	{
		_body.InputClimb = Input.GetButton("Climb");
	}

	/// <summary>
	/// Reads the input for the walking state
	/// </summary>
	void ReadWalkingStateInput()
	{
		ControlHorMovement();
		//REVISAR
		if (_movInput != Vector2.zero) FollowCameraRotation();

		//Jump
		if (Input.GetButton("Jump")) _body.InputJump = true;

		//Crouch
		ControlCrouchInput();

		ControlClimbInput();
	}

	/// <summary>
	/// Reads the input for the Jumping State
	/// </summary>
	void ReadJumpingStateInput()
	{
		ControlHorMovement();
		//REVISAR
		if (_movInput != Vector2.zero) FollowCameraRotation();

		//Glide
		_body.InputGlide = Input.GetButton("Jump");

		//Crouch
		ControlCrouchInput();

		ControlClimbInput();
	}

	void ReadClimbingStateInput()
	{
		_movInput = ReadHorInput();
		_body.Climb(_movInput);
	}

	/// <summary>
	/// Checks if the player has stopped pressing the jump button
	/// </summary>
	void CheckIfJumpStops()
	{
		if (_body.Velocity.y > 0 && !Input.GetButton("Jump"))
		{
			_body.StopJump();
		}
	}
	#endregion

	#region EventHandlers
	/// <summary>
	/// Setups the event handlers for the body events
	/// </summary>
	void SetupHandlers()
	{
		_body.PlayerJumpedEvent += JumpEventHandler;
		_body.PlayerClimbingEvent += ClimbEventHandler;
		_body.PlayerLandedEvent += LandEventHandler;
		_damageHandler.LifeChangedEvent += LifeChangedHandler;
	}

	/// <summary>
	/// Handles the body event when the player jumps
	/// </summary>
	void JumpEventHandler()
	{
		_flags[(int)Flag.IS_JUMPING] = true;
		_flags[(int)Flag.IS_GROUND] = false;
	}

	/// <summary>
	/// Handles the body event when the player starts climbing
	/// </summary>
	void ClimbEventHandler()
	{
		_flags[(int)Flag.IS_JUMPING] = false;
		_flags[(int)Flag.IS_CLIMBING] = true;
	}

	/// <summary>
	/// Handles the body event when the player climbs
	/// </summary>
	void LandEventHandler()
	{
		_flags[(int)Flag.IS_GROUND] = true;
		_flags[(int)Flag.IS_JUMPING] = false;
	}

	/// <summary>
	/// Handles the damage_handler event for when the player's life has changed
	/// </summary>
	/// <param name="newLife"></param>
	void LifeChangedHandler(float newLife)
	{
		if (newLife <= 0) Die();
		else
		{
			_flags[(int)Flag.IS_HIT] = true;
		}
	}
	protected override void TimerFinishedHandler(string ID)
	{
	}
	#endregion

	/// <summary>
	/// Here goes everything to do when the player dies
	/// </summary>
	void Die()
	{
		if (GodMode || _flags[(int)Flag.IS_DEAD]) return;
		_flags[(int)Flag.IS_DEAD] = true;
		_manager.PlayerIsDead(false);
	}

	/// <summary>
	/// returns the new state for the player
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
	PlayerState ControlStates(PlayerState state)
	{
		if (_flags[(int)Flag.IS_DEAD]) return PlayerState.DEAD;
		if (_flags[(int)Flag.IS_HIT])
		{
			_flags[(int)Flag.IS_HIT] = false;
			return PlayerState.GOT_HIT;
		}
		switch (state)
		{
			case PlayerState.WALKING:
			{
				if (_flags[(int)Flag.IS_JUMPING])
				{
					_flags[(int)Flag.IS_JUMPING] = false;
					return PlayerState.JUMPING;
				}
				if (_flags[(int)Flag.IS_CLIMBING])
				{
					_flags[(int)Flag.IS_CLIMBING] = false;
					return PlayerState.CLIMBING;
				}
				break;
			}
			case PlayerState.JUMPING:
			{
				if (_flags[(int)Flag.IS_GROUND] || !_body.IsInTheAir)
				{
					_flags[(int)Flag.IS_GROUND] = false;
					return PlayerState.WALKING;
				}
				if (_flags[(int)Flag.IS_CLIMBING])
				{
					_flags[(int)Flag.IS_CLIMBING] = false;
					return PlayerState.CLIMBING;
				}
				break;
			}
			case PlayerState.CLIMBING:
			{
				if (_flags[(int)Flag.IS_GROUND])
				{
					_flags[(int)Flag.IS_GROUND] = false;
					return PlayerState.WALKING;
				}
				if (_flags[(int)Flag.IS_JUMPING])
				{
					_flags[(int)Flag.IS_JUMPING] = false;
					return PlayerState.JUMPING;
				}
				break;
			}
			case PlayerState.GOT_HIT:
			{
				if (_damageHandler.IsInmune) return PlayerState.GOT_HIT;
				if (_flags[(int)Flag.IS_GROUND])
				{
					_flags[(int)Flag.IS_GROUND] = false;
					return PlayerState.WALKING;
				}
				if (_flags[(int)Flag.IS_JUMPING])
				{
					_flags[(int)Flag.IS_JUMPING] = false;
					return PlayerState.JUMPING;
				}
				print("WALKING");
				return PlayerState.WALKING;
			}
			case PlayerState.DEAD:
			{

				break;
			}
		}
		return state;
	}

	/// <summary>
	/// Makes the body rotate to the camera direction
	/// </summary>
	void FollowCameraRotation()
	{
		if (_followCameraTimer.Counting) return;
		_followCameraTimer.GottaCount = true;
	}
	void UpdateForward()
	{
		float pivot = SmoothFormula(_followCameraTimer.CurrentTime, cameraFollowTime);
		if (_followCameraTimer.Counting) transform.eulerAngles = new Vector3(0, Mathf.LerpAngle(transform.eulerAngles.y, _manager.GiveCamera().eulerAngles.y, pivot), 0);
	}
	#endregion

	#region Gizmos
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawLine(transform.position, transform.position + transform.forward);
	}

	#endregion
}