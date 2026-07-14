using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    public class CloudLayer : MonoBehaviour
    {
        [Header("Unlock Settings")]
        [SerializeField] private int _unlockCost = 10;
        [SerializeField] private bool _isLocked = true;

        [Header("References")]
        [SerializeField] private GameObject _barrierObj;

        private GameObject _billboardObj;
        private TextMeshPro _billboardText;
        private GameObject _signBg;

        private void Start()
        {
            CreateBillboard();
            UpdateVisuals();
        }

        private void Update()
        {
            // Simple distance check to orient billboard towards camera
            if (_billboardObj != null && Camera.main != null)
            {
                _billboardObj.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
            }
        }

        private void CreateBillboard()
        {
            _billboardObj = new GameObject("LockBillboard");
            _billboardObj.transform.SetParent(transform, false);
            // Hover above cloud stair/edge
            _billboardObj.transform.localPosition = new Vector3(0, 1f, 0);

            _billboardText = _billboardObj.AddComponent<TextMeshPro>();
            _billboardText.fontSize = 2f;
            _billboardText.alignment = TextAlignmentOptions.Center;
            _billboardText.color = Color.white;

            // Glass background card
            _signBg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _signBg.name = "SignBg";
            _signBg.transform.SetParent(_billboardObj.transform, false);
            _signBg.transform.localPosition = new Vector3(0, 0, 0.01f);
            _signBg.transform.localScale = new Vector3(1.8f, 0.4f, 0.005f);
            _signBg.GetComponent<Renderer>().material = CreateMat(new Color(0.6f, 0.15f, 0.15f, 0.85f)); // Lock Red
            Destroy(_signBg.GetComponent<Collider>());
        }

        private Material CreateMat(Color c)
        {
            var shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Standard");
            var mat = new Material(shader);
            mat.color = c;
            return mat;
        }

        private void UpdateVisuals()
        {
            if (_barrierObj != null)
            {
                _barrierObj.SetActive(_isLocked);
            }

            if (_billboardObj != null)
            {
                _billboardObj.SetActive(_isLocked);
            }

            if (_isLocked && _billboardText != null)
            {
                _billboardText.text = $"Mở Khóa\n{_unlockCost} Coins (Phím E)";
            }
        }

        public bool IsLocked => _isLocked;

        public void TryInteract(PlayerController player)
        {
            if (!_isLocked) return;

            if (player.RangerCoins >= _unlockCost)
            {
                // Subtract coins and unlock!
                player.AddCoins(-_unlockCost);
                _isLocked = false;
                UpdateVisuals();

                // Spawn unlock feedback flash
                var flash = new GameObject("UnlockFlash");
                flash.transform.position = transform.position + Vector3.up * 0.5f;
                var light = flash.AddComponent<Light>();
                light.color = Color.green;
                light.range = 5f;
                light.intensity = 3f;
                Destroy(flash, 0.2f);
            }
            else
            {
                // Show temporary warning on billboard
                StopAllCoroutines();
                StartCoroutine(WarningCoroutine());
            }
        }

        private System.Collections.IEnumerator WarningCoroutine()
        {
            if (_billboardText != null && _signBg != null)
            {
                _billboardText.text = "⚠️ Không Đủ Coins!\nCần thêm coins";
                _signBg.GetComponent<Renderer>().material.color = new Color(0.9f, 0.1f, 0.1f, 0.95f);
                yield return new WaitForSeconds(1.5f);
                UpdateVisuals();
                _signBg.GetComponent<Renderer>().material.color = new Color(0.6f, 0.15f, 0.15f, 0.85f);
            }
        }
    }
}
