using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.IO;
using static styles;
using static labels;

public class DiscordWebhookSender : EditorWindow
{
    private string webhookUrl = "https://discord.com/api/webhooks/1188319133574635642/TFZxaCa3jea50kGNjv1H5WrMgUpRaNdekJ30eGfzo01ZAMp-H1B7cxLQ_92ZZp3bEas-";
    private static string title = "";
    private static string authorName = "";
    private static string emailAddress = "";
    private static string messageContent = "";
    private static string lng = LanguageUtility.GetCurrentLanguage();

    // ウィンドウを開くためのメソッド
    public static void OpenDiscordWebhookSenderWindow()
    {
        GetWindow<DiscordWebhookSender>("Feedback").minSize = new Vector2(400, 200);
        title = "";
        messageContent = "";
    }

    private void OnGUI()
    {
        bool isEmptyMessage = true;
        bool sentFeedback = false;
        // 背景を白に設定するスタイルを作成
        var backgroundColorStyle = new GUIStyle();
        backgroundColorStyle.normal.background = EditorGUIUtility.whiteTexture;

        // 背景の描画
        GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, backgroundColorStyle);

        GUILayout.Label(Get(700), h4CenterLabelStyle);

        GUILayout.Label(Get(701), NoMarginh5BlackLabelStyle);
        authorName = GUILayout.TextField(authorName, TextFieldStyle);

        GUILayout.Label(Get(702), NoMarginh5BlackLabelStyle);
        emailAddress = GUILayout.TextField(emailAddress, TextFieldStyle);

        GUILayout.Label(Get(703), NoMarginh5BlackLabelStyle);
        title = GUILayout.TextField(title, TextFieldStyle);

        GUILayout.Label(Get(704), NoMarginh5BlackLabelStyle);
        messageContent = GUILayout.TextArea(messageContent, TextAreaStyle, GUILayout.Height(200));

        GUILayout.Space(10);

        GUILayout.Label(Get(705), h5BlackLabelStyle);

        if (messageContent == "")
        {
            GUILayout.Label(Get(708), eLabel);
        }
        else
        {
            isEmptyMessage = false;
        }
        if (GUILayout.Button(Get(706), SubButtonStyle))
        {
            if (!isEmptyMessage)
            {
                sentFeedback = true;
                SendMessageToDiscord(webhookUrl, title, authorName, emailAddress, messageContent);
            }
        }
        if (sentFeedback)
        {
            GUILayout.Label(Get(707), h5BlackLabelStyle);
        }
    }

    private void SendMessageToDiscord(string url, string title, string author, string email, string content)
    {
        using (WebClient client = new WebClient())
        {
            client.Headers[HttpRequestHeader.ContentType] = "application/json";

            // テキストエリアの改行をすべて \r に置き換える
            content = content.Replace("\n", "\r");

            string json = BuildJson(title, author, email, content);

            // JSONデータをログに出力
            Debug.Log("Sending JSON: " + json);

            try
            {
                client.UploadString(url, json);
            }
            catch (WebException e)
            {
                // エラー内容をログに出力
                Debug.LogError("Error sending webhook: " + e.Message);
                if (e.Response != null)
                {
                    using (var stream = e.Response.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        Debug.LogError("Response: " + reader.ReadToEnd());
                    }
                }
            }
        }
    }

    private string BuildJson(string title, string author, string email, string content)
    {
        // JSON用に特殊文字をエスケープする
        title = EscapeStringForJson(title);
        author = EscapeStringForJson(author);
        email = EscapeStringForJson(email);
        content = EscapeStringForJson(content);

        string emailField = string.IsNullOrEmpty(email) ? "\"Not provided\"" : $"\"{email}\"";

        return $"{{" +
            $"  \"content\": null," +
            $"  \"embeds\": [{{" +
            $"    \"title\": \"{title}\"," +
            $"    \"description\": \"{content}\"," +
            $"    \"color\": 36231," +
            $"    \"fields\": [" +
            $"      {{" +
            $"        \"name\": \"メールアドレス\"," +
            $"        \"value\": {emailField}" +
            $"      }}," +
            $"      {{" +
            $"        \"name\": \"言語設定\"," +
            $"        \"value\": \"{lng}\"" +
            $"      }}" +
            $"    ]," +
            $"    \"author\": {{" +
            $"      \"name\": \"{author}\"" +
            $"    }}" +
            $"  }}]" +
            $"}}";
    }

    private string EscapeStringForJson(string input)
    {
        // JSONで使用される特殊文字をエスケープする
        return input.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t")
                    .Replace("\b", "\\b")
                    .Replace("\f", "\\f");
    }
}
