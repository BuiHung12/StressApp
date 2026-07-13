using UnityEngine;
using TMPro;

namespace RangerCity.Lobby
{
    /// <summary>
    /// Hiệu ứng đấm kiểu comic: "POW!", "BANG!", "OUCH!" bay lên và mờ đi.
    /// Tự động spawn tại vị trí bị đấm.
    /// </summary>
    public class ComicPunchEffect : MonoBehaviour
    {
        private static readonly string[] PUNCH_WORDS = {
            "POW!", "BANG!", "OUCH!", "BAM!", "WHAM!", "ZAP!", "BOP!", "BONK!"
        };

        private static readonly Color[] EFFECT_COLORS = {
            new(1f, 0.2f, 0.2f),    // Red
            new(1f, 0.6f, 0f),       // Orange
            new(1f, 0.9f, 0f),       // Yellow
            new(0.2f, 0.8f, 1f),     // Cyan
        };

        /// <summary>
        /// Spawn comic effect tại vị trí world.
        /// </summary>
        public static void SpawnAt(Vector3 worldPosition)
        {
            var effectObj = new GameObject("PunchEffect");
            effectObj.transform.position = worldPosition + Vector3.up * 2f;
            var effect = effectObj.AddComponent<ComicPunchEffect>();
            effect.Initialize();
        }

        private TextMeshPro _text;
        private float _lifetime = 1.2f;
        private float _elapsed;
        private Vector3 _startScale;
        private Vector3 _startPos;

        private void Initialize()
        {
            // Random word and color
            string word = PUNCH_WORDS[Random.Range(0, PUNCH_WORDS.Length)];
            Color color = EFFECT_COLORS[Random.Range(0, EFFECT_COLORS.Length)];

            // Create 3D text (visible in world)
            _text = gameObject.AddComponent<TextMeshPro>();
            _text.text = word;
            _text.fontSize = 8;
            _text.alignment = TextAlignmentOptions.Center;
            _text.color = color;
            _text.fontStyle = FontStyles.Bold;

            var rt = _text.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(5, 2);

            // Start small, scale up
            _startScale = Vector3.one * 0.3f;
            transform.localScale = _startScale;
            _startPos = transform.position;

            // Random slight offset
            transform.position += new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = _elapsed / _lifetime;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            // Scale: pop up quickly then stay
            float scaleT = Mathf.Min(t * 4f, 1f); // Quick pop in first 25% of lifetime
            float scale = Mathf.Lerp(0.3f, 1.5f, Mathf.SmoothStep(0, 1, scaleT));
            // Then slight shrink at end
            if (t > 0.7f) scale *= Mathf.Lerp(1f, 0.5f, (t - 0.7f) / 0.3f);
            transform.localScale = Vector3.one * scale;

            // Float upward
            transform.position = _startPos + Vector3.up * t * 1.5f;

            // Fade out in last 40%
            if (t > 0.6f && _text != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                Color c = _text.color;
                c.a = alpha;
                _text.color = c;
            }

            // Billboard (face camera)
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
