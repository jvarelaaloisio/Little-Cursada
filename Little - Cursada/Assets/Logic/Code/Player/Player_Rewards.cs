using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Rewards : EnumeratorManager
{
	#region Public
	public int levelAmount;
	public float[] specialCooldowns;

	#endregion

	#region Private
	GameManager Manager;
	int _actualLevel;
	int[] _coinsPerLevel;
	int[] _moonsPerLevel;
	//bool[] _specialAdded;
	//float[] _specialTimer;

	int specialCount;
	#endregion

	#region Unity
	private void Awake()
	{
		_actualLevel = SceneManager.GetActiveScene().buildIndex;
		Manager = GameObject.FindObjectOfType<GameManager>();
		InitializeArrays(levelAmount);
	}
	#endregion

	#region Private
	void InitializeArrays(int amount)
	{
		_coinsPerLevel = new int[amount];
		_moonsPerLevel = new int[amount];
	}
	void UpdateTimers()
	{

	}
	#endregion

	#region Public
	public void GetReward(int kindOfReward)
	{
		switch (kindOfReward)
		{
			default:
			{
				print("The kind " + kindOfReward + "has not been defined, it will be passed as a coin");
				_coinsPerLevel[_actualLevel] += 1;
				break;
			}
			case (int)Reward.coin:
			{
				_coinsPerLevel[_actualLevel] += 1;
				break;
			}
			case (int)Reward.moon:
			{
				_moonsPerLevel[_actualLevel] += 1;
				break;
			}
			case (int)Reward.win:
			{
				specialCount += 1;
				if(specialCount >= 2)
				{
					Manager.PlayerIsDead(true);
				}
				break;
			}
		}
	}
	#endregion
}