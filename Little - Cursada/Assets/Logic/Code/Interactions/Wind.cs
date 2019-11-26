using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Wind : MonoBehaviour
{
	#region Variables

	#region Public
	[SerializeField]
	Vector3 direction;
	[SerializeField]
	float force;
	#endregion


	#endregion

	#region Unity
	void Awake()
	{
		GetComponent<Collider>().isTrigger = true;
	}
	#endregion

	#region Collisions
	private void OnTriggerEnter(Collider other)
	{
		IBody characterBody = other.GetComponent<IBody>();
		if(characterBody != null)
		{
			characterBody.Push(direction, force);
		}
	}
	#endregion
}
