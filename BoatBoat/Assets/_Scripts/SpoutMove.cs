﻿using UnityEngine;
using System.Collections;

public class SpoutMove : MonoBehaviour
{
	public float speed;
	public float waitSpawn;
	public Vector3 spoutHeight;
	public Vector3 minSpoutHeight;
	public bool goingUp = true;
	// Use this for initialization
	void Start ()
	{	
		waitSpawn = 5;
	}
	
	// Update is called once per frame
	void Update () 
	{	
		if (waitSpawn < 0)
		{
			if (goingUp)
			{
				rigidbody.position += Vector3.up * speed * Time.deltaTime;
				if (rigidbody.position.y > spoutHeight.y)
				{
					goingUp = false;	
				}
			}
			else
			{
				rigidbody.position += Vector3.down * speed * Time.deltaTime;
				if (rigidbody.position.y < minSpoutHeight.y)
				{
					goingUp = true;	
				}
			}
		}
		else
		{
			waitSpawn -= Time.deltaTime;
		}

	
	}
}