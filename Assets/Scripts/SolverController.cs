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
    private ObiSolver solver;
    public SoftBodyBluePrintPack bpPack;
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

            var canMerge = bubbleA.color == bubbleB.color;//&& bubbleA.canMerge && bubbleB.canMerge;
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
                change[main as ObiSoftbody] = bpPack.BpList[sumSize - 1];
                main.GetComponent<Bubble>().size = sumSize;
                foreach (var one in component.Value)
                {
                    if (one != main)
                    {
                        Destroy(one.gameObject);
                    }
                }
            }
        }
        change.ForEach(x => x.Key.softbodyBlueprint = x.Value);
    }
    
}
