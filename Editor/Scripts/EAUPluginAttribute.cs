using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace EAUploader
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EAUPluginAttribute : Attribute
    {
        public EAUPluginAttribute()
        {
            var stackTrace = new System.Diagnostics.StackTrace();
            var methodInfo = (MethodInfo)stackTrace.GetFrame(1).GetMethod();
            EAUPluginHelper.AddMethod(this, methodInfo);
        }
    }

    public static class EAUPluginHelper
    {
        private static List<MethodInfo> methods = new List<MethodInfo>();

        public static void AddMethod(EAUPluginAttribute attribute, MethodInfo methodInfo)
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