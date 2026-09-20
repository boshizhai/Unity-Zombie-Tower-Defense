using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance;

    public static T Instance
    {
        get
        {
            // 如果为空，首先尝试去指定资源路径下加载对应的数据资源文件
            if (instance == null)
            {
                // 约定规则：
                // 1. 所有需要复用的数据资源文件都放在 Resources 文件夹下的 ScriptableObject 目录中
                // 2. 数据资源文件的文件名，与类名保持一致
                instance = Resources.Load<T>("ScriptableObject/" + typeof(T).Name);
            }

            // 如果没有找到对应的资源文件，为了安全起见，直接在内存中创建一个临时数据对象兜底
            if (instance == null)
            {
                instance = CreateInstance<T>();
            }

            // 甚至可以在这里扩展：从 Json 中读取数据实现持久化
            // 但不建议用 ScriptableObject 来做数据持久化，除非有特殊需求

            return instance;
        }
    }
}