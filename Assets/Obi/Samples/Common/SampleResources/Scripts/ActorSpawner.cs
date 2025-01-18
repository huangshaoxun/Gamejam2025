using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Obi;

public class ActorSpawner : MonoBehaviour {

	public ObiActor template;

	public int maxInstances = 32;
	public float spawnDelay = 0.3f;

	private int instances = 0;
	private float timeFromLastSpawn = 0;
	
	// Update is called once per frame
	void Update () {

		timeFromLastSpawn += Time.deltaTime;

		if (Input.GetMouseButtonDown(0) && instances < maxInstances && timeFromLastSpawn > spawnDelay)
		{
			GameObject go = Instantiate(template.gameObject,transform.position,Quaternion.identity);
            go.transform.SetParent(transform.parent);

            // 按鼠标位置生成，并且设置为主角, 主角是无色，并且主角位移强度为100
            Vector3 pos = Input.mousePosition; // GetMousePosition();
            pos.z = 10;// Distance;
            go.transform.position = Camera.main.ScreenToWorldPoint(pos);
            var bubble  = go.GetComponent<Bubble>();
            if (bubble != null)
            {
	            bubble.isPlayer = true; //     
	            bubble.color = BubbleColor.None;
	            bubble.GetComponent<AddRandomVelocity>().intensity = 100;
            }
            
            go.SetActive(true);
			instances++;
			timeFromLastSpawn = 0;
		}
	}
}
