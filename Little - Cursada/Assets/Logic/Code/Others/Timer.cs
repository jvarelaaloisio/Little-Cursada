﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void ClockTicking(string ID);

public class Timer : MonoBehaviour, IUpdateable
{
	#region Variables

	#region Public
	public event ClockTicking ClockTickingEvent;
	#endregion

	#region Setters
	public bool GottaCount
	{
		set
		{
			_counting = value;
			_currentTime = 0;
		}
	}
	#endregion

	#region Getters
	public bool Counting
	{
		get
		{
			return _counting;
		}
	}
	public float CurrentTime
	{
		get
		{
			return _currentTime;
		}
	}
	#endregion

	#region Private
	UpdateManager _uManager;
	float _currentTime = 0,
		_totalTime = 0;
	bool _counting;
	string _id;
	#endregion

	#endregion

	#region Unity
	private void Awake()
	{
		_uManager = GameObject.FindObjectOfType<UpdateManager>();
		_uManager.AddItem(this);
		ClockTickingEvent = delegate { };
	}
	public void OnUpdate()
	{
		UpdateTimer();
	}
	#endregion

	#region Private
	void UpdateTimer()
	{
		if (!_counting) return;
		_currentTime += Time.deltaTime;
		CheckIfFinished();
	}
	void CheckIfFinished()
	{
		if (_currentTime >= _totalTime)
		{
			_counting = false;
			_currentTime = 0;
			ClockTickingEvent(_id);
		}
	}
	#endregion

	#region Public
	public void Instantiate(float newTotalTime, string newID)
	{
		_totalTime = newTotalTime;
		_id = newID;
	}
	#endregion
}
