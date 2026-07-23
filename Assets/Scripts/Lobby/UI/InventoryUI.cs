using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    public partial class LobbyUI : MonoBehaviour
    {
        private GameObject _inventoryToggleButton;
        private GameObject _inventoryPanel;
        private bool _inventoryOpen = false;
        private PlayerInventory _playerInventory;
        private GameObject[] _inventorySlotObjs = new GameObject[PlayerInventory.MAX_SLOTS];

        // 3D Live Preview Stage for Inventory
        private GameObject _invPreviewCharacter;
        private Camera _invPreviewCamera;
        private RenderTexture _invPreviewRT;
        private float _invPreviewRotation;

        public void CreateInventoryUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            // Attach PlayerInventory component if missing
            if (_player != null)
            {
                _playerInventory = _player.GetComponent<PlayerInventory>();
                if (_playerInventory == null)
                {
                    _playerInventory = _player.gameObject.AddComponent<PlayerInventory>();
                }
                _playerInventory.OnInventoryUpdated += RefreshInventoryUI;
            }

            // ── 1. HUD Inventory Button (Bottom-Right next to Emoji button) ──
            if (_inventoryToggleButton == null)
            {
                _inventoryToggleButton = new GameObject("InventoryToggleBtn");
                _inventoryToggleButton.transform.SetParent(canvas.transform, false);

                var toggleRt = _inventoryToggleButton.AddComponent<RectTransform>();
                toggleRt.anchorMin = new Vector2(1f, 0f); // Bottom-right anchor
                toggleRt.anchorMax = new Vector2(1f, 0f);
                toggleRt.pivot = new Vector2(0.5f, 0.5f);
                toggleRt.anchoredPosition = new Vector2(-430f, 150f); // Aligned with Emoji (-290) & Punch (-150)
                toggleRt.sizeDelta = new Vector2(85f, 85f);

                var toggleImg = _inventoryToggleButton.AddComponent<Image>();
                Color darkGlass = new Color(0.08f, 0.09f, 0.12f, 0.65f);
                Color emeraldBorder = new Color(0.15f, 0.85f, 0.45f, 0.9f);
                toggleImg.sprite = CreateCircleSprite(darkGlass, emeraldBorder, 4f);
                toggleImg.color = Color.white;

                var toggleBtn = _inventoryToggleButton.AddComponent<Button>();
                toggleBtn.onClick.AddListener(ToggleInventoryPanel);

                // Backpack 2D Image Icon (Procedural Vector Sprite)
                var iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(_inventoryToggleButton.transform, false);
                var iconRt = iconObj.AddComponent<RectTransform>();
                iconRt.anchorMin = Vector2.zero; iconRt.anchorMax = Vector2.one;
                iconRt.offsetMin = new Vector2(20, 20);
                iconRt.offsetMax = new Vector2(-20, -20);
                var iconImg = iconObj.AddComponent<Image>();
                iconImg.sprite = GetBackpackSprite();
                iconImg.raycastTarget = false;

                // Hotkey Label Badge Below
                var labelObj = CreateRoundedPanel(_inventoryToggleButton.transform, "Badge",
                    new Vector2(0, -50f), new Vector2(90, 22), new Color(0.06f, 0.08f, 0.12f, 0.95f), false);
                MakeText(labelObj.transform, "Txt", "TÚI ĐỒ (I)", 11,
                    Vector2.zero, new Vector2(85, 20), TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.7f));
            }

            // ── 2. Dual-Pane Inventory Modal Panel (Center Anchored, Width 820) ──
            if (_inventoryPanel == null)
            {
                _inventoryPanel = new GameObject("InventoryPanel");
                _inventoryPanel.transform.SetParent(canvas.transform, false);

                var panelRt = _inventoryPanel.AddComponent<RectTransform>();
                panelRt.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
                panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = Vector2.zero;
                panelRt.sizeDelta = new Vector2(820f, 500f);

                var bgCard = CreateRoundedPanel(_inventoryPanel.transform, "CardBg",
                    Vector2.zero, new Vector2(820, 500), new Color(0.08f, 0.1f, 0.16f, 0.97f), true);

                var shadow = bgCard.AddComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.6f);
                shadow.effectDistance = new Vector2(4, -4);

                var border = CreateRoundedPanel(bgCard.transform, "Border",
                    Vector2.zero, new Vector2(824, 504), new Color(0.15f, 0.85f, 0.45f, 0.6f), false);
                border.transform.SetAsFirstSibling();

                // Header
                MakeText(bgCard.transform, "HeaderTitle", "TÚI ĐỒ & TỦ ĐỒ CÁ NHÂN", 22,
                    new Vector2(0, 220), new Vector2(760, 36), TextAlignmentOptions.Center, new Color(0.4f, 1f, 0.7f));

                // Close Button
                var closeBtnObj = CreateRoundedPanel(bgCard.transform, "CloseBtn",
                    new Vector2(375, 220), new Vector2(32, 32), new Color(0.85f, 0.25f, 0.25f, 0.9f), true);
                MakeText(closeBtnObj.transform, "X", "X", 16, Vector2.zero, new Vector2(30, 30), TextAlignmentOptions.Center, Color.white);
                closeBtnObj.AddComponent<Button>().onClick.AddListener(ToggleInventoryPanel);

                // ═══════════════════════════════════
                //  LEFT PANE — 3D Live Wardrobe Preview Stage
                // ═══════════════════════════════════
                var leftPane = CreateRoundedPanel(bgCard.transform, "LeftPane",
                    new Vector2(-225, -15), new Vector2(310, 420), new Color(0.05f, 0.06f, 0.1f, 0.9f), true);

                MakeText(leftPane.transform, "PreviewTitle", "THỬ ĐỒ (LIVE 3D)", 13,
                    new Vector2(0, 185), new Vector2(290, 24), TextAlignmentOptions.Center, new Color(0.4f, 0.85f, 1f));

                CreateInventoryPreviewStage(leftPane.transform);

                // ═══════════════════════════════════
                //  RIGHT PANE — 3x3 Inventory Grid (9 Slots)
                // ═══════════════════════════════════
                var rightPane = CreateRoundedPanel(bgCard.transform, "RightPane",
                    new Vector2(175, -15), new Vector2(440, 420), new Color(0.05f, 0.06f, 0.1f, 0.9f), true);

                MakeText(rightPane.transform, "GridTitle", "DANH SÁCH VẬT PHẨM (9 Ô)", 13,
                    new Vector2(0, 185), new Vector2(410, 24), TextAlignmentOptions.Center, new Color(0.4f, 0.85f, 1f));

                // 3x3 Grid Slots
                float slotSize = 115f;
                float spacing = 15f;
                float startX = -130f;
                float startY = 115f;

                for (int i = 0; i < PlayerInventory.MAX_SLOTS; i++)
                {
                    int slotIdx = i;
                    int row = i / 3;
                    int col = i % 3;
                    Vector2 pos = new Vector2(startX + col * (slotSize + spacing), startY - row * (slotSize + spacing));

                    var slotObj = CreateRoundedPanel(rightPane.transform, $"Slot_{i}",
                        pos, new Vector2(slotSize, slotSize), new Color(0.12f, 0.14f, 0.2f, 0.9f), true);

                    var slotBorder = CreateRoundedPanel(slotObj.transform, "SlotBorder",
                        Vector2.zero, new Vector2(slotSize + 4, slotSize + 4), new Color(0.2f, 0.25f, 0.35f, 0.4f), false);
                    slotBorder.transform.SetAsFirstSibling();

                    MakeText(slotObj.transform, "ItemName", "[Trống]", 11,
                        new Vector2(0, 36), new Vector2(105, 20), TextAlignmentOptions.Center, new Color(0.6f, 0.65f, 0.75f));

                    // 2D Visual Graphic Icon for Item
                    var itemIconObj = new GameObject("ItemIcon");
                    itemIconObj.transform.SetParent(slotObj.transform, false);
                    var iconRt = itemIconObj.AddComponent<RectTransform>();
                    iconRt.anchoredPosition = new Vector2(0, 3);
                    iconRt.sizeDelta = new Vector2(46, 46);
                    var iconImg = itemIconObj.AddComponent<Image>();
                    iconImg.raycastTarget = false;
                    itemIconObj.SetActive(false);

                    var equipBadge = CreateRoundedPanel(slotObj.transform, "EquipBadge",
                        new Vector2(0, -38), new Vector2(90, 20), new Color(0.15f, 0.85f, 0.45f, 0.9f), false);
                    MakeText(equipBadge.transform, "BadgeText", "ĐANG MẶC", 10,
                        Vector2.zero, new Vector2(85, 18), TextAlignmentOptions.Center, Color.white);
                    equipBadge.SetActive(false);

                    var slotButton = slotObj.AddComponent<Button>();
                    slotButton.onClick.AddListener(() => OnInventorySlotClicked(slotIdx));

                    _inventorySlotObjs[i] = slotObj;
                }

                _inventoryPanel.SetActive(false);
            }
        }

        private void CreateInventoryPreviewStage(Transform parent)
        {
            _invPreviewRT = new RenderTexture(512, 512, 16);
            _invPreviewRT.Create();

            var rawObj = new GameObject("InvPreviewRawImage", typeof(RectTransform));
            rawObj.transform.SetParent(parent, false);
            var rawRt = rawObj.GetComponent<RectTransform>();
            rawRt.anchoredPosition = new Vector2(0, 15);
            rawRt.sizeDelta = new Vector2(280, 310);
            var rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = _invPreviewRT;
            rawImg.raycastTarget = false;

            _invPreviewCharacter = new GameObject("InvPreviewChar");
            _invPreviewCharacter.transform.position = new Vector3(300f, 0f, 300f);

            var camObj = new GameObject("InvPreviewCamera");
            camObj.transform.SetParent(_invPreviewCharacter.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.52f, 1.25f);
            camObj.transform.localRotation = Quaternion.Euler(6f, 180f, 0f);

            _invPreviewCamera = camObj.AddComponent<Camera>();
            _invPreviewCamera.targetTexture = _invPreviewRT;
            _invPreviewCamera.depth = -4f;
            _invPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _invPreviewCamera.backgroundColor = new Color(0.05f, 0.07f, 0.12f, 1f);
            _invPreviewCamera.orthographic = true;
            _invPreviewCamera.orthographicSize = 0.52f;

            var lightObj = new GameObject("InvPreviewLight");
            lightObj.transform.SetParent(camObj.transform, false);
            lightObj.transform.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1.2f;

            UpdateInventoryPreviewCharacter();
        }

        private void UpdateInventoryPreviewCharacter()
        {
            if (_invPreviewCharacter == null) return;

            // Clear old children
            foreach (Transform child in _invPreviewCharacter.transform)
            {
                if (child.name.StartsWith("PreviewCamera") || child.name.StartsWith("InvPreviewCamera")) continue;
                Destroy(child.gameObject);
            }

            int gender = PlayerPrefs.GetInt("PlayerGender", 0);
            int hairStyle = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            int hairColorIdx = PlayerPrefs.GetInt("PlayerHairColor", 0);
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(hairColorIdx, 0, NetworkPlayer.HairColorPalette.Length - 1)];

            int outfitStyle = PlayerPrefs.GetInt("PlayerOutfitStyle", 0);
            Color bodyColor = NetworkPlayer.BodyColorPalette[0];

            int pantsStyle = PlayerPrefs.GetInt("PlayerPantsStyle", 0);
            Color pantsColor = NetworkPlayer.PantsColorPalette[0];

            // If player inventory exists, read equipped items
            if (_playerInventory != null)
            {
                for (int i = 0; i < PlayerInventory.MAX_SLOTS; i++)
                {
                    var item = _playerInventory.slots[i];
                    if (item != null && item.isEquipped)
                    {
                        if (item.type == ItemType.Top)
                        {
                            outfitStyle = item.styleIndex;
                            bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(item.colorIndex, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
                        }
                        else if (item.type == ItemType.Pants)
                        {
                            pantsStyle = item.styleIndex;
                            pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(item.colorIndex, 0, NetworkPlayer.PantsColorPalette.Length - 1)];
                        }
                    }
                }
            }

            Color skinColor = gender == 0 ? new Color(0.96f, 0.82f, 0.72f) : new Color(0.98f, 0.85f, 0.77f);
            var charModel = CharacterVisuals.CreateCharacterTopDown("Visual", bodyColor, skinColor);
            charModel.transform.SetParent(_invPreviewCharacter.transform, false);
            charModel.transform.localPosition = Vector3.zero;

            CharacterVisuals.ApplyCustomization(charModel, gender, hairStyle, hairColor, outfitStyle, bodyColor, pantsStyle, pantsColor);
        }

        private void RotateInventoryPreview()
        {
            if (_invPreviewCharacter != null && _inventoryOpen)
            {
                _invPreviewRotation += Time.deltaTime * 35f;
                _invPreviewCharacter.transform.rotation = Quaternion.Euler(0, _invPreviewRotation, 0);
            }
        }

        private void UpdateInventoryHotkeys()
        {
            if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleInventoryPanel();
            }

            RotateInventoryPreview();
        }

        public void ToggleInventoryPanel()
        {
            _inventoryOpen = !_inventoryOpen;
            if (_inventoryPanel != null)
            {
                _inventoryPanel.SetActive(_inventoryOpen);
                if (_inventoryOpen)
                {
                    if (_playerInventory == null)
                    {
                        var scenePlayer = GameObject.FindWithTag("Player");
                        if (scenePlayer != null)
                        {
                            _playerInventory = scenePlayer.GetComponent<PlayerInventory>();
                            if (_playerInventory == null) _playerInventory = scenePlayer.AddComponent<PlayerInventory>();
                        }
                    }

                    if (_playerInventory != null)
                    {
                        int gender = PlayerPrefs.GetInt("PlayerGender", 0);
                        _playerInventory.EnsureStarterItems(gender);
                    }

                    UpdateInventoryPreviewCharacter();
                    RefreshInventoryUI();
                }
            }
        }

        private void OnInventorySlotClicked(int slotIndex)
        {
            if (_playerInventory == null && _player != null)
            {
                _playerInventory = _player.GetComponent<PlayerInventory>();
            }

            if (_playerInventory != null)
            {
                _playerInventory.EquipItem(slotIndex);
                UpdateInventoryPreviewCharacter();
                RefreshInventoryUI();
            }
        }

        public void RefreshInventoryUI()
        {
            if (_playerInventory == null && _player != null)
            {
                _playerInventory = _player.GetComponent<PlayerInventory>();
            }

            if (_playerInventory != null)
            {
                int gender = PlayerPrefs.GetInt("PlayerGender", 0);
                _playerInventory.EnsureStarterItems(gender);
            }

            if (_playerInventory == null || _inventorySlotObjs == null) return;

            for (int i = 0; i < PlayerInventory.MAX_SLOTS; i++)
            {
                var slotObj = _inventorySlotObjs[i];
                if (slotObj == null) continue;

                var item = _playerInventory.slots[i];
                var nameTxt = slotObj.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                var equipBadge = slotObj.transform.Find("EquipBadge")?.gameObject;
                var slotBorderImg = slotObj.transform.Find("SlotBorder")?.GetComponent<Image>();
                var itemIconObj = slotObj.transform.Find("ItemIcon")?.gameObject;
                var itemIconImg = itemIconObj?.GetComponent<Image>();

                if (item != null && !string.IsNullOrEmpty(item.name) && item.id != "empty")
                {
                    if (nameTxt != null)
                    {
                        nameTxt.text = item.name;
                        nameTxt.color = item.isEquipped ? new Color(0.4f, 1f, 0.7f) : Color.white;
                    }

                    if (equipBadge != null) equipBadge.SetActive(item.isEquipped);
                    if (slotBorderImg != null)
                    {
                        slotBorderImg.color = item.isEquipped ?
                            new Color(0.15f, 0.85f, 0.45f, 0.9f) : new Color(0.25f, 0.3f, 0.45f, 0.6f);
                    }

                    if (itemIconObj != null && itemIconImg != null)
                    {
                        itemIconObj.SetActive(true);
                        if (item.type == ItemType.Top)
                        {
                            Color shirtColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(item.colorIndex, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
                            itemIconImg.sprite = GetShirtSprite(shirtColor);
                        }
                        else if (item.type == ItemType.Pants)
                        {
                            Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(item.colorIndex, 0, NetworkPlayer.PantsColorPalette.Length - 1)];
                            itemIconImg.sprite = GetPantsSprite(pantsColor);
                        }
                        else if (item.type == ItemType.Shoes)
                        {
                            itemIconImg.sprite = GetShoesSprite(new Color(0.2f, 0.6f, 1f));
                        }
                    }
                }
                else
                {
                    if (nameTxt != null)
                    {
                        nameTxt.text = "[Trống]";
                        nameTxt.color = new Color(0.45f, 0.5f, 0.6f);
                    }

                    if (equipBadge != null) equipBadge.SetActive(false);
                    if (slotBorderImg != null)
                    {
                        slotBorderImg.color = new Color(0.18f, 0.2f, 0.28f, 0.4f);
                    }

                    if (itemIconObj != null) itemIconObj.SetActive(false);
                }
            }
        }
    }
}
