using UnityEngine;
using Mirror;
using kcp2k;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Tự động thiết lập Mirror NetworkManager + KCP Transport.
    /// Chạy ở chế độ Host trên server Linux, Client trên máy người chơi.
    /// </summary>
    public class NetworkSetup : MonoBehaviour
    {
        [Header("Network Settings")]
        [SerializeField] private int _port = 7777;
        [SerializeField] private int _maxConnections = 20;
        [SerializeField] private string _serverAddress = "100.89.39.103";
        [SerializeField] private bool _autoStartHost = true;

        private NetworkManager _networkManager;

        private void Awake()
        {
            // Kiểm tra nếu đã có NetworkManager
            if (NetworkManager.singleton != null)
            {
                Debug.Log("[NetworkSetup] NetworkManager already exists, skipping.");
                return;
            }

            SetupNetworkManager();
        }

        private void SetupNetworkManager()
        {
            // Thêm KCP Transport (UDP-based, nhanh và đáng tin cậy)
            var transport = gameObject.AddComponent<KcpTransport>();
            transport.port = (ushort)_port;

            // Thêm NetworkManager
            _networkManager = gameObject.AddComponent<NetworkManager>();
            _networkManager.transport = transport;
            _networkManager.maxConnections = _maxConnections;

            // Đăng ký player prefab sẽ được set sau khi LobbySetup tạo xong
            Debug.Log($"[NetworkSetup] Mirror initialized on port {_port}, max {_maxConnections} connections");
        }

        /// <summary>
        /// Đăng ký player prefab cho NetworkManager spawn.
        /// Gọi bởi LobbySetup sau khi tạo xong player prefab.
        /// </summary>
        public void RegisterPlayerPrefab(GameObject prefab)
        {
            if (_networkManager == null)
                _networkManager = NetworkManager.singleton;

            if (_networkManager != null)
            {
                _networkManager.playerPrefab = prefab;
                Debug.Log($"[NetworkSetup] Player prefab registered: {prefab.name}");
            }
        }

        /// <summary>
        /// Bắt đầu chạy ở chế độ Host (Server + Client).
        /// Gọi sau khi mọi thứ đã sẵn sàng.
        /// </summary>
        public void StartAsHost()
        {
            if (_networkManager == null)
                _networkManager = NetworkManager.singleton;

            if (_networkManager != null && !NetworkServer.active)
            {
                _networkManager.StartHost();
                Debug.Log("[NetworkSetup] Started as HOST");
            }
        }

        /// <summary>
        /// Kết nối vào server ở chế độ Client.
        /// </summary>
        public void StartAsClient(string address = null)
        {
            if (_networkManager == null)
                _networkManager = NetworkManager.singleton;

            if (_networkManager != null && !NetworkClient.active)
            {
                _networkManager.networkAddress = address ?? _serverAddress;
                _networkManager.StartClient();
                Debug.Log($"[NetworkSetup] Connecting to {_networkManager.networkAddress}:{_port}");
            }
        }

        /// <summary>
        /// Dừng kết nối.
        /// </summary>
        public void Stop()
        {
            if (_networkManager == null) return;

            if (NetworkServer.active && NetworkClient.isConnected)
                _networkManager.StopHost();
            else if (NetworkClient.isConnected)
                _networkManager.StopClient();
            else if (NetworkServer.active)
                _networkManager.StopServer();
        }

        /// <summary>
        /// Kiểm tra server có đang chạy headless/batch mode không.
        /// </summary>
        public static bool IsHeadlessServer()
        {
            return SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null
                || Application.isBatchMode;
        }
    }
}
