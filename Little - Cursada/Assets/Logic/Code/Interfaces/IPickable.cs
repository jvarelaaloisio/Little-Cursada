using UnityEngine;
public interface IPickable
{
	void Pick(Transform picker);
	void Release();
}