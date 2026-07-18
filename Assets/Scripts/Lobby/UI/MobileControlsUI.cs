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
        private PlayerController _player;

        public static MobileControlsUI Instance { get; private set; }

        private void Awake()
        {
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

            // === Action Buttons (góc phải dưới) ===
            controls._punchButton = CreateActionButton(root.transform, "PunchBtn", "👊",
                new Vector2(-80f, 100f), new Color(0.9f, 0.25f, 0.2f, 0.75f), 70f);
            controls._punchButton.onClick.AddListener(controls.OnPunchPressed);

            controls._interactButton = CreateActionButton(root.transform, "InteractBtn", "💬",
                new Vector2(-80f, 190f), new Color(0.2f, 0.6f, 0.9f, 0.75f), 60f);
            controls._interactButton.onClick.AddListener(controls.OnInteractPressed);
            controls._interactButton.gameObject.SetActive(false); // Ẩn mặc định, hiện khi gần NPC

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

            // Hiện/ẩn nút Interact dựa vào có đối tượng gần không
            bool hasNearby = _player.GetNearestInteractable() != null;
            if (_interactButton != null && _interactButton.gameObject.activeSelf != hasNearby)
            {
                _interactButton.gameObject.SetActive(hasNearby);
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
        /// Tạo một nút tròn với emoji text bên trong.
        /// anchoredPosition relative to bottom-right anchor.
        /// </summary>
        private static Button CreateActionButton(Transform parent, string name, string emoji,
            Vector2 offsetFromBottomRight, Color bgColor, float size)
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
            img.color = bgColor;
            img.sprite = CreateCircleSprite();
            img.type = Image.Type.Simple;

            var btn = btnObj.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(bgColor.r + 0.15f, bgColor.g + 0.15f, bgColor.b + 0.15f, 0.9f);
            colors.pressedColor = new Color(bgColor.r - 0.1f, bgColor.g - 0.1f, bgColor.b - 0.1f, 0.95f);
            btn.colors = colors;

            // Emoji text
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);

            var textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            var tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = emoji;
            tmp.fontSize = size * 0.45f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;

            return btn;
        }

        /// <summary>
        /// Tạo sprite hình tròn bằng code.
        /// </summary>
        private static Sprite CreateCircleSprite()
        {
            int sz = 64;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            float center = sz * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                        tex.SetPixel(x, y, Color.white);
                    else if (dist <= radius + 1f)
                        tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(radius + 1f - dist)));
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
