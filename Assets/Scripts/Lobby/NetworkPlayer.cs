using UnityEngine;
using Mirror;

namespace RangerCity.Lobby
{
    /// <summary>
    /// NetworkPlayer — đồng bộ nhân vật qua mạng.
    /// Gắn kèm PlayerController trên cùng GameObject.
    /// Quản lý: position sync, animation sync, identity.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        [Header("Sync Settings")]
        [SerializeField] private float _syncInterval = 0.05f; // 20 updates/sec

        /// <summary>
        /// Tên hiển thị của người chơi, đồng bộ giữa tất cả clients.
        /// </summary>
        [SyncVar(hook = nameof(OnDisplayNameChanged))]
        public string DisplayName = "Player";

        /// <summary>
        /// Màu áo của nhân vật, đồng bộ giữa tất cả clients.
        /// </summary>
        [SyncVar(hook = nameof(OnBodyColorChanged))]
        public Color BodyColor = new Color(0.26f, 0.65f, 0.96f);

        // Sync vị trí mượt mà
        [SyncVar] private Vector3 _syncPosition;
        [SyncVar] private float _syncRotationY;
        [SyncVar] private bool _syncIsMoving;

        private PlayerController _playerController;
        private float _syncTimer;
        private Vector3 _smoothVelocity;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();

            // Chỉ local player mới có PlayerController active
            _playerController = GetComponent<PlayerController>();
            if (_playerController != null)
            {
                _playerController.enabled = true;
            }

            // Đặt tên ngẫu nhiên
            string[] names = { "Ranger", "Scout", "Explorer", "Hero", "Knight", "Mage", "Archer" };
            string randomName = names[Random.Range(0, names.Length)] + Random.Range(100, 999);
            CmdSetDisplayName(randomName);

            // Đặt màu ngẫu nhiên
            Color randomColor = Color.HSVToRGB(Random.value, 0.6f, 0.9f);
            CmdSetBodyColor(randomColor);

            Debug.Log($"[NetworkPlayer] Local player started: {randomName}");
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            // Remote player — tắt PlayerController, chỉ dùng sync position
            if (!isLocalPlayer)
            {
                var pc = GetComponent<PlayerController>();
                if (pc != null) pc.enabled = false;
            }
        }

        private void Update()
        {
            if (isLocalPlayer)
            {
                // Gửi vị trí lên server định kỳ
                _syncTimer += Time.deltaTime;
                if (_syncTimer >= _syncInterval)
                {
                    _syncTimer = 0f;
                    CmdSyncPosition(transform.position, transform.eulerAngles.y, IsMoving());
                }
            }
            else
            {
                // Remote player — smooth lerp tới vị trí sync
                transform.position = Vector3.SmoothDamp(
                    transform.position, _syncPosition, ref _smoothVelocity, 0.1f);

                // Cập nhật animation dựa trên sync state
                UpdateRemoteAnimation();
            }
        }

        private bool IsMoving()
        {
            // Kiểm tra qua velocity hoặc input
            if (_playerController != null)
            {
                float speed = (transform.position - _syncPosition).magnitude / _syncInterval;
                return speed > 0.1f;
            }
            return false;
        }

        private void UpdateRemoteAnimation()
        {
            // CharacterAnimator tự xử lý dựa trên velocity,
            // nên remote players sẽ tự động có animation khi vị trí thay đổi
        }

        // ── Commands (Client → Server) ──

        [Command]
        private void CmdSyncPosition(Vector3 position, float rotationY, bool isMoving)
        {
            _syncPosition = position;
            _syncRotationY = rotationY;
            _syncIsMoving = isMoving;

            // Server cũng cập nhật vị trí thực tế
            transform.position = position;
        }

        [Command]
        private void CmdSetDisplayName(string name)
        {
            DisplayName = name;
        }

        [Command]
        private void CmdSetBodyColor(Color color)
        {
            BodyColor = color;
        }

        // ── Punch sync ──

        [Command]
        public void CmdPunch(Vector3 direction)
        {
            // Broadcast punch effect to all clients
            RpcShowPunchEffect(direction);
        }

        [ClientRpc]
        private void RpcShowPunchEffect(Vector3 direction)
        {
            // Hiệu ứng punch sẽ được hiển thị cho tất cả clients
            Debug.Log($"[NetworkPlayer] {DisplayName} punched!");
        }

        // ── SyncVar Hooks ──

        private void OnDisplayNameChanged(string oldName, string newName)
        {
            // Cập nhật name tag trên đầu nhân vật
            var nameTag = transform.Find("NameTag");
            if (nameTag != null)
            {
                var tmp = nameTag.GetComponentInChildren<TMPro.TextMeshPro>();
                if (tmp != null) tmp.text = newName;
            }
            gameObject.name = newName;
            Debug.Log($"[NetworkPlayer] Name changed: {oldName} -> {newName}");
        }

        private void OnBodyColorChanged(Color oldColor, Color newColor)
        {
            // Cập nhật màu áo
            var body = transform.Find("Body");
            if (body != null)
            {
                var renderer = body.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = newColor;
                }
            }
            Debug.Log($"[NetworkPlayer] Color changed for {DisplayName}");
        }
    }
}
