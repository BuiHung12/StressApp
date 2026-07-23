using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace RangerCity.Lobby
{
    public struct ShopItemData
    {
        public string id;
        public string name;
        public ItemType type;
        public int gender;
        public int styleIndex;
        public int colorIndex;
        public int price;

        public ShopItemData(string id, string name, ItemType type, int gender, int styleIndex, int colorIndex, int price)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.gender = gender;
            this.styleIndex = styleIndex;
            this.colorIndex = colorIndex;
            this.price = price;
        }
    }

    public partial class LobbyUI : MonoBehaviour
    {
        private GameObject _fashionShopPanel;
        private bool _fashionShopOpen = false;
        private GameObject[] _shopItemSlotObjs;
        private List<ShopItemData> _shopCatalog = new List<ShopItemData>();

        // 3D Live Preview Stage for Fashion Shop
        private GameObject _shopPreviewCharacter;
        private Camera _shopPreviewCamera;
        private RenderTexture _shopPreviewRT;
        private float _shopPreviewRotation;
        private int _selectedShopItemIndex = 0;

        private void InitShopCatalog()
        {
            _shopCatalog.Clear();
            int gender = PlayerPrefs.GetInt("PlayerGender", 0);

            _shopCatalog.Add(new ShopItemData("biker_jacket", "Áo Jacket Da Biker", ItemType.Top, gender, 4, 1, 100));
            _shopCatalog.Add(new ShopItemData("cyber_hoodie", "Áo Hoodie Cyber Neon", ItemType.Top, gender, 2, 2, 80));
            _shopCatalog.Add(new ShopItemData("luxury_blazer", "Áo Blazer Sang Trọng", ItemType.Top, gender, 1, 0, 120));
            _shopCatalog.Add(new ShopItemData("sport_joggers", "Quần Jogger Thể Thao", ItemType.Pants, gender, 2, 0, 75));
            _shopCatalog.Add(new ShopItemData("pleated_skirt", gender == 0 ? "Quần Slim Fit Khaki" : "Chân Váy Xếp Ly", ItemType.Pants, gender, 1, 0, 60));
            _shopCatalog.Add(new ShopItemData("chunky_sneakers", "Giày Sneaker Chunky", ItemType.Shoes, gender, 0, 0, 90));
        }

        public void CreateFashionShopUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            InitShopCatalog();

            if (_fashionShopPanel == null)
            {
                _fashionShopPanel = new GameObject("FashionShopPanel");
                _fashionShopPanel.transform.SetParent(canvas.transform, false);

                var panelRt = _fashionShopPanel.AddComponent<RectTransform>();
                panelRt.anchorMin = new Vector2(0.5f, 0.5f);
                panelRt.anchorMax = new Vector2(0.5f, 0.5f);
                panelRt.pivot = new Vector2(0.5f, 0.5f);
                panelRt.anchoredPosition = Vector2.zero;
                panelRt.sizeDelta = new Vector2(850f, 520f);

                var bgCard = CreateRoundedPanel(_fashionShopPanel.transform, "CardBg",
                    Vector2.zero, new Vector2(850, 520), new Color(0.06f, 0.08f, 0.14f, 0.98f), true);

                var border = CreateRoundedPanel(bgCard.transform, "Border",
                    Vector2.zero, new Vector2(854, 524), new Color(0.85f, 0.35f, 0.9f, 0.7f), false);
                border.transform.SetAsFirstSibling();

                // Header Title
                MakeText(bgCard.transform, "HeaderTitle", "🛍️ CỬA HÀNG TRANG PHỤC SANG TRỌNG", 22,
                    new Vector2(0, 230), new Vector2(780, 36), TextAlignmentOptions.Center, new Color(1f, 0.7f, 0.95f));

                // Close Button
                var closeBtnObj = CreateRoundedPanel(bgCard.transform, "CloseBtn",
                    new Vector2(390, 230), new Vector2(32, 32), new Color(0.85f, 0.25f, 0.25f, 0.9f), true);
                MakeText(closeBtnObj.transform, "X", "X", 16, Vector2.zero, new Vector2(30, 30), TextAlignmentOptions.Center, Color.white);
                closeBtnObj.AddComponent<Button>().onClick.AddListener(ToggleFashionShopPanel);

                // ═══════════════════════════════════
                //  LEFT PANE — 3D Live Try-On Mannequin Stage
                // ═══════════════════════════════════
                var leftPane = CreateRoundedPanel(bgCard.transform, "LeftPane",
                    new Vector2(-235, -15), new Vector2(330, 440), new Color(0.04f, 0.05f, 0.09f, 0.95f), true);

                MakeText(leftPane.transform, "PreviewTitle", "MẶC THỬ TRỰC TIẾP (3D)", 13,
                    new Vector2(0, 195), new Vector2(310, 24), TextAlignmentOptions.Center, new Color(0.4f, 0.85f, 1f));

                CreateShopPreviewStage(leftPane.transform);

                // ═══════════════════════════════════
                //  RIGHT PANE — Fashion Catalog Grid & Buy Actions
                // ═══════════════════════════════════
                var rightPane = CreateRoundedPanel(bgCard.transform, "RightPane",
                    new Vector2(185, -15), new Vector2(450, 440), new Color(0.04f, 0.05f, 0.09f, 0.95f), true);

                MakeText(rightPane.transform, "CatalogTitle", "DANH MỤC TRANG PHỤC THỜI TRANG", 13,
                    new Vector2(0, 195), new Vector2(420, 24), TextAlignmentOptions.Center, new Color(0.4f, 0.85f, 1f));

                // 2x3 Shop Item Cards Grid
                float cardW = 195f;
                float cardH = 115f;
                float spaceX = 20f;
                float spaceY = 15f;
                float startX = -108f;
                float startY = 115f;

                _shopItemSlotObjs = new GameObject[_shopCatalog.Count];

                for (int i = 0; i < _shopCatalog.Count; i++)
                {
                    int itemIdx = i;
                    int row = i / 2;
                    int col = i % 2;
                    Vector2 pos = new Vector2(startX + col * (cardW + spaceX), startY - row * (cardH + spaceY));

                    var itemCardObj = CreateRoundedPanel(rightPane.transform, $"ShopCard_{i}",
                        pos, new Vector2(cardW, cardH), new Color(0.1f, 0.12f, 0.18f, 0.95f), true);

                    var itemData = _shopCatalog[i];

                    // Item Name
                    MakeText(itemCardObj.transform, "Name", itemData.name, 12,
                        new Vector2(0, 36), new Vector2(180, 22), TextAlignmentOptions.Center, Color.white);

                    // 2D Icon Graphic
                    var iconObj = new GameObject("Icon");
                    iconObj.transform.SetParent(itemCardObj.transform, false);
                    var iconRt = iconObj.AddComponent<RectTransform>();
                    iconRt.anchoredPosition = new Vector2(-55, 0);
                    iconRt.sizeDelta = new Vector2(45, 45);
                    var iconImg = iconObj.AddComponent<Image>();
                    iconImg.raycastTarget = false;

                    if (itemData.type == ItemType.Top)
                    {
                        Color shirtColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(itemData.colorIndex, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
                        iconImg.sprite = GetShirtSprite(shirtColor);
                    }
                    else if (itemData.type == ItemType.Pants)
                    {
                        Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(itemData.colorIndex, 0, NetworkPlayer.PantsColorPalette.Length - 1)];
                        iconImg.sprite = GetPantsSprite(pantsColor);
                    }
                    else if (itemData.type == ItemType.Shoes)
                    {
                        iconImg.sprite = GetShoesSprite(new Color(0.2f, 0.6f, 1f));
                    }

                    // Price Tag
                    MakeText(itemCardObj.transform, "Price", $"💲 {itemData.price} Xu", 12,
                        new Vector2(25, 10), new Vector2(110, 20), TextAlignmentOptions.Left, new Color(1f, 0.84f, 0.2f));

                    // Buy / Select Button
                    var buyBtnObj = CreateRoundedPanel(itemCardObj.transform, "BuyBtn",
                        new Vector2(25, -25), new Vector2(110, 26), new Color(0.15f, 0.75f, 0.4f, 0.9f), true);
                    MakeText(buyBtnObj.transform, "BtnTxt", "MUA NGAY", 11,
                        Vector2.zero, new Vector2(100, 22), TextAlignmentOptions.Center, Color.white);

                    buyBtnObj.AddComponent<Button>().onClick.AddListener(() => OnBuyShopItemClicked(itemIdx));

                    // Select Card Action
                    var cardBtn = itemCardObj.AddComponent<Button>();
                    cardBtn.onClick.AddListener(() => SelectShopItemPreview(itemIdx));

                    _shopItemSlotObjs[i] = itemCardObj;
                }

                _fashionShopPanel.SetActive(false);
            }
        }

        private void CreateShopPreviewStage(Transform parent)
        {
            _shopPreviewRT = new RenderTexture(512, 512, 16);
            _shopPreviewRT.Create();

            var rawObj = new GameObject("ShopPreviewRawImage", typeof(RectTransform));
            rawObj.transform.SetParent(parent, false);
            var rawRt = rawObj.GetComponent<RectTransform>();
            rawRt.anchoredPosition = new Vector2(0, 15);
            rawRt.sizeDelta = new Vector2(300, 330);
            var rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = _shopPreviewRT;
            rawImg.raycastTarget = false;

            _shopPreviewCharacter = new GameObject("ShopPreviewChar");
            _shopPreviewCharacter.transform.position = new Vector3(400f, 0f, 400f);

            var camObj = new GameObject("ShopPreviewCamera");
            camObj.transform.SetParent(_shopPreviewCharacter.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.52f, 1.25f);
            camObj.transform.localRotation = Quaternion.Euler(6f, 180f, 0f);

            _shopPreviewCamera = camObj.AddComponent<Camera>();
            _shopPreviewCamera.targetTexture = _shopPreviewRT;
            _shopPreviewCamera.depth = -3f;
            _shopPreviewCamera.clearFlags = CameraClearFlags.SolidColor;
            _shopPreviewCamera.backgroundColor = new Color(0.04f, 0.06f, 0.1f, 1f);
            _shopPreviewCamera.orthographic = true;
            _shopPreviewCamera.orthographicSize = 0.52f;

            var lightObj = new GameObject("ShopPreviewLight");
            lightObj.transform.SetParent(camObj.transform, false);
            lightObj.transform.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1.2f;

            UpdateShopPreviewCharacter();
        }

        private void SelectShopItemPreview(int index)
        {
            if (index >= 0 && index < _shopCatalog.Count)
            {
                _selectedShopItemIndex = index;
                UpdateShopPreviewCharacter();
            }
        }

        private void UpdateShopPreviewCharacter()
        {
            if (_shopPreviewCharacter == null) return;

            foreach (Transform child in _shopPreviewCharacter.transform)
            {
                if (child.name.StartsWith("ShopPreviewCamera")) continue;
                Destroy(child.gameObject);
            }

            int gender = PlayerPrefs.GetInt("PlayerGender", 0);
            int hairStyle = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            int hairColorIdx = PlayerPrefs.GetInt("PlayerHairColor", 0);
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(hairColorIdx, 0, NetworkPlayer.HairColorPalette.Length - 1)];

            int outfitStyle = PlayerPrefs.GetInt("PlayerOutfitStyle", 1);
            Color bodyColor = NetworkPlayer.BodyColorPalette[0];

            int pantsStyle = PlayerPrefs.GetInt("PlayerPantsStyle", 1);
            Color pantsColor = NetworkPlayer.PantsColorPalette[0];

            // Apply selected preview item
            if (_selectedShopItemIndex >= 0 && _selectedShopItemIndex < _shopCatalog.Count)
            {
                var selItem = _shopCatalog[_selectedShopItemIndex];
                if (selItem.type == ItemType.Top)
                {
                    outfitStyle = selItem.styleIndex;
                    bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(selItem.colorIndex, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
                }
                else if (selItem.type == ItemType.Pants)
                {
                    pantsStyle = selItem.styleIndex;
                    pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(selItem.colorIndex, 0, NetworkPlayer.PantsColorPalette.Length - 1)];
                }
            }

            Color skinColor = gender == 0 ? new Color(0.96f, 0.82f, 0.72f) : new Color(0.98f, 0.85f, 0.77f);
            var charModel = CharacterVisuals.CreateCharacterTopDown("Visual", bodyColor, skinColor);
            charModel.transform.SetParent(_shopPreviewCharacter.transform, false);
            charModel.transform.localPosition = Vector3.zero;

            CharacterVisuals.ApplyCustomization(charModel, gender, hairStyle, hairColor, outfitStyle, bodyColor, pantsStyle, pantsColor);
        }

        public void ToggleFashionShopPanel()
        {
            _fashionShopOpen = !_fashionShopOpen;
            if (_fashionShopPanel == null) CreateFashionShopUI();
            if (_fashionShopPanel != null)
            {
                _fashionShopPanel.SetActive(_fashionShopOpen);
                if (_fashionShopOpen)
                {
                    UpdateShopPreviewCharacter();
                }
            }
        }

        private void OnBuyShopItemClicked(int index)
        {
            if (index < 0 || index >= _shopCatalog.Count) return;
            var item = _shopCatalog[index];

            if (_playerInventory == null && _player != null)
            {
                _playerInventory = _player.GetComponent<PlayerInventory>();
            }

            if (_playerInventory == null)
            {
                var scenePlayer = GameObject.FindWithTag("Player");
                if (scenePlayer != null) _playerInventory = scenePlayer.GetComponent<PlayerInventory>();
            }

            if (_playerInventory != null)
            {
                // Find empty slot or item slot to add
                for (int i = 0; i < PlayerInventory.MAX_SLOTS; i++)
                {
                    if (_playerInventory.slots[i] == null || string.IsNullOrEmpty(_playerInventory.slots[i].name) || _playerInventory.slots[i].id == "empty")
                    {
                        _playerInventory.slots[i] = new InventoryItem(item.id, item.name, item.type, item.gender, item.styleIndex, item.colorIndex, false);
                        _playerInventory.SaveInventory();
                        _playerInventory.EquipItem(i);
                        RefreshInventoryUI();
                        UpdateShopPreviewCharacter();
                        Debug.Log($"[FashionShop] Successfully purchased {item.name} into inventory slot {i}!");
                        return;
                    }
                }

                Debug.LogWarning("[FashionShop] Inventory is full!");
            }
        }

        private void UpdateFashionShopHotkeys()
        {
            if (_shopPreviewCharacter != null && _fashionShopOpen)
            {
                _shopPreviewRotation += Time.deltaTime * 35f;
                _shopPreviewCharacter.transform.rotation = Quaternion.Euler(0, _shopPreviewRotation, 0);
            }
        }
    }
}
