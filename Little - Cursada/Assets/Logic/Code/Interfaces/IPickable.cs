using UnityEngine;
public interface IPickable
{
	void Pick(Transform picker);
	void Release();

	/// <summary>
	/// Throw the object with an explosive force
	/// </summary>
	/// <param name="force">amount of force</param>
	/// <param name="Origin">position of the thrower</param>
	void Throw(float force, Vector3 Origin);
}