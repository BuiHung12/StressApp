using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    public enum FishingState
    {
        Idle,
        Waiting,
        Biting
    }

    public class FishingSpot : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _minWaitTime = 3f;
        [SerializeField] private float _maxWaitTime = 5f;

        private FishingState _state = FishingState.Idle;
        public FishingState State => _state;

        private float _waitTimer;
        private GameObject _bobber;
        private GameObject _splashEffect;
        private GameObject _rodModel;
        private GameObject _billboardObj;
        private TextMeshPro _billboardText;
        private Camera _cachedCamera;

        private void Start()
        {
            if (NetworkSetup.IsHeadlessServer())
            {
                enabled = false;
                return;
            }
            CreateVisuals();
            UpdateVisuals();
            _cachedCamera = Camera.main;
        }

        private void Update()
        {
            if (_state == FishingState.Waiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0f)
                {
                    _state = FishingState.Biting;
                    UpdateVisuals();
                }
                else
                {
                    UpdateText();
                    if (_bobber != null)
                    {
                        // Bobber floats up and down in water
                        float floatY = Mathf.Sin(Time.time * 5f) * 0.05f;
                        _bobber.transform.localPosition = new Vector3(0, 0.02f + floatY, 2.2f);
                    }
                }
            }
            else if (_state == FishingState.Biting)
            {
                if (_bobber != null)
                {
                    // Rapid splash shaking bobber when fish bites
                    float shakeX = (Mathf.PingPong(Time.time * 25f, 1f) - 0.5f) * 0.12f;
                    float shakeY = Mathf.Sin(Time.time * 15f) * 0.08f;
                    _bobber.transform.localPosition = new Vector3(shakeX, 0.01f + shakeY, 2.2f);
                }
                UpdateText();
            }

            // Orient billboard banner to face camera
            if (_billboardObj != null && _cachedCamera != null)
            {
                _billboardObj.transform.rotation = Quaternion.LookRotation(_cachedCamera.transform.forward);
            }
        }

        private void CreateVisuals()
        {
            // Wooden Pier Fishing Platform Base
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = "FishingPlatform";
            platform.transform.SetParent(transform, false);
            platform.transform.localPosition = new Vector3(0, 0.08f, 0);
            platform.transform.localScale = new Vector3(1.8f, 0.12f, 1.8f);
            platform.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.45f, 0.32f, 0.2f));
            Destroy(platform.GetComponent<Collider>());

            // Fishing Stool
            var stool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stool.name = "FishingStool";
            stool.transform.SetParent(transform, false);
            stool.transform.localPosition = new Vector3(0, 0.25f, -0.3f);
            stool.transform.localScale = new Vector3(0.4f, 0.15f, 0.4f);
            stool.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.55f, 0.38f, 0.22f));
            Destroy(stool.GetComponent<Collider>());

            // Fishing Rod extending out into water
            _rodModel = new GameObject("FishingRodGroup");
            _rodModel.transform.SetParent(transform, false);
            _rodModel.transform.localPosition = new Vector3(0.2f, 0.35f, 0.2f);

            var rodPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            rodPole.transform.SetParent(_rodModel.transform, false);
            rodPole.transform.localPosition = new Vector3(0, 0.6f, 0.8f);
            rodPole.transform.localScale = new Vector3(0.03f, 0.8f, 0.03f);
            rodPole.transform.localRotation = Quaternion.Euler(55f, 0, 0);
            rodPole.GetComponent<Renderer>().material = ZoneFactory.WoodMat(new Color(0.6f, 0.42f, 0.2f));
            Destroy(rodPole.GetComponent<Collider>());

            // Water Bobber / Float
            _bobber = new GameObject("BobberGroup");
            _bobber.transform.SetParent(transform, false);
            _bobber.transform.localPosition = new Vector3(0, 0.02f, 2.2f);

            var floatTop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            floatTop.transform.SetParent(_bobber.transform, false);
            floatTop.transform.localPosition = new Vector3(0, 0.08f, 0);
            floatTop.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            floatTop.GetComponent<Renderer>().material = ZoneFactory.GlossyMat(new Color(0.95f, 0.2f, 0.2f));
            Destroy(floatTop.GetComponent<Collider>());

            var floatBottom = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            floatBottom.transform.SetParent(_bobber.transform, false);
            floatBottom.transform.localPosition = new Vector3(0, 0.02f, 0);
            floatBottom.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
            floatBottom.GetComponent<Renderer>().material = ZoneFactory.CreateMat(Color.white);
            Destroy(floatBottom.GetComponent<Collider>());

            // Water Ripple Ring around bobber
            var ripple = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ripple.transform.SetParent(_bobber.transform, false);
            ripple.transform.localPosition = Vector3.zero;
            ripple.transform.localScale = new Vector3(0.5f, 0.005f, 0.5f);
            ripple.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(1f, 1f, 1f, 0.4f));
            Destroy(ripple.GetComponent<Collider>());

            // Bait Bucket prop beside stool
            var bucket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bucket.name = "BaitBucket";
            bucket.transform.SetParent(transform, false);
            bucket.transform.localPosition = new Vector3(-0.55f, 0.22f, 0f);
            bucket.transform.localScale = new Vector3(0.3f, 0.18f, 0.3f);
            bucket.GetComponent<Renderer>().material = ZoneFactory.MetalMat(new Color(0.5f, 0.52f, 0.55f));
            Destroy(bucket.GetComponent<Collider>());

            // Text Billboard Banner
            _billboardObj = new GameObject("SpotBillboard");
            _billboardObj.transform.SetParent(transform, false);
            _billboardObj.transform.localPosition = new Vector3(0, 1.15f, 0);

            _billboardText = _billboardObj.AddComponent<TextMeshPro>();
            _billboardText.fontSize = 2.4f;
            _billboardText.alignment = TextAlignmentOptions.Center;
            _billboardText.color = Color.white;

            var bg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bg.name = "TextBg";
            bg.transform.SetParent(_billboardObj.transform, false);
            bg.transform.localPosition = new Vector3(0, 0, 0.01f);
            bg.transform.localScale = new Vector3(1.5f, 0.45f, 0.005f);
            bg.GetComponent<Renderer>().material = ZoneFactory.CreateMat(new Color(0.06f, 0.12f, 0.22f, 0.85f));
            Destroy(bg.GetComponent<Collider>());
        }

        private void UpdateVisuals()
        {
            if (_bobber != null)
            {
                _bobber.SetActive(_state != FishingState.Idle);
            }
            UpdateText();
        }

        private void UpdateText()
        {
            if (_billboardText == null) return;

            switch (_state)
            {
                case FishingState.Idle:
                    _billboardText.text = "<color=#33CCFF>[+] Thả Cần</color>";
                    break;
                case FishingState.Waiting:
                    _billboardText.text = $"<color=#FFFF33>Đang Đợi ({Mathf.Ceil(_waitTimer):0}s)</color>";
                    break;
                case FishingState.Biting:
                    _billboardText.text = "<color=#FF3366>[!] CÁ CẮN! GIẬT!</color>";
                    break;
            }
        }

        public void TryInteract(PlayerController player)
        {
            if (_state == FishingState.Idle)
            {
                // Cast line into water!
                _state = FishingState.Waiting;
                _waitTimer = Random.Range(_minWaitTime, _maxWaitTime);
                UpdateVisuals();

                var netPlayer = player.GetComponent<Mirror.NetworkIdentity>()?.GetComponent<NetworkPlayer>();
                if (netPlayer != null && netPlayer.isLocalPlayer)
                {
                    netPlayer.CmdCastFish(GetSpotIndex());
                }
            }
            else if (_state == FishingState.Biting)
            {
                // Catch Fish!
                _state = FishingState.Idle;
                UpdateVisuals();

                // Roll fish catch reward
                int roll = Random.Range(0, 100);
                string fishName;
                int rewardCoins;

                if (roll < 45)
                {
                    fishName = "Cá Vàng 🐠";
                    rewardCoins = 25;
                }
                else if (roll < 75)
                {
                    fishName = "Cá Hồng 🐟";
                    rewardCoins = 40;
                }
                else if (roll < 90)
                {
                    fishName = "Cá Mập Con 🦈";
                    rewardCoins = 75;
                }
                else
                {
                    fishName = "Rương Báu Bến Cảng 🪙";
                    rewardCoins = 150;
                }

                player.AddCoins(rewardCoins);

                var netPlayer = player.GetComponent<Mirror.NetworkIdentity>()?.GetComponent<NetworkPlayer>();
                if (netPlayer != null && netPlayer.isLocalPlayer)
                {
                    netPlayer.CmdCatchFish(GetSpotIndex());
                }

                // Spawn Catch FX Light + Fish Pop
                var flash = new GameObject("CatchFlash");
                flash.transform.position = transform.position + Vector3.up * 0.5f;
                var light = flash.AddComponent<Light>();
                light.color = new Color(0.2f, 0.8f, 1f);
                light.range = 4f;
                light.intensity = 3f;
                Destroy(flash, 0.3f);
            }
        }

        public void ForceSetState(FishingState newState)
        {
            _state = newState;
            if (newState == FishingState.Waiting) _waitTimer = 4f;
            UpdateVisuals();
        }

        public int GetSpotIndex()
        {
            var all = FindObjectsByType<FishingSpot>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] == this) return i;
            }
            return -1;
        }
    }
}
