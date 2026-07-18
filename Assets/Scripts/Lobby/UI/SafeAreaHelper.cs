using UnityEngine;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Điều chỉnh RectTransform của Canvas con để nằm trong Safe Area của thiết bị.
    /// Tránh UI bị cắt bởi notch, Dynamic Island, home indicator trên iPhone.
    /// Gắn vào một GameObject con trực tiếp của Canvas — tất cả UI elements
    /// nên là con của GameObject này.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaHelper : MonoBehaviour
    {
        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void Update()
        {
            // Reapply if screen size or orientation changes (rotation, split screen, etc.)
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreenSize.x ||
                Screen.height != _lastScreenSize.y ||
                Screen.orientation != _lastOrientation)
            {
                ApplySafeArea();
            }
        }

        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            _lastOrientation = Screen.orientation;

            if (Screen.width <= 0 || Screen.height <= 0) return;

            // Convert safe area from pixels to anchor coordinates (0-1)
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            Debug.Log($"[SafeAreaHelper] Applied safe area: {safeArea} → anchors ({anchorMin} - {anchorMax})");
        }

        /// <summary>
        /// Tạo SafeArea wrapper cho Canvas.
        /// Tất cả UI elements hiện tại sẽ được move vào trong wrapper.
        /// </summary>
        public static RectTransform CreateSafeAreaWrapper(Canvas canvas)
        {
            // Tạo wrapper object
            var wrapper = new GameObject("SafeArea");
            wrapper.transform.SetParent(canvas.transform, false);

            var rt = wrapper.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            wrapper.AddComponent<SafeAreaHelper>();

            // Move tất cả children hiện tại của canvas vào wrapper
            // (trừ wrapper chính nó)
            var childrenToMove = new System.Collections.Generic.List<Transform>();
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                var child = canvas.transform.GetChild(i);
                if (child.gameObject != wrapper)
                {
                    childrenToMove.Add(child);
                }
            }

            foreach (var child in childrenToMove)
            {
                child.SetParent(wrapper.transform, false);
            }

            return rt;
        }
    }
}
