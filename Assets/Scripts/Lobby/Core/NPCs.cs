using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class LobbySetup : MonoBehaviour
    {
        private void CreateNPCs()
        {
            CreateNPC("Chief Rosa", "", new Vector3(-5, 0.03f, 4), new Color(0.81f, 0.58f, 0.86f), new[] {
                "Chào mừng Junior Ranger!",
                "Tôi là Chief Rosa, người hướng dẫn.",
                "Hãy khám phá sảnh chờ nhé!",
            });

            CreateNPC("Sr. Stoplight", "", new Vector3(5, 0.03f, 4), new Color(0.4f, 0.73f, 0.42f), new[] {
                "Xin chào! Tôi là Señor Stoplight!",
                "Đèn đỏ = DỪNG, đèn xanh = ĐI!",
                "Kiên nhẫn là chìa khóa!",
            });

            CreateNPC("Milo (Chủ Cửa Hàng Trang Phục)", "🛍️", new Vector3(9f, 0.03f, 6.2f), new Color(0.95f, 0.35f, 0.5f), new[] {
                "Chào mừng bạn tới Cửa Hàng Trang Phục Sang Trọng!",
                "Hãy chọn những bộ trang phục đẹp nhất nhé!",
            });

            CreateNPC("Zhang Guang Yu", "", new Vector3(-2f, 0.03f, -61.2f), new Color(0.12f, 0.12f, 0.12f), new[] {
                "Xin lỗi Xiao Ling, bây giờ tôi đã biết Xiao Ling có người yêu rồi! Tôi không được phép tăm tia nữa!",
                "Tôi là người có tội!"
            }, new Color(0.25f, 0.18f, 0.12f), wanderRadius: 0.5f);

            CreateNPC("Yan Min Sheng", "", new Vector3(6f, 0.03f, -61.2f), new Color(0.15f, 0.1f, 0.08f), new[] {
                "Xin lỗi Xiao Ling từ nay tui không dám giao việc linh tinh cho Xiao Ling nữa huhuhu"
            }, new Color(0.2f, 0.14f, 0.09f), wanderRadius: 0.5f);

            CreateNPC("Tang Xu Yu", "", new Vector3(-6, 0.03f, -61.2f), new Color(0.2f, 0.4f, 0.7f), new[] {
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

            if (name.Contains("Milo") || name.Contains("Trang Phục"))
            {
                SetField(ctrl, "_canBePunched", false);
            }

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
                ("Luna", new(4, 0, -2), new(0.94f, 0.33f, 0.31f), 1, new[] { "Chào! Mình là Luna!", "Sảnh này vui lắm!" }),
                ("Kai", new(-4, 0, 0), new(0.4f, 0.73f, 0.42f), 0, new[] { "Yo!", "Đừng đấm mình nha!" }),
                ("Sakura", new(7, 0, -5), new(0.49f, 0.34f, 0.76f), 1, new[] { "Konnichiwa!", "Mình thích sảnh này!" }),
                ("Tí", new(-7, 0, -5), new(1f, 0.54f, 0.4f), 0, new[] { "Ê bạn!", "Tìm được bí mật chưa?" }),
                ("Mochi", new(0, 0, 8), new(1f, 0.65f, 0.15f), 1, new[] { "Zzz... mình đang nghỉ!", "Ồ xin lỗi!" }),
                ("Rex", new(8, 0, 2), new(0.36f, 0.42f, 0.75f), 0, new[] { "Hey! Bạn cũng Ranger hả?", "Đã xong 50 nhiệm vụ!" }),
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


    }
    }
