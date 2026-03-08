namespace DemoSystem.Library.Effects
{
    /// <summary>
    /// Electricity arcs that strike from the top to the bottom of the window.
    /// </summary>
    internal class ElectricArcs : IEffect
    {
        private readonly int _boltCount; // how many arcs per frame
        private readonly double _displacement; // max random sway
        private readonly byte _glowRadius; // extra glow thickness
        private readonly Color _coreColor; // bright core
        private readonly Color _glowColor; // outer glow

        public ElectricArcs(int boltCount = 3,
            double displacement = 0.15,
            byte glowRadius = 2,
            string coreHex = "#E0F0FF",
            string glowHex = "#4080FF")
        {
            _boltCount = boltCount;
            _displacement = displacement;
            _glowRadius = glowRadius;
            _coreColor = ColorTranslator.FromHtml(coreHex);
            _glowColor = ColorTranslator.FromHtml(glowHex);
        }

        public void Render(Graphics g, int width, int height,
            int localFrame, int globalFrame)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // draw a dark background so the glow pops
            g.Clear(Color.FromArgb(10, 0, 0, 20));

            for (int i = 0; i < _boltCount; i++)
            {
                float x0 = RandomFloat(width);
                float x1 = x0 + RandomFloat(-width * 0.2f, width * 0.2f);
                var pts = BuildBolt(new PointF(x0, 0),
                    new PointF(x1, height),
                    6); // recursion depth
                DrawBolt(g, pts);
            }
        }

        /* ------------------------------------------------------------------ */

        // Midpoint displacement lightning
        private List<PointF> BuildBolt(PointF p0, PointF p1, int depth)
        {
            var res = new List<PointF> { p0 };

            void Split(PointF a, PointF b, int lvl)
            {
                if (lvl <= 0)
                {
                    res.Add(b);
                    return;
                }

                float mx = (a.X + b.X) * 0.5f;
                float my = (a.Y + b.Y) * 0.5f;

                // perpendicular displacement
                float dx = b.X - a.X;
                float dy = b.Y - a.Y;
                float nx = -dy;
                float ny = dx;
                float len = (float)Math.Sqrt(nx * nx + ny * ny);
                nx /= len;
                ny /= len;

                float displace = (float)(RandomFloat(-0.5f, 0.5f) * _displacement * len);
                mx += nx * displace;
                my += ny * displace;

                var mid = new PointF(mx, my);
                Split(a, mid, lvl - 1);
                Split(mid, b, lvl - 1);
            }

            Split(p0, p1, depth);
            return res;
        }

        // Render core + glow
        private void DrawBolt(Graphics g, List<PointF> pts)
        {
            using var glowPen = new Pen(_glowColor, (_glowRadius + 1) * 2)
            {
                EndCap = LineCap.Round,
                StartCap = LineCap.Round
            };
            using var corePen = new Pen(_coreColor, 1f);

            g.DrawLines(glowPen, pts.ToArray());
            g.DrawLines(corePen, pts.ToArray());
        }

        private static readonly Random _rng = new();
        private static float RandomFloat(float max) => (float)_rng.NextDouble() * max;
        private static float RandomFloat(float min, float max) => min + (float)_rng.NextDouble() * (max - min);
    }
}