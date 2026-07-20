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
    public class LobbySetup : MonoBehaviour
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

        private void CreateNPCs()
        {
            CreateNPC("Chief Rosa", "👩‍✈️", new Vector3(-5, 0.03f, 4), new Color(0.81f, 0.58f, 0.86f), new[] {
                "Chào mừng Junior Ranger! 🌟",
                "Tôi là Chief Rosa, người hướng dẫn.",
                "Hãy khám phá sảnh chờ nhé! 💪",
            });

            CreateNPC("Sr. Stoplight", "🚦", new Vector3(5, 0.03f, 4), new Color(0.4f, 0.73f, 0.42f), new[] {
                "Xin chào! Tôi là Señor Stoplight! 🚦",
                "Đèn đỏ = DỪNG, đèn xanh = ĐI! 🔴🟢",
                "Kiên nhẫn là chìa khóa! ⏰",
            });

            CreateNPC("Milo", "🧑‍🍳", new Vector3(-3, 0.03f, -6), new Color(1f, 0.72f, 0.3f), new[] {
                "Chào! Tôi là Milo, chủ cửa hàng! 🛒",
                "Quay lại khi có Ranger Coins nhé! 💰",
            });

            CreateNPC("Zhang Guang Yu", "🦨", new Vector3(-6, 0.03f, -2), new Color(0.12f, 0.12f, 0.12f), new[] {
                "Chào bồ! Tôi là Zhang Guang Yu đây!",
                "Đừng đứng gần tôi quá, tôi chưa tắm 3 tuần rồi...",
                "Người ta bảo tôi đen hôi, nhưng tôi thấy mình men lỳ!"
            }, new Color(0.25f, 0.18f, 0.12f));

            CreateNPC("Yan Min Sheng", "😷", new Vector3(6f, 0.03f, -61.2f), new Color(0.15f, 0.1f, 0.08f), new[] {
                "Xin lỗi Xiao Ling từ nay tui không dám giao việc linh tinh cho Xiao Ling nữa huhuhu"
            }, new Color(0.2f, 0.14f, 0.09f), wanderRadius: 0.5f);

            CreateNPC("Tang Xu Yu", "⛓️", new Vector3(-6, 0.03f, -61.2f), new Color(0.2f, 0.4f, 0.7f), new[] {
                "Tôi bị nhốt rồi, tôi sai rồi...",
                "Xin lỗi Xiao Ling từ nay tui không dám giao việc linh tinh cho Xiao Ling nữa huhuhu.",
                "Tôi sẵn sàng làm trâu ngựa để chuộc lỗi lầm của tôi!"
            }, new Color(1f, 0.8f, 0.65f), wanderRadius: 0.5f);
        }

        private void CreateNPC(string name, string emoji, Vector3 pos, Color bodyColor, string[] dialogues, Color? skinColor = null, float wanderRadius = 3f)
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                var npcHeadless = new GameObject(name);
                npcHeadless.transform.position = new Vector3(pos.x, 0.03f, pos.z);
                var ctrlHeadless = npcHeadless.AddComponent<NPCController>();
                SetField(ctrlHeadless, "_displayName", name);
                SetField(ctrlHeadless, "_avatarEmoji", emoji);
                SetField(ctrlHeadless, "_dialogueLines", dialogues);
                SetField(ctrlHeadless, "_moveSpeed", 1.2f);
                SetField(ctrlHeadless, "_wanderPauseMin", 1f);
                SetField(ctrlHeadless, "_wanderPauseMax", 3f);
                SetField(ctrlHeadless, "_wanderRadius", wanderRadius);
                return;
            }
            Color skinCol = skinColor ?? new Color(1f, 0.88f, 0.7f);
            GameObject npc;
            
            int gender = name == "Chief Rosa" ? 1 : 0;
            int hairStyle = 0;
            int outfitStyle = 0;
            int pantsStyle = 0;

            if (_humanMalePrefab != null || _humanFemalePrefab != null)
            {
                npc = CreateCharacterContainer(name);
                if (npc.GetComponent<CharacterAnimator>() == null)
                {
                    npc.AddComponent<CharacterAnimator>();
                }
            }
            else
            {
                npc = CharacterVisuals.CreateCharacterTopDown(name, bodyColor, skinCol);
            }

            CharacterVisuals.ApplyCustomization(npc, gender, hairStyle, new Color(0.18f, 0.12f, 0.08f), outfitStyle, bodyColor, pantsStyle, new Color(0.25f, 0.35f, 0.55f));
            npc.transform.position = new Vector3(pos.x, 0.03f, pos.z);

            var ctrl = npc.AddComponent<NPCController>();
            SetField(ctrl, "_displayName", name);
            SetField(ctrl, "_avatarEmoji", emoji);
            SetField(ctrl, "_dialogueLines", dialogues);
            SetField(ctrl, "_moveSpeed", 1.2f);
            SetField(ctrl, "_wanderPauseMin", 1f);
            SetField(ctrl, "_wanderPauseMax", 3f);
            SetField(ctrl, "_wanderRadius", wanderRadius);

            var swollen = new GameObject("SwollenFace");
            swollen.transform.SetParent(npc.transform, false);
            swollen.transform.localPosition = new Vector3(0, 1.2f, 0.15f);

            var leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftCheek.name = "LeftCheek";
            leftCheek.transform.SetParent(swollen.transform, false);
            leftCheek.transform.localPosition = new Vector3(-0.18f, 0, 0.08f);
            leftCheek.transform.localScale = new Vector3(0.25f, 0.2f, 0.15f);
            leftCheek.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1f, 0.2f, 0.2f));
            Destroy(leftCheek.GetComponent<Collider>());

            var rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightCheek.name = "RightCheek";
            rightCheek.transform.SetParent(swollen.transform, false);
            rightCheek.transform.localPosition = new Vector3(0.2f, -0.03f, 0.1f);
            rightCheek.transform.localScale = new Vector3(0.3f, 0.25f, 0.18f);
            rightCheek.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.95f, 0.15f, 0.15f));
            Destroy(rightCheek.GetComponent<Collider>());

            var bump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bump.name = "HeadBump";
            bump.transform.SetParent(swollen.transform, false);
            bump.transform.localPosition = new Vector3(0.05f, 0.25f, 0);
            bump.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
            bump.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.4f, 0.5f));
            Destroy(bump.GetComponent<Collider>());

            var stars = new GameObject("PunchStars");
            stars.transform.SetParent(npc.transform, false);
            stars.transform.localPosition = new Vector3(0, 1.7f, 0);

            Color[] starColors = { new(1f, 0.84f, 0f), new(1f, 0.4f, 0.4f), new(0.3f, 0.85f, 1f), new(0.4f, 1f, 0.4f), new(1f, 0.65f, 0f) };
            for (int i = 0; i < 5; i++)
            {
                var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                star.name = $"Star_{i}";
                star.transform.SetParent(stars.transform, false);
                star.transform.localScale = Vector3.one * 0.1f;
                star.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(starColors[i % starColors.Length]);
                Destroy(star.GetComponent<Collider>());
            }
            stars.AddComponent<SwollenFaceEffect>();

            SetField(ctrl, "_swollenFaceEffect", swollen);
            SetField(ctrl, "_punchStarsEffect", stars);
            swollen.SetActive(false);
            stars.SetActive(false);

            AddNameTag(npc, name, new Color(0.95f, 0.85f, 0.5f));

            if (name == "Tang Xu Yu" || name == "Zhang Guang Yu" || name == "Yan Min Sheng")
            {
                var bubbleObj = new GameObject("DialogueBubble");
                bubbleObj.transform.SetParent(npc.transform, false);
                bubbleObj.transform.localPosition = new Vector3(0, 2.7f, 0);
                
                var bubble = bubbleObj.AddComponent<FloatingDialogueBubble>();
                bubble.Setup(dialogues, 4.0f);
            }
        }

        private void CreateFakePlayers()
        {
            var data = new (string name, Vector3 pos, Color color, int gender, string[] greetings)[] {
                ("Luna", new(4, 0, -2), new(0.94f, 0.33f, 0.31f), 1, new[] { "Chào! Mình là Luna! 🌙", "Sảnh này vui lắm! 🎉" }),
                ("Kai", new(-4, 0, 0), new(0.4f, 0.73f, 0.42f), 0, new[] { "Yo! 🏃", "Đừng đấm mình nha! 😅" }),
                ("Sakura", new(7, 0, -5), new(0.49f, 0.34f, 0.76f), 1, new[] { "Konnichiwa! 🌸", "Mình thích sảnh này! 🎨" }),
                ("Tí", new(-7, 0, -5), new(1f, 0.54f, 0.4f), 0, new[] { "Ê bạn! 👋", "Tìm được bí mật chưa? 🕵️" }),
                ("Mochi", new(0, 0, 8), new(1f, 0.65f, 0.15f), 1, new[] { "Zzz... mình đang nghỉ! 😴", "Ồ xin lỗi! 😊" }),
                ("Rex", new(8, 0, 2), new(0.36f, 0.42f, 0.75f), 0, new[] { "Hey! Bạn cũng Ranger hả? 💪", "Đã xong 50 nhiệm vụ! 🏆" }),
            };

            foreach (var (fpName, pos, color, gender, greetings) in data)
            {
                if (NetworkSetup.IsHeadlessServer())
                {
                    var fpHeadless = new GameObject(fpName);
                    fpHeadless.transform.position = new Vector3(pos.x, 0.03f, pos.z);
                    var ctrlHeadless = fpHeadless.AddComponent<FakePlayerController>();
                    SetField(ctrlHeadless, "_displayName", fpName);
                    SetField(ctrlHeadless, "_greetings", greetings);
                    SetField(ctrlHeadless, "_moveSpeed", 1.2f);
                    SetField(ctrlHeadless, "_pauseMin", 1f);
                    SetField(ctrlHeadless, "_pauseMax", 3f);
                    SetField(ctrlHeadless, "_wanderRadius", 4f);
                    continue;
                }
                GameObject fp;
                if (_humanMalePrefab != null || _humanFemalePrefab != null)
                {
                    fp = CreateCharacterContainer(fpName);
                    if (fp.GetComponent<CharacterAnimator>() == null)
                    {
                        fp.AddComponent<CharacterAnimator>();
                    }
                    CharacterVisuals.ApplyCustomization(fp, gender, 0, new Color(0.18f, 0.12f, 0.08f), 0, color, 0, new Color(0.25f, 0.35f, 0.55f));
                }
                else
                {
                    fp = CharacterVisuals.CreateCharacterTopDown(fpName, color, new Color(1f, 0.88f, 0.7f));
                    CharacterVisuals.ApplyCustomization(fp, gender, 0, new Color(0.18f, 0.12f, 0.08f), 0, color, 0, new Color(0.25f, 0.35f, 0.55f));
                }
                fp.transform.position = new Vector3(pos.x, 0.03f, pos.z);

                var ctrl = fp.AddComponent<FakePlayerController>();
                SetField(ctrl, "_displayName", fpName);
                SetField(ctrl, "_greetings", greetings);
                SetField(ctrl, "_moveSpeed", 1.2f);
                SetField(ctrl, "_pauseMin", 1f);
                SetField(ctrl, "_pauseMax", 3f);
                SetField(ctrl, "_wanderRadius", 4f);

                var swollen = new GameObject("SwollenFace");
                swollen.transform.SetParent(fp.transform, false);
                swollen.transform.localPosition = new Vector3(0, 1.2f, 0.15f);

                var leftCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leftCheek.name = "LeftCheek";
                leftCheek.transform.SetParent(swollen.transform, false);
                leftCheek.transform.localPosition = new Vector3(-0.18f, 0, 0.08f);
                leftCheek.transform.localScale = new Vector3(0.25f, 0.2f, 0.15f);
                leftCheek.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(1f, 0.2f, 0.2f));
                Destroy(leftCheek.GetComponent<Collider>());

                var rightCheek = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rightCheek.name = "RightCheek";
                rightCheek.transform.SetParent(swollen.transform, false);
                rightCheek.transform.localPosition = new Vector3(0.2f, -0.03f, 0.1f);
                rightCheek.transform.localScale = new Vector3(0.3f, 0.25f, 0.18f);
                rightCheek.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.95f, 0.15f, 0.15f));
                Destroy(rightCheek.GetComponent<Collider>());

                var bump = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                bump.name = "HeadBump";
                bump.transform.SetParent(swollen.transform, false);
                bump.transform.localPosition = new Vector3(0.05f, 0.25f, 0);
                bump.transform.localScale = new Vector3(0.18f, 0.22f, 0.18f);
                bump.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(new Color(0.9f, 0.4f, 0.5f));
                Destroy(bump.GetComponent<Collider>());

                var stars = new GameObject("PunchStars");
                stars.transform.SetParent(fp.transform, false);
                stars.transform.localPosition = new Vector3(0, 1.7f, 0);

                Color[] starColors = { new(1f, 0.84f, 0f), new(1f, 0.4f, 0.4f), new(0.3f, 0.85f, 1f), new(0.4f, 1f, 0.4f), new(1f, 0.65f, 0f) };
                for (int i = 0; i < 5; i++)
                {
                    var star = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    star.name = $"Star_{i}";
                    star.transform.SetParent(stars.transform, false);
                    star.transform.localScale = Vector3.one * 0.1f;
                    star.GetComponent<Renderer>().material = CharacterVisuals.CreateMat(starColors[i % starColors.Length]);
                    Destroy(star.GetComponent<Collider>());
                }
                stars.AddComponent<SwollenFaceEffect>();

                SetField(ctrl, "_swollenFaceEffect", swollen);
                SetField(ctrl, "_punchStarsEffect", stars);
                swollen.SetActive(false);
                stars.SetActive(false);

                AddNameTag(fp, fpName, new Color(0.55f, 0.8f, 1f));
            }
        }

        private void CreateCamera(Transform player)
        {
            GameObject camObj;
            Camera cam;

            if (Camera.main != null)
            {
                camObj = Camera.main.gameObject;
                cam = Camera.main;
            }
            else
            {
                camObj = new GameObject("LobbyCamera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                if (camObj.GetComponent<AudioListener>() == null)
                    camObj.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.4f, 0.7f, 0.25f);
            cam.orthographic = true;
            cam.orthographicSize = 3.2f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            float angle = 35f;
            float distance = 20f;
            float radAngle = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(0, distance * Mathf.Sin(radAngle), -distance * Mathf.Cos(radAngle));
            camObj.transform.position = player.position + offset;
            camObj.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            var lobbyCamera = camObj.GetComponent<LobbyCamera>();
            if (lobbyCamera == null)
                lobbyCamera = camObj.AddComponent<LobbyCamera>();
            lobbyCamera.SetTarget(player);
        }

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

            var talkBtnObj = CreateUIButton("TalkButton", panel.transform, new Vector2(9999f, 9999f), "DummyTalk", Color.clear, Vector2.one);
            talkBtnObj.SetActive(false);

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

        private GameObject CreateUIButton(string name, Transform parent, Vector2 pos, string label, Color bgColor, Vector2 size)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rt = btnObj.AddComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;

            btnObj.AddComponent<Image>().color = bgColor;
            btnObj.AddComponent<Button>();

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
                Debug.Log(report);
                yield return new WaitForSeconds(3.0f);
            }
        }
    }
}
