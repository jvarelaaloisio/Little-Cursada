using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickable_Item : MonoBehaviour, IPickable
{
	#region Variables

	#region Public

	#endregion

	#region Private

	#endregion

	#endregion

	#region Unity
	void Start()
    {
        
    }

    void Update()
    {
        
    }
	#endregion

	#region Public
	public void Pick(Transform picker)
	{
		transform.parent = picker;
	}

	public void Release()
	{
		transform.parent = null;
	}

	public void Throw(float force, Vector3 Origin)
	{

	}
	#endregion

	#region Private

	#endregion
}