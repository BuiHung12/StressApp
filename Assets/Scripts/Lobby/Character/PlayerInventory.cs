using System;
using System.Collections.Generic;
using UnityEngine;

namespace RangerCity.Lobby
{
    public enum ItemType
    {
        Top,
        Pants,
        Shoes,
        Misc
    }

    [Serializable]
    public class InventoryItem
    {
        public string id = "";
        public string name = "";
        public ItemType type = ItemType.Misc;
        public int gender = 0; // 0 = Nam, 1 = Nữ
        public int styleIndex = 0;
        public int colorIndex = 0;
        public bool isEquipped = false;

        public InventoryItem() { }

        public InventoryItem(string id, string name, ItemType type, int gender, int styleIndex, int colorIndex, bool isEquipped = false)
        {
            this.id = id;
            this.name = name;
            this.type = type;
            this.gender = gender;
            this.styleIndex = styleIndex;
            this.colorIndex = colorIndex;
            this.isEquipped = isEquipped;
        }
    }

    [Serializable]
    public class InventoryDataWrapper
    {
        public List<InventoryItem> items = new List<InventoryItem>();
    }

    public class PlayerInventory : MonoBehaviour
    {
        public const int MAX_SLOTS = 9;
        public InventoryItem[] slots = new InventoryItem[MAX_SLOTS];

        public event Action OnInventoryUpdated;

        private PlayerController _playerController;
        private NetworkPlayer _networkPlayer;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
            _networkPlayer = GetComponent<NetworkPlayer>();
        }

        private void Start()
        {
            int gender = PlayerPrefs.GetInt("PlayerGender", 0);
            LoadInventory(gender);
        }

        public void InitializeStarterItems(int gender)
        {
            int outfitStyle = PlayerPrefs.GetInt("PlayerOutfitStyle", 1);
            int pantsStyle = PlayerPrefs.GetInt("PlayerPantsStyle", 1);
            int outfitColor = PlayerPrefs.GetInt("PlayerColorIndex", 0);
            int pantsColor = PlayerPrefs.GetInt("PlayerPantsColor", 0);

            // Fashion Collection Items
            string topName = gender == 0 ? "Áo Blazer Sang Trọng" : "Áo Chic Pink Thời Trang";
            string pantsName = gender == 0 ? "Quần Slim Fit Khaki" : "Chân Váy Xếp Ly Nhạt";
            string shoesName = "Giày Sneaker Chunky Đế Trắng";

            slots[0] = new InventoryItem("starter_top", topName, ItemType.Top, gender, outfitStyle, outfitColor, true);
            slots[1] = new InventoryItem("starter_pants", pantsName, ItemType.Pants, gender, pantsStyle, pantsColor, true);
            slots[2] = new InventoryItem("starter_shoes", shoesName, ItemType.Shoes, gender, 0, 0, true);

            // Extra Fashion Wardrobe Collection (Slots 3, 4, 5)
            slots[3] = new InventoryItem("fashion_biker_jacket", "Áo Jacket Da Biker", ItemType.Top, gender, 4, 1, false);
            slots[4] = new InventoryItem("fashion_joggers", "Quần Jogger Thể Thao", ItemType.Pants, gender, 2, 0, false);
            slots[5] = new InventoryItem("fashion_hoodie", "Áo Hoodie Cyber Neon", ItemType.Top, gender, 2, 2, false);

            for (int i = 6; i < MAX_SLOTS; i++)
            {
                slots[i] = null;
            }

            SaveInventory();
            ApplyEquippedItemsToCharacter();
        }

        public void EnsureStarterItems(int gender)
        {
            bool hasTop = false;
            bool hasPants = false;
            bool hasShoes = false;

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (slots[i] != null && !string.IsNullOrEmpty(slots[i].name) && slots[i].id != "empty")
                {
                    if (slots[i].type == ItemType.Top) hasTop = true;
                    if (slots[i].type == ItemType.Pants) hasPants = true;
                    if (slots[i].type == ItemType.Shoes) hasShoes = true;
                }
            }

            if (!hasTop || !hasPants || !hasShoes)
            {
                InitializeStarterItems(gender);
            }
        }

        public void LoadInventory(int gender)
        {
            string json = PlayerPrefs.GetString("PlayerInventory_V4", "");
            if (string.IsNullOrEmpty(json))
            {
                InitializeStarterItems(gender);
                return;
            }

            try
            {
                InventoryDataWrapper wrapper = JsonUtility.FromJson<InventoryDataWrapper>(json);
                if (wrapper != null && wrapper.items != null && wrapper.items.Count > 0)
                {
                    for (int i = 0; i < MAX_SLOTS; i++)
                    {
                        var it = i < wrapper.items.Count ? wrapper.items[i] : null;
                        if (it != null && !string.IsNullOrEmpty(it.name) && it.id != "empty")
                        {
                            slots[i] = it;
                        }
                        else
                        {
                            slots[i] = null;
                        }
                    }
                }
                else
                {
                    InitializeStarterItems(gender);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PlayerInventory] JSON parse error: {e.Message}. Re-initializing starter items.");
                InitializeStarterItems(gender);
                return;
            }

            EnsureStarterItems(gender);
            ApplyEquippedItemsToCharacter();
            OnInventoryUpdated?.Invoke();
        }

        public void SaveInventory()
        {
            InventoryDataWrapper wrapper = new InventoryDataWrapper();
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (slots[i] != null && !string.IsNullOrEmpty(slots[i].name) && slots[i].id != "empty")
                {
                    wrapper.items.Add(slots[i]);
                }
                else
                {
                    wrapper.items.Add(new InventoryItem("empty", "", ItemType.Misc, 0, 0, 0, false));
                }
            }

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString("PlayerInventory_V4", json);
            PlayerPrefs.Save();
        }

        public void EquipItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return;
            var item = slots[slotIndex];
            if (item == null || string.IsNullOrEmpty(item.name) || item.id == "empty") return;

            if (item.isEquipped)
            {
                // Unequip
                item.isEquipped = false;
            }
            else
            {
                // Unequip all items of same type first
                for (int i = 0; i < MAX_SLOTS; i++)
                {
                    if (slots[i] != null && slots[i].type == item.type)
                    {
                        slots[i].isEquipped = false;
                    }
                }
                item.isEquipped = true;
            }

            SaveInventory();
            ApplyEquippedItemsToCharacter();
            OnInventoryUpdated?.Invoke();
        }

        public void ApplyEquippedItemsToCharacter()
        {
            int gender = PlayerPrefs.GetInt("PlayerGender", 0);
            int hairStyle = PlayerPrefs.GetInt("PlayerHairStyle", 0);
            int hairColorIdx = PlayerPrefs.GetInt("PlayerHairColor", 0);
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(hairColorIdx, 0, NetworkPlayer.HairColorPalette.Length - 1)];

            int outfitStyle = -1;
            Color bodyColor = NetworkPlayer.BodyColorPalette[0];

            int pantsStyle = -1;
            Color pantsColor = NetworkPlayer.PantsColorPalette[0];

            for (int i = 0; i < MAX_SLOTS; i++)
            {
                var item = slots[i];
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

            if (outfitStyle >= 0) PlayerPrefs.SetInt("PlayerOutfitStyle", outfitStyle);
            if (pantsStyle >= 0) PlayerPrefs.SetInt("PlayerPantsStyle", pantsStyle);
            PlayerPrefs.Save();

            // Apply 3D visuals to local character
            if (gameObject != null)
            {
                CharacterVisuals.ApplyCustomization(gameObject, gender, hairStyle, hairColor,
                    outfitStyle >= 0 ? outfitStyle : 0, bodyColor,
                    pantsStyle >= 0 ? pantsStyle : 0, pantsColor);
            }

            // Sync over network if NetworkPlayer exists
            if (_networkPlayer != null && _networkPlayer.isLocalPlayer)
            {
                _networkPlayer.CmdSetFullCustomization(gender, bodyColor, hairStyle, hairColor,
                    outfitStyle >= 0 ? outfitStyle : 0,
                    pantsStyle >= 0 ? pantsStyle : 0, pantsColor);
            }
        }
    }
}
