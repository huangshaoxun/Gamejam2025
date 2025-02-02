using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using Obi;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class SolverController : MonoBehaviour
{
    public static SolverController Instance;

    private ObiSolver solver;
    public SoftBodyBluePrintPack bpPack;
    public int score;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private Vector3 lastMousePosition; // 上一帧的鼠标位置
    private float currentVolume = 0f; // 当前音量
    private bool isRightMouseReleased = false; // 标志：是否松开了右键
    public float maxVolume = 1f; // 最大音量
    public float speedToVolumeFactor = 0.01f; // 鼠标速度到音量的转换因子
    public float volumeFadeSpeed = 2f; // 音量淡出速度
    public float mouseSpeed;

    void Start()
    {
        solver = GetComponent<ObiSolver>();
        solver.OnParticleCollision += DoCollision;
    }
    

    private UniFind<ObiActor> merge = new UniFind<ObiActor>();
    private HashSet<(ObiActor, ObiActor)> touched = new();
    Dictionary<ObiSoftbody, ObiSoftbodyBlueprintBase> change = new ();
    private void DoCollision(ObiSolver obiSolver, ObiNativeContactList contacts)
    {
        merge.Refresh(solver.actors);
        touched.Clear();
        change.Clear();
        foreach (var contact in contacts.Where(x=>x.distance < 0.01))
        { 
            var actorA = obiSolver.particleToActor[obiSolver.simplices[contact.bodyA]]?.actor;
            var actorB = obiSolver.particleToActor[obiSolver.simplices[contact.bodyB]]?.actor;
            if (actorA == null || actorB == null || actorA == actorB || touched.Contains((actorA, actorB)) || touched.Contains((actorB, actorA)))
            {
                continue;
            }
            touched.Add((actorA, actorB));
            var bubbleA = actorA.GetComponent<Bubble>();
            var bubbleB = actorB.GetComponent<Bubble>();
            
            if (bubbleA.isPlayer)
            {
                bubbleB.TouchByPlayer();
            }else if (bubbleB.isPlayer)
            {
                bubbleA.TouchByPlayer();
            }

            var canMerge = bubbleA.color == bubbleB.color && (bubbleA.canMerge + bubbleB.canMerge >= 3);
            if (canMerge) merge.Union(actorA, actorB);
        }

        var components = merge.GetAllComponents();
        foreach (var component in components)
        {
            if (component.Value.Count > 1)
            {
                var pos = Vector3.zero;
                var sumSize = 0;
                var maxSize = component.Value.Max(x => x.GetComponent<Bubble>().size);
                var main = component.Value.FirstOrDefault(x => x.GetComponent<Bubble>().size == maxSize);
                component.Value.ForEach(x => pos += x.transform.position * x.GetComponent<Bubble>().size);
                component.Value.ForEach(x => sumSize += x.GetComponent<Bubble>().size);
                pos /= sumSize;
                component.Key.AddForce((pos - component.Key.transform.position) * 2, ForceMode.VelocityChange);
                if (sumSize - 1 > bpPack.BpList.Count - 1)
                {
                    continue;
                }
                change[main as ObiSoftbody] = bpPack.BpList[sumSize - 1];
                var bubble = main.GetComponent<Bubble>();
                if (bubble != null)
                {
                    bubble.size = sumSize;
                    bubble.name = "New Big" + sumSize;
                    bubble.RefreshMaterial();
                    // EditorApplication.isPaused = true;
                    bubble.RefreshOnNewBigOne(BubbleSpawner.Instance.newBigOneStartScale, BubbleSpawner.Instance.newBigOneTransitionTime);
                }

                //Todo Audio 合并
                AudioManager.Instance.PlaySFX("104942__glaneur-de-sons__bubble-3",false,0.3f,5f);

                FaceController.Instance.SetFaceType(FaceController.FaceType.Wow);
                score += sumSize * component.Value.Count;
                CanvasController.Instance.SetScore(score);
                foreach (var one in component.Value)
                {
                    if (one != main)
                    {
                        Destroy(one.gameObject);
                    }
                }
            }
        }

        // solver.synchronization = change.Count > 0 ? ObiSolver.Synchronization.Synchronous: ObiSolver.Synchronization.Asynchronous;

        change.ForEach(x => x.Key.softbodyBlueprint = x.Value);

    }

    private void Update()
    {
        //Todo Audio 旋转
        if (Input.GetMouseButtonDown(1))
        {
            FaceController.Instance.SetFaceType(FaceController.FaceType.BrainStorm);

            AudioManager.Instance.PlaySFX("Audio_Rotation",true);
            AudioManager.Instance.SetSFXVolume("Audio_Rotation", 0f);
        }
        else if (Input.GetMouseButton(1))
        {
            Vector3 pos = Input.mousePosition; // GetMousePosition();
            pos.z = 10; // Distance;
            var worldPos = Camera.main.ScreenToWorldPoint(pos);
            FaceController.Instance.SetEyePos(-(GameDef.GlobalCenter - worldPos).normalized);


            Vector3 currentMousePosition = Input.mousePosition;
            // 计算鼠标移动速度
            Vector3 deltaPosition = currentMousePosition - lastMousePosition; // 鼠标位置差
            float speed = deltaPosition.magnitude / Time.deltaTime; // 速度 = 距离 / 时间
            mouseSpeed = speed;

            // 根据速度计算音量
            currentVolume = Mathf.Lerp(currentVolume, Mathf.Clamp(speed * speedToVolumeFactor, 0, maxVolume), Time.deltaTime * 5f);

            // 设置音量
            AudioManager.Instance.SetSFXVolume("Audio_Rotation", currentVolume);
        } 
        else if (Input.GetMouseButtonUp(1))
        {
            FaceController.Instance.SetFaceType(FaceController.FaceType.Default);
            FaceController.Instance.SetEyePos(Vector3.zero);

            AudioManager.Instance.StopSFX("Audio_Rotation");
        }
    }
}
