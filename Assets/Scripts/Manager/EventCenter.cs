using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 用一个空接口包裹一个泛型类
/// 泛型类里面存储 UnityAction<T>
/// EventCenter的字典里面存储的是接口
/// 用里氏替换原则我们就能实现了一个泛型委托
/// 而不用在EventCenter管理器里面定制<T>泛型
/// </summary>
public interface IEventInfo { }
class EventInfo<T> : IEventInfo
{
    public UnityAction<T> actions;
    public EventInfo(UnityAction<T> action)
    {
        actions += action;
    }
}
//非泛型类，用来构建不需要参数的重载
class EventInfo : IEventInfo
{
    public UnityAction actions;
    public EventInfo(UnityAction action)
    {
        actions += action;
    }
}

/// <summary>
/// 事件中心：单例模式对象
/// 1.Dictionary
/// 2.委托、字典
/// 3.观察者模式
/// </summary>
public class EventCenter : BaseManager<EventCenter>
{
    protected EventCenter() { }

    /// <summary>
    /// key——事件的名字(比如：怪物死亡，玩家死亡，通关 等等）
    /// value——对应的是 监听这个事件 对应的委托函数们
    /// UnityAction<object>——传入object泛型的委托事件，方便传入事件参数
    /// </summary>
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();



    //判断要触发的事件名称是否合规
    private static void ValidateEventName(string name)
    {
        //IsNullOrWhiteSpace 可以将："  "也判断为空
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException("事件名不能为空。", nameof(name));
        }
    }


    // 统一处理查找 事件容器 + 判断类型
    // 根据事件名查找事件，并检查实际类型是否与期望类型一致。
    private bool TryGetEvent<T>(string name, out T eventInfo) where T : class, IEventInfo
    {
        //验证字符串名称
        ValidateEventName(name);

        //尝试从字典中取事件参数泛型
        if (!eventDic.TryGetValue(name, out IEventInfo storedInfo))
        {
            eventInfo = null;
            return false;
        }

        //里氏替换原则得到事件类型
        eventInfo = storedInfo as T;

        if (eventInfo == null)
        {
            throw new InvalidOperationException(
                $"事件“{name}”的类型不一致。" +
                $"已注册类型：{storedInfo.GetType()}；" +
                $"本次需要类型：{typeof(T)}。");
        }
        return true;
    }




    // 往事件监听容器中添加事件
    public void AddEventListener<T>(string name, UnityAction<T> action)
    {
        if (action == null)
            throw new ArgumentNullException($"事件类型{name},传入的监听函数为空：{nameof(action)}");

        //存在该对应事件的事件容器
        if (TryGetEvent<EventInfo<T>>(name, out EventInfo<T> eventInfo))
        {
            eventInfo.actions += action;
        }
        //没有该对应的事件的监听
        else
        {
            //新增一个类型的事件监听
            eventDic.Add(name, new EventInfo<T>(action));
        }
    }
    public void AddEventListener(string name, UnityAction action)
    {
        if (action == null)
            throw new ArgumentNullException($"事件类型{name},传入的监听函数为空：{nameof(action)}");

        //存在对应类型的事件容器
        if (TryGetEvent<EventInfo>(name, out EventInfo eventInfo))
        {
            eventInfo.actions += action;
        }
        //没有该对应的事件的监听
        else
        {
            //新增一个类型的事件监听
            eventDic.Add(name, new EventInfo(action));
        }
    }



    // 触发事件，直接执行具体的事件监听器
    public void EventTrigger<T>(string name, T info)
    {
        if (TryGetEvent<EventInfo<T>>(name, out EventInfo<T> eventInfo))
        {
            eventInfo.actions?.Invoke(info);
        }
    }
    public void EventTrigger(string name)
    {
        if (TryGetEvent<EventInfo>(name, out EventInfo eventInfo))
        {
            eventInfo.actions?.Invoke();
        }
    }



    // 从事件监听器容器中移除指定的事件函数
    public void RemoveEventListener<T>(string name, UnityAction<T> action)
    {
        if (action == null)
            throw new ArgumentNullException($"事件类型{name},传入的监听函数为空：{nameof(action)}");

        if (TryGetEvent<EventInfo<T>>(name, out EventInfo<T> eventInfo))
        {
            eventInfo.actions -= action;
            if (eventInfo.actions == null)
                eventDic.Remove(name);
        }
    }
    public void RemoveEventListener(string name, UnityAction action)
    {
        if (action == null)
            throw new ArgumentNullException($"事件类型{name},传入的监听函数为空：{nameof(action)}");

        if (TryGetEvent<EventInfo>(name, out EventInfo eventInfo))
        {
            eventInfo.actions -= action;
            if (eventInfo.actions == null)
                eventDic.Remove(name);
        }
    }

    /// <summary>
    /// 清除全部监听。
    /// 仅用于明确的整体重置；普通对象退出时应移除自己的监听。
    /// </summary>
    public void ClearEventListener()
    {
        eventDic.Clear();
    }



}