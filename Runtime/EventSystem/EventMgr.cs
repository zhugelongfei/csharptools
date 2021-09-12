using System;
using System.Collections.Generic;

namespace Lonfee.EventSystem
{
    public static class EventMgr
    {
        private const int EVENT_CAPACITY_DEFAULT_COUNT = 2;

        private static Dictionary<Type, List<Delegate>> eventMap = new Dictionary<Type, List<Delegate>>();
        private static Stack<List<Delegate>> eventCacheStack = new Stack<List<Delegate>>();

        public static void RegisterEvent<T>(Action<T> callback)
        {
            if (callback == null)
                return;

            Type type = typeof(T);

            // 1: init list if need
            if (!eventMap.ContainsKey(type))
                eventMap.Add(type, new List<Delegate>(EVENT_CAPACITY_DEFAULT_COUNT));

            // 2: add to list
            List<Delegate> callbackList = eventMap[type];
            if (!callbackList.Contains(callback))
                callbackList.Add(callback);
        }

        public static void Dispatch<T>(T data)
        {
            if (data == null)
                throw new Exception("Lonfee.EventSystem Error: dispatch data can not be null.");

            Type type = typeof(T);
            if (!eventMap.ContainsKey(type))
                return;

            if(eventCacheStack.Count == 0)
                eventCacheStack.Push(new List<Delegate>(4));

            // call delegate
            List<Delegate> cacheList = eventCacheStack.Pop();
            cacheList.AddRange(eventMap[type]);
            cacheList.ForEach(callback => { if (callback != null) { callback.DynamicInvoke(data); } });
            cacheList.Clear();
            eventCacheStack.Push(cacheList);
        }

        public static void Clear()
        {
            eventMap.Clear();
        }

        public static void RemoveEvent<T>(Action<T> callback)
        {
            if (callback == null)
                return;

            Type type = typeof(T);
            if (!eventMap.ContainsKey(type))
                return;

            List<Delegate> callbackList = eventMap[type];
            if (callbackList.Contains(callback))
            {
                callbackList.Remove(callback);

                if (callbackList.Count == 0)
                    eventMap.Remove(type);
            }
        }
    }
}