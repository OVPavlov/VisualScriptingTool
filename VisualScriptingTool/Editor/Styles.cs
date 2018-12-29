using UnityEngine;

namespace NodeEditor
{
    public static class Styles
    {
        static bool _initializd;
        public const int CellSize = 16;
        public static Color BackgroundColor;
        public static Color NodeColor;
        public static GUIStyle Background;
        public static GUIStyle Node;
        public static GUIStyle NodeActive;
        public static GUIStyle DefaultValue;
        public static GUIStyle Title;
        public static GUIStyle GraphicPoint;
        public static GUIStyle GraphicPointActive;
        public static GUIStyle BigMessageLabel;
        public static GUIStyle ExternalIndicator;
        public static Texture2D ExternalIndicatorImage;
        public static Texture2D ExternalIndicatorImageShadow;

        public static Color[] TypeColors;

        public static Texture2D BackgroundGrid;

        public static Vector2 InputShift;
        public static Vector2 OutputShift;
        public static float InputLinkShift;
        public static float OutputLinkShift;
        public static float IOSize;

        public static void Initialize()
        {
            if (BackgroundGrid != null && Background != null) return;

            const float bg = 0.35f;
            const float lines = 0.3f;
            BackgroundGrid = GetGridTexture((int)CellSize, new Color(bg, bg, bg, 1f), new Color(lines, lines, lines, 1f));


            Background = new GUIStyle("flow background");

            Node = new GUIStyle("flow node 0");//TE NodeBox
            NodeActive = new GUIStyle("flow node 0 on");

            DefaultValue = new GUIStyle("miniButton");


            Title = new GUIStyle("ShurikenLabel");
            Title.alignment = TextAnchor.LowerCenter;
            Title.fontStyle = FontStyle.Bold;
            Title.normal.textColor = Gray(1f, 1f);
            //Title.normal.background = GetRectTexture(Gray(0, 0.2f), Gray(0.0f, 0.4f));
            Title.border = new RectOffset(1, 1, 1, 1);

            NodeEditorConfig config = NodeEditorConfig.Instance;
            if (config == null) config = GetDefaultConfig();
            var ioConfig = config.InputsOutputs;
            if (config.InputsOutputs.Shape == ShapeType.Round)
            {
                GraphicPoint = new GUIStyle();
                GraphicPoint.margin = GraphicPoint.padding = new RectOffset();
                GraphicPoint.normal.background = GetRoundTexture(Gray(0.8f, 0), Gray(0.8f, 1), ioConfig.Edge, ioConfig.Size);
                GraphicPoint.border = new RectOffset();
                GraphicPointActive = new GUIStyle(GraphicPoint);
                GraphicPointActive.normal.background = GetRoundTexture(Gray(1, 0), Gray(1, 1), ioConfig.Edge, ioConfig.Size);
            }
            else
            {
                GraphicPoint = new GUIStyle("box");//flow overlay box, HelpBox
                GraphicPoint.normal.background = GetRectTexture(Gray(0.8f, 1f), Gray(0.4f, 1f));
                GraphicPoint.border = new RectOffset(1, 1, 1, 1);
                GraphicPointActive = new GUIStyle(GraphicPoint);
                GraphicPointActive.normal.background = GetRectTexture(Gray(1, 1f), Gray(0.6f, 1f));
            }
            float halfSize = ioConfig.Size * 0.5f;
            InputShift = new Vector2(-ioConfig.Size - ioConfig.Padding, CellSize * 0.5f - halfSize);
            OutputShift = new Vector2(ioConfig.Padding, -halfSize);
            InputLinkShift = -halfSize - ioConfig.Padding;
            OutputLinkShift = halfSize + ioConfig.Padding;
            IOSize = ioConfig.Size;

            TypeColors = config.Colors;
            BackgroundColor = config.BackgroundColor;
            NodeColor = config.NodeColor;


            BigMessageLabel = new GUIStyle("HeaderLabel");
            BigMessageLabel.fontSize = 32;
            BigMessageLabel.alignment = TextAnchor.MiddleCenter;


            ExternalIndicator = new GUIStyle();
            ExternalIndicator.imagePosition = ImagePosition.ImageOnly;
            ExternalIndicator.alignment = TextAnchor.UpperRight;
            ExternalIndicator.padding = new RectOffset(3, 3, 3, 3);
            ExternalIndicatorImage = GetRoundTexture(Gray(1, 0), Gray(1, 1), 0.6f, 8);
            ExternalIndicatorImageShadow = GetRoundTexture(Gray(1, 0), Gray(1, 1), 0.2f, 8);
        }

        public static void SetDirty()
        {
            BackgroundGrid = null;
            Background = null;
            var windows = Resources.FindObjectsOfTypeAll<NodeEditorWindow>();
            foreach (var window in windows)
                window.Repaint();
        }

        public static Color GetColor(ValueType type)
        {
            return TypeColors[(int)type];
        }

        static Texture2D GetGridTexture(int size, Color background, Color lines)
        {
            Color[] colors = new Color[size * size];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = background;

            for (int i = 0; i < size; i++)
                colors[i] = lines;

            for (int i = 0; i < colors.Length; i += size)
                colors[i] = lines;


            Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, true);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(colors);
            tex.Apply(true);
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }

        static Texture2D GetRectTexture(Color background, Color lines)
        {
            Color[] colors = new Color[]
            {
                lines, lines, lines,
                lines, background, lines,
                lines, lines, lines,
            };
            Texture2D tex = new Texture2D(3, 3);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(colors);
            tex.Apply(false);
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }


        static Texture2D GetRoundTexture(Color background, Color foreground, float edge, int size)
        {
            Color[] colors = new Color[size * size];

            for (int y = 0, i = 0; y < size; y++)
                for (int x = 0; x < size; x++,i++)
                {
                    float fx = x / (float)size;
                    float fy = y / (float)size;
                    fx = fx * 2f - 1f;
                    fy = fy * 2f - 1f;
                    float length = Mathf.Sqrt(fx * fx + fy * fy);
                    colors[i] = Color.Lerp(background, foreground, (1f - length) / edge);
                    //Debug.Log(i + "  " + colors[i]);
                }



            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(colors);
            tex.Apply(false);
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }


        public static Color Gray(float light, float alpha)
        {
            return new Color(light, light, light, alpha);
        }

        static Texture2D GetOnePixelTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.SetPixels(new[] {color});
            tex.Apply(false);
            tex.hideFlags = HideFlags.DontSave;
            return tex;
        }

        public static void SnapPosition(ref Vector2 pos)
        {
            pos.x = ((int)(pos.x / CellSize)) * CellSize;
            pos.y = ((int)(pos.y / CellSize)) * CellSize;
        }

        static NodeEditorConfig GetDefaultConfig()
        {
            NodeEditorConfig config = new NodeEditorConfig();
            config.InputsOutputs = new NodeEditorConfig.InputsOutputsData()
            {
                Shape = ShapeType.Round,
                Size = 9,
                Edge = 0.2f,
                Padding = 2
            };
            config.BackgroundColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
            config.NodeColor = new Color(1f, 1f, 1f, 0.19607843f);

            config.Colors = new[]
            {
                new Color(1f, 1f, 1f, 1f),
                new Color(0.74509805f, 0f, 0.74509805f, 1f),
                new Color(0.81353325f, 0.9012218f, 0.9019608f, 1f),
                new Color(0.5697809f, 0.65573025f, 0.74509805f, 1f),
                new Color(0.52733564f, 0.91551334f, 0.9960785f, 0.50980395f),
                new Color(0.29411763f, 0.6401771f, 1f, 0.627451f),
                new Color(0.29411763f, 0.52220225f, 1f, 0.627451f),
                new Color(0.3333333f, 0.43732592f, 1f, 0.627451f),
                new Color(1f, 0.58495826f, 0f, 0.316f),
                new Color(0.94f, 0.64509803f, 0.64509803f, 1f),
                new Color(0.703f, 0.57654524f, 0.553964f, 1f),
                new Color(0.16470589f, 0.8980393f, 0.1137255f, 1f),
                new Color(0.16470589f, 0.8980393f, 0.1137255f, 1f),
                new Color(0.16470589f, 0.8980393f, 0.1137255f, 1f),
                new Color(1f, 1f, 1f, 1f),
                new Color(1f, 1f, 1f, 1f)
            };

            return config;
        }

        public enum ShapeType
        {
            Box,
            Round
        }
    }
}
