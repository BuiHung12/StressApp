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
        private IEnumerator LoadFromDatabaseServerCoroutine()
        {
            if (_isSyncingWithServer) yield break;
            _isSyncingWithServer = true;

            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string url = $"{_apiBaseUrl}?deviceId={UnityWebRequest.EscapeURL(deviceId)}";

            Debug.Log($"[LobbyUIConnection] Querying database server for device: {deviceId}...");
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                webRequest.timeout = 3; // 3 seconds timeout
                yield return webRequest.SendWebRequest();

                _isSyncingWithServer = false;

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string json = webRequest.downloadHandler.text;
                    Debug.Log($"[LobbyUIConnection] Database loaded: {json}");
                    try
                    {
                        ServerPlayerData serverData = JsonUtility.FromJson<ServerPlayerData>(json);
                        if (serverData != null)
                        {
                            _selectedGender = serverData.gender;
                            _selectedBodyColor = serverData.bodyColorIndex;
                            _selectedHairStyle = serverData.hairStyle;
                            _selectedHairColor = serverData.hairColorIndex;
                            _selectedOutfitStyle = serverData.outfitStyle;
                            _selectedPantsStyle = serverData.pantsStyle;
                            _selectedPantsColor = serverData.pantsColorIndex;

                            // Cache to PlayerPrefs
                            PlayerPrefs.SetString("PlayerName", serverData.name);
                            PlayerPrefs.SetInt("PlayerGender", _selectedGender);
                            PlayerPrefs.SetInt("PlayerColorIndex", _selectedBodyColor);
                            PlayerPrefs.SetInt("PlayerHairStyle", _selectedHairStyle);
                            PlayerPrefs.SetInt("PlayerHairColor", _selectedHairColor);
                            PlayerPrefs.SetInt("PlayerOutfitStyle", _selectedOutfitStyle);
                            PlayerPrefs.SetInt("PlayerPantsStyle", _selectedPantsStyle);
                            PlayerPrefs.SetInt("PlayerPantsColor", _selectedPantsColor);
                            PlayerPrefs.Save();

                            var scenePlayer = GameObject.FindWithTag("Player");
                            if (scenePlayer != null)
                            {
                                var inv = scenePlayer.GetComponent<PlayerInventory>();
                                if (inv == null) inv = scenePlayer.AddComponent<PlayerInventory>();
                                inv.EnsureStarterItems(_selectedGender);
                            }

                            Debug.Log($"[LobbyUIConnection] Rebuilding UI with loaded data for player: {serverData.name}");
                            RebuildConnectionUI();
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[LobbyUIConnection] Failed to parse player JSON: {e.Message}");
                    }
                }
                else
                {
                    Debug.Log($"[LobbyUIConnection] Database server unreachable or player not found. Using local PlayerPrefs fallback. Error: {webRequest.error}");
                }
            }
        }

        private IEnumerator SaveToDatabaseServerCoroutine()
        {
            string deviceId = SystemInfo.deviceUniqueIdentifier;
            string playerName = _nameInput != null ? _nameInput.text : "Ranger";

            ServerPlayerData data = new ServerPlayerData();
            data.deviceId = deviceId;
            data.name = playerName;
            data.gender = _selectedGender;
            data.bodyColorIndex = _selectedBodyColor;
            data.hairStyle = _selectedHairStyle;
            data.hairColorIndex = _selectedHairColor;
            data.outfitStyle = _selectedOutfitStyle;
            data.pantsStyle = _selectedPantsStyle;
            data.pantsColorIndex = _selectedPantsColor;

            string json = JsonUtility.ToJson(data);

            using (UnityWebRequest webRequest = new UnityWebRequest(_apiBaseUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 3;

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"[LobbyUIConnection] Successfully saved character configuration for device: {deviceId} to database server.");
                }
                else
                {
                    Debug.Log($"[LobbyUIConnection] Saved locally via PlayerPrefs (Database server offline: {webRequest.error})");
                }
            }
        }

        private void RebuildConnectionUI()
        {
            if (_connectionPanel == null) return;

            bool wasActive = _connectionPanel.activeSelf;

            if (_previewCharacter != null) Destroy(_previewCharacter);
            if (_previewCamera != null)
            {
                _previewCamera.targetTexture = null;
                Destroy(_previewCamera.gameObject);
                _previewCamera = null;
            }
            if (_previewRT != null)
            {
                if (RenderTexture.active == _previewRT) RenderTexture.active = null;
                _previewRT.Release();
                Destroy(_previewRT);
                _previewRT = null;
            }
            if (_connectionPanel != null) Destroy(_connectionPanel);
            if (_noInternetOverlay != null) Destroy(_noInternetOverlay);
            _noInternetOverlay = null;

            CreateConnectionUI();

            if (_connectionPanel != null)
            {
                _connectionPanel.SetActive(wasActive);
            }
        }

        private void LoadDbConfig()
        {
            // Always write fresh config to ensure phone doesn't use stale cached values
            string path = Path.Combine(Application.persistentDataPath, "db_config.json");
            try
            {
                DbConfig config = new DbConfig();
                string json = JsonUtility.ToJson(config, true);
                File.WriteAllText(path, json);
                _apiBaseUrl = $"http://{config.apiHost}:{config.apiPort}/api/player";
                Debug.Log($"[LobbyUIConnection] Loaded DB Config: {_apiBaseUrl}");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[LobbyUIConnection] Failed to write db_config.json: {e.Message}");
            }
        }
    }

    [System.Serializable]
    public class ServerPlayerData
    {
        public string deviceId;
        public string name;
        public int gender;
        public int bodyColorIndex;
        public int hairStyle;
        public int hairColorIndex;
        public int outfitStyle;
        public int pantsStyle;
        public int pantsColorIndex;
    }

    [System.Serializable]
    public class DbConfig
    {
        public string apiHost = "bore.pub";
        public int apiPort = 57223;
    }

    }
