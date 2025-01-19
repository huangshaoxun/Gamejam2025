using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;
using Obi;
using Sirenix.OdinInspector;
// using Random = System.Random;

public class PlaceData
{
    public Vector3 placePosition;
    public BubbleColor bubbleColor;
}

/// <summary>
/// 泡泡生成器
/// </summary>
public class BubbleSpawner : MonoBehaviour 
{
    public static BubbleSpawner Instance;
    public GameObject baseBubble; // todo: 对象池
    [LabelText("生成数量[min, max]")] public Vector2 numRange;
    [LabelText("生成点范围 - x")] public Vector2 xAxisRange;
    [LabelText("生成点范围 - y")] public Vector2 yAixsRange;
    [LabelText("生成大小范围[min, max]")] public Vector2 sizeRange;
    
    [LabelText("创建新object 的Shader - Scale 起始值")] public float newBigOneStartScale = -0.25f;
    [LabelText("创建新object 的过渡时长")] public float newBigOneTransitionTime = 0.5f;
    
    
    private List<PlaceData> placedPositions = new List<PlaceData>(); // 已经放置的物体位置
    public float minDistance = 2f;    // 物体之间的最小距离 【粗暴算法，主要为了避免生成位置重合】
    
    void Awake()
    {
        Instance = this;
        StartGenerate();
    }

    /// <summary>
    /// 随机生成、自动散布，颜色，粘性，大小随机
    /// </summary>
    [Button("开始随机生成")]
    public void StartGenerate()
    {
        int num = (int)Random.Range(numRange.x, numRange.y);
        Debug.Log("随机生成数量：" + num);
        for (int i = 0; i < num; i++)
        {
            Generate();
        }
    }

    private void Generate()
    {
        bool isPositionValid = false;
        PlaceData placeData = RandomData(out isPositionValid);
        if (!isPositionValid)
        {
            // Debug.LogError("生成位置不合法");
            return;
        }
        
        GameObject go = Instantiate(baseBubble);
        go.transform.SetParent(transform); // 位置随机
        go.transform.position = placeData.placePosition;
        
        var bubble  = go.GetComponent<Bubble>();
        if (bubble != null)
        {
            bubble.color = placeData.bubbleColor;
            bubble.size = (int)Random.Range(sizeRange.x, sizeRange.y);
            placedPositions.Add(placeData);
        }
        
        // 同色的避免生成位置重合
    }
    
    private PlaceData RandomData(out bool isValid)
    {
        isValid = false;
        int attempts = 0;
        PlaceData data = new PlaceData(); //  // 这里暂不考虑性能
        
        // 尝试生成一个有效的位置
        do
        {
            data.bubbleColor = Utils.GetRandomEnumValue<BubbleColor>();
            data.placePosition = new Vector3(Random.Range(xAxisRange.x, xAxisRange.y), Random.Range(yAixsRange.x, yAixsRange.y), 0);
            isValid = IsValid(data);

            attempts++;
            if (attempts > 1000)
            {
                Debug.LogWarning("尝试次数过多，可能达不到要求的布局条件！");
                break;
            }
        } while (!isValid);

        return data;
    }

    private bool IsValid(PlaceData targetData)
    {
        // 白色为主角色，标记为无效参数
        if (targetData.bubbleColor == BubbleColor.None)
            return false;
        
        foreach (PlaceData data in placedPositions)
        {
            if (Vector3.Distance(targetData.placePosition, data.placePosition) < minDistance 
                && targetData.bubbleColor.Equals(data.bubbleColor))
            {
                return false; // 位置无效，因为同色的物体距离太近
            }
        }
        return true; // 位置有效
    }
}
