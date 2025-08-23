using System.Collections.Generic;
using UnityEngine;

namespace CatlikeCodings.ObjectManagement.Behaviors
{
    /// Copyright (C) 2025-present Zhu Xiaohe(aka ZeromaXHe)
    /// Author: Zhu XH (ZeromaXHe)
    /// Date: 2025-08-23 16:18:27
    public static class ShapeBehaviorPool<T> where T : ShapeBehavior, new()
    {
        private static readonly Stack<T> Stack = new();

        public static T Get()
        {
            if (Stack.Count > 0)
            {
                var behavior = Stack.Pop();
#if UNITY_EDITOR
                behavior.IsReclaimed = false;
#endif
                return behavior;
            }

#if UNITY_EDITOR
            return ScriptableObject.CreateInstance<T>();
#else
		    return new T();
#endif
        }

        public static void Reclaim(T behavior)
        {
#if UNITY_EDITOR
            behavior.IsReclaimed = true;
#endif
            Stack.Push(behavior);
        }
    }
}