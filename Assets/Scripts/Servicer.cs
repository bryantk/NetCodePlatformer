﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Servicer : MonoBehaviour
{

	public PlatformGNM Netcode;
    public ChatManager ChatManager;
	public TrackedPool TrackedObjects;
	public Spawner Spawn;

	private static Servicer _instance;
	public static Servicer Instance
	{
		get { return _instance; }
		private set
		{
			if (_instance != null)
				Destroy(value);
			_instance = value;
		}
	}

	void Awake()
	{
		Instance = this;
		if (Instance != this) return;

	}

}
