using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class LobbySetup : MonoBehaviour
    {
        private void CreateUI()
        {
            var canvasObj = new GameObject("LobbyCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            var interactionPanel = CreateInteractionPanel(canvasObj.transform);
            var dialoguePanel = CreateDialoguePanel(canvasObj.transform);

            var ui = canvasObj.AddComponent<LobbyUI>();
            SetField(ui, "_interactionPanel", interactionPanel);
            SetField(ui, "_dialoguePanel", dialoguePanel);
            SetField(ui, "_talkButton", interactionPanel.transform.Find("TalkButton").GetComponent<Button>());
            SetField(ui, "_punchButton", interactionPanel.transform.Find("PunchButton").GetComponent<Button>());
            SetField(ui, "_targetNameText", interactionPanel.transform.Find("TargetName").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueNameText", dialoguePanel.transform.Find("NameText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueContentText", dialoguePanel.transform.Find("ContentText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueHintText", dialoguePanel.transform.Find("HintText").GetComponent<TextMeshProUGUI>());
            SetField(ui, "_dialogueNextButton", dialoguePanel.transform.Find("NextButton").GetComponent<Button>());

            // === Safe Area wrapper (iOS notch/Dynamic Island) ===
            SafeAreaHelper.CreateSafeAreaWrapper(canvas);

            // === Mobile Controls (joystick + action buttons) ===
            MobileControlsUI.Create(canvas);
        }

        private GameObject CreateInteractionPanel(Transform parent)
        {
            var panel = new GameObject("InteractionPanel");
            panel.transform.SetParent(parent, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(54, 54);

            var punchBtnObj = CreateUIButton("PunchButton", panel.transform, Vector2.zero, "", Color.clear, new Vector2(54, 54));
            
            var fistIcon = new GameObject("FistIcon");
            fistIcon.transform.SetParent(punchBtnObj.transform, false);
            var fistRT = fistIcon.AddComponent<RectTransform>();
            fistRT.sizeDelta = new Vector2(50, 50);
            var fistImg = fistIcon.AddComponent<Image>();
            fistImg.sprite = CreateFistSprite();
            fistImg.color = Color.white;

            var talkBtnObj = CreateUIButton("TalkButton", panel.transform, Vector2.zero, "", Color.clear, new Vector2(54, 54));
            talkBtnObj.SetActive(false);

            var talkIcon = new GameObject("TalkIcon");
            talkIcon.transform.SetParent(talkBtnObj.transform, false);
            var talkRT = talkIcon.AddComponent<RectTransform>();
            talkRT.sizeDelta = new Vector2(50, 50);
            var talkImg = talkIcon.AddComponent<Image>();
            talkImg.sprite = CreateSpeechBubbleSprite();
            talkImg.color = Color.white;

            var nameObj = new GameObject("TargetName");
            nameObj.transform.SetParent(panel.transform, false);
            nameObj.transform.localPosition = new Vector3(9999f, 9999f, 0);
            var nameTmp = nameObj.AddComponent<TextMeshProUGUI>();
            nameTmp.text = "";

            panel.SetActive(false);
            return panel;
        }

        private Sprite CreateFistSprite()
        {
            // 1. Try loading from Resources first (works on both Editor and built Android/iOS APKs)
            var resourceTex = Resources.Load<Texture2D>("fist");
            if (resourceTex != null)
            {
                try
                {
                    // Create a duplicate texture that is readable at runtime
                    Texture2D readableTex = new Texture2D(resourceTex.width, resourceTex.height);
                    RenderTexture tmp = RenderTexture.GetTemporary(
                        resourceTex.width,
                        resourceTex.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);
                    Graphics.Blit(resourceTex, tmp);
                    RenderTexture previous = RenderTexture.active;
                    RenderTexture.active = tmp;
                    readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                    readableTex.Apply();
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(tmp);

                    Color[] pixels = readableTex.GetPixels();
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i].r > 0.99f && pixels[i].g > 0.99f && pixels[i].b > 0.99f)
                        {
                            pixels[i] = new Color(0, 0, 0, 0);
                        }
                    }
                    readableTex.SetPixels(pixels);
                    readableTex.Apply();
                    readableTex.filterMode = FilterMode.Bilinear;
                    return Sprite.Create(readableTex, new Rect(0, 0, readableTex.width, readableTex.height), new Vector2(0.5f, 0.5f), readableTex.width);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to key white from Resources texture, returning directly: {ex.Message}");
                    return Sprite.Create(resourceTex, new Rect(0, 0, resourceTex.width, resourceTex.height), new Vector2(0.5f, 0.5f), resourceTex.width);
                }
            }

            // 2. Fallback to reading file system (original logic)
            string[] searchPaths = {
                System.IO.Path.Combine(Application.dataPath, "Textures/fist.png"),
                System.IO.Path.Combine(Application.persistentDataPath, "Textures/fist.png")
            };
            string customPath = null;
            foreach (var p in searchPaths)
            {
                if (System.IO.File.Exists(p)) { customPath = p; break; }
            }
            if (customPath != null && System.IO.File.Exists(customPath))
            {
                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(customPath);
                    Texture2D customTex = new Texture2D(2, 2);
                    if (customTex.LoadImage(data))
                    {
                        Color[] pixels = customTex.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            if (pixels[i].r > 0.99f && pixels[i].g > 0.99f && pixels[i].b > 0.99f)
                            {
                                pixels[i] = new Color(0, 0, 0, 0);
                            }
                        }
                        customTex.SetPixels(pixels);
                        customTex.Apply();

                        customTex.filterMode = FilterMode.Bilinear;
                        return Sprite.Create(customTex, new Rect(0, 0, customTex.width, customTex.height), new Vector2(0.5f, 0.5f), customTex.width);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"Failed to load custom fist image: {ex.Message}");
                }
            }

            const int w = 32;
            const int h = 32;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            Color outline = new Color(0.55f, 0.22f, 0.00f);
            Color fill = new Color(1.00f, 0.78f, 0.12f);
            Color highlight = new Color(1.00f, 0.92f, 0.35f);

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, clear);

            string[] mask = {
                "................................",
                "................................",
                "................................",
                "................................",
                "......OOO...OO...OO...OO...OOO..",
                ".....OFFFO.OFFO.OFFO.OFFO.OFFFO.",
                "....OFFFFOFFFFOFFFFOFFFFOFFFFO..",
                "...OFFFFFFOFFFFOFFFFOFFFFFFFO...",
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "..OFFFFFFFFFFFFFFFFFFFFFFFFFFO..",
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...",
                "...OFFFFFFFFFFFFFFFFFFFFFFFFO...",
                "....OFFFFFFFFFFFFFFFFFFFFFFO....",
                ".....OFFFFFFFFFFFFFFFFFFFFO.....",
                "......OFFFFFFFFFFFFFFFFFFO......",
                ".......OFFFFFFFFFFFFFFFFO.......",
                ".........OFFFFFFFFFFFFO.........",
                "...........OFFFFFFFFO...........",
                ".............OOOOOO.............",
                "................................",
                "................................",
                "................................",
                "................................",
                "................................"
            };

            for (int row = 0; row < mask.Length; row++)
            {
                string line = mask[row];
                for (int col = 0; col < line.Length; col++)
                {
                    int px = col;
                    int py = row;
                    if (px < 0 || px >= w || py < 0 || py >= h) continue;
                    char c = line[col];
                    switch (c)
                    {
                        case 'O':
                            tex.SetPixel(px, py, outline);
                            break;
                        case 'F':
                            float t = (float)row / mask.Length;
                            tex.SetPixel(px, py, Color.Lerp(fill, highlight, t));
                            break;
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f);
        }

        private GameObject CreateDialoguePanel(Transform parent)
        {
            var panel = new GameObject("DialoguePanel");
            panel.transform.SetParent(parent, false);

            var rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.1f, 0.02f);
            rt.anchorMax = new Vector2(0.9f, 0.22f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.16f, 0.92f);

            CreateTMPText("NameText", panel.transform, Vector2.zero, "NPC", 20, TextAlignmentOptions.TopLeft, new Color(0.39f, 0.71f, 1f), new Vector2(0.05f, 0.7f), new Vector2(0.95f, 0.95f));
            CreateTMPText("ContentText", panel.transform, Vector2.zero, "", 15, TextAlignmentOptions.TopLeft, Color.white, new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.7f));
            CreateTMPText("HintText", panel.transform, Vector2.zero, "", 12, TextAlignmentOptions.BottomRight, new Color(0.62f, 0.66f, 0.78f), new Vector2(0.5f, 0.02f), new Vector2(0.95f, 0.2f));

            var nextBtn = new GameObject("NextButton");
            nextBtn.transform.SetParent(panel.transform, false);
            var nrt = nextBtn.AddComponent<RectTransform>();
            nrt.anchorMin = Vector2.zero; nrt.anchorMax = Vector2.one;
            nrt.offsetMin = nrt.offsetMax = Vector2.zero;
            nextBtn.AddComponent<Button>();
            nextBtn.AddComponent<Image>().color = Color.clear;

            panel.SetActive(false);
            return panel;
        }

        private void AddNameTag(GameObject character, string nameText, Color textColor)
        {
            var tag = new GameObject("NameTag");
            tag.transform.SetParent(character.transform, false);
            tag.transform.localPosition = new Vector3(0, 1.8f, 0);

            var tmp = tag.AddComponent<TextMeshPro>();
            tmp.text = nameText;
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;
            tmp.outlineColor = Color.black;
            tmp.outlineWidth = 0.25f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 1);

            tag.AddComponent<BillboardText>();
        }

        private GameObject CreateCharacterTopDown(string charName, Color bodyColor, Color skinColor)
        {
            // Reflection compatibility fallback
            return CharacterVisuals.CreateCharacterTopDown(charName, bodyColor, skinColor);
        }

        private void CreateTMPText(string name, Transform parent, Vector2 pos, string text, float size, TextAlignmentOptions align, Color color, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            tmp.color = color;

            var rt = tmp.GetComponent<RectTransform>();
            if (anchorMin.HasValue && anchorMax.HasValue)
            {
                rt.anchorMin = anchorMin.Value;
                rt.anchorMax = anchorMax.Value;
                rt.offsetMin = rt.offsetMax = Vector2.zero;
            }
            else
            {
                rt.anchoredPosition = pos;
                rt.sizeDelta = new Vector2(200, 30);
            }
        }

        private Sprite CreateSpeechBubbleSprite()
        {
            int sz = 64;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            Color bubbleCol = new Color(0.18f, 0.65f, 0.95f, 0.95f);

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    tex.SetPixel(x, y, Color.clear);

            for (int y = 20; y <= 54; y++)
                for (int x = 8; x <= 56; x++)
                    tex.SetPixel(x, y, bubbleCol);

            // Tail
            for (int y = 8; y < 20; y++)
                for (int x = 16; x <= 16 + (y - 8); x++)
                    tex.SetPixel(x, y, bubbleCol);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }

        private GameObject CreateUIButton(string name, Transform parent, Vector2 pos, string label, Color bgColor, Vector2 size)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btnObj.AddComponent<Image>().color = bgColor;
            btnObj.AddComponent<Button>();

            if (!string.IsNullOrEmpty(label))
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(btnObj.transform, false);
                var tmp = labelObj.AddComponent<TextMeshProUGUI>();
                tmp.text = label;
                tmp.fontSize = 15;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
                tmp.fontStyle = FontStyles.Bold;
                var lrt = tmp.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
                lrt.offsetMin = lrt.offsetMax = Vector2.zero;
            }
            return btnObj;
        }

        private void SetField<T>(object obj, string fieldName, T value)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }
    }

    /// <summary>
    /// Hiển thị bóng hội thoại (dialogue bubble) trên đầu nhân vật,
    /// tự động chạy qua các câu thoại lặp đi lặp lại.
    /// </summary>
    public class FloatingDialogueBubble : MonoBehaviour
    {
        private string[] _lines;
        private float _interval = 4.0f;
        private TextMeshPro _tmp;
        private int _currentIndex = 0;
        private float _timer;

        public void Setup(string[] lines, float interval = 4.0f)
        {
            _lines = lines;
            _interval = interval;
            _timer = interval;
        }

        private void Start()
        {
            _tmp = gameObject.AddComponent<TextMeshPro>();
            _tmp.fontSize = 3.5f;
            _tmp.alignment = TextAlignmentOptions.Center;
            _tmp.enableWordWrapping = true;
            _tmp.rectTransform.sizeDelta = new Vector2(5.5f, 2f);

            // Đảm bảo đối tượng luôn xoay mặt về camera
            gameObject.AddComponent<BillboardText>();

            // Màu chữ đen kèm viền trắng nổi bật cho bóng thoại
            _tmp.color = Color.black;
            _tmp.outlineColor = Color.white;
            _tmp.outlineWidth = 0.25f;
            _tmp.fontStyle = FontStyles.Bold;

            UpdateText();
        }

        private void Update()
        {
            if (_lines == null || _lines.Length == 0) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = _interval;
                _currentIndex = (_currentIndex + 1) % _lines.Length;
                UpdateText();
            }
        }

        private void UpdateText()
        {
            if (_lines == null || _currentIndex >= _lines.Length) return;

            string rawText = _lines[_currentIndex];
            _tmp.text = $"\"{rawText}\"";
        }
    }

    public class DebugScenePlayers : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(LogLoop());
        }

        private IEnumerator LogLoop()
        {
            while (true)
            {
                var allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                string report = "--- PLAYER OBJECTS IN RUNNING SCENE ---\n";
                int count = 0;
                foreach (var go in allObjects)
                {
                    if (go != null && (go.GetComponent<PlayerController>() != null || go.GetComponent<NetworkPlayer>() != null || go.name.Contains("Preview")))
                    {
                        count++;
                        report += $"- {go.name} (Active: {go.activeInHierarchy}, Tag: {go.tag}, Pos: {go.transform.position})\n";
                        report += "  Children: ";
                        for (int i = 0; i < go.transform.childCount; i++)
                        {
                            var child = go.transform.GetChild(i);
                            report += $"{child.name} (Active: {child.gameObject.activeSelf})";
                            if (child.childCount > 0)
                            {
                                report += "[";
                                for (int j = 0; j < child.childCount; j++)
                                {
                                    report += $"{child.GetChild(j).name}, ";
                                }
                                report += "]";
                            }
                            report += ", ";
                        }
                        report += "\n";
                    }
                }
                if (count == 0)
                {
                    report += "No player-related GameObjects found.\n";
                }
                // Debug.Log(report);
                yield return new WaitForSeconds(10.0f);
            }
        }
    }

    }
