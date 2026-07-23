using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using System.Collections;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Coordinates waiting lobby initialization (2D top-down style).
    /// </summary>
    public partial class LobbySetup : MonoBehaviour
    {
        [Header("Lobby Settings")]
        private float _lobbySize = 30f;

        [Header("Colors")]
        [SerializeField] private Color _grassColor = new Color(0.45f, 0.78f, 0.22f);
        [SerializeField] private Color _pathColor = new Color(0.92f, 0.87f, 0.78f);

        [Header("Asset Prefabs")]
        [SerializeField] private GameObject _treePrefab;
        [SerializeField] private GameObject _flowerPrefab;
        [SerializeField] private GameObject _humanMalePrefab;
        [SerializeField] private GameObject _humanFemalePrefab;
        [SerializeField] private GameObject _gardenPrefab;

        private void Awake()
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                Debug.Log("[LobbySetup] Headless mode detected. Disabling all cameras and canvases in the scene...");
                foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
                {
                    c.enabled = false;
                }
                foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                {
                    canvas.gameObject.SetActive(false);
                }
            }

            var defaultLight = FindAnyObjectByType<Light>();
            if (defaultLight != null) Destroy(defaultLight.gameObject);

            // Only enable debug scene player logging in development builds
            if (Debug.isDebugBuild)
            {
                gameObject.AddComponent<DebugScenePlayers>();
            }

            CreateEnvironment();
            CreateLighting();
            var player = CreatePlayer();

            CreateNPCs();
            CreateFakePlayers();
            if (!NetworkSetup.IsHeadlessServer())
            {
                CreateCamera(player.transform);
                CreateUI();
            }
            else
            {
                // Disable all renderers in the environment created during setup
                foreach (var r in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    r.enabled = false;
                }
            }

            Debug.Log("🎮 Ranger City Lobby (2D Top-Down) loaded!");
        }

        private void Start()
        {
            Debug.Log("[LobbySetup] Start began");
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Debug.Log("[LobbySetup] Player found");
                // Tải tùy chọn đã lưu từ PlayerPrefs và áp dụng ngay lập tức cho nhân vật local
                int gender = PlayerPrefs.GetInt("PlayerGender", 0);
                int bodyColorIdx = PlayerPrefs.GetInt("PlayerColorIndex", 0);
                Color bodyColor = (bodyColorIdx >= 0 && bodyColorIdx < NetworkPlayer.BodyColorPalette.Length)
                    ? NetworkPlayer.BodyColorPalette[bodyColorIdx]
                    : new Color(0.26f, 0.65f, 0.96f);

                int hairStyleIdx = PlayerPrefs.GetInt("PlayerHairStyle", 0);
                int hairColorIdx = PlayerPrefs.GetInt("PlayerHairColor", 0);
                Color hairColor = (hairColorIdx >= 0 && hairColorIdx < NetworkPlayer.HairColorPalette.Length)
                    ? NetworkPlayer.HairColorPalette[hairColorIdx]
                    : NetworkPlayer.HairColorPalette[0];

                int outfitIdx = PlayerPrefs.GetInt("PlayerOutfitStyle", 0);

                int pantsStyleIdx = PlayerPrefs.GetInt("PlayerPantsStyle", 0);
                int pantsColorIdx = PlayerPrefs.GetInt("PlayerPantsColor", 0);
                Color pantsColor = (pantsColorIdx >= 0 && pantsColorIdx < NetworkPlayer.PantsColorPalette.Length)
                    ? NetworkPlayer.PantsColorPalette[pantsColorIdx]
                    : NetworkPlayer.PantsColorPalette[0];

                Debug.Log("[LobbySetup] Applying customization");
                CharacterVisuals.ApplyCustomization(player, gender, hairStyleIdx, hairColor, outfitIdx, bodyColor, pantsStyleIdx, pantsColor);

                // Load saved player name and apply to name tag
                string savedName = PlayerPrefs.GetString("PlayerName", "");
                if (string.IsNullOrEmpty(savedName))
                {
                    savedName = "Player";
                }
                var nameTag = player.transform.Find("NameTag");
                if (nameTag != null)
                {
                    var tmp = nameTag.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (tmp != null) tmp.text = savedName;
                }

                Debug.Log("[LobbySetup] Setting up networking");
                SetupNetworking(player);
                Debug.Log("🌐 Multiplayer: Mirror networking enabled!");
            }
            Debug.Log("[LobbySetup] Start finished");
        }

        private void SetupNetworking(GameObject player)
        {
            // 1. Create a hidden templates parent to hold our active prefabs safely
            GameObject templatesParent = new GameObject("NetworkTemplates");
            templatesParent.SetActive(false);
            DontDestroyOnLoad(templatesParent);

            // 2. Clone the player to create the prefab template
            GameObject playerPrefab = Instantiate(player);
            playerPrefab.name = "PlayerPrefab";
            playerPrefab.transform.SetParent(templatesParent.transform);
            playerPrefab.SetActive(true); // Keep it active internally so clones are spawned active

            var identity = playerPrefab.GetComponent<NetworkIdentity>();
            if (identity == null)
                identity = playerPrefab.AddComponent<NetworkIdentity>();

            if (identity.assetId == 0)
            {
                var field = typeof(NetworkIdentity).GetField("_assetId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(identity, (uint)424242);
                }
            }

            if (playerPrefab.GetComponent<NetworkPlayer>() == null)
                playerPrefab.AddComponent<NetworkPlayer>();

            if (playerPrefab.GetComponent<EmojiSystem>() == null)
                playerPrefab.AddComponent<EmojiSystem>();

            // 3. Register the prefab to the network setup
            var networkSetup = gameObject.AddComponent<NetworkSetup>();
            networkSetup.RegisterPlayerPrefab(playerPrefab);

            // 4. Destroy the original scene player since Mirror will spawn the correct networked player
            Destroy(player);

            // 5. Start the server/host or client based on command line arguments
            string[] args = System.Environment.GetCommandLineArgs();
            bool isClient = false;
            string connectAddr = "127.0.0.1";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-client")
                {
                    isClient = true;
                }
                else if (args[i] == "-connect" && i + 1 < args.Length)
                {
                    connectAddr = args[i + 1];
                }
            }

            if (isClient)
            {
                networkSetup.StartAsClient(connectAddr);
            }
            else if (NetworkSetup.IsHeadlessServer())
            {
                networkSetup.StartAsServer();
            }
            else
            {
                networkSetup.StartAsHost();
            }
        }

        private void CreateEnvironment()
        {
            EnvironmentBuilder.CreateFlat("Ground", Vector3.zero, new Vector2(_lobbySize, _lobbySize), _grassColor);

            float pathLen = _lobbySize * 0.8f;
            EnvironmentBuilder.CreateFlat("Path_H", new Vector3(0, 0.01f, 0), new Vector2(pathLen, 2f), _pathColor);
            EnvironmentBuilder.CreateFlat("Path_V", new Vector3(0, 0.01f, 0), new Vector2(2f, pathLen), _pathColor);

            Color curbColor = new Color(0.35f, 0.35f, 0.35f);
            EnvironmentBuilder.CreateFlat("Curb_H_Top", new Vector3(0, 0.012f, 1.05f), new Vector2(pathLen, 0.1f), curbColor);
            EnvironmentBuilder.CreateFlat("Curb_H_Bot", new Vector3(0, 0.012f, -1.05f), new Vector2(pathLen, 0.1f), curbColor);
            EnvironmentBuilder.CreateFlat("Curb_V_Left", new Vector3(-1.05f, 0.012f, 0), new Vector2(0.1f, pathLen), curbColor);
            EnvironmentBuilder.CreateFlat("Curb_V_Right", new Vector3(1.05f, 0.012f, 0), new Vector2(0.1f, pathLen), curbColor);

            EnvironmentBuilder.CreateCircle("Plaza", new Vector3(0, 0.02f, 0), 4f, new Color(0.78f, 0.74f, 0.72f));

            EnvironmentBuilder.CreateTrees3D(_treePrefab);
            EnvironmentBuilder.CreateFlowers(_flowerPrefab);
            EnvironmentBuilder.CreateFountain3D();
            EnvironmentBuilder.CreateBenches3D();
            EnvironmentBuilder.CreateFences(_lobbySize);
            EnvironmentBuilder.CreateBuildings();
            EnvironmentBuilder.CreateStreetLamps();
            EnvironmentBuilder.CreatePortals(_lobbySize);

            // Construct zones (delegated to builders)
            GardenZoneBuilder.Build(_treePrefab, _flowerPrefab);
            PrisonZoneBuilder.Build();
            FishingZoneBuilder.Build();
            StudyZoneBuilder.Build();
        }

        private void CreateLighting()
        {
            if (NetworkSetup.IsHeadlessServer()) return;
            var sunObj = new GameObject("Sun");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.98f, 0.92f);
            sun.intensity = 0.8f;
            sun.shadows = LightShadows.None;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.35f, 0.37f, 0.4f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = false;
        }

        private GameObject CreateCharacterContainer(string name)
        {
            GameObject container = new GameObject(name);
            if (_humanMalePrefab != null)
            {
                var male = Instantiate(_humanMalePrefab, container.transform);
                male.name = "MaleModel";
                male.transform.localPosition = Vector3.zero;
                male.transform.localRotation = Quaternion.identity;
            }
            if (_humanFemalePrefab != null)
            {
                var female = Instantiate(_humanFemalePrefab, container.transform);
                female.name = "FemaleModel";
                female.transform.localPosition = Vector3.zero;
                female.transform.localRotation = Quaternion.identity;
            }
            return container;
        }

        private GameObject CreatePlayer()
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                var headlessPlayer = new GameObject("Player");
                headlessPlayer.transform.position = new Vector3(0, 0.03f, -3);
                headlessPlayer.AddComponent<PlayerController>();
                headlessPlayer.tag = "Player";
                return headlessPlayer;
            }

            GameObject player;
            if (_humanMalePrefab != null || _humanFemalePrefab != null)
            {
                player = CreateCharacterContainer("Player");
            }
            else
            {
                player = CharacterVisuals.CreateCharacterTopDown("Player", new Color(0.26f, 0.65f, 0.96f), new Color(1f, 0.88f, 0.7f));
            }
            player.transform.position = new Vector3(0, 0.03f, -3);
            player.AddComponent<PlayerController>();
            player.tag = "Player";

            // Add NameTag for multiplayer name display
            AddNameTag(player, "Player", new Color(0.3f, 0.8f, 1f));

            return player;
        }


}
}
