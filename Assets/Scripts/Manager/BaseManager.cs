using System;
using UnityEngine;

/// <summary>
/// 单例模式基类
/// </summary>
public class BaseManager<T> where T : class
{
    private static T instance;

    // 构造函数设为 protected：禁止外部用 new 创建实例，只能通过 Instance 属性获取
    protected BaseManager() { }

    public static T Instance
    {
        get
        {
            // 通过反射调用 T 的无参构造函数来创建实例（nonPublic: true 允许调用非 public 构造函数）
            if (instance == null)
                instance = Activator.CreateInstance(typeof(T), nonPublic: true) as T;
            return instance;
        }
    }
}

