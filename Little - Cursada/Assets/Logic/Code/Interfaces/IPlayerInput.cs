using UnityEngine;
public interface IPlayerInput
{
	/// <summary>
	/// Returns true if the player is pressing the climb button
	/// </summary>
	/// <returns></returns>
	bool ReadClimbInput();
	/// <summary>
	/// Returns the input in the hor axis
	/// </summary>
	/// <returns></returns>
	Vector2 ReadHorInput();
	/// <summary>
	/// Returns true if the player is pressing the jump button
	/// </summary>
	/// <returns></returns>
	bool ReadJumpInput();
}