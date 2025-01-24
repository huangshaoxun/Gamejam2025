using System;
using UnityEngine;

public class EyeAnimation : MonoBehaviour
{
    private Renderer eyeRenderer;
    private MaterialPropertyBlock materialPropertyBlock;
    public Camera mainCamera;
    private Vector3 eyePos;
    public float MaxDistance =10.0f ;

    private void Start()
    {
        eyePos = this.transform.position;
        eyeRenderer = this.GetComponent<Renderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    void Update()
    {
        if (eyeRenderer == null)
        {
            Debug.LogError("No Renderer");
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("未找到主摄像机！");
                return;
            }
        }
        eyeRenderer.GetPropertyBlock(materialPropertyBlock);
        

        // 获取鼠标在屏幕上的位置
        Vector3 mouseScreenPos = Input.mousePosition;
        

        // 将屏幕坐标转换为世界坐标
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, mainCamera.nearClipPlane));
        
        //相对位置
        Vector3 relativeMousePos = mouseWorldPos - eyePos;

        // 归一化到 -1 到 1 的范围
        Vector2 normalizedMousePos = new Vector2(
            Mathf.Clamp(relativeMousePos.x / MaxDistance, -1f, 1f), // 假设最大距离为10
            Mathf.Clamp(relativeMousePos.y / MaxDistance, -1f, 1f)
        );

        // 设置材质参数
        materialPropertyBlock.SetFloat("_PositionRangeX", normalizedMousePos.x);
        materialPropertyBlock.SetFloat("_PositionRangeY", normalizedMousePos.y);
        
        eyeRenderer.SetPropertyBlock(materialPropertyBlock);
    }
}