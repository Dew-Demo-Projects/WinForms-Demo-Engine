namespace DemoSystem.Library.Effects
{
    internal sealed class InteractiveGravityBox : IInteractiveEffect
    {
        private readonly List<Ball> _balls = new();
        private readonly float _ww, _hh; // box half-width / height
        private float _rotSpeed = 1.5f;
        private float _angle;

        public InteractiveGravityBox(int ballCount = 30,
            float boxWidth = 300,
            float boxHeight = 200)
        {
            _ww = boxWidth * 0.5f;
            _hh = boxHeight * 0.5f;
            var rng = new Random();
            for (int i = 0; i < ballCount; i++)
                _balls.Add(new Ball
                {
                    R = 4 + rng.NextSingle() * 4,
                    X = (rng.NextSingle() - 0.5f) * boxWidth * 0.8f,
                    Y = (rng.NextSingle() - 0.5f) * boxHeight * 0.8f,
                    Vx = (rng.NextSingle() - 0.5f) * 6,
                    Vy = (rng.NextSingle() - 0.5f) * 6
                });
        }

        public void Render(Graphics g, int w, int h,
            int localFrame, int globalFrame,
            in InputState inp)
        {
            float cx = w * 0.5f, cy = h * 0.5f;

            /* ---------- 1.  build world gravity vector (tilt) ---------- */
            float gx = 0, gy = 0.35f; // default down
            float dx = inp.Mouse.X - cx;
            float dy = inp.Mouse.Y - cy;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len > 2)
            {
                float s = Math.Min(len / 300f, 1f);
                gx = (dx / len) * s * 0.5f;
                gy = -(dy / len) * s * 0.5f; // flip y (screen coords)
            }

            /* ---------- 2.  wheel -> rotation speed ---------- */
            _rotSpeed += inp.WheelDelta / 120f * 0.2f;
            _rotSpeed = Math.Max(0, _rotSpeed);

            /* ---------- 3.  physics in local (un-rotated) space ---------- */
            _angle += _rotSpeed;
            var (gravX, gravY) = Rotate(gx, gy, -_angle);

            foreach (var b in _balls)
            {
                /* gravity */
                b.Vx += gravX;
                b.Vy += gravY;

                /* mouse forces (world space) */
                var worldBall = Rotate(b.X, b.Y, _angle);
                float mx = inp.Mouse.X - cx, my = inp.Mouse.Y - cy;
                float fx = 0, fy = 0;
                float dx2 = mx - worldBall.x;
                float dy2 = my - worldBall.y;
                float d2 = dx2 * dx2 + dy2 * dy2;
                if (d2 < 1) d2 = 1;
                float f = 0;
                if (inp.LeftDown) f = 120f / d2; // ATTRACT
                if (inp.RightDown) f = -120f / d2; // REPEL
                b.Vx += (dx2 / MathF.Sqrt(d2)) * f;
                b.Vy += (dy2 / MathF.Sqrt(d2)) * f;

                /* integrate */
                b.X += b.Vx;
                b.Y += b.Vy;

                /* collide with axis-aligned walls */
                void Bounce(float bound, ref float p, ref float v)
                {
                    if (p - b.R < -bound)
                    {
                        p = -bound + b.R;
                        v = -v * 0.85f;
                    }

                    if (p + b.R > bound)
                    {
                        p = bound - b.R;
                        v = -v * 0.85f;
                    }
                }

                Bounce(_ww, ref b.X, ref b.Vx);
                Bounce(_hh, ref b.Y, ref b.Vy);

                /* damping */
                b.Vx *= 0.98f;
                b.Vy *= 0.98f;
            }

            /* ---------- 4.  draw ---------- */
            g.SmoothingMode = SmoothingMode.AntiAlias;

            /* 4a. box (rotated) */
            g.ResetTransform();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(_angle);
            using (var pen = new Pen(Color.LightSteelBlue, 3))
                g.DrawRectangle(pen, -_ww, -_hh, _ww * 2, _hh * 2);

            /* 4b. balls (still in rotated space) */
            using (var bsh = new SolidBrush(Color.OrangeRed))
                foreach (var b in _balls)
                    g.FillEllipse(bsh, b.X - b.R, b.Y - b.R, b.R * 2, b.R * 2);

            /* 4c. speed text (screen space, top-left) */
            g.ResetTransform();
            g.DrawString($"Speed: {_rotSpeed:F2}", SystemFonts.DefaultFont,
                Brushes.White, 5, 5);
        }

        /* ------------------ helpers ------------------ */
        private static (float x, float y) Rotate(float x, float y, float deg)
        {
            double r = deg * Math.PI / 180.0;
            double c = Math.Cos(r), s = Math.Sin(r);
            return ((float)(x * c - y * s), (float)(x * s + y * c));
        }

        private class Ball
        {
            public float X, Y, Vx, Vy, R;
        }
    }
}