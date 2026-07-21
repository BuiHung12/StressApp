using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Virtual Joystick cho điều khiển mobile.
    /// Fixed style — luôn hiện ở góc trái dưới màn hình.
    /// Hỗ trợ cả touch (mobile) và mouse drag (PC test).
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("Settings")]
        [SerializeField] private float _handleRange = 1f;
        [SerializeField] private float _deadZone = 0.1f;

        private RectTransform _baseRect;
        private RectTransform _handleRect;
        private Canvas _canvas;

        private Vector2 _input = Vector2.zero;

        /// <summary>
        /// Hướng di chuyển chuẩn hóa (x, y) trong phạm vi [-1, 1].
        /// PlayerController đọc giá trị này mỗi frame.
        /// </summary>
        public Vector2 Direction => _input;

        /// <summary>
        /// Singleton tạm thời để PlayerController truy cập dễ dàng.
        /// </summary>
        public static VirtualJoystick Instance { get; private set; }

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
        /// Khởi tạo joystick UI bằng code (không cần prefab).
        /// Gọi bởi MobileControlsUI.
        /// </summary>
        public static VirtualJoystick CreateJoystick(Transform canvasTransform)
        {
            // === Outer ring (base) ===
            var baseObj = new GameObject("JoystickBase");
            baseObj.transform.SetParent(canvasTransform, false);

            var baseRT = baseObj.AddComponent<RectTransform>();
            baseRT.anchorMin = new Vector2(0f, 0f);
            baseRT.anchorMax = new Vector2(0f, 0f);
            baseRT.pivot = new Vector2(0.5f, 0.5f);
            baseRT.anchoredPosition = new Vector2(250f, 250f);
            baseRT.sizeDelta = new Vector2(260f, 260f);

            var baseImg = baseObj.AddComponent<Image>();
            baseImg.color = new Color(1f, 1f, 1f, 0.25f);
            baseImg.raycastTarget = true;

            // Make circular appearance via sprite (Unity built-in Knob)
            baseImg.sprite = CreateCircleSprite();
            baseImg.type = Image.Type.Simple;

            // === Inner knob (handle) ===
            var handleObj = new GameObject("JoystickHandle");
            handleObj.transform.SetParent(baseObj.transform, false);

            var handleRT = handleObj.AddComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0.5f, 0.5f);
            handleRT.anchorMax = new Vector2(0.5f, 0.5f);
            handleRT.pivot = new Vector2(0.5f, 0.5f);
            handleRT.anchoredPosition = Vector2.zero;
            handleRT.sizeDelta = new Vector2(100f, 100f);
            
            var handleImg = handleObj.AddComponent<Image>();
            handleImg.color = new Color(1f, 1f, 1f, 0.6f);
            handleImg.sprite = CreateCircleSprite();
            handleImg.type = Image.Type.Simple;
            handleImg.raycastTarget = false;

            // === Attach joystick component ===
            var joystick = baseObj.AddComponent<VirtualJoystick>();
            joystick._baseRect = baseRT;
            joystick._handleRect = handleRT;
            joystick._canvas = canvasTransform.GetComponentInParent<Canvas>();

            return joystick;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_canvas == null || _baseRect == null) return;

            Vector2 position;
            Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _baseRect, eventData.position, cam, out position))
                return;

            // Normalize to [-1, 1] range based on base size
            Vector2 sizeDelta = _baseRect.sizeDelta;
            position = new Vector2(
                position.x / (sizeDelta.x * 0.5f),
                position.y / (sizeDelta.y * 0.5f)
            );

            // Clamp to circle
            _input = position.magnitude > 1f ? position.normalized : position;

            // Apply dead zone
            if (_input.magnitude < _deadZone)
                _input = Vector2.zero;

            // Move handle visual
            if (_handleRect != null)
            {
                _handleRect.anchoredPosition = new Vector2(
                    _input.x * sizeDelta.x * 0.5f * _handleRange,
                    _input.y * sizeDelta.y * 0.5f * _handleRange
                );
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _input = Vector2.zero;
            if (_handleRect != null)
                _handleRect.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Tạo sprite hình tròn đơn giản bằng code (không cần asset file).
        /// </summary>
        private static Sprite CreateCircleSprite()
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size * 0.5f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
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

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
