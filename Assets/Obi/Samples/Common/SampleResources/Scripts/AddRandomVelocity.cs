using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Obi;

public class AddRandomVelocity : MonoBehaviour {

	public float intensity = 5;
	public Bubble bubble;

	private void Start()
	{
		bubble = GetComponent<Bubble>();
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)){
			GetComponent<ObiActor>().AddForce(UnityEngine.Random.onUnitSphere*intensity,ForceMode.VelocityChange);
		}

		if (bubble.isPlayer)
		{
			GetComponent<ObiActor>().AddForce(Vector3.up * intensity,ForceMode.Acceleration);
		}
	}
}
