using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbCollider : MonoBehaviour
{
	#region Variables
	#region Private
	Player_Body _body;
	#endregion

	#endregion

	#region Unity
	void Start()
	{
		_body = GetComponentInParent<Player_Body>();
	}
	#endregion

	#region Collisions
	private void OnTriggerEnter(Collider other)
	{
		_body.CollidingWithClimbable = true;
	}
	private void OnTriggerExit(Collider other)
	{
		_body.CollidingWithClimbable = false;
	}
	#endregion
}
