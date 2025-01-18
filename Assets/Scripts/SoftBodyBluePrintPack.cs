using System.Collections.Generic;
using Obi;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DefaultNamespace
{
    
    [CreateAssetMenu(fileName = "SoftBodyBluePrintPack")]
    public class SoftBodyBluePrintPack : ScriptableObject
    {
        public List<ObiSoftbodySurfaceBlueprint> BpList;
        
        #if UNITY_EDITOR
        [Button]
        void GenerateBp()
        {
            int i = 1;
            foreach (var bp in BpList)
            {
                var str = AssetDatabase.GetAssetPath(bp);
                var data = AssetDatabase.LoadAssetAtPath<ObiSoftbodySurfaceBlueprint>(str);
                data.scale = new Vector3((1 + (i - 1) * 0.25f), (1 + (i - 1) * 0.25f), 1);
                data.surfaceResolution = i + 4;
                i++;
                bp.GenerateImmediate();
                EditorUtility.SetDirty(data);
            }
            AssetDatabase.Refresh();
        }
        #endif
    }
}