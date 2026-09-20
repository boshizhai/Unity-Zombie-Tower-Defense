using LitJson;
using System.IO;
using UnityEngine;

/// <summary>
/// 存储Json的方案
/// </summary>
public enum E_JsonType
{
    JsonUtility,
    LiteJson
}

/// <summary>
/// Json数据管理类，负责Json序列化存储和反序列化取出
/// </summary>
public class JsonMgr
{

    //单例模式
    private static JsonMgr instance=new JsonMgr();

    private JsonMgr() { }

    public static JsonMgr Instance => instance;


    //存储Json数据 序列化方法
    public void SaveData(object data,string fileName,E_JsonType type=E_JsonType.LiteJson)
    {
        //第一步：确定存储路径
        string path = Application.persistentDataPath + "/" + fileName + ".json";

        //序列化 得到Json字符串
        string jsonStr = "";
        switch (type)
        {
            case E_JsonType.JsonUtility:
                jsonStr = JsonUtility.ToJson(data);
                break;
            case E_JsonType.LiteJson:
                jsonStr = JsonMapper.ToJson(data);
                break;
        }
        //把序列化的Json字符串存储到指定路径的文件中
        File.WriteAllText(path, jsonStr);
    }


    //读取Json数据，反序列化方法
    public T LoadData<T>(string fileName,E_JsonType type = E_JsonType.LiteJson) where T:new()
    {
        //确定从哪个路径读取
        //首先判断 默认数据文件夹中是否存储数据 如果有 就从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //如果streamingAssetsPath文件夹中不存在默认数据文件 就从 persistentDataPath读写文件夹中去寻找
        if (!File.Exists(path))
            path = Application.persistentDataPath + "/" + fileName + ".json";
        //如果persistentDataPath文件夹中都还没有
        if (!File.Exists(path))
            return new T();

        //进行反序列化
        string jsonStr = File.ReadAllText(path);

        T data = default(T);
        switch (type)
        {
            case E_JsonType.JsonUtility:
                data = JsonUtility.FromJson<T>(jsonStr);
                break;
            case E_JsonType.LiteJson:
                data = JsonMapper.ToObject<T>(jsonStr);
                break;
        }

        //把对象返回出去
        return data;
    }
}