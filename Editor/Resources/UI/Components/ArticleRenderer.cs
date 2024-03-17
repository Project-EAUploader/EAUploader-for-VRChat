using EAUploader.UI.Components;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EAUploader.UI.Components
{
    public class ArticleRenderer : VisualElement
    {
        private readonly string _contentPath;

        private readonly List<VisualElement> articleElements = new List<VisualElement>();

        private const string k_sectionUssClass = "article-section";
        private const string k_imageUssClass = "article-image";
        private const string k_headerUssClass = "article-header";

        public ArticleRenderer(string contentPath)
        {
            _contentPath = contentPath;
            styleSheets.Add(Resources.Load<StyleSheet>("UI/Components/ArticleRenderer"));

            RenderRichTextContent();
        }

        public void RenderRichTextContent()
        {
            var textElements = ParseRichTextElements();

            foreach (var textElement in textElements)
            {
                switch (textElement.Type)
                {
                    case RichTextElementType.Text:
                        RenderText(textElement);
                        break;
                    case RichTextElementType.Image:
                        RenderImage(textElement);
                        break;
                    case RichTextElementType.Button:
                        RenderButton(textElement);
                        break;
                    case RichTextElementType.HorizontalLine:
                        RenderHorizontalLine(textElement);
                        break;
                    case RichTextElementType.HeaderText:
                        RenderHeaderText(textElement);
                        break;
                    case RichTextElementType.Section:
                        RenderSection(textElement);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void RenderText(RichTextElement textElement)
        {
            var label = new Label(textElement.Content)
            {
                name = "article-text"
            };
            articleElements.Add(label);
            Add(label);
        }

        private void RenderImage(RichTextElement imageElement)
        {
            string[] parts = imageElement.Content.Split(',');
            string imagePath = parts[0].Trim();
            int widthOverride = -1;

            if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out widthOverride))
            {
                // Horizontal width value was provided.
            }

            string fullPath = Path.Combine(Path.GetDirectoryName(_contentPath), imagePath);
            Texture2D image = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

            if (image != null)
            {
                var newImageElement = new VisualElement();
                newImageElement.style.backgroundColor = Color.clear; // 透明
                newImageElement.AddToClassList(k_imageUssClass);

                var imageEl = new Image()
                {
                    image = image
                };
                if (widthOverride > 0)
                {
                    imageEl.style.width = widthOverride;
                }

                newImageElement.Add(imageEl);
                articleElements.Add(newImageElement);
                Add(newImageElement);
            }
            else
            {
                Debug.LogError($"Image not found: {imagePath}");
            }
        }

        private void RenderButton(RichTextElement buttonElement)
        {
            string[] parts = buttonElement.Content.Split(',');
            string label = parts[0].Trim();
            string styleName = parts[1].Trim();
            string url = parts[2].Trim();

            var button = new ShadowButton()
            {
                name = "article-button",
                text = label
            };

            button.clicked += () => Application.OpenURL(url);
            button.AddToClassList("article-button");
            button.userData = url;
            articleElements.Add(button);
            Add(button);
        }

        private void RenderHorizontalLine(RichTextElement horizontalLineElement)
        {
            string[] parts = horizontalLineElement.Content.Split(',');
            string colorPart = parts[0].Trim();
            Color lineColor;
            if (!ColorUtility.TryParseHtmlString(colorPart, out lineColor))
            {
                lineColor = Color.black;
            }
            if (int.TryParse(parts[1].Trim(), out int lineThickness))
            {
                var lineElement = new VisualElement();
                lineElement.style.backgroundColor = lineColor;
                lineElement.style.height = 2;
                lineElement.style.width = this.layout.width;
                lineElement.style.marginBottom = lineThickness;
                articleElements.Add(lineElement);
                Add(lineElement);
            }
            else
            {
                Debug.LogError("Invalid line thickness in HR tag: " + parts[1].Trim());
            }
        }

        private void RenderHeaderText(RichTextElement headerTextElement)
        {
            var label = new Label(headerTextElement.Content)
            {
                name = "header-text"
            };
            label.AddToClassList(k_headerUssClass);
            articleElements.Add(label);
            Add(label);
        }

        private void RenderSection(RichTextElement sectionElement)
        {
            var sectionRoot = new VisualElement()
            {
                name = "section-root"
            };
            sectionRoot.AddToClassList(k_sectionUssClass);

            var sectionParts = sectionElement.Content.Split(',');
            string imagePath = sectionParts[0].Trim();
            string textContent = sectionParts.Length > 2 ? sectionParts[2].Trim() : "";
            var imageElement = new VisualElement()
            {
                name = "section-image-container"
            };
            imageElement.style.backgroundColor = Color.clear;
            var paragraphElement = new VisualElement()
            {
                name = "section-paragraph"
            };

            sectionRoot.Add(imageElement);
            sectionRoot.Add(paragraphElement);

            if (!string.IsNullOrEmpty(imagePath))
            {
                imageElement.AddToClassList(k_imageUssClass);

                string fullPath = Path.Combine(Path.GetDirectoryName(_contentPath), imagePath);
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

                if (texture != null)
                {
                    var img = new Image()
                    {
                        image = texture,
                        scaleMode = ScaleMode.ScaleToFit
                    };
                    img.style.height = 200;
                    imageElement.Add(img);
                }
                else
                {
                    Debug.LogError($"Image not found: {imagePath}");
                }
            }

            var paragraphLabel = new Label()
            {
                text = textContent
            };

            paragraphElement.Add(paragraphLabel);

            articleElements.Add(sectionRoot);
            Add(sectionRoot);
        }

        private List<RichTextElement> ParseRichTextElements()
        {
            var content = File.ReadAllText(_contentPath);

            var textElements = new List<RichTextElement>();
            var currentIndex = 0;

            if (string.IsNullOrEmpty(content))
            {
                return textElements;
            }

            while (currentIndex < content.Length)
            {
                var tagStartIndex = content.IndexOf('<', currentIndex);
                if (tagStartIndex != -1)
                {
                    if (tagStartIndex > currentIndex)
                    {
                        string textBeforeTag = content.Substring(currentIndex, tagStartIndex - currentIndex).Trim('\n');
                        if (!string.IsNullOrEmpty(textBeforeTag))
                        {
                            textElements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = textBeforeTag });
                        }
                    }

                    var tagEndIndex = content.IndexOf('>', tagStartIndex);
                    if (tagEndIndex != -1)
                    {
                        string tag = content.Substring(tagStartIndex, tagEndIndex - tagStartIndex + 1);
                        currentIndex = tagEndIndex + 1;

                        if (tag.StartsWith("<txt>"))
                        {
                            var txtEndIndex = content.IndexOf("</txt>", currentIndex);
                            if (txtEndIndex != -1)
                            {
                                string richTextContent = content.Substring(currentIndex, txtEndIndex - currentIndex);
                                textElements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = richTextContent });
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
                            var sectionEndIndex = content.IndexOf("</section>", currentIndex);
                            if (sectionEndIndex != -1)
                            {
                                string sectionData = content.Substring(currentIndex, sectionEndIndex - currentIndex).Trim('\n');
                                textElements.Add(new RichTextElement { Type = RichTextElementType.Section, Content = sectionData });
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
                            var imageEndIndex = content.IndexOf("</image>", currentIndex);
                            if (imageEndIndex != -1)
                            {
                                string imageData = content.Substring(currentIndex, imageEndIndex - currentIndex).Trim('\n');
                                textElements.Add(new RichTextElement { Type = RichTextElementType.Image, Content = imageData });
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
                            var buttonEndIndex = content.IndexOf("</button>", currentIndex);
                            if (buttonEndIndex != -1)
                            {
                                string buttonData = content.Substring(currentIndex, buttonEndIndex - currentIndex).Trim('\n');
                                textElements.Add(new RichTextElement { Type = RichTextElementType.Button, Content = buttonData });
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
                            var hrEndIndex = content.IndexOf("</hr>", currentIndex);
                            if (hrEndIndex != -1)
                            {
                                string hrData = content.Substring(currentIndex, hrEndIndex - currentIndex).Trim('\n');
                                textElements.Add(new RichTextElement { Type = RichTextElementType.HorizontalLine, Content = hrData });
                                currentIndex = hrEndIndex + "</hr>".Length;
                                currentIndex = SkipNewLines(content, currentIndex);
                            }
                            else
                            {
                                Debug.LogError("Horizontal line tag not closed");
                                break;
                            }
                        }
                        else if (tag.StartsWith("<t>"))
                        {
                            var textEndIndex = content.IndexOf("</t>", currentIndex);
                            if (textEndIndex != -1)
                            {
                                string textContent = content.Substring(currentIndex, textEndIndex - currentIndex).Trim('\n');
                                textElements.Add(new RichTextElement { Type = RichTextElementType.HeaderText, Content = textContent });
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
                        textElements.Add(new RichTextElement { Type = RichTextElementType.Text, Content = remainingText });
                    }
                    break;
                }
            }

            return textElements;
        }

        private int SkipNewLines(string content, int currentIndex)
        {
            while (currentIndex < content.Length && (content[currentIndex] == '\n' || content[currentIndex] == '\r'))
            {
                currentIndex++;
            }
            return currentIndex;
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

        private class DottedLine : VisualElement
        {
            public DottedLine(Rect rect, Color lineColor, int dotCount, int dotLength)
            {
                style.position = Position.Absolute;
                style.left = rect.x;
                style.top = rect.y;
                style.width = rect.width;
                style.height = rect.height;
                style.backgroundColor = lineColor;

                for (int i = 0; i < dotCount; i++)
                {
                    var dot = new VisualElement();
                    dot.AddToClassList("article-dotted-line-dot");
                    dot.style.width = dotLength;
                    dot.style.height = rect.height;
                    dot.style.backgroundColor = lineColor;
                    dot.style.marginLeft = i * (rect.width / dotCount);
                    Add(dot);
                }
            }
        }
    }
}