using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Mushroom : Hazzard
{
	#region Variables
	#region Serialized
	[SerializeField]
	float damageTimeToOn = 3,
		damageTimeToOff = 1,
		ticTime = 0.5f;
	[SerializeField]
	ParticleSystem particles = null;
	#endregion

	#region Private
	SphereCollider _collider;
	Timer _damageTurnOnTimer,
		_damageTurnOffTimer,
		_ticTimer;
	#endregion

	#endregion

	#region Unity
	void Start()
	{
		_collider = GetComponent<SphereCollider>();
		_collider.enabled = false;
		InitializeTimers();
		_damageTurnOnTimer.GottaCount = true;
	}
	#endregion

	#region Private
	/// <summary>
	/// Setup of all the timers
	/// </summary>
	void InitializeTimers()
	{
		_ticTimer = SetupTimer(ticTime, "Tic Timer");
		_damageTurnOffTimer = SetupTimer(damageTimeToOff, "Damage Off Timer");
		_damageTurnOnTimer = SetupTimer(damageTimeToOn, "Damage On Timer");

	}
	protected override void TimerFinishedHandler(string ID)
	{
		switch (ID)
		{
			case "Damage On Timer":
			{
				_collider.enabled = true;
				_ticTimer.GottaCount = true;
				_damageTurnOffTimer.GottaCount = true;
				particles.Stop();
				break;
			}
			case "Damage Off Timer":
			{
				if(_damageables.Count <= 0) _collider.enabled = false;
				_ticTimer.GottaCount = false;
				_damageTurnOnTimer.GottaCount = true;
				particles.Play();
				break;
			}
			case "Tic Timer":
			{
				Attack();
				_ticTimer.GottaCount = true;
				break;
			}
		}
	}
	#endregion

	#region Collisions
	#endregion
}
