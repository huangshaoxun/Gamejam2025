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


	public bool isMouseDown;
	public Vector3 lastMouse;
	public Vector3 rot2;
	void Update () {
		// if (Input.GetKeyDown(KeyCode.Space)){
		// 	GetComponent<ObiActor>().AddForce(UnityEngine.Random.onUnitSphere*intensity,ForceMode.VelocityChange);
		// }

		if (bubble.isPlayer)
		{
			GetComponent<ObiActor>().AddForce(Vector3.up * intensity,ForceMode.Acceleration);
			if (bubble.transform.position.y > 5f)
			{
				Destroy(gameObject);
			}
		}
		else
		{
			if (Input.GetMouseButton(1))
			{
				Vector3 pos = Input.mousePosition; // GetMousePosition();
				pos.z = 10; // Distance;
				var worldPos = Camera.main.ScreenToWorldPoint(pos);
				var v = (Camera.main.ScreenToWorldPoint(pos) - transform.position);
				v = v.normalized * Mathf.Clamp(v.magnitude, 5, 10);
				GetComponent<ObiActor>().AddForce(v, ForceMode.Acceleration);
				if (!isMouseDown)
				{
					isMouseDown = true;
					GetComponent<ObiActor>().AddForce(-(GameDef.GlobalCenter - transform.position), ForceMode.VelocityChange);
				}
				else
				{
					var rot = Quaternion.FromToRotation(lastMouse - GameDef.GlobalCenter, transform.position - GameDef.GlobalCenter);
					var v2 = Vector3.Dot((worldPos - lastMouse), (lastMouse - GameDef.GlobalCenter).normalized) * (lastMouse - GameDef.GlobalCenter).normalized + (worldPos - lastMouse);
					v2 = v2.normalized * Mathf.Clamp(v2.magnitude, 5, 10);
					rot2 = (rot * v2);
					GetComponent<ObiActor>().AddForce((rot * v2), ForceMode.Acceleration);
				}
				lastMouse = worldPos;
			}
			else
			{
				if (isMouseDown)
				{
				}
				isMouseDown = false;
				//GetComponent<ObiActor>().AddForce(GameDef.GlobalCenter - transform.position, ForceMode.Acceleration);
			}
			GetComponent<ObiActor>().AddForce(GameDef.GlobalCenter - transform.position, ForceMode.Acceleration);

		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawLine(transform.position, transform.position + rot2);
	}
}
