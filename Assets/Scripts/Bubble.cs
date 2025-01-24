using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DG.Tweening;
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
        Cyan
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
            { BubbleColor.Cyan, new Color(0,0.7f,1f) },
        };

        public static Vector3 GlobalCenter = new Vector3(0, 0, 0);

        public static int MaxHp = 30;
        public static int TotalBall = 65;
    }


    public class Bubble : MonoBehaviour
    {
        public BubbleColor color;
        public int size = 1;
        public SoftBodyBluePrintPack BpPack;
        private float _mergeTime;
        public bool isPlayer;
        public int canMerge => isPlayer ? 0 : (_mergeTime > 0 && _mergeTime < 1.98f ? 2 : (_mergeTime > 0 ? 1 : 0));

        private Material _material;
        public const string SHADER_PROPERTY_MAINCOL = "_BaseColor";
        public const string SHADER_PROPERTY_SCALE = "_Scale";
        public const string SHADER_PROPERTY_WRAPSCALE = "_WrapScale";
        public const string SHADER_PROPERTY_SIZE = "_Size";
        
        
        public void TouchByPlayer()
        {
            _mergeTime = 2;
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

        public void ResetSize(int s)
        {
            size = s;
            GetComponent<ObiSoftbody>().softbodyBlueprint = BpPack.BpList[size - 1];
            RefreshMaterial();
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
                _material.SetFloat(SHADER_PROPERTY_SIZE,Mathf.Sqrt(size));
                // TODO:待蛋蛋补充.
                
            }
        }


        /// <summary>
        /// 合并时的表现
        /// </summary>
        public void RefreshOnMerge(float startWrapScale, float finalScale, float needTime)
        {
            // _material.SetFloat(SHADER_PROPERTY_SCALE, targetScale);
            // _material.SetFloat(SHADER_PROPERTY_WRAPSCALE, targetsScale);
            // 扭曲
            DOTween.To(() => startWrapScale, x => _material.SetFloat(SHADER_PROPERTY_WRAPSCALE, x), 0.01f, needTime);
            // 慢慢变小
            var currentScale = _material.GetFloat(SHADER_PROPERTY_SCALE);
            DOTween.To(() => currentScale, x => _material.SetFloat(SHADER_PROPERTY_SCALE, x), finalScale, needTime);
        }

        /// <summary>
        /// 新的大泡泡生成的放大过渡表现
        /// </summary>
        public void RefreshOnNewBigOne(float startScale, float needTime)
        {
            // var currentScale = _material.GetFloat(SHADER_PROPERTY_SCALE);
            // DOTween.To(() => currentScale, x => _material.SetFloat(SHADER_PROPERTY_SCALE, x), targetScale, needTime);
            // DOTween.To(() => 0, x => _material.SetFloat(SHADER_PROPERTY_SCALE, x), targetScale, needTime);
            DOTween.To(() => startScale, x => _material.SetFloat(SHADER_PROPERTY_SCALE, x), 0, needTime);
        }

        #endregion
    }
}