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

        // Danh sách emoji có sẵn
        public static readonly string[] AvailableEmojis = new string[]
        {
            "❤️", "😂", "😮", "👋", "💪", "🔥",
            "⭐", "😴", "😡", "🎉", "👍", "😢"
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

        /// <summary>
        /// Người chơi local gọi hàm này để thả emoji.
        /// Gửi command lên server, server broadcast cho tất cả.
        /// </summary>
        public void ShowEmoji(int emojiIndex)
        {
            if (emojiIndex < 0 || emojiIndex >= AvailableEmojis.Length) return;
            CmdShowEmoji(AvailableEmojis[emojiIndex]);
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
            // Xóa emoji cũ nếu có
            DestroyEmoji();

            // Tạo emoji 3D text phía trên đầu nhân vật
            _currentEmojiObj = new GameObject("EmojiDisplay");
            _currentEmojiObj.transform.SetParent(transform, false);

            // Tính vị trí trên đầu nhân vật (dựa trên scale)
            float headHeight = 1.6f; // Chiều cao nhân vật
            _emojiBasePos = new Vector3(0, headHeight, 0);
            _currentEmojiObj.transform.localPosition = _emojiBasePos;

            // TextMeshPro 3D cho emoji
            var tmp = _currentEmojiObj.AddComponent<TextMeshPro>();
            tmp.text = emoji;
            tmp.fontSize = 36;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = false;

            // Scale nhỏ lại cho vừa 3D world
            _currentEmojiObj.transform.localScale = Vector3.one * _emojiScale;

            // Billboard — luôn quay mặt về camera
            _currentEmojiObj.AddComponent<BillboardText>();

            // Bắt đầu timer cho animation
            _emojiTimer = 0f;

            Debug.Log($"[EmojiSystem] Showing emoji: {emoji} on {gameObject.name}");
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
