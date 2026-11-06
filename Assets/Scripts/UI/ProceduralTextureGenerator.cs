using UnityEngine;

namespace IceWorldRunner
{
    /// <summary>
    /// Generates stylized ice-world sprites and textures procedurally.
    /// Ensures 100% standalone functionality without requiring external image files.
    /// </summary>
    public static class ProceduralTextureGenerator
    {
        public static Sprite CreateIceTileSprite(int width = 64, int height = 64)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color baseIce = new Color(0.55f, 0.85f, 0.98f, 0.92f);
            Color highlightIce = new Color(0.85f, 0.96f, 1.0f, 1.0f);
            Color deepIce = new Color(0.25f, 0.60f, 0.88f, 0.95f);
            Color frostTop = new Color(0.96f, 0.98f, 1.0f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    Color c = Color.Lerp(deepIce, baseIce, ny);

                    // Frosted snow crust on top edge
                    if (y >= height - 6)
                    {
                        float snowBlend = Mathf.Clamp01((y - (height - 6)) / 5f);
                        c = Color.Lerp(c, frostTop, snowBlend);
                    }

                    // Crystalline facet highlights (diagonal shine)
                    if (Mathf.Abs(nx - (1f - ny)) < 0.08f)
                    {
                        c = Color.Lerp(c, highlightIce, 0.45f);
                    }

                    // Border bevel
                    if (x < 2 || x >= width - 2 || y < 2)
                    {
                        c = Color.Lerp(c, deepIce * 0.8f, 0.5f);
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32);
        }

        public static Sprite CreateCrackedIceSprite(int width = 64, int height = 64)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color baseIce = new Color(0.6f, 0.88f, 0.98f, 0.90f);
            Color deepIce = new Color(0.2f, 0.55f, 0.82f, 0.95f);
            Color crackColor = new Color(0.12f, 0.28f, 0.45f, 1.0f);
            Color frostWhite = new Color(0.95f, 0.98f, 1.0f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    Color c = Color.Lerp(deepIce, baseIce, ny);

                    if (y >= height - 5)
                        c = Color.Lerp(c, frostWhite, 0.8f);

                    // Jagged cracks simulation
                    bool isCrack = (Mathf.Abs(nx - 0.4f - 0.2f * Mathf.Sin(ny * 12f)) < 0.035f && ny > 0.2f) ||
                                  (Mathf.Abs(ny - 0.55f - 0.15f * Mathf.Cos(nx * 10f)) < 0.04f && nx > 0.3f && nx < 0.8f) ||
                                  (Mathf.Abs((nx + ny * 0.5f) - 0.7f) < 0.03f && ny < 0.6f);

                    if (isCrack)
                    {
                        c = crackColor;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32);
        }

        public static Sprite CreateIcicleSprite(int width = 32, int height = 64)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color iceCenter = new Color(0.85f, 0.95f, 1.0f, 0.95f);
            Color iceEdge = new Color(0.35f, 0.70f, 0.95f, 0.85f);
            Color clear = new Color(0, 0, 0, 0);

            float halfW = width * 0.5f;

            for (int y = 0; y < height; y++)
            {
                // Icicle tapers from top (y=height) down to point at y=0
                float t = (float)y / height; // 0 at tip, 1 at base
                float allowedHalfWidth = Mathf.Lerp(1.2f, halfW - 2, Mathf.Pow(t, 0.8f));

                for (int x = 0; x < width; x++)
                {
                    float distFromCenter = Mathf.Abs(x - halfW);
                    if (distFromCenter <= allowedHalfWidth)
                    {
                        float innerRatio = 1f - (distFromCenter / allowedHalfWidth);
                        Color c = Color.Lerp(iceEdge, iceCenter, innerRatio);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            // Pivot at top center so hanging icicles position easily
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 1.0f), 32);
        }

        public static Sprite CreateHazardSpikeSprite(int width = 48, int height = 48)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color spikeTip = new Color(0.95f, 0.98f, 1.0f, 1.0f);
            Color spikeBody = new Color(0.2f, 0.45f, 0.75f, 0.95f);
            Color spikeBase = new Color(0.1f, 0.25f, 0.55f, 1.0f);
            Color clear = new Color(0, 0, 0, 0);

            float halfW = width * 0.5f;

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height; // 0 at base, 1 at tip
                float allowedW = Mathf.Lerp(halfW - 2, 1.5f, t);

                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - halfW);
                    if (dx <= allowedW)
                    {
                        Color c = Color.Lerp(spikeBase, Color.Lerp(spikeBody, spikeTip, t), t);
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 32);
        }

        public static Sprite CreateCrystalSprite(int size = 48)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color glow = new Color(0.4f, 0.9f, 1.0f, 0.95f);
            Color core = new Color(0.95f, 1.0f, 1.0f, 1.0f);
            Color darkFacet = new Color(0.15f, 0.5f, 0.85f, 0.9f);

            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - center) / center;
                    float dy = Mathf.Abs(y - center) / center;

                    // Diamond shape: dx + dy <= 1.0
                    if (dx + dy <= 0.85f)
                    {
                        float inner = 1f - (dx + dy) / 0.85f;
                        Color c = Color.Lerp(glow, core, Mathf.Pow(inner, 1.5f));
                        // Left/right facet contrast
                        if (x < center)
                            c = Color.Lerp(c, darkFacet, 0.25f);

                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }

        public static Sprite CreatePlayerSprite(int width = 36, int height = 56)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            Color parka = new Color(0.15f, 0.45f, 0.75f, 1.0f);
            Color parkaTrim = new Color(0.92f, 0.95f, 0.98f, 1.0f);
            Color skin = new Color(0.98f, 0.82f, 0.70f, 1.0f);
            Color goggles = new Color(0.98f, 0.75f, 0.15f, 1.0f);
            Color boots = new Color(0.2f, 0.2f, 0.25f, 1.0f);
            Color icePick = new Color(0.8f, 0.85f, 0.9f, 1.0f);

            float cx = width * 0.5f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = Mathf.Abs(x - cx);
                    Color c = clear;

                    // Boots (y: 0 to 8)
                    if (y < 8)
                    {
                        if (dx < 10) c = boots;
                    }
                    // Legs / Parka Bottom (y: 8 to 18)
                    else if (y < 18)
                    {
                        if (dx < 11) c = parka;
                        if (y == 8 && dx < 11) c = parkaTrim;
                    }
                    // Parka Torso (y: 18 to 36)
                    else if (y < 36)
                    {
                        if (dx < 13) c = parka;
                        // Ice pick in hand
                        if (x > cx + 11 && x < cx + 15 && y > 20 && y < 34) c = icePick;
                    }
                    // Head Hood & Face (y: 36 to 52)
                    else if (y < 52)
                    {
                        if (dx < 12) c = parkaTrim; // Fluffy hood trim
                        if (dx < 9 && y > 38 && y < 48) c = skin; // Face
                        if (dx < 7 && y >= 42 && y <= 45) c = goggles; // Goggles
                    }
                    // Hood top (y: 52 to 55)
                    else if (y < 56)
                    {
                        if (dx < 8) c = parka;
                    }

                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.0f), 32);
        }

        public static Sprite CreatePlayerSlideSprite(int width = 56, int height = 28)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            Color parka = new Color(0.15f, 0.45f, 0.75f, 1.0f);
            Color parkaTrim = new Color(0.92f, 0.95f, 0.98f, 1.0f);
            Color goggles = new Color(0.98f, 0.75f, 0.15f, 1.0f);
            Color boots = new Color(0.2f, 0.2f, 0.25f, 1.0f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = clear;
                    // Low profile sliding posture
                    if (y >= 2 && y <= 22)
                    {
                        if (x >= 4 && x <= 16) c = boots; // feet trailing
                        else if (x > 16 && x <= 40) c = parka; // body sliding
                        else if (x > 40 && x <= 50 && y >= 6 && y <= 20) c = parkaTrim; // hooded head
                        if (x >= 44 && x <= 48 && y >= 10 && y <= 14) c = goggles;
                    }
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.0f), 32);
        }

        public static Sprite CreateSnowFlakeSprite(int size = 16)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color snow = new Color(1, 1, 1, 0.9f);
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (d <= center * 0.85f)
                    {
                        float alpha = Mathf.Clamp01(1f - (d / (center * 0.85f)));
                        tex.SetPixel(x, y, new Color(snow.r, snow.g, snow.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);
        }

        public static Sprite CreateMountainSprite(int width = 128, int height = 80, Color color = default)
        {
            if (color == default) color = new Color(0.2f, 0.35f, 0.55f, 0.8f);
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            Color snowCap = new Color(0.9f, 0.96f, 1f, color.a);

            for (int y = 0; y < height; y++)
            {
                float ny = (float)y / height;
                for (int x = 0; x < width; x++)
                {
                    float nx = (float)x / width;
                    // Dual mountain peaks
                    float peak1 = Mathf.Max(0, 1f - Mathf.Abs(nx - 0.35f) * 2.8f);
                    float peak2 = Mathf.Max(0, 1f - Mathf.Abs(nx - 0.75f) * 3.4f) * 0.75f;
                    float peakHeight = Mathf.Max(peak1, peak2);

                    if (ny <= peakHeight)
                    {
                        // Snow on mountain tops
                        Color c = (ny > peakHeight * 0.7f) ? snowCap : color;
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0f), 32);
        }

        public static Sprite CreatePowerUpSprite(int size = 48, Color mainColor = default)
        {
            if (mainColor == default) mainColor = new Color(1.0f, 0.55f, 0.1f, 1f); // Flame Warmth
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color clear = new Color(0, 0, 0, 0);
            float center = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (d <= center * 0.85f)
                    {
                        float inner = 1f - (d / (center * 0.85f));
                        Color c = Color.Lerp(mainColor, Color.white, Mathf.Pow(inner, 2f));
                        tex.SetPixel(x, y, c);
                    }
                    else
                    {
                        tex.SetPixel(x, y, clear);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32);
        }
    }
}
