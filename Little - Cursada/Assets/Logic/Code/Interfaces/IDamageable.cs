using UnityEngine;
public interface IDamageable
{
	void TakeDamage(float damage);
}
public interface IPickable
{
	void Pick(Transform picker);
	void Release();
}
public enum PlayerState
{
	WALKING,
	JUMPING,
	CLIMBING,
	GOT_HIT,
	DEAD
}