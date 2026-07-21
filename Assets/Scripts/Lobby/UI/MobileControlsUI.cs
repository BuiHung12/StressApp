using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Bộ điều khiển ảo cho mobile: Joystick + Punch + Interact + Emoji buttons.
    /// Tự động ẩn trên PC, hiện trên mobile/touch devices.
    /// Tạo toàn bộ UI bằng code, gắn vào Canvas hiện có.
    /// </summary>
    public class MobileControlsUI : MonoBehaviour
    {
        private VirtualJoystick _joystick;
        private GameObject _controlsRoot;
        private Button _punchButton;
        private Button _interactButton;
        private Image _interactButtonImage;
        private TextMeshProUGUI _interactButtonLabel;
        private TextMeshProUGUI _interactIconTMP;
        private string _lastInteractionState = "";
        private PlayerController _player;

        public static MobileControlsUI Instance { get; private set; }

        private void Awake()
        {
            if (NetworkSetup.IsHeadlessServer()) return;
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// Tạo toàn bộ mobile controls UI trên canvas đã cho.
        /// </summary>
        public static MobileControlsUI Create(Canvas canvas)
        {
            var root = new GameObject("MobileControls");
            root.transform.SetParent(canvas.transform, false);

            var rt = root.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var controls = root.AddComponent<MobileControlsUI>();
            controls._controlsRoot = root;

            // === Virtual Joystick (góc trái dưới) ===
            controls._joystick = VirtualJoystick.CreateJoystick(root.transform);

            // === Punch Button (nút đấm tròn góc phải dưới) ===
            controls._punchButton = CreateActionButton(root.transform, "PunchBtn", "",
                new Vector2(-160f, 150f), new Color(0.9f, 0.25f, 0.2f, 0.85f), 95f, "fist");
            controls._punchButton.onClick.AddListener(controls.OnPunchPressed);

            // === Dynamic Capsule Action Button (nút Tương Tác / Gieo Hạt / Thu Hoạch cao cấp) ===
            controls._interactButton = CreateCapsuleActionButton(root.transform, "InteractBtn", "✨", "TƯƠNG TÁC",
                new Vector2(-185f, 280f), new Color(0.2f, 0.75f, 0.95f, 0.9f), new Vector2(175f, 74f),
                out controls._interactIconTMP, out controls._interactButtonLabel, out controls._interactButtonImage);
            controls._interactButton.onClick.AddListener(controls.OnInteractPressed);
            controls._interactButton.gameObject.SetActive(false); // Ẩn mặc định, hiện khi gần NPC/Vườn rau/Mây

            // Chỉ hiện trên mobile hoặc khi test trong Editor
            bool showControls = Application.isMobilePlatform;
#if UNITY_EDITOR
            // Cho phép test trong Editor: luôn hiện nếu đang simulate mobile
            showControls = true;
#endif
            root.SetActive(showControls);

            return controls;
        }

        private void Start()
        {
            _player = FindAnyObjectByType<PlayerController>();
        }

        private void Update()
        {
            if (_player == null)
            {
                _player = FindAnyObjectByType<PlayerController>();
                return;
            }

            // Hiện/ẩn nút Interact dựa vào có đối tượng gần không (NPC, Luống rau GardenPlot, Đám mây CloudLayer)
            bool hasNearby = _player.HasAnyNearbyInteractable();
            if (_interactButton != null)
            {
                if (_interactButton.gameObject.activeSelf != hasNearby)
                {
                    _interactButton.gameObject.SetActive(hasNearby);
                }

                if (hasNearby)
                {
                    string state = _player.GetInteractionLabel();
                    if (_lastInteractionState != state)
                    {
                        _lastInteractionState = state;
                        string icon = "✨";
                        Color theme = new Color(0.2f, 0.75f, 0.95f, 0.95f);
                        switch (state)
                        {
                            case "GIEO HẠT":
                                icon = "🌱";
                                theme = new Color(0.0f, 1.0f, 0.45f, 0.95f);
                                break;
                            case "THU HOẠCH":
                                icon = "🧺";
                                theme = new Color(1.0f, 0.85f, 0.1f, 0.95f);
                                break;
                            case "ĐANG LỚN":
                                icon = "⏳";
                                theme = new Color(0.3f, 0.75f, 1.0f, 0.7f);
                                break;
                            case "TRÒ CHUYỆN":
                                icon = "💬";
                                theme = new Color(0.0f, 0.9f, 1.0f, 0.95f);
                                break;
                            case "NHẢY MÂY":
                                icon = "☁️";
                                theme = new Color(0.75f, 0.45f, 1.0f, 0.95f);
                                break;
                        }

                        if (_interactIconTMP != null) _interactIconTMP.text = icon;
                        if (_interactButtonLabel != null)
                        {
                            _interactButtonLabel.text = state;
                            _interactButtonLabel.color = theme;
                        }
                        if (_interactButtonImage != null)
                        {
                            Color darkGlass = new Color(0.06f, 0.08f, 0.12f, 0.82f);
                            _interactButtonImage.sprite = CreateCapsuleSprite(darkGlass, theme, 5f);
                        }
                    }
                }
            }
        }

        private void OnPunchPressed()
        {
            if (_player != null && !_player.IsPunching)
            {
                _player.ExecutePunch();
            }
        }

        private void OnInteractPressed()
        {
            if (_player != null)
            {
                _player.ExecuteInteraction();
            }
        }

        /// <summary>
        /// Tạo một nút tròn với emoji text bên trong hoặc icon sprite.
        /// anchoredPosition relative to bottom-right anchor.
        /// </summary>
        private static Button CreateActionButton(Transform parent, string name, string labelText,
            Vector2 offsetFromBottomRight, Color themeColor, float size, string spriteResourceName = null)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f); // Bottom-right
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offsetFromBottomRight;
            rt.sizeDelta = new Vector2(size, size);

            var img = btnObj.AddComponent<Image>();
            // Tạo nền kính mờ tối (Dark Glass) kết hợp viền neon sáng theo màu chủ đề (themeColor)
            Color darkGlass = new Color(0.08f, 0.09f, 0.12f, 0.65f);
            img.sprite = CreateCircleSprite(darkGlass, themeColor, 5f);
            img.color = Color.white;
            img.type = Image.Type.Simple;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 0.95f);
            btn.colors = colors;

            // Nếu truyền tên ảnh, vẽ icon đè lên trên nút tròn
            if (!string.IsNullOrEmpty(spriteResourceName))
            {
                var iconObj = new GameObject("Icon", typeof(RectTransform));
                iconObj.transform.SetParent(btnObj.transform, false);
                var iconRT = iconObj.GetComponent<RectTransform>();
                iconRT.anchorMin = Vector2.zero;
                iconRT.anchorMax = Vector2.one;
                iconRT.offsetMin = new Vector2(size * 0.22f, size * 0.22f);
                iconRT.offsetMax = new Vector2(-size * 0.22f, -size * 0.22f);
                
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
                
                // Nạp ảnh từ Resources
                var loadedObj = Resources.Load(spriteResourceName);
                if (loadedObj != null)
                {
                    Texture2D srcTex = null;
                    if (loadedObj is Sprite spriteAsset) srcTex = spriteAsset.texture;
                    else if (loadedObj is Texture2D textureAsset) srcTex = textureAsset;

                    if (srcTex != null)
                    {
                        // Tạo Texture2D có thể đọc ghi được bằng RenderTexture để xử lý ảnh
                        RenderTexture tmpRT = RenderTexture.GetTemporary(
                            srcTex.width, srcTex.height, 0, 
                            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                            
                        Graphics.Blit(srcTex, tmpRT);
                        RenderTexture previous = RenderTexture.active;
                        RenderTexture.active = tmpRT;
                        
                        Texture2D readableTex = new Texture2D(srcTex.width, srcTex.height);
                        readableTex.ReadPixels(new Rect(0, 0, tmpRT.width, tmpRT.height), 0, 0);
                        readableTex.Apply();
                        
                        RenderTexture.active = previous;
                        RenderTexture.ReleaseTemporary(tmpRT);
                        
                        // Lọc bỏ hoàn toàn nền màu trắng (Chroma Keying)
                        Color[] pixels = readableTex.GetPixels();
                        for (int i = 0; i < pixels.Length; i++)
                        {
                            // Nếu pixel gần như màu trắng, cho nó trong suốt hoàn toàn
                            if (pixels[i].r > 0.9f && pixels[i].g > 0.9f && pixels[i].b > 0.9f)
                            {
                                pixels[i] = Color.clear;
                            }
                            else if (pixels[i].a > 0.1f)
                            {
                                // Chuyển các pixel nắm đấm sang màu trắng sữa để hiển thị cực nét trên nền kính tối
                                float brightness = (pixels[i].r + pixels[i].g + pixels[i].b) / 3f;
                                if (brightness < 0.9f)
                                {
                                    pixels[i] = new Color(0.95f, 0.96f, 1f, pixels[i].a);
                                }
                            }
                        }
                        readableTex.SetPixels(pixels);
                        readableTex.Apply();
                        
                        iconImg.sprite = Sprite.Create(readableTex, 
                            new Rect(0, 0, readableTex.width, readableTex.height), 
                            new Vector2(0.5f, 0.5f));
                    }
                }
            }
            else if (!string.IsNullOrEmpty(labelText))
            {
                var textObj = new GameObject("Label");
                textObj.transform.SetParent(btnObj.transform, false);

                var textRT = textObj.AddComponent<RectTransform>();
                textRT.anchorMin = Vector2.zero;
                textRT.anchorMax = Vector2.one;
                textRT.offsetMin = textRT.offsetMax = Vector2.zero;

                var tmp = textObj.AddComponent<TextMeshProUGUI>();
                tmp.text = labelText;
                tmp.fontSize = size * 0.25f; // Chữ vừa vặn
                tmp.fontStyle = FontStyles.Bold;
                tmp.color = new Color(0.9f, 0.95f, 1f); // Màu xanh băng nhẹ hiện đại
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
            }

            return btn;
        }

        /// <summary>
        /// Tạo sprite hình tròn với nền và viền tùy chỉnh (Khử răng cưa mịn).
        /// </summary>
        private static Sprite CreateCircleSprite(Color fillColor, Color borderColor, float borderThickness)
        {
            int sz = 128; // Tăng độ phân giải cho mịn màng
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            float center = sz * 0.5f;
            float radius = center - 2f;
            float innerRadius = radius - borderThickness;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > radius)
                    {
                        // Khử răng cưa viền ngoài
                        if (dist <= radius + 1.5f)
                        {
                            float alpha = Mathf.Clamp01(radius + 1.5f - dist);
                            tex.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha));
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                    else if (dist >= innerRadius)
                    {
                        // Vành viền neon
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        // Ruột trong (kính mờ)
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Tạo nút capsule nằm ngang sang trọng với Icon + Text và viền Neon Glassmorphism.
        /// </summary>
        private static Button CreateCapsuleActionButton(Transform parent, string name, string defaultIcon, string defaultText,
            Vector2 offsetFromBottomRight, Color themeColor, Vector2 size, out TextMeshProUGUI iconTMP, out TextMeshProUGUI labelTMP, out Image bgImage)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);

            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 0f); // Bottom-right anchor
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = offsetFromBottomRight;
            rt.sizeDelta = size;

            bgImage = btnObj.AddComponent<Image>();
            Color darkGlass = new Color(0.06f, 0.08f, 0.12f, 0.82f);
            bgImage.sprite = CreateCapsuleSprite(darkGlass, themeColor, 5f);
            bgImage.color = Color.white;
            bgImage.type = Image.Type.Simple;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 0.95f);
            btn.colors = colors;

            // Icon on left
            var iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(btnObj.transform, false);
            var iconRT = iconObj.AddComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0f, 0f);
            iconRT.anchorMax = new Vector2(0.38f, 1f);
            iconRT.offsetMin = iconRT.offsetMax = Vector2.zero;

            iconTMP = iconObj.AddComponent<TextMeshProUGUI>();
            iconTMP.text = defaultIcon;
            iconTMP.fontSize = 30f;
            iconTMP.alignment = TextAlignmentOptions.Center;
            iconTMP.raycastTarget = false;

            // Text on right
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.35f, 0f);
            textRT.anchorMax = new Vector2(0.96f, 1f);
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            labelTMP = textObj.AddComponent<TextMeshProUGUI>();
            labelTMP.text = defaultText;
            labelTMP.fontSize = 19f;
            labelTMP.fontStyle = FontStyles.Bold;
            labelTMP.color = themeColor;
            labelTMP.alignment = TextAlignmentOptions.Left;
            labelTMP.verticalAlignment = VerticalAlignmentOptions.Middle;
            labelTMP.raycastTarget = false;

            return btn;
        }

        /// <summary>
        /// Tạo sprite capsule bo tròn 2 đầu với hiệu ứng Glassmorphism và viền Neon sáng mịn.
        /// </summary>
        private static Sprite CreateCapsuleSprite(Color fillColor, Color borderColor, float borderThickness)
        {
            int w = 256;
            int h = 112;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float radius = (h * 0.5f) - 2f;
            float innerRadius = radius - borderThickness;

            Vector2 leftCenter = new Vector2(radius + 2f, h * 0.5f);
            Vector2 rightCenter = new Vector2(w - radius - 2f, h * 0.5f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    float distToCenter;
                    if (x < leftCenter.x)
                        distToCenter = Vector2.Distance(p, leftCenter);
                    else if (x > rightCenter.x)
                        distToCenter = Vector2.Distance(p, rightCenter);
                    else
                        distToCenter = Mathf.Abs(y - h * 0.5f);

                    if (distToCenter <= innerRadius - 1f)
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                    else if (distToCenter <= innerRadius)
                    {
                        float alpha = 1f - (distToCenter - (innerRadius - 1f));
                        tex.SetPixel(x, y, Color.Lerp(fillColor, borderColor, alpha));
                    }
                    else if (distToCenter <= radius - 1f)
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else if (distToCenter <= radius)
                    {
                        float alpha = 1f - (distToCenter - (radius - 1f));
                        tex.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
