using System;
using System.Collections.Generic;
using System.Linq;

public class UniFind<T>
{
    private Dictionary<T, T> parent;
    private Dictionary<T, int> rank; // 用于按秩合并
    private int count;  // 连通分量的数量

    public UniFind()
    {
    }
    
    // 重置并查集状态
    public void Refresh(IEnumerable<T> elements)
    {
        parent = new Dictionary<T, T>();
        rank = new Dictionary<T, int>();
        parent.Clear();
        rank.Clear();
        count = 0;
        foreach (var element in elements)
        {
            parent[element] = element; // 初始时，每个节点的父节点是自己
            rank[element] = 0;         // 初始时，每个节点的秩为0
            count++;
        }
    }

    // 查找节点x的根节点，并进行路径压缩
    public T Find(T x)
    {
        if (!parent.ContainsKey(x))
        {
            throw new ArgumentException("Element not found in UnionFind.");
        }

        if (!object.Equals(parent[x],x))
        {
            parent[x] = Find(parent[x]); // 路径压缩
        }
        return parent[x];
    }

    // 合并两个节点所在的集合
    public bool Union(T x, T y)
    {
        T rootX = Find(x);
        T rootY = Find(y);

        if (rootX.Equals(rootY))
        {
            return false; // 已经在同一个集合中
        }

        // 按秩合并
        if (rank[rootX] < rank[rootY])
        {
            parent[rootX] = rootY;
        }
        else if (rank[rootX] > rank[rootY])
        {
            parent[rootY] = rootX;
        }
        else
        {
            parent[rootY] = rootX;
            rank[rootX]++;
        }

        count--; // 合并后，连通分量的数量减少
        return true;
    }

    // 判断两个节点是否在同一个集合中
    public bool Connected(T x, T y)
    {
        return Find(x).Equals(Find(y));
    }

    // 获取连通分量的数量
    public int Count
    {
        get { return count; }
    }

    // 获取所有节点及其对应的根节点
    public Dictionary<T, T> GetAllRoots()
    {
        var roots = new Dictionary<T, T>();
        foreach (var element in parent.Keys.ToList())
        {
            roots[element] = Find(element);
        }
        return roots;
    }

    // 获取所有连通分量
    public Dictionary<T, List<T>> GetAllComponents()
    {
        var roots = GetAllRoots();
        var components = new Dictionary<T, List<T>>();

        foreach (var kvp in roots)
        {
            T root = kvp.Value;
            T element = kvp.Key;

            if (!components.ContainsKey(root))
            {
                components[root] = new List<T>();
            }
            components[root].Add(element);
        }

        return components;
    }
}