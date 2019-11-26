using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBody
{
	void Push(Vector3 direction, float force);
}