using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class LobbyUI : MonoBehaviour
    {
        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size)
        {
            return CreatePanel(parent, name, pos, size, false);
        }

        private GameObject CreatePanel(Transform parent, string name, Vector2 pos, Vector2 size, bool raycastTarget)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.raycastTarget = raycastTarget;
            return obj;
        }

        private static Sprite _defaultSprite;
        private static Sprite GetDefaultSprite()
        {
            if (_defaultSprite == null)
            {
                Texture2D tex = new Texture2D(2, 2);
                Color[] pixels = new Color[4] { Color.white, Color.white, Color.white, Color.white };
                tex.SetPixels(pixels);
                tex.Apply();
                _defaultSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            }
            return _defaultSprite;
        }

        private GameObject CreateRoundedPanel(Transform parent, string name, Vector2 pos, Vector2 size, Color color, bool raycastTarget = false)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.sprite = GetDefaultSprite();
            img.type = Image.Type.Simple;
            img.color = color;
            img.raycastTarget = raycastTarget;
            return obj;
        }

        private GameObject MakeText(Transform parent, string name, string text, float fontSize, Vector2 pos, Vector2 size, TextAlignmentOptions align, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = align;
            tmp.color = color;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            tmp.raycastTarget = false;
            return obj;
        }

        private TMP_InputField CreateInputFieldV2(Transform parent, string defaultText, string placeholder, Vector2 pos, Vector2 size)
        {
            var obj = new GameObject("Input", typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            var rt = obj.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.sprite = GetDefaultSprite();
            img.type = Image.Type.Simple;
            img.color = new Color(0.06f, 0.06f, 0.1f, 1f);
            img.raycastTarget = true;

            var textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(obj.transform, false);
            var trt = textObj.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
            trt.offsetMin = new Vector2(10, 2); trt.offsetMax = new Vector2(-10, -2);
            var txt = textObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.enableWordWrapping = false;
            txt.raycastTarget = false;

            if (!string.IsNullOrEmpty(placeholder))
            {
                var phObj = new GameObject("Placeholder", typeof(RectTransform));
                phObj.transform.SetParent(obj.transform, false);
                var phRt = phObj.GetComponent<RectTransform>();
                phRt.anchorMin = Vector2.zero; phRt.anchorMax = Vector2.one;
                phRt.offsetMin = new Vector2(10, 2); phRt.offsetMax = new Vector2(-10, -2);
                var phTxt = phObj.AddComponent<TextMeshProUGUI>();
                phTxt.text = placeholder;
                phTxt.fontSize = 14;
                phTxt.fontStyle = FontStyles.Italic;
                phTxt.color = new Color(0.4f, 0.4f, 0.5f);
                phTxt.raycastTarget = false;

                var input = obj.AddComponent<TMP_InputField>();
                input.textComponent = txt;
                input.placeholder = phTxt;
                input.text = defaultText;
                input.characterLimit = 20;
                return input;
            }
            else
            {
                var input = obj.AddComponent<TMP_InputField>();
                input.textComponent = txt;
                input.text = defaultText;
                input.characterLimit = 40;
                return input;
            }
        }

        private void CreateGradientButton(Transform parent, string name, string label, Color colorA, Color colorB, Vector2 pos, Vector2 size, UnityEngine.Events.UnityAction action, float fontSize = 15f)
        {
            var btnObj = CreateRoundedPanel(parent, name, pos, size, Color.Lerp(colorA, colorB, 0.5f), true);
            var img = btnObj.GetComponent<Image>();
            img.sprite = GetDefaultSprite();
            img.type = Image.Type.Simple;

            var btn = btnObj.AddComponent<Button>();
            btn.onClick.AddListener(action);

            var colors = btn.colors;
            colors.highlightedColor = colorB;
            colors.pressedColor = colorA * 0.8f;
            colors.normalColor = Color.white;
            btn.colors = colors;

            MakeText(btnObj.transform, "Label", label, fontSize, Vector2.zero, size, TextAlignmentOptions.Center, Color.white);
        }


    }
    }
