using UnityEngine.UIElements;
using UnityEditor;
using System;
using EAUploader.CustomPrefabUtility;
using EAUploader.UI.Components;

namespace EAUploader.Editor.Resource.API
{
    /// <summary>
    /// EAUploaderのスタイル、独自コンポーネントの機能を提供するAPIクラス群
    /// </summary>
    public static class EAUResourceAPI
    {
        /// <summary>
        /// EAUploaderのスタイルシート
        /// </summary>
        public static StyleSheet EAUStyles => EAUploader.UI.EAUploader.styles;

        /// <summary>
        /// Tailwindのスタイルシート
        /// <summary>
        public static StyleSheet EAUTailwind => EAUploader.UI.EAUploader.tailwind;
        
    }
        /// <summary>
        /// ヘルプボタンの機能を提供するAPIクラス
        /// </summary>
    public static class HelpButtonAPI
    {
        /// <summary>
        /// 指定したメッセージ番号のメッセージを表示するEAUploaderMessageウィンドウを表示します。
        /// </summary>
        /// <param name="msgNum">表示するメッセージの番号</param>
        public static void ShowMessage(int msgNum)
        {
            EAUploaderMessageWindow.ShowMsg(msgNum);
        }
    }

        /// <summary>
        /// リンクの機能を提供するAPIクラス
        /// </summary>
    public static class LinkAPI
    {
        /// <summary>
        /// 指定したテキストとURLを持つリンクを作成します。
        /// </summary>
        /// <param name="text">リンクのテキスト</param>
        /// <param name="url">リンクのURL</param>
        /// <returns>作成されたリンクのVisualElement</returns>
        public static VisualElement CreateLink(string text, string url)
        {
            var link = new Link(text);
            link.href = url;
            return link;
        }
    }

    /// <summary>
    /// マテリアルアイコンの機能を提供するAPIクラス
    /// </summary>
    public static class MaterialIconAPI
    {
        /// <summary>
        /// 指定したアイコン名を持つマテリアルアイコンを作成します。
        /// Material icon: https://fonts.google.com/icons?icon.set=Material+Icons
        /// </summary>
        /// <param name="iconName">アイコンの名前</param>
        /// <returns>作成されたマテリアルアイコンのVisualElement</returns>
        public static VisualElement CreateMaterialIcon(string iconName)
        {
            var icon = new MaterialIcon(iconName);
            return icon;
        }
    }

    /// <summary>
    /// プレハブアイテムボタンの機能を提供するAPIクラス
    /// </summary>
    public static class PrefabItemButtonAPI
    {
        /// <summary>
        /// 指定したプレハブ情報とクリックアクションを持つプレハブアイテムボタンを作成します。
        /// </summary>
        /// <param name="prefab">プレハブの情報</param>
        /// <param name="clickAction">クリック時のアクション</param>
        /// <param name="disabled">ボタンを無効にするかどうか</param>
        /// <returns>作成されたプレハブアイテムボタンのVisualElement</returns>
        public static VisualElement CreatePrefabItemButton(PrefabInfo prefab, Action clickAction, bool disabled = false)
        {
            var button = new PrefabItemButton(prefab, clickAction, disabled);
            return button;
        }
    }

    /// <summary>
    /// シャドウボタンの機能を提供するAPIクラス
    /// </summary>
    public static class ShadowButtonAPI
    {
        /// <summary>
        /// 指定したテキストとクリックアクションを持つシャドウボタンを作成します。
        /// </summary>
        /// <param name="text">ボタンのテキスト</param>
        /// <param name="clickAction">クリック時のアクション</param>
        /// <returns>作成されたシャドウボタンのVisualElement</returns>
        public static VisualElement CreateShadowButton(string text, Action clickAction)
        {
            var button = new ShadowButton();
            button.text = text;
            button.clicked += clickAction;
            return button;
        }
    }

    /// <summary>
    /// プレビューの機能を提供するAPIクラス
    /// ※このプレビューはTexture2Dのプレビューではなく、操作可能な描画されるプレビュー
    /// </summary>
    public static class PreviewAPI
    {
        /// <summary>
        /// 指定したルート要素とプレハブパスを持つプレビューを作成します。
        /// </summary>
        /// <param name="rootElement">プレビューのルート要素</param>
        /// <param name="prefabPath">プレビューするプレハブのパス</param>
        /// <returns>作成されたプレビュー</returns>
        public static Preview CreatePreview(VisualElement rootElement, string prefabPath)
        {
            var preview = new Preview(rootElement, prefabPath);
            return preview;
        }

        /// <summary>
        /// プレビューのコンテンツを表示します。
        /// </summary>
        /// <param name="preview">プレビュー</param>
        public static void ShowContent(Preview preview)
        {
            preview.ShowContent();
        }

        /// <summary>
        /// 指定したプレハブパスでプレビューを更新します。
        /// </summary>
        /// <param name="preview">プレビュー</param>
        /// <param name="prefabPath">プレビューするプレハブのパス</param>
        public static void UpdatePreview(Preview preview, string prefabPath)
        {
            preview.UpdatePreview(prefabPath);
        }

        /// <summary>
        /// プレビューをリセットします。
        /// </summary>
        /// <param name="preview">プレビュー</param>
        public static void ResetPreview(Preview preview)
        {
            preview.ResetPreview();
        }
    }

}