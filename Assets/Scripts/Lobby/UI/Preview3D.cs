using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using Mirror;

namespace RangerCity.Lobby
{
    public partial class LobbyUI : MonoBehaviour
    {
        private void BuildHairTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU TÓC", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 6; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Hair_{i}", NetworkPlayer.MaleHairStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedHairStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedHairStyle = idx; RefreshHairStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "HColorLabel", "MÀU TÓC", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.HairColorPalette, _selectedHairColor, cy - 20f,
                (idx) => { _selectedHairColor = idx; UpdatePreview(); }, "HairCol");
        }

        private void BuildOutfitTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU ÁO", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 5; i++)
            {
                int idx = i;
                int row = i / 3; int col = i % 3;
                float x = -130f + col * 130f;
                float yPos = y - row * 75f;

                var slot = CreateStyleSlot(parent, $"Outfit_{i}", NetworkPlayer.MaleOutfitStyleNames[i], new Vector2(x, yPos), new Vector2(110, 66),
                    i == _selectedOutfitStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedOutfitStyle = idx; RefreshOutfitStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "OColorLabel", "MÀU ÁO", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.BodyColorPalette, _selectedBodyColor, cy - 20f,
                (idx) => { _selectedBodyColor = idx; UpdatePreview(); }, "BodyCol");
        }

        private void BuildPantsTab(Transform parent)
        {
            MakeText(parent, "StyleLabel", "KIỂU QUẦN", 14,
                new Vector2(0, 115), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));

            float y = 70f;
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                int col = i;
                float x = -145f + col * 97f;

                var slot = CreateStyleSlot(parent, $"Pants_{i}", NetworkPlayer.MalePantsStyleNames[i], new Vector2(x, y), new Vector2(85, 66),
                    i == _selectedPantsStyle);
                slot.GetComponent<Button>().onClick.AddListener(() => { _selectedPantsStyle = idx; RefreshPantsStyleHighlight(); UpdatePreview(); });
            }

            float cy = -75f;
            MakeText(parent, "PColorLabel", "MÀU QUẦN", 14,
                new Vector2(0, cy + 15), new Vector2(380, 20), TextAlignmentOptions.Left, new Color(0.55f, 0.6f, 0.75f));
            BuildColorRow(parent, NetworkPlayer.PantsColorPalette, _selectedPantsColor, cy - 20f,
                (idx) => { _selectedPantsColor = idx; UpdatePreview(); }, "PantsCol");
        }

        private void BuildColorRow(Transform parent, Color[] palette, int selected, float yPos, System.Action<int> onSelect, string prefix)
        {
            float swatchSize = 34f;
            float spacing = 6f;
            float totalWidth = palette.Length * (swatchSize + spacing) - spacing;
            float startX = -totalWidth / 2f + swatchSize / 2f;

            for (int i = 0; i < palette.Length; i++)
            {
                int idx = i;
                var swatch = CreatePanel(parent, $"{prefix}_{i}", new Vector2(startX + i * (swatchSize + spacing), yPos), new Vector2(swatchSize, swatchSize));
                swatch.GetComponent<Image>().color = palette[i];
                swatch.GetComponent<Image>().raycastTarget = true;

                var btn = swatch.AddComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    onSelect(idx);
                    RefreshColorHighlights(parent, idx, palette.Length, prefix);
                });

                var ring = CreatePanel(swatch.transform, "Ring", Vector2.zero, new Vector2(swatchSize + 6, swatchSize + 6));
                ring.transform.SetAsFirstSibling();
                var ringImg = ring.GetComponent<Image>();
                ringImg.color = Color.clear;
                ringImg.raycastTarget = false; // CRITICAL: Never block button clicks!

                var outline = ring.AddComponent<Outline>();
                outline.effectColor = (i == selected) ? Color.white : Color.clear;
                outline.effectDistance = new Vector2(2, -2);
            }
        }

        private void RefreshColorHighlights(Transform parent, int selectedIdx, int count, string prefix)
        {
            for (int i = 0; i < count; i++)
            {
                var swatch = parent.Find($"{prefix}_{i}");
                if (swatch != null)
                {
                    var ring = swatch.Find("Ring");
                    if (ring != null)
                    {
                        var outline = ring.GetComponent<Outline>();
                        if (outline != null) outline.effectColor = (i == selectedIdx) ? Color.white : Color.clear;
                    }
                }
            }
        }

        private GameObject CreateStyleSlot(Transform parent, string name, string label, Vector2 pos, Vector2 size, bool selected)
        {
            var slot = CreatePanel(parent, name, pos, size, true);
            var slotImg = slot.GetComponent<Image>();
            slotImg.color = selected ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
            slot.AddComponent<Button>();

            MakeText(slot.transform, "Label", label, 14, Vector2.zero, new Vector2(size.x - 10, size.y - 10), TextAlignmentOptions.Center, Color.white);
            return slot;
        }

        private void CreatePreviewCharacter(Transform parent)
        {
            _previewRT = new RenderTexture(512, 512, 16);
            _previewRT.Create();

            var rawObj = new GameObject("PreviewRawImage", typeof(RectTransform));
            rawObj.transform.SetParent(parent, false);
            var rawRt = rawObj.GetComponent<RectTransform>();
            rawRt.anchoredPosition = new Vector2(0, 30);
            rawRt.sizeDelta = new Vector2(300, 320);
            var rawImg = rawObj.AddComponent<RawImage>();
            rawImg.texture = _previewRT;
            rawImg.raycastTarget = false;

            _previewCharacter = new GameObject("PreviewChar");
            _previewCharacter.transform.position = new Vector3(200f, 0f, 200f);

            var camObj = new GameObject("PreviewCamera");
            camObj.transform.SetParent(_previewCharacter.transform, false);
            camObj.transform.localPosition = new Vector3(0f, 0.52f, 1.25f);
            camObj.transform.localRotation = Quaternion.Euler(6f, 180f, 0f);

            _previewCamera = camObj.AddComponent<Camera>();
            _previewCamera.targetTexture = _previewRT;
            _previewCamera.depth = -5f;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.06f, 0.06f, 0.1f, 1f);
            _previewCamera.orthographic = true;
            _previewCamera.orthographicSize = 0.52f;

            var lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(camObj.transform, false);
            lightObj.transform.localPosition = new Vector3(0.5f, 0.5f, -0.5f);
            var previewLight = lightObj.AddComponent<Light>();
            previewLight.type = LightType.Point;
            previewLight.range = 5f;
            previewLight.intensity = 2f;
            previewLight.color = Color.white;

            var setup = FindAnyObjectByType<LobbySetup>();
            if (setup != null)
            {
                var charObj = CharacterVisuals.CreateCharacterTopDown("PreviewModel",
                    NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)],
                    new Color(1f, 0.88f, 0.7f));
                if (charObj != null)
                {
                    charObj.transform.SetParent(_previewCharacter.transform, false);
                    charObj.transform.localPosition = Vector3.zero;
                    charObj.transform.localRotation = Quaternion.identity;
                    charObj.transform.localScale = Vector3.one * 0.45f;

                    var anim = charObj.GetComponent<CharacterAnimator>();
                    if (anim != null) Destroy(anim);
                }
            }

            UpdatePreview();
            StartCoroutine(RotatePreview());
        }

        private IEnumerator RotatePreview()
        {
            while (_previewCharacter != null && _connectionPanel != null && _connectionPanel.activeSelf)
            {
                _previewRotation += 35f * Time.deltaTime;
                _previewCharacter.transform.rotation = Quaternion.Euler(0, _previewRotation, 0);
                yield return null;
            }
        }

        private void UpdatePreview()
        {
            if (_previewCharacter == null) return;

            var model = _previewCharacter.transform.Find("PreviewModel")?.gameObject;
            if (model == null) return;

            Color bodyColor = NetworkPlayer.BodyColorPalette[Mathf.Clamp(_selectedBodyColor, 0, NetworkPlayer.BodyColorPalette.Length - 1)];
            Color hairColor = NetworkPlayer.HairColorPalette[Mathf.Clamp(_selectedHairColor, 0, NetworkPlayer.HairColorPalette.Length - 1)];
            Color pantsColor = NetworkPlayer.PantsColorPalette[Mathf.Clamp(_selectedPantsColor, 0, NetworkPlayer.PantsColorPalette.Length - 1)];

            CharacterVisuals.ApplyCustomization(model, _selectedGender, _selectedHairStyle, hairColor, _selectedOutfitStyle, bodyColor, _selectedPantsStyle, pantsColor);
        }

        private void RefreshGenderUI()
        {
            PlayerPrefs.SetInt("PlayerGender", _selectedGender);
            PlayerPrefs.Save();

            for (int i = 0; i < 2; i++)
            {
                if (_genderButtons[i] != null)
                {
                    _genderButtons[i].color = i == _selectedGender
                        ? new Color(0.25f, 0.45f, 0.8f, 0.9f)
                        : new Color(0.18f, 0.18f, 0.24f, 0.8f);
                }
            }

            if (_tabHairContent != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    var slot = _tabHairContent.transform.Find($"Hair_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MaleHairStyleNames[i] : NetworkPlayer.FemaleHairStyleNames[i];
                    }
                }
            }

            if (_tabOutfitContent != null)
            {
                for (int i = 0; i < 5; i++)
                {
                    var slot = _tabOutfitContent.transform.Find($"Outfit_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MaleOutfitStyleNames[i] : NetworkPlayer.FemaleOutfitStyleNames[i];
                    }
                }
            }

            if (_tabPantsContent != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    var slot = _tabPantsContent.transform.Find($"Pants_{i}");
                    if (slot != null)
                    {
                        var tmp = slot.GetComponentInChildren<TextMeshProUGUI>();
                        if (tmp != null)
                            tmp.text = _selectedGender == 0 ? NetworkPlayer.MalePantsStyleNames[i] : NetworkPlayer.FemalePantsStyleNames[i];
                    }
                }
            }

            UpdatePreview();
        }

        private void SwitchTab(int tabIndex)
        {
            _activeTab = tabIndex;
            if (_tabHairContent != null) _tabHairContent.SetActive(tabIndex == 0);
            if (_tabOutfitContent != null) _tabOutfitContent.SetActive(tabIndex == 1);
            if (_tabPantsContent != null) _tabPantsContent.SetActive(tabIndex == 2);

            for (int i = 0; i < _tabButtons.Length; i++)
            {
                if (_tabButtons[i] != null)
                    _tabButtons[i].color = i == tabIndex
                        ? new Color(0.25f, 0.45f, 0.8f, 0.9f)
                        : new Color(0.18f, 0.18f, 0.24f, 0.8f);
            }
        }

        private void RefreshHairStyleHighlight()
        {
            if (_tabHairContent == null) return;
            for (int i = 0; i < 6; i++)
            {
                var slot = _tabHairContent.transform.Find($"Hair_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedHairStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
        }

        private void RefreshOutfitStyleHighlight()
        {
            if (_tabOutfitContent == null) return;
            for (int i = 0; i < 5; i++)
            {
                var slot = _tabOutfitContent.transform.Find($"Outfit_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedOutfitStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
        }

        private void RefreshPantsStyleHighlight()
        {
            if (_tabPantsContent == null) return;
            for (int i = 0; i < 4; i++)
            {
                var slot = _tabPantsContent.transform.Find($"Pants_{i}");
                if (slot != null)
                {
                    var img = slot.GetComponent<Image>();
                    if (img != null)
                        img.color = i == _selectedPantsStyle
                            ? new Color(0.25f, 0.45f, 0.8f, 0.9f) : new Color(0.15f, 0.15f, 0.22f, 0.7f);
                }
            }
        }


    }
    }
