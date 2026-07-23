using UnityEngine;

namespace RangerCity.Lobby
{
    public partial class LobbyUI : MonoBehaviour
    {
        private static Sprite _backpackSprite;

        private static Sprite GetBackpackSprite()
        {
            if (_backpackSprite != null) return _backpackSprite;
            int sz = 128;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            Color bgCol = new Color(0.15f, 0.85f, 0.45f, 0.95f);
            Color pocketCol = new Color(0.1f, 0.65f, 0.35f, 0.95f);
            Color strapCol = new Color(0.95f, 0.85f, 0.3f, 0.95f);

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    tex.SetPixel(x, y, Color.clear);

            // Handle
            for (int y = 95; y < 112; y++)
                for (int x = 50; x < 78; x++)
                    if ((x < 58 || x > 70) || y > 104)
                        tex.SetPixel(x, y, strapCol);

            // Main Body
            for (int y = 25; y < 95; y++)
            {
                for (int x = 30; x < 98; x++)
                {
                    if (y > 75)
                    {
                        float dx = x < 64 ? (30 + 15 - x) : (x - (98 - 15));
                        float dy = y - 75;
                        if (dx > 0 && (dx * dx + dy * dy > 15 * 15)) continue;
                    }
                    tex.SetPixel(x, y, bgCol);
                }
            }

            // Pocket
            for (int y = 30; y < 60; y++)
                for (int x = 40; x < 88; x++)
                    tex.SetPixel(x, y, pocketCol);

            // Strap line
            for (int x = 38; x < 90; x++)
            {
                tex.SetPixel(x, 60, strapCol);
                tex.SetPixel(x, 61, strapCol);
            }

            tex.Apply();
            _backpackSprite = Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
            return _backpackSprite;
        }

        private static Sprite GetShirtSprite(Color color)
        {
            int sz = 64;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    tex.SetPixel(x, y, Color.clear);

            // T-Shirt Torso Body
            for (int y = 10; y <= 46; y++)
                for (int x = 18; x <= 46; x++)
                    tex.SetPixel(x, y, color);

            // Left & Right Sleeves
            for (int y = 30; y <= 46; y++)
            {
                for (int x = 8; x <= 18; x++) tex.SetPixel(x, y, color);
                for (int x = 46; x <= 56; x++) tex.SetPixel(x, y, color);
            }

            // White Collar Rim
            for (int y = 42; y <= 46; y++)
                for (int x = 26; x <= 38; x++)
                    tex.SetPixel(x, y, Color.white);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite GetPantsSprite(Color color)
        {
            int sz = 64;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    tex.SetPixel(x, y, Color.clear);

            // Waistband
            for (int y = 48; y <= 54; y++)
                for (int x = 18; x <= 46; x++)
                    tex.SetPixel(x, y, new Color(0.2f, 0.2f, 0.25f));

            // Left & Right Legs
            for (int y = 10; y <= 48; y++)
            {
                for (int x = 18; x <= 30; x++) tex.SetPixel(x, y, color);
                for (int x = 34; x <= 46; x++) tex.SetPixel(x, y, color);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite GetShoesSprite(Color color)
        {
            int sz = 64;
            var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);

            for (int y = 0; y < sz; y++)
                for (int x = 0; x < sz; x++)
                    tex.SetPixel(x, y, Color.clear);

            // Pair of Sneakers Body
            for (int y = 18; y <= 34; y++)
            {
                for (int x = 10; x <= 28; x++) tex.SetPixel(x, y, color);
                for (int x = 36; x <= 54; x++) tex.SetPixel(x, y, color);
            }

            // White Rubber Soles
            for (int y = 14; y <= 18; y++)
            {
                for (int x = 8; x <= 30; x++) tex.SetPixel(x, y, Color.white);
                for (int x = 34; x <= 56; x++) tex.SetPixel(x, y, Color.white);
            }

            // Toe Caps
            for (int y = 22; y <= 30; y++)
            {
                for (int x = 22; x <= 28; x++) tex.SetPixel(x, y, Color.white);
                for (int x = 48; x <= 54; x++) tex.SetPixel(x, y, Color.white);
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, sz, sz), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
