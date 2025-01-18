//using System;
using UnityEngine;

namespace Gumou {

    public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour {
        private static T _S;
        public static T S {
            get {
                if (_S == null) {
                    Object[] objs = FindObjectsOfType(typeof(T));
                    if (objs.Length > 1) {
                        Debug.LogError($"There are {objs.Length} instances of the SingletonBehaviour<{typeof(T)}> in the scene!!!!");
                    }
                    if (objs.Length == 1) {
                        _S = (T)objs[0];
                    } else {
                        Debug.LogWarning("singleton == null");
                    }
                }
                return _S;
            }
            set {
                _S = value;
                Debug.LogWarning($"Froce set singleton of {typeof(T).Name}");
            }
        }
    }
}

