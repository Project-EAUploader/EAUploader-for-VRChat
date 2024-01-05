using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using static styles;
using static Texture;

public class ArticleRenderer
{
    public static string currentArticleFilePath;

    /// <summary>
    /// 記事をレンダリングするAPI
    /// </summary>
    /// <param name="area"></param>
    /// <param name="content"></param>
    public static void RenderRichTextContent(Rect area, string content)
    {
        var elements = ParseRichTextElements(content);

        foreach (var element in elements)
        {
            switch (element.Type)
            {
                case RichTextElementType.Text:
                    GUILayout.Label(element.Content, RichTextLabelStyle);
                    break;
                case RichTextElementType.Image:
                    RenderImage(area, element.Content);
                    GUILayout.Space(10);
                    break;
                case RichTextElementType.Button:
                    RenderButton(element.Content);
                    break;
                case RichTextElementType.HorizontalLine:
                    RenderHorizontalLine(area, element.Content);
                    break;
                case RichTextElementType.HeaderText:
                    GUILayout.Label(element.Content, h1LabelStyle);
                    break;
                case RichTextElementType.Section:
                    RenderSection(area, element.Content);
                    GUILayout.Space(10);
                    break;
            }
        }
    }

    private static List<RichTextElement> ParseRichTextElements(string content)
    {
        var elements = new List<RichTextElement>();
        int currentIndex = 0;

        if (string.IsNullOrEmpty(content))
        {
            return elements;
        }

        while (currentIndex < content.Length)
        {
            int tagStartIndex = content.IndexOf('<', currentIndex);
            if (tagStartIndex != -1)
            {
                if (tagStartIndex > currentIndex)
                {
                    string textBeforeTag = content.Substring(currentIndex, tagStartIndex - currentIndex).Trim('\n');
                    if (!string.IsNullOrEmpty(textBeforeTag))
                    {
                        elements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = textBeforeTag });
                    }
                }

                int tagEndIndex = content.IndexOf('>', tagStartIndex);
                if (tagEndIndex != -1)
                {
                    string tag = content.Substring(tagStartIndex, tagEndIndex - tagStartIndex + 1);
                    currentIndex = tagEndIndex + 1;

                    if (tag.StartsWith("<txt>"))
                    {
                        int txtEndIndex = content.IndexOf("</txt>", currentIndex);
                        if (txtEndIndex != -1)
                        {
                            string richTextContent = content.Substring(currentIndex, txtEndIndex - currentIndex);
                            elements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = richTextContent });
                            currentIndex = txtEndIndex + "</txt>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Unclosed <txt> tag");
                            break;
                        }
                    }
                    else if (tag.StartsWith("<section>"))
                    {
                        int sectionEndIndex = content.IndexOf("</section>", currentIndex);
                        if (sectionEndIndex != -1)
                        {
                            string sectionData = content.Substring(currentIndex, sectionEndIndex - currentIndex).Trim('\n');
                            elements.Add(new RichTextElement { Type = RichTextElementType.Section, Content = sectionData });
                            currentIndex = sectionEndIndex + "</section>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Section tag not closed");
                            break;
                        }
                    }
                    else if (tag.StartsWith("<image>"))
                    {
                        int imageEndIndex = content.IndexOf("</image>", currentIndex);
                        if (imageEndIndex != -1)
                        {
                            string imageData = content.Substring(currentIndex, imageEndIndex - currentIndex).Trim('\n');
                            elements.Add(new RichTextElement { Type = RichTextElementType.Image, Content = imageData });
                            currentIndex = imageEndIndex + "</image>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Image tag not closed");
                            break;
                        }
                    }
                    else if (tag.StartsWith("<button>"))
                    {
                        // [Button タグの処理]
                        int buttonEndIndex = content.IndexOf("</button>", currentIndex);
                        if (buttonEndIndex != -1)
                        {
                            string buttonData = content.Substring(currentIndex, buttonEndIndex - currentIndex).Trim('\n');
                            elements.Add(new RichTextElement { Type = RichTextElementType.Button, Content = buttonData });
                            currentIndex = buttonEndIndex + "</button>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Button tag not closed");
                            break;
                        }
                    }
                    else if (tag.StartsWith("<hr>"))
                    {
                        int hrEndIndex = content.IndexOf("</hr>", currentIndex);
                        if (hrEndIndex != -1)
                        {
                            string hrData = content.Substring(currentIndex, hrEndIndex - currentIndex).Trim('\n');
                            elements.Add(new RichTextElement { Type = RichTextElementType.HorizontalLine, Content = hrData });
                            currentIndex = hrEndIndex + "</hr>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Horizontal line tag not closed");
                            break;
                        }
                    }
                    else if(tag.StartsWith("<t>"))
                    {
                        int textEndIndex = content.IndexOf("</t>", currentIndex);
                        if (textEndIndex != -1)
                        {
                            string textContent = content.Substring(currentIndex, textEndIndex - currentIndex).Trim('\n');
                            elements.Add(new RichTextElement { Type = RichTextElementType.HeaderText, Content = textContent });
                            currentIndex = textEndIndex + "</t>".Length;
                            currentIndex = SkipNewLines(content, currentIndex);
                        }
                        else
                        {
                            Debug.LogError("Header text tag not closed");
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Invalid tag in rich text: " + content.Substring(tagStartIndex));
                    break;
                }
            }
            else
            {
                // 残りの全てのテキストを追加
                string remainingText = content.Substring(currentIndex).Trim('\n');
                if (!string.IsNullOrEmpty(remainingText))
                {
                    elements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = remainingText });
                }
                break;
            }
        }

        return elements;
    }

    private static int SkipNewLines(string content, int currentIndex)
    {
        while (currentIndex < content.Length && (content[currentIndex] == '\n' || content[currentIndex] == '\r'))
        {
            currentIndex++;
        }
        return currentIndex;
    }

    static GUIStyle RichTextLabelStyle = new GUIStyle(GUI.skin.label)
    {
        font = styles.custumfont,
        fontSize = 14,
        normal = { textColor = Color.black },
        hover = { textColor = Color.black },
        padding = new RectOffset(10, 10, 10, 10),
        wordWrap = true,
        richText = true
    };

    private static void RenderSection(Rect area, string sectionData)
    {
        string[] parts = sectionData.Split(',');
        if (parts.Length >= 3)
        {
            string imagePath = parts[0].Trim();
            int widthPercent = int.Parse(parts[1].Trim());
            string textContent = parts[2].Trim();

            GUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(imagePath))
            {
                RenderImageForSection(area, imagePath, widthPercent);
            }

            GUILayout.Label(textContent, RichTextLabelStyle);

            GUILayout.EndHorizontal();
        }
    }

    private static void RenderImage(Rect area, string imageData)
    {
        string[] parts = imageData.Split(',');
        string imagePath = parts[0].Trim();
        int widthOverride = -1;

        if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out widthOverride))
        {
            // 横幅の値が取得された場合
        }

        if (string.IsNullOrEmpty(currentArticleFilePath) || string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("パスが null または空です。");
            return;
        }

        string fullPath = Path.Combine(Path.GetDirectoryName(currentArticleFilePath), imagePath);
        Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

        if (image != null)
        {
            float aspectRatio = (float)image.height / image.width;

            float imageWidth = widthOverride > 0 ? widthOverride : area.width - 10;
            float imageHeight = imageWidth * aspectRatio;

            GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
            Rect lastRect = GUILayoutUtility.GetLastRect();
            GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), image);
        }
        else
        {
            GUILayout.Label("Image not found: " + imagePath);
        }
    }

    private static void RenderImageForSection(Rect area, string imagePath, int widthPercent)
    {
        if (string.IsNullOrEmpty(currentArticleFilePath) || string.IsNullOrEmpty(imagePath))
        {
            Debug.LogError("パスが null または空です。");
            return;
        }

        string fullPath = Path.Combine(Path.GetDirectoryName(currentArticleFilePath), imagePath);
        Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

        if (image != null)
        {
            Sprite imageSprite = Texture2DToSprite(image);

            if (imageSprite != null)
            {
                float aspectRatio = (float)image.height / image.width;
                float imageWidth = area.width * widthPercent / 100;
                float imageHeight = imageWidth * aspectRatio;

                GUILayout.Label("", GUILayout.Width(imageWidth), GUILayout.Height(imageHeight));
                Rect lastRect = GUILayoutUtility.GetLastRect();
                GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, imageWidth, imageHeight), imageSprite.texture);
            }
        }
        else
        {
            GUILayout.Label("Image not found: " + imagePath);
        }
    }

    private static Sprite Texture2DToSprite(Texture2D texture)
    {
        if (texture == null) return null;
        return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private static void RenderButton(string buttonData)
    {
        string[] parts = buttonData.Split(',');
        if (parts.Length >= 3)
        {
            string label = parts[0].Trim();
            string styleName = parts[1].Trim();
            string url = parts[2].Trim();

            GUIStyle buttonStyle;
            switch (styleName)
            {
                case "Main":
                    buttonStyle = MainButtonStyle;
                    break;
                case "Sub":
                    buttonStyle = SubButtonStyle;
                    break;
                case "Link":
                    buttonStyle = LinkButtonStyle;
                    break;
                case "Trans":
                    buttonStyle = LibraryButtonStyle;
                    break;
                default:
                    buttonStyle = GUI.skin.button;
                    break;
            }

            if (GUILayout.Button(label, buttonStyle))
            {
                Application.OpenURL(url);
            }
        }
    }

    private static void RenderHorizontalLine(Rect area, string hrData)
    {
        string[] parts = hrData.Split(',');
        if (parts.Length >= 2)
        {
            string colorPart = parts[0].Trim();
            Color lineColor;
            if (!ColorUtility.TryParseHtmlString(colorPart, out lineColor))
            {
                lineColor = Color.black;
            }
            if (int.TryParse(parts[1].Trim(), out int lineThickness))
            {
                DrawHorizontalDottedCenterLine(lineColor, lineThickness, area.width);
            }
            else
            {
                Debug.LogError("Invalid line thickness in HR tag: " + parts[1].Trim());
            }
        }
        else
        {
            Debug.LogError("Invalid HR tag format: " + hrData);
        }
    }

    private class RichTextElement
    {
        public RichTextElementType Type { get; set; }
        public string Content { get; set; }
    }

    private enum RichTextElementType
    {
        Text,
        Image,
        Button,
        HorizontalLine,
        HeaderText,
        Section
    }
}