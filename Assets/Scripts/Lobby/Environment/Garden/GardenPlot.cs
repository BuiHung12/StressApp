using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    public enum PlotState
    {
        Empty,
        Growing,
        Ripe
    }

    public class GardenPlot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _growthDuration = 10f;
        [SerializeField] private int _harvestReward = 10;

        private PlotState _state = PlotState.Empty;
        private float _growthTimer;

        // Visuals
        private GameObject _soil;
        private GameObject _plantSprout;
        private GameObject _plantMature;
        private GameObject _billboardObj;
        private TextMeshPro _billboardText;

        private Material _greenMat;
        private Material _redMat;
        private Material _brownMat;

        private Camera _cachedCamera;

        private void Start()
        {
            CreateMaterials();
            CreateVisuals();
            UpdateVisuals();
            _cachedCamera = Camera.main;
        }

        private void Update()
        {
            if (_state == PlotState.Growing)
            {
                _growthTimer -= Time.deltaTime;
                if (_growthTimer <= 0f)
                {
                    _state = PlotState.Ripe;
                    UpdateVisuals();
                }
                else
                {
                    UpdateText();
                }
            }

            // Orient billboard towards camera (using cached ref)
            if (_billboardObj != null && _cachedCamera != null)
            {
                _billboardObj.transform.rotation = Quaternion.LookRotation(_cachedCamera.transform.forward);
            }
        }

        private void CreateMaterials()
        {
            // Use ZoneFactory cached materials instead of creating new ones per plot
            _greenMat = ZoneFactory.CreateMat(new Color(0.2f, 0.8f, 0.3f));
            _redMat = ZoneFactory.CreateMat(new Color(0.9f, 0.15f, 0.15f));
            _brownMat = ZoneFactory.CreateMat(new Color(0.45f, 0.3f, 0.15f));
        }

        private void CreateVisuals()
        {
            // Pot Base (Soil)
            _soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _soil.name = "Soil";
            _soil.transform.SetParent(transform, false);
            _soil.transform.localPosition = new Vector3(0, 0.05f, 0);
            _soil.transform.localScale = new Vector3(0.5f, 0.05f, 0.5f);
            _soil.GetComponent<Renderer>().material = _brownMat;
            Destroy(_soil.GetComponent<Collider>());

            // Outer Pot Ring (Decorative)
            var potOuter = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            potOuter.name = "OuterPot";
            potOuter.transform.SetParent(transform, false);
            potOuter.transform.localPosition = new Vector3(0, 0.04f, 0);
            potOuter.transform.localScale = new Vector3(0.58f, 0.04f, 0.58f);
            potOuter.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.7f, 0.45f, 0.35f));
            Destroy(potOuter.GetComponent<Collider>());

            // Sprout plant (Small green sphere)
            _plantSprout = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _plantSprout.name = "Sprout";
            _plantSprout.transform.SetParent(transform, false);
            _plantSprout.transform.localPosition = new Vector3(0, 0.15f, 0);
            _plantSprout.transform.localScale = Vector3.one * 0.15f;
            _plantSprout.GetComponent<Renderer>().material = _greenMat;
            Destroy(_plantSprout.GetComponent<Collider>());

            // Mature plant (Nice cartoon apple bush)
            _plantMature = new GameObject("MaturePlant");
            _plantMature.transform.SetParent(transform, false);
            _plantMature.transform.localPosition = Vector3.zero;

            // Foliage base (Larger green sphere)
            var leaves = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leaves.transform.SetParent(_plantMature.transform, false);
            leaves.transform.localPosition = new Vector3(0, 0.3f, 0);
            leaves.transform.localScale = Vector3.one * 0.35f;
            leaves.GetComponent<Renderer>().material = _greenMat;
            Destroy(leaves.GetComponent<Collider>());

            // Apple 1 (Red)
            var a1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a1.transform.SetParent(_plantMature.transform, false);
            a1.transform.localPosition = new Vector3(0.12f, 0.38f, 0.08f);
            a1.transform.localScale = Vector3.one * 0.08f;
            a1.GetComponent<Renderer>().material = _redMat;
            Destroy(a1.GetComponent<Collider>());

            // Apple 2 (Red)
            var a2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a2.transform.SetParent(_plantMature.transform, false);
            a2.transform.localPosition = new Vector3(-0.1f, 0.28f, 0.1f);
            a2.transform.localScale = Vector3.one * 0.08f;
            a2.GetComponent<Renderer>().material = _redMat;
            Destroy(a2.GetComponent<Collider>());

            // Apple 3 (Red)
            var a3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            a3.transform.SetParent(_plantMature.transform, false);
            a3.transform.localPosition = new Vector3(0.02f, 0.25f, -0.12f);
            a3.transform.localScale = Vector3.one * 0.08f;
            a3.GetComponent<Renderer>().material = _redMat;
            Destroy(a3.GetComponent<Collider>());

            // Text Billboard
            _billboardObj = new GameObject("PlotBillboard");
            _billboardObj.transform.SetParent(transform, false);
            _billboardObj.transform.localPosition = new Vector3(0, 0.8f, 0);

            _billboardText = _billboardObj.AddComponent<TextMeshPro>();
            _billboardText.fontSize = 2f;
            _billboardText.alignment = TextAlignmentOptions.Center;
            _billboardText.color = Color.white;

            // Text background card
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "TextBg";
            bg.transform.SetParent(_billboardObj.transform, false);
            bg.transform.localPosition = new Vector3(0, 0, 0.01f);
            bg.transform.localScale = new Vector3(1.2f, 0.32f, 0.005f);
            bg.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.1f, 0.1f, 0.1f, 0.8f));
            Destroy(bg.GetComponent<Collider>());
        }

        private void UpdateVisuals()
        {
            _plantSprout.SetActive(_state == PlotState.Growing);
            _plantMature.SetActive(_state == PlotState.Ripe);
            UpdateText();
        }

        private void UpdateText()
        {
            if (_billboardText == null) return;

            switch (_state)
            {
                case PlotState.Empty:
                    string hintE = Application.isMobilePlatform ? "Nhấn 💬" : "Phím E";
                    _billboardText.text = $"[+] Gieo Hạt\n({hintE})";
                    break;
                case PlotState.Growing:
                    _billboardText.text = $"Đang Lớn\n({Mathf.Ceil(_growthTimer):0}s)";
                    break;
                case PlotState.Ripe:
                    string hintH = Application.isMobilePlatform ? "Nhấn 💬" : "Phím E";
                    _billboardText.text = $"[*] Thu Hoạch\n({hintH})";
                    break;
            }
        }

        public void TryInteract(PlayerController player)
        {
            if (_state == PlotState.Empty)
            {
                // Plant seed!
                _state = PlotState.Growing;
                _growthTimer = _growthDuration;
                UpdateVisuals();

                // Sync to other players
                var netPlayer = player.GetComponent<Mirror.NetworkIdentity>()?.GetComponent<NetworkPlayer>();
                if (netPlayer != null && netPlayer.isLocalPlayer)
                {
                    netPlayer.CmdPlantSeed(GetPlotIndex());
                }
            }
            else if (_state == PlotState.Ripe)
            {
                // Harvest!
                _state = PlotState.Empty;
                UpdateVisuals();

                // Add coins to player
                player.AddCoins(_harvestReward);

                // Sync to other players
                var netPlayer = player.GetComponent<Mirror.NetworkIdentity>()?.GetComponent<NetworkPlayer>();
                if (netPlayer != null && netPlayer.isLocalPlayer)
                {
                    netPlayer.CmdHarvestPlot(GetPlotIndex());
                }

                // Spawn harvest feedback effect
                var flash = new GameObject("HarvestFlash");
                flash.transform.position = transform.position + Vector3.up * 0.3f;
                var light = flash.AddComponent<Light>();
                light.color = Color.yellow;
                light.range = 3f;
                light.intensity = 2f;
                Destroy(flash, 0.2f);
            }
        }

        /// <summary>
        /// Called by NetworkPlayer RPC to force state on remote clients.
        /// </summary>
        public void ForceSetState(PlotState newState)
        {
            _state = newState;
            if (newState == PlotState.Growing)
                _growthTimer = _growthDuration;
            UpdateVisuals();
        }

        /// <summary>
        /// Returns index of this plot among all GardenPlots in the scene.
        /// Used for network sync identification.
        /// </summary>
        public int GetPlotIndex()
        {
            var all = FindObjectsByType<GardenPlot>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == this) return i;
            }
            return -1;
        }
    }
}
