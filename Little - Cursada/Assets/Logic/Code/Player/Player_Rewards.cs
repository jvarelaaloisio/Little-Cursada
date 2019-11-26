using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Player_Rewards : EnumeratorManager, IUpdateable
{
	#region Public
	public int levelAmount;
	public float[] specialCooldowns;
	public Text coinsText,
		moonsText,
		specialsText;
	public int SpecialsEarned
	{
		get
		{
			return specialCount;
		}
	}
	#endregion

	#region Private
	GameManager Manager;
	UpdateManager _uManager;
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
		try
		{
			_uManager = GameObject.FindObjectOfType<UpdateManager>();
			_uManager.AddItem(this);
		}
		catch (NullReferenceException)
		{
			print(this.name + "update manager not found");
		}
		InitializeArrays(levelAmount);
	}

	public void OnUpdate()
	{
		if (coinsText != null)
		{
			coinsText.text = _coinsPerLevel[_actualLevel].ToString();
		}
		if (moonsText != null)
		{
			moonsText.text = _moonsPerLevel[_actualLevel].ToString();
		}
		if (specialsText != null)
		{
			specialsText.text = specialCount.ToString();
		}
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
				break;
			}
		}
	}
	#endregion
}