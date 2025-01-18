using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using Obi;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    [Serializable]
    public enum BubbleColor
    {
        Red,
        Yellow,
        Green,
        Blue,
    }

    public static class GameDef
    {
        public static Dictionary<BubbleColor, Color> Map = new Dictionary<BubbleColor, Color>
        {
            { BubbleColor.Blue, Color.blue },
            { BubbleColor.Red, Color.red },
            { BubbleColor.Yellow, Color.yellow },
            { BubbleColor.Green, Color.green },
        };
    }


    public class Bubble : MonoBehaviour
    {
        public BubbleColor color;
        public int size = 1;
        public SoftBodyBluePrintPack BpPack;
        private float _mergeTime;
        public bool isPlayer;
        public bool canMerge => _mergeTime > 0 && _mergeTime < 0.25f;


        public void TouchByPlayer()
        {
            _mergeTime = 0.5f;
        }
        private void Start()
        {
            GetComponent<ObiSoftbody>().softbodyBlueprint = BpPack.BpList[size - 1];
        }

        private void FixedUpdate()
        {
            _mergeTime -= Time.fixedDeltaTime;
        }
    }
}