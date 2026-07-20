using UnityEngine;
using Mirror;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Hệ thống emoji — cho phép người chơi thả biểu tượng cảm xúc 
    /// hiển thị trên đầu nhân vật, mọi người đều nhìn thấy.
    /// Gắn vào Player GameObject (có NetworkIdentity).
    /// </summary>
    public class EmojiSystem : NetworkBehaviour
    {
        [Header("Emoji Settings")]
        [SerializeField] private float _emojiDuration = 3f;
        [SerializeField] private float _emojiFloatSpeed = 0.3f;
        [SerializeField] private float _emojiScale = 0.008f;

        // Danh sách emoji có sẵn (được hỗ trợ bởi EmojiOne của TextMesh Pro)
        // Sử dụng thẻ <sprite> để TextMesh Pro tự vẽ các icon có màu chuẩn mà không cần font hệ thống
        public static readonly string[] AvailableEmojis = new string[]
        {
            "<sprite=\"EmojiOne\" name=\"Grinning face\">",                  // 😀
            "<sprite=\"EmojiOne\" name=\"1f601\">",                          // 😁
            "<sprite=\"EmojiOne\" name=\"Face with tears of joy\">",         // 😂
            "<sprite=\"EmojiOne\" name=\"1f603\">",                          // 😃
            "<sprite=\"EmojiOne\" name=\"1f604\">",                          // 😄
            "<sprite=\"EmojiOne\" name=\"1f605\">",                          // 😅
            "<sprite=\"EmojiOne\" name=\"1f606\">",                          // 😆
            "<sprite=\"EmojiOne\" name=\"1f609\">",                          // 😉
            "<sprite=\"EmojiOne\" name=\"Smiling face with smiling eyes\">", // 😊
            "<sprite=\"EmojiOne\" name=\"1f60b\">",                          // 😋
            "<sprite=\"EmojiOne\" name=\"1f60d\">",                          // 😍
            "<sprite=\"EmojiOne\" name=\"1f60e\">",                          // 😎
            "<sprite=\"EmojiOne\" name=\"1f618\">",                          // 😘
            "<sprite=\"EmojiOne\" name=\"1f923\">",                          // 🤣
            "<sprite=\"EmojiOne\" name=\"263a\">",                           // ☺
            "<sprite=\"EmojiOne\" name=\"2639\">"                            // ☹
        };

        // Emoji hiển thị hiện tại
        private GameObject _currentEmojiObj;
        private float _emojiTimer;
        private Vector3 _emojiBasePos;

        /// <summary>
        /// SyncVar — khi giá trị thay đổi, tất cả client sẽ nhận được
        /// và gọi OnEmojiChanged để hiển thị emoji mới.
        /// </summary>
        [SyncVar(hook = nameof(OnEmojiChanged))]
        private string _currentEmoji = "";

        public void ShowEmoji(int emojiIndex)
        {
            if (emojiIndex < 0 || emojiIndex >= AvailableEmojis.Length) return;

            string emoji = AvailableEmojis[emojiIndex];
            if (NetworkClient.active)
            {
                CmdShowEmoji(emoji);
            }
            else
            {
                // Fallback offline
                SpawnEmojiVisual(emoji);
                CancelInvoke(nameof(LocalClearEmoji));
                Invoke(nameof(LocalClearEmoji), _emojiDuration);
            }
        }

        private void LocalClearEmoji()
        {
            DestroyEmoji();
        }

        /// <summary>
        /// Command gửi từ client lên server.
        /// Server cập nhật SyncVar → tất cả client nhận được.
        /// </summary>
        [Command]
        private void CmdShowEmoji(string emoji)
        {
            _currentEmoji = emoji;

            // Reset emoji sau _emojiDuration giây
            // (gọi trên server, SyncVar sẽ tự broadcast)
            Invoke(nameof(ServerClearEmoji), _emojiDuration);
        }

        private void ServerClearEmoji()
        {
            _currentEmoji = "";
        }

        /// <summary>
        /// Hook được gọi trên TẤT CẢ client khi SyncVar thay đổi.
        /// </summary>
        private void OnEmojiChanged(string oldEmoji, string newEmoji)
        {
            if (string.IsNullOrEmpty(newEmoji))
            {
                DestroyEmoji();
            }
            else
            {
                SpawnEmojiVisual(newEmoji);
            }
        }

        private void SpawnEmojiVisual(string emoji)
        {
            DestroyEmoji();

            _currentEmojiObj = new GameObject("EmojiVisual");
            _currentEmojiObj.transform.SetParent(transform, false);

            float headHeight = 2.4f;
            _emojiBasePos = new Vector3(0, headHeight, 0);
            _currentEmojiObj.transform.localPosition = _emojiBasePos;

            // TextMeshPro 3D cho emoji (Tự động map sang EmojiOne sprite)
            var tmp = _currentEmojiObj.AddComponent<TextMeshPro>();
            tmp.text = emoji;
            tmp.fontSize = 6;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;

            // Scale to match parent (fully readable)
            _currentEmojiObj.transform.localScale = Vector3.one;

            // Billboard — luôn quay mặt về camera
            _currentEmojiObj.AddComponent<BillboardText>();

            // Bắt đầu timer cho animation
            _emojiTimer = 0f;

            Debug.Log($"[EmojiSystem] Showing actual emoji: {emoji} on {gameObject.name}");
        }

        private void DestroyEmoji()
        {
            if (_currentEmojiObj != null)
            {
                Destroy(_currentEmojiObj);
                _currentEmojiObj = null;
            }
        }

        private void Update()
        {
            // Animation: emoji nhẹ nhàng bay lên trên
            if (_currentEmojiObj != null)
            {
                _emojiTimer += Time.deltaTime;

                // Bay lên nhẹ nhàng
                float floatOffset = Mathf.Sin(_emojiTimer * 2f) * 0.05f;
                _currentEmojiObj.transform.localPosition = _emojiBasePos + new Vector3(0, floatOffset + _emojiTimer * _emojiFloatSpeed * 0.1f, 0);

                // Fade out trong 0.5 giây cuối
                float remaining = _emojiDuration - _emojiTimer;
                if (remaining < 0.5f && remaining > 0f)
                {
                    var tmp = _currentEmojiObj.GetComponent<TextMeshPro>();
                    if (tmp != null)
                    {
                        Color c = tmp.color;
                        c.a = remaining / 0.5f;
                        tmp.color = c;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            DestroyEmoji();
        }
    }
}
