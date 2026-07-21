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
        public PlotState State => _state;
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
            if (NetworkSetup.IsHeadlessServer())
            {
                enabled = false;
                return;
            }
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
                    if (_plantMature != null)
                    {
                        _plantMature.transform.localScale = Vector3.one * 1.5f; // Pop effect
                    }
                }
                else
                {
                    UpdateText();
                    if (_plantSprout != null)
                    {
                        // Pulse the sprout scale during growth
                        float pulse = 1f + Mathf.Sin(Time.time * 6f) * 0.15f;
                        _plantSprout.transform.localScale = Vector3.one * (0.15f * pulse);
                    }
                }
            }
            else if (_state == PlotState.Ripe)
            {
                if (_plantMature != null)
                {
                    // Rotate and bounce the plant when ripe
                    _plantMature.transform.localRotation = Quaternion.Euler(0, Time.time * 35f, 0);
                    float bounce = 1f + Mathf.Sin(Time.time * 4f) * 0.08f;
                    _plantMature.transform.localScale = Vector3.Lerp(_plantMature.transform.localScale, Vector3.one * bounce, Time.deltaTime * 5f);
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
            _greenMat = ZoneFactory.CreateMat(new Color(0.18f, 0.72f, 0.25f));
            _redMat = ZoneFactory.GlossyMat(new Color(0.95f, 0.18f, 0.18f));
            _brownMat = ZoneFactory.CreateMat(new Color(0.28f, 0.18f, 0.09f));
        }

        private void CreateVisuals()
        {
            // Planter Box Frame (Rich dark wood)
            var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frame.name = "PlanterBoxFrame";
            frame.transform.SetParent(transform, false);
            frame.transform.localPosition = new Vector3(0, 0.06f, 0);
            frame.transform.localScale = new Vector3(2.4f, 0.12f, 1.8f);
            frame.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.42f, 0.28f, 0.14f));
            Destroy(frame.GetComponent<Collider>());

            // Inner Fertile Soil Bed
            _soil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _soil.name = "SoilBed";
            _soil.transform.SetParent(transform, false);
            _soil.transform.localPosition = new Vector3(0, 0.08f, 0);
            _soil.transform.localScale = new Vector3(2.22f, 0.10f, 1.62f);
            _soil.GetComponent<Renderer>().material = _brownMat;
            Destroy(_soil.GetComponent<Collider>());

            // Corner Metallic Brackets
            Vector3[] cornerOffsets = {
                new(1.18f, 0.07f, 0.88f), new(-1.18f, 0.07f, 0.88f),
                new(1.18f, 0.07f, -0.88f), new(-1.18f, 0.07f, -0.88f)
            };
            foreach (var offset in cornerOffsets)
            {
                var bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = "CornerBracket";
                bracket.transform.SetParent(transform, false);
                bracket.transform.localPosition = offset;
                bracket.transform.localScale = new Vector3(0.1f, 0.14f, 0.1f);
                bracket.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.25f, 0.25f, 0.28f));
                Destroy(bracket.GetComponent<Collider>());
            }

            // Sprout Plant (Cute multi-leaf seedling)
            _plantSprout = new GameObject("SproutGroup");
            _plantSprout.transform.SetParent(transform, false);
            _plantSprout.transform.localPosition = new Vector3(0, 0.14f, 0);

            Vector3[] sproutPos = { new(0, 0, 0), new(-0.35f, 0, 0.2f), new(0.35f, 0, -0.2f) };
            foreach (var pos in sproutPos)
            {
                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(_plantSprout.transform, false);
                stem.transform.localPosition = pos + new Vector3(0, 0.06f, 0);
                stem.transform.localScale = new Vector3(0.03f, 0.06f, 0.03f);
                stem.GetComponent<Renderer>().material = _greenMat;
                Destroy(stem.GetComponent<Collider>());

                var leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                leaf.transform.SetParent(_plantSprout.transform, false);
                leaf.transform.localPosition = pos + new Vector3(0, 0.12f, 0);
                leaf.transform.localScale = new Vector3(0.18f, 0.08f, 0.12f);
                leaf.GetComponent<Renderer>().material = _greenMat;
                Destroy(leaf.GetComponent<Collider>());
            }

            // Mature Plant (3 Crop Varieties based on plot index position)
            _plantMature = new GameObject("MaturePlant");
            _plantMature.transform.SetParent(transform, false);
            _plantMature.transform.localPosition = Vector3.zero;

            int plotIndex = GetPlotIndex();
            int cropType = (plotIndex >= 0 ? plotIndex : Mathf.Abs(gameObject.GetInstanceID())) % 3;

            if (cropType == 0)
            {
                // Crop Variety 0: Carrot Patch 🥕
                Vector3[] carrotOffsets = { new(-0.5f, 0.14f, 0.3f), new(0.4f, 0.14f, 0.25f), new(-0.3f, 0.14f, -0.3f), new(0.5f, 0.14f, -0.25f) };
                var carrotMat = ZoneFactory.CreateMat(new Color(0.98f, 0.45f, 0.05f));
                foreach (var pos in carrotOffsets)
                {
                    // Carrot Root Top
                    var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    root.transform.SetParent(_plantMature.transform, false);
                    root.transform.localPosition = pos;
                    root.transform.localScale = new Vector3(0.14f, 0.12f, 0.14f);
                    root.GetComponent<Renderer>().material = carrotMat;
                    Destroy(root.GetComponent<Collider>());

                    // Leafy Top
                    var foliage = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    foliage.transform.SetParent(_plantMature.transform, false);
                    foliage.transform.localPosition = pos + new Vector3(0, 0.16f, 0);
                    foliage.transform.localScale = new Vector3(0.22f, 0.28f, 0.22f);
                    foliage.GetComponent<Renderer>().material = _greenMat;
                    Destroy(foliage.GetComponent<Collider>());
                }
            }
            else if (cropType == 1)
            {
                // Crop Variety 1: Tomato Bush 🍅
                var mainBush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mainBush.transform.SetParent(_plantMature.transform, false);
                mainBush.transform.localPosition = new Vector3(0, 0.35f, 0);
                mainBush.transform.localScale = new Vector3(0.75f, 0.55f, 0.65f);
                mainBush.GetComponent<Renderer>().material = _greenMat;
                Destroy(mainBush.GetComponent<Collider>());

                Vector3[] tomOffsets = {
                    new(0.25f, 0.38f, 0.22f), new(-0.28f, 0.28f, 0.2f),
                    new(0.18f, 0.25f, -0.25f), new(-0.2f, 0.42f, -0.15f), new(0.0f, 0.48f, 0.05f)
                };
                foreach (var tPos in tomOffsets)
                {
                    var tom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tom.transform.SetParent(_plantMature.transform, false);
                    tom.transform.localPosition = tPos;
                    tom.transform.localScale = Vector3.one * 0.15f;
                    tom.GetComponent<Renderer>().material = _redMat;
                    Destroy(tom.GetComponent<Collider>());
                }
            }
            else
            {
                // Crop Variety 2: Golden Pumpkin Patch 🎃
                var pumpkinMat = ZoneFactory.GlossyMat(new Color(0.96f, 0.55f, 0.05f));
                var p1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p1.transform.SetParent(_plantMature.transform, false);
                p1.transform.localPosition = new Vector3(0, 0.28f, 0);
                p1.transform.localScale = new Vector3(0.65f, 0.45f, 0.65f);
                p1.GetComponent<Renderer>().material = pumpkinMat;
                Destroy(p1.GetComponent<Collider>());

                var stem = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stem.transform.SetParent(_plantMature.transform, false);
                stem.transform.localPosition = new Vector3(0, 0.52f, 0);
                stem.transform.localScale = new Vector3(0.06f, 0.08f, 0.06f);
                stem.transform.localRotation = Quaternion.Euler(0, 0, 15f);
                stem.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.2f, 0.5f, 0.15f));
                Destroy(stem.GetComponent<Collider>());

                // Vines around
                var vine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                vine.transform.SetParent(_plantMature.transform, false);
                vine.transform.localPosition = new Vector3(0, 0.12f, 0);
                vine.transform.localScale = new Vector3(0.9f, 0.02f, 0.7f);
                vine.GetComponent<Renderer>().material = _greenMat;
                Destroy(vine.GetComponent<Collider>());
            }

            // Text Billboard UI Banner
            _billboardObj = new GameObject("PlotBillboard");
            _billboardObj.transform.SetParent(transform, false);
            _billboardObj.transform.localPosition = new Vector3(0, 0.95f, 0);

            _billboardText = _billboardObj.AddComponent<TextMeshPro>();
            _billboardText.fontSize = 2.4f;
            _billboardText.alignment = TextAlignmentOptions.Center;
            _billboardText.color = Color.white;

            // Glassmorphic background card
            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "TextBg";
            bg.transform.SetParent(_billboardObj.transform, false);
            bg.transform.localPosition = new Vector3(0, 0, 0.01f);
            bg.transform.localScale = new Vector3(1.4f, 0.45f, 0.005f);
            bg.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.06f, 0.09f, 0.15f, 0.85f));
            Destroy(bg.GetComponent<Collider>());
        }

        private void UpdateVisuals()
        {
            if (_plantSprout != null) _plantSprout.SetActive(_state == PlotState.Growing);
            if (_plantMature != null)
            {
                _plantMature.SetActive(_state == PlotState.Ripe);
                if (_state != PlotState.Ripe)
                {
                    _plantMature.transform.localScale = Vector3.one;
                    _plantMature.transform.localRotation = Quaternion.identity;
                }
            }
            UpdateText();
        }

        private void UpdateText()
        {
            if (_billboardText == null) return;

            switch (_state)
            {
                case PlotState.Empty:
                    _billboardText.text = "<color=#00FF66>[+] Gieo Hạt</color>";
                    break;
                case PlotState.Growing:
                    _billboardText.text = $"<color=#FFFF33>Đang Lớn ({Mathf.Ceil(_growthTimer):0}s)</color>";
                    break;
                case PlotState.Ripe:
                    _billboardText.text = "<color=#FFDD00>[!] Thu Hoạch</color>";
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
