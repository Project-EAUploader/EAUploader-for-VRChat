#if !EA_ONBUILD
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
    private string webhookUrl = "https://discord.com/api/webhooks/1197382760873074728/CNcOYFDeVcIQbLm2pHoSlxIsZMJzxPKhUfGbG2ObKwD9XMXNzvsXHc4A21NKIz-Tz37D";
    private static string feedbacktitle = "";
    private static string authorName = "";
    private static string emailAddress = "";
    private static string messageContent = "";
    private static string lng = LanguageUtility.GetCurrentLanguage();

    public static void OpenDiscordWebhookSenderWindow()
    {
        GetWindow<DiscordWebhookSender>("Feedback").minSize = new Vector2(400, 200);
        feedbacktitle = "";
        messageContent = "";
    }

    private void OnGUI()
    {
        bool isEmptyMessage = true;
        bool sentFeedback = false;
        var backgroundColorStyle = new GUIStyle();
        backgroundColorStyle.normal.background = EditorGUIUtility.whiteTexture;

        GUI.Box(new Rect(0, 0, position.width, position.height), GUIContent.none, backgroundColorStyle);

        GUILayout.Label(Get(700), h4CenterLabelStyle);

        GUILayout.Label(Get(701), NoMarginh5BlackLabelStyle);
        authorName = GUILayout.TextField(authorName, TextFieldStyle);

        GUILayout.Label(Get(702), NoMarginh5BlackLabelStyle);
        emailAddress = GUILayout.TextField(emailAddress, TextFieldStyle);

        GUILayout.Label(Get(703), NoMarginh5BlackLabelStyle);
        feedbacktitle = GUILayout.TextField(feedbacktitle, TextFieldStyle);

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
                SendMessageToDiscord(webhookUrl, feedbacktitle, authorName, emailAddress, messageContent);
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

            content = content.Replace("\n", "\r");

            string json = BuildJson(title, author, email, content);

            // Debug.Log("Sending JSON: " + json);

            try
            {
                client.UploadString(url, json);
            }
            catch (WebException e)
            {
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
        // JSON用に特殊文字をエスケープ
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
        // JSONで使用される特殊文字をエスケープ
        return input.Replace("\\", "\\\\")
                    .Replace("\"", "\\\"")
                    .Replace("\n", "\\n")
                    .Replace("\r", "\\r")
                    .Replace("\t", "\\t")
                    .Replace("\b", "\\b")
                    .Replace("\f", "\\f");
    }
}
#endif