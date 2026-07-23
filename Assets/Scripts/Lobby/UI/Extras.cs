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
        private void CreateCoinUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            var coinObj = new GameObject("CoinUI");
            coinObj.transform.SetParent(canvas.transform, false);

            var rt = coinObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.8f, 0.9f);
            rt.anchorMax = new Vector2(0.98f, 0.98f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _coinText = coinObj.AddComponent<TextMeshProUGUI>();
            _coinText.fontSize = 20;
            _coinText.alignment = TextAlignmentOptions.Right;
            _coinText.color = new Color(1f, 0.84f, 0f);
            
            var shadow = coinObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(2, -2);

            UpdateCoinUI(_player.RangerCoins);
        }

        private void UpdateCoinUI(int coins)
        {
            if (_coinText != null)
            {
                _coinText.text = $"$ Coins: {coins}";
            }
        }

        private void HideJailNotification()
        {
            if (_jailPanel != null)
                _jailPanel.SetActive(false);
        }

        private void CreateEmojiUI()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;

            _emojiToggleButton = new GameObject("EmojiToggleBtn");
            _emojiToggleButton.transform.SetParent(canvas.transform, false);

            var toggleRt = _emojiToggleButton.AddComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(1f, 0f); // Bottom-right
            toggleRt.anchorMax = new Vector2(1f, 0f);
            toggleRt.pivot = new Vector2(0.5f, 0.5f);
            toggleRt.anchoredPosition = new Vector2(-290f, 150f);
            toggleRt.sizeDelta = new Vector2(85f, 85f);

            var toggleImg = _emojiToggleButton.AddComponent<Image>();
            Color darkGlass = new Color(0.08f, 0.09f, 0.12f, 0.65f);
            Color themeColor = new Color(0.2f, 0.6f, 0.9f, 0.85f); // neon blue border
            toggleImg.sprite = CreateCircleSprite(darkGlass, themeColor, 4f);
            toggleImg.color = Color.white;

            var toggleBtn = _emojiToggleButton.AddComponent<Button>();
            toggleBtn.onClick.AddListener(ToggleEmojiPanel);
            var colors = toggleBtn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 0.95f);
            toggleBtn.colors = colors;

            var toggleTextObj = new GameObject("Text");
            toggleTextObj.transform.SetParent(_emojiToggleButton.transform, false);
            var toggleTextRt = toggleTextObj.AddComponent<RectTransform>();
            toggleTextRt.anchorMin = Vector2.zero;
            toggleTextRt.anchorMax = Vector2.one;
            toggleTextRt.offsetMin = toggleTextRt.offsetMax = Vector2.zero;
            var toggleTxt = toggleTextObj.AddComponent<TextMeshProUGUI>();
            toggleTxt.text = "<sprite=\"EmojiOne\" name=\"Grinning face\">";
            toggleTxt.fontSize = 26;
            toggleTxt.alignment = TextAlignmentOptions.Center;

            _emojiPanel = new GameObject("EmojiPanel");
            _emojiPanel.transform.SetParent(canvas.transform, false);

            var panelRt = _emojiPanel.AddComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(1f, 0f); // Bottom-right
            panelRt.anchorMax = new Vector2(1f, 0f);
            panelRt.pivot = new Vector2(0.5f, 0.5f);
            panelRt.anchoredPosition = new Vector2(-290f, 480f);
            panelRt.sizeDelta = new Vector2(416f, 416f);

            var circleSprite = CreateCircleSprite(new Color(0.1f, 0.12f, 0.18f, 0.95f), new Color(0.3f, 0.5f, 0.8f, 0.8f), 3f);
            var itemSprite = CreateCircleSprite(new Color(0.18f, 0.22f, 0.35f, 0.9f), new Color(0.4f, 0.55f, 0.85f, 0.6f), 2f);

            var panelImg = _emojiPanel.AddComponent<Image>();
            panelImg.color = Color.white;
            panelImg.sprite = circleSprite;

            var grid = _emojiPanel.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            grid.cellSize = new Vector2(92f, 92f);
            grid.spacing = new Vector2(8f, 8f);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            for (int i = 0; i < EmojiSystem.AvailableEmojis.Length; i++)
            {
                int index = i;
                var emojiBtn = new GameObject($"Emoji_{i}");
                emojiBtn.transform.SetParent(_emojiPanel.transform, false);

                var btnImg = emojiBtn.AddComponent<Image>();
                btnImg.color = Color.white;
                btnImg.sprite = itemSprite;

                var btn = emojiBtn.AddComponent<Button>();
                btn.onClick.AddListener(() => OnEmojiClicked(index));

                var btnColors = btn.colors;
                btnColors.highlightedColor = new Color(0.35f, 0.45f, 0.75f, 1f);
                btnColors.pressedColor = new Color(0.5f, 0.6f, 0.95f, 1f);
                btn.colors = btnColors;

                var emojiTextObj = new GameObject("Text");
                emojiTextObj.transform.SetParent(emojiBtn.transform, false);
                var emojiTextRt = emojiTextObj.AddComponent<RectTransform>();
                emojiTextRt.anchorMin = Vector2.zero;
                emojiTextRt.anchorMax = Vector2.one;
                emojiTextRt.offsetMin = emojiTextRt.offsetMax = Vector2.zero;
                var emojiTxt = emojiTextObj.AddComponent<TextMeshProUGUI>();
                emojiTxt.text = EmojiSystem.AvailableEmojis[i];
                emojiTxt.fontSize = 52;
                emojiTxt.alignment = TextAlignmentOptions.Center;
            }

            _emojiPanel.SetActive(false);
        }

        private void ToggleEmojiPanel()
        {
            _emojiPanelOpen = !_emojiPanelOpen;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(_emojiPanelOpen);
        }

        private void OnEmojiClicked(int index)
        {
            if (_player != null)
            {
                var emojiSystem = _player.GetComponent<EmojiSystem>();
                if (emojiSystem != null)
                {
                    emojiSystem.ShowEmoji(index);
                }
            }

            _emojiPanelOpen = false;
            if (_emojiPanel != null)
                _emojiPanel.SetActive(false);
        }

        private IEnumerator InternetCheckLoop()
        {
            while (true)
            {
                if (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    SetInternetState(false);
                }
                else
                {
                    using (UnityEngine.Networking.UnityWebRequest webRequest = UnityEngine.Networking.UnityWebRequest.Get("http://clients3.google.com/generate_204"))
                    {
                        webRequest.timeout = 3;
                        yield return webRequest.SendWebRequest();

                        if (webRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                        {
                            SetInternetState(true);
                        }
                        else
                        {
                            using (UnityEngine.Networking.UnityWebRequest dbRequest = UnityEngine.Networking.UnityWebRequest.Get("http://bore.pub:57223/api/player/health"))
                            {
                                dbRequest.timeout = 3;
                                yield return dbRequest.SendWebRequest();
                                SetInternetState(dbRequest.result == UnityEngine.Networking.UnityWebRequest.Result.Success);
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(3.0f);
            }
        }

        private void SetInternetState(bool available)
        {
            _isInternetAvailable = available;

            if (_noInternetOverlay != null)
            {
                _noInternetOverlay.SetActive(!available);
                if (!available)
                {
                    _noInternetOverlay.transform.SetAsLastSibling();
                }
            }

            if (_connectionPanel != null)
            {
                var canvasGroup = _connectionPanel.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = _connectionPanel.AddComponent<CanvasGroup>();
                canvasGroup.interactable = available;
                canvasGroup.blocksRaycasts = available;
                canvasGroup.alpha = available ? 1.0f : 0.4f;
            }
        }

        private static Sprite CreateCircleSprite(Color fillColor, Color borderColor, float borderThickness)
        {
            int sz = 128;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            float center = sz * 0.5f;
            float radius = center - 2f;
            float innerRadius = radius - borderThickness;

            for (int y = 0; y < sz; y++)
            {
                for (int x = 0; x < sz; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist > radius)
                    {
                        if (dist <= radius + 1.5f)
                        {
                            float alpha = Mathf.Clamp01(radius + 1.5f - dist);
                            tex.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, borderColor.a * alpha));
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                    else if (dist >= innerRadius)
                    {
                        tex.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        tex.SetPixel(x, y, fillColor);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }
    }

    }
