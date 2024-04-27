using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EAUploader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EAUInitializeOnLoadAttribute : Attribute
    {
        public EAUInitializeOnLoadAttribute()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var methodInfo = (MethodInfo)stackTrace.GetFrame(1).GetMethod();
            EAUInitializeOnLoadHelper.AddMethod(this, methodInfo);
        }
    }

    public static class EAUInitializeOnLoadHelper
    {
        private static List<MethodInfo> methods = new List<MethodInfo>();

        public static void AddMethod(EAUInitializeOnLoadAttribute attribute, MethodInfo methodInfo)
        {
            if (methodInfo != null)
            {
                methods.Add(methodInfo);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Execute()
        {
            foreach (var method in methods)
            {
                method.Invoke(null, null);
            }
        }
    }
}