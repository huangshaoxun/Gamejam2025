using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Obi;

public class ActorSpawner : MonoBehaviour {

	public ObiActor template;

	private int instances = 0;

	public float holdValue;
	
	// Update is called once per frame
	void Update () {
		
		
		if (Input.GetMouseButton(0))
		{
			holdValue += Time.deltaTime * 2;
			//Todo Audio 蓄力
			if (holdValue - Time.deltaTime * 2 < 0.15f && holdValue > 0.15f)
			{
				FaceController.Instance.SetFaceType(FaceController.FaceType.Thinking);
			}
		}
		
		if (Input.GetMouseButtonUp(0))
		{
			var size = Mathf.Clamp((int)holdValue+1, 1, 3);
			holdValue = 0;
			GameObject go = Instantiate(template.gameObject,transform.position,Quaternion.identity);
			go.transform.SetParent(transform.parent);

			// 按鼠标位置生成，并且设置为主角, 主角是无色，并且主角位移强度为100
			Vector3 pos = Input.mousePosition; // GetMousePosition();
			pos.z = 10;// Distance;
			pos = Camera.main.ScreenToWorldPoint(pos);
			const int sz = 6;
			pos.x = Mathf.Clamp(pos.x, -sz, sz);
			pos.y = -Mathf.Cos(Mathf.Abs(pos.x / sz) * 90 * Mathf.Deg2Rad) * sz;
			go.transform.position = pos;
			var bubble  = go.GetComponent<Bubble>();
			if (bubble != null)
			{
				bubble.isPlayer = true; 
				bubble.color = BubbleColor.None;
				bubble.ResetSize(size);
				bubble.GetComponent<AddRandomVelocity>().intensity = 50 - size * 10;
				//Todo Audio 发射
				FaceController.Instance.SetFaceType(FaceController.FaceType.Question);
			}
            
			go.SetActive(true);
		}
		
	}
}
