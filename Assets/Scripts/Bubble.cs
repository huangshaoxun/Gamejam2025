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
        None, // 无色，代表主角
        Red,
        Yellow,
        Green,
        Blue,
    }

    public static class GameDef
    {
        public static Dictionary<BubbleColor, Color> Map = new Dictionary<BubbleColor, Color>
        {
            { BubbleColor.None, Color.white},
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

        private Material _material;
        public const string SHADER_PROPERTY_MAINCOL = "_BaseColor";  // TODO: 等蛋蛋补充
        
        
        public void TouchByPlayer()
        {
            _mergeTime = 0.5f;
        }
        private void Start()
        {
            _material = transform.GetComponent<Renderer>().material;
            GetComponent<ObiSoftbody>().softbodyBlueprint = BpPack.BpList[size - 1];

            RefreshMaterial();
        }

        private void FixedUpdate()
        {
            _mergeTime -= Time.fixedDeltaTime;
        }
        
        
        
        #region 表现层刷新

        /// <summary>
        /// 刷新材质表现
        /// </summary>
        public void RefreshMaterial()
        {
            if (GameDef.Map.ContainsKey(color) && _material != null)
            {
                var matColor = GameDef.Map[color];
                _material.SetColor(SHADER_PROPERTY_MAINCOL, matColor);
                // TODO:待蛋蛋补充.
                
            }
        }
        
        #endregion
    }
}