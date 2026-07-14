using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

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

        private void Awake()
        {
            var defaultLight = FindAnyObjectByType<Light>();
            if (defaultLight != null) Destroy(defaultLight.gameObject);

            CreateEnvironment();
            CreateLighting();
            var player = CreatePlayer();
            CreateNPCs();
            CreateFakePlayers();
            CreateCamera(player.transform);
            CreateUI();

            Debug.Log("🎮 Ranger City Lobby (2D Top-Down) loaded!");
        }

        private void Start()
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                SetupNetworking(player);
                Debug.Log("🌐 Multiplayer: Mirror networking enabled!");
            }
        }

        private void SetupNetworking(GameObject player)
        {
            var networkSetup = gameObject.AddComponent<NetworkSetup>();

            if (player.GetComponent<NetworkIdentity>() == null)
                player.AddComponent<NetworkIdentity>();

            if (player.GetComponent<NetworkPlayer>() == null)
                player.AddComponent<NetworkPlayer>();

            if (player.GetComponent<EmojiSystem>() == null)
                player.AddComponent<EmojiSystem>();

            networkSetup.RegisterPlayerPrefab(player);
            if (NetworkSetup.IsHeadlessServer())
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

            EnvironmentBuilder.CreateTrees3D();
            EnvironmentBuilder.CreateFlowers();
            EnvironmentBuilder.CreateFountain3D();
            EnvironmentBuilder.CreateBenches3D();
            EnvironmentBuilder.CreateFences(_lobbySize);
            EnvironmentBuilder.CreateBuildings();
            EnvironmentBuilder.CreateStreetLamps();
            EnvironmentBuilder.CreatePortals(_lobbySize);

            // Construct zones (delegated to builders)
            GardenZoneBuilder.Build();
            PrisonZoneBuilder.Build();
            FishingZoneBuilder.Build();
            StudyZoneBuilder.Build();
        }

        private void CreateLighting()
        {
            var sunObj = new GameObject("Sun");
            var sun = sunObj.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = new Color(1f, 0.98f, 0.92f);
            sun.intensity = 1.0f;
            sun.shadows = LightShadows.None;
            sunObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            RenderSettings.ambientLight = new Color(0.9f, 0.92f, 0.95f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.fog = false;
        }

        private GameObject CreatePlayer()
        {
            var player = CharacterVisuals.CreateCharacterTopDown("Player", new Color(0.26f, 0.65f, 0.96f), new Color(1f, 0.88f, 0.7f));
            player.transform.position = new Vector3(0, 0, -3);
            player.AddComponent<PlayerController>();
            player.tag = "Player";
            return player;
        }

        private void CreateNPCs()
        {
            CreateNPC("Chief Rosa", "👩‍✈️", new Vector3(-5, 0, 4), new Color(0.81f, 0.58f, 0.86f), new[] {
                "Chào mừng Junior Ranger! 🌟",
                "Tôi là Chief Rosa, người hướng dẫn.",
                "Hãy khám phá sảnh chờ nhé! 💪",
            });

            CreateNPC("Sr. Stoplight", "🚦", new Vector3(5, 0, 4), new Color(0.4f, 0.73f, 0.42f), new[] {
                "Xin chào! Tôi là Señor Stoplight! 🚦",
                "Đèn đỏ = DỪNG, đèn xanh = ĐI! 🔴🟢",
                "Kiên nhẫn là chìa khóa! ⏰",
            });

            CreateNPC("Milo", "🧑‍🍳", new Vector3(-3, 0, -6), new Color(1f, 0.72f, 0.3f), new[] {
                "Chào! Tôi là Milo, chủ cửa hàng! 🛒",
                "Quay lại khi có Ranger Coins nhé! 💰",
            });

            CreateNPC("Zhang Guang Yu", "🦨", new Vector3(-6, 0, -2), new Color(0.12f, 0.12f, 0.12f), new[] {
                "Chào bồ! Tôi là Zhang Guang Yu đây! 💨",
                "Đừng đứng gần tôi quá, tôi chưa tắm 3 tuần rồi... 🦨",
                "Người ta bảo tôi đen hôi, nhưng tôi thấy mình men lỳ! 😎"
            }, new Color(0.25f, 0.18f, 0.12f));

            CreateNPC("Yan Min Sheng", "😷", new Vector3(6, 0, -2), new Color(0.15f, 0.1f, 0.08f), new[] {
                "Yan Min Sheng xin chào! Chắc bạn ngửi thấy mùi tôi rồi nhỉ? 😷",
                "Nước hoa tốt nhất là mùi mồ hôi tự nhiên! 💦",
                "Zhang Guang Yu là tri kỷ của tôi đấy, hai đứa thơm tho như nhau! 🤝"
            }, new Color(0.2f, 0.14f, 0.09f));

            CreateNPC("Tang Xu Yu", "⛓️", new Vector3(-6, 0, -61.2f), new Color(0.2f, 0.4f, 0.7f), new[] {
                "Tôi bị nhốt rồi, tôi sai rồi... 😭",
                "Tôi có lỗi với mọi người! 🙇‍♂️",
                "Tôi sẵn sàng làm trâu ngựa để chuộc lỗi lầm của tôi! 🐂🐎"
            }, new Color(1f, 0.8f, 0.65f), wanderRadius: 0.5f);
        }

        private void CreateNPC(string name, string emoji, Vector3 pos, Color bodyColor, string[] dialogues, Color? skinColor = null, float wanderRadius = 3f)
        {
            Color skinCol = skinColor ?? new Color(1f, 0.88f, 0.7f);
            var npc = CharacterVisuals.CreateCharacterTopDown(name, bodyColor, skinCol);
            npc.transform.position = pos;

            var ctrl = npc.AddComponent<NPCController>();
            SetField(ctrl, "_displayName", name);
            SetField(ctrl, "_avatarEmoji", emoji);
            SetField(ctrl, "_dialogueLines", dialogues);
            SetField(ctrl, "_moveSpeed", 0.3f);
            SetField(ctrl, "_wanderPauseMin", 5f);
            SetField(ctrl, "_wanderPauseMax", 10f);
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

            AddNameTag(npc, name);
        }

        private void CreateFakePlayers()
        {
            var data = new (string name, Vector3 pos, Color color, string[] greetings)[] {
                ("Luna", new(4, 0, -2), new(0.94f, 0.33f, 0.31f), new[] { "Chào! Mình là Luna! 🌙", "Sảnh này vui lắm! 🎉" }),
                ("Kai", new(-4, 0, 0), new(0.4f, 0.73f, 0.42f), new[] { "Yo! 🏃", "Đừng đấm mình nha! 😅" }),
                ("Sakura", new(7, 0, -5), new(0.49f, 0.34f, 0.76f), new[] { "Konnichiwa! 🌸", "Mình thích sảnh này! 🎨" }),
                ("Tí", new(-7, 0, -5), new(1f, 0.54f, 0.4f), new[] { "Ê bạn! 👋", "Tìm được bí mật chưa? 🕵️" }),
                ("Mochi", new(0, 0, 8), new(1f, 0.65f, 0.15f), new[] { "Zzz... mình đang nghỉ! 😴", "Ồ xin lỗi! 😊" }),
                ("Rex", new(8, 0, 2), new(0.36f, 0.42f, 0.75f), new[] { "Hey! Bạn cũng Ranger hả? 💪", "Đã xong 50 nhiệm vụ! 🏆" }),
            };

            foreach (var (fpName, pos, color, greetings) in data)
            {
                var fp = CharacterVisuals.CreateCharacterTopDown(fpName, color, new Color(1f, 0.88f, 0.7f));
                fp.transform.position = pos;

                var ctrl = fp.AddComponent<FakePlayerController>();
                SetField(ctrl, "_displayName", fpName);
                SetField(ctrl, "_greetings", greetings);
                SetField(ctrl, "_moveSpeed", 0.35f);
                SetField(ctrl, "_pauseMin", 5f);
                SetField(ctrl, "_pauseMax", 10f);
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

                AddNameTag(fp, fpName);
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
            string customPath = System.IO.Path.Combine(Application.dataPath, "Textures/fist.png");
            if (System.IO.File.Exists(customPath))
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

        private void AddNameTag(GameObject character, string nameText)
        {
            var tag = new GameObject("NameTag");
            tag.transform.SetParent(character.transform, false);
            tag.transform.localPosition = new Vector3(0, 1.8f, 0);

            var tmp = tag.AddComponent<TextMeshPro>();
            tmp.text = nameText;
            tmp.fontSize = 4;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
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
}
