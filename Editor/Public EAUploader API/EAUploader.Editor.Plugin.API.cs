using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using VRC.Core;
using System.Threading.Tasks;
using UnityEditor;

namespace EAUploader.Editor.Plugin.API
{
    /// <summary>
    /// プラグイン関連の公開APIを提供するクラス
    /// </summary>
    public static class PluginAPI
    {
        /// <summary>
        /// 新しいエディタが登録されたときに発生するイベント
        /// </summary>
        public static event EAUploaderEditorManager.EditorRegisteredHandler OnEditorRegistered
        {
            add => EAUploaderEditorManager.OnEditorRegistered += value;
            remove => EAUploaderEditorManager.OnEditorRegistered -= value;
        }
        /// <summary>
        /// エディタマネージャーの読み込み時に呼び出されます。
        /// </summary>
        public static void OnEditorManagerLoad()
        {
            EAUploaderEditorManager.OnEditorManagerLoad();
        }

        /// <summary>
        /// 新しいエディタを登録します。
        /// </summary>
        /// <param name="editorRegistration">登録するエディタの情報</param>
        public static void RegisterEditor(EditorRegistration editorRegistration)
        {
            EAUploaderEditorManager.RegisterEditor(new EAUploader.EditorRegistration
            {
                MenuName = editorRegistration.MenuName,
                EditorName = editorRegistration.EditorName,
                Description = editorRegistration.Description,
                Version = editorRegistration.Version,
                Author = editorRegistration.Author,
                Url = editorRegistration.Url,
                Requirement = editorRegistration.Requirement,
                RequirementDescription = editorRegistration.RequirementDescription
            });
        }

        /// <summary>
        /// 登録されているエディタのリストを取得します。
        /// </summary>
        /// <returns>登録されているエディタのリスト</returns>
        public static IEnumerable<EditorRegistration> GetRegisteredEditors()
        {
            return ConvertEditorRegistrations(EAUploaderEditorManager.GetRegisteredEditors());
        }

        private static IEnumerable<EditorRegistration> ConvertEditorRegistrations(IEnumerable<EAUploader.EditorRegistration> registrations)
        {
            foreach (var registration in registrations)
            {
                yield return new EditorRegistration
                {
                    MenuName = registration.MenuName,
                    EditorName = registration.EditorName,
                    Description = registration.Description,
                    Version = registration.Version,
                    Author = registration.Author,
                    Url = registration.Url,
                    Requirement = registration.Requirement,
                    RequirementDescription = registration.RequirementDescription
                };
            }
        }

        /// <summary>
        /// エディタの登録情報を表すクラス
        /// </summary>
        public class EditorRegistration
        {
            public string MenuName { get; set; }
            public string EditorName { get; set; }
            public string Description { get; set; }
            public string Version { get; set; }
            public string Author { get; set; }
            public string Url { get; set; }
            public Func<string, bool> Requirement { get; set; } = null;
            public string RequirementDescription { get; set; } = null;
        }

        /// <summary>
        /// プラグインの初期化メソッドを指定するための属性
        /// </summary>
        [AttributeUsage(AttributeTargets.Method)]
        public class EAUPluginAttribute : Attribute
        {
            public EAUPluginAttribute()
            {
                var stackTrace = new System.Diagnostics.StackTrace();
                var methodInfo = (System.Reflection.MethodInfo)stackTrace.GetFrame(1).GetMethod();
                EAUPluginHelper.AddMethod(this, methodInfo);
            }
        }

        private static class EAUPluginHelper
        {
            private static List<System.Reflection.MethodInfo> methods = new List<System.Reflection.MethodInfo>();

            internal static void AddMethod(EAUPluginAttribute attribute, System.Reflection.MethodInfo methodInfo)
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
}