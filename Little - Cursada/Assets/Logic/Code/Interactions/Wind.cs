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
	void Start()
	{
		GetComponent<Collider>().isTrigger = true;
		GetComponentInChildren<ParticleSystem>().Play();
	}
	#endregion

	#region Collisions
	//private void OnTriggerEnter(Collider other)
	//{
	//	if (!_isOn) return;
	//	IBody characterBody = other.GetComponent<IBody>();
	//	if (characterBody == null) return;
	//	characterBody.Push(direction, force);
	//	//_particles.Stop();
	//	//_isOn = false;
	//}
	private void OnTriggerStay(Collider other)
	{
		IBody characterBody = other.GetComponent<IBody>();
		if (characterBody == null) return;
		print("push");
		characterBody.Push(transform.up, force);
	}
	#endregion
}
