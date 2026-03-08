namespace DemoSystem.Library.Effects
{
    /// <summary>
    /// A rectangle that spins; balls inside feel gravity and collide
    /// with the rotated walls.
    /// </summary>
    internal class RotatingGravityBox : IEffect
    {
        private readonly List<Ball> _balls = new();
        private readonly float _width; // box width  (world units)
        private readonly float _height; // box height (world units)
        private readonly float _rotationSpeed; // degrees per frame
        private readonly float _gravity; // px / frame²
        private readonly float _damping; // 0..1  (energy loss on bounce)
        private readonly float _restitution; // 0..1  (bounce factor)

        public RotatingGravityBox(int ballCount = 25,
            float boxWidth = 300,
            float boxHeight = 200,
            float rotSpeed = 1.5f,
            float gravity = 0.4f,
            float damping = 0.98f,
            float restitution = 0.85f)
        {
            _width = boxWidth;
            _height = boxHeight;
            _rotationSpeed = rotSpeed;
            _gravity = gravity;
            _damping = damping;
            _restitution = restitution;

            // seed balls with random positions / speeds
            var rng = new Random();
            for (int i = 0; i < ballCount; i++)
            {
                _balls.Add(new Ball
                {
                    R = 4 + rng.NextSingle() * 4,
                    X = (rng.NextSingle() - 0.5f) * boxWidth * 0.8f,
                    Y = (rng.NextSingle() - 0.5f) * boxHeight * 0.8f,
                    Vx = (rng.NextSingle() - 0.5f) * 6,
                    Vy = (rng.NextSingle() - 0.5f) * 6
                });
            }
        }

        public void Render(Graphics g, int screenW, int screenH,
            int localFrame, int globalFrame)
        {
            float angle = globalFrame * _rotationSpeed;

            // centre of screen
            float cx = screenW * 0.5f;
            float cy = screenH * 0.5f;

            // integrate physics in local (un-rotated) space
            foreach (var b in _balls)
            {
                // gravity in world space; rotate into local space
                (float gx, float gy) = Rotate(0, _gravity, -angle);
                b.Vx += gx;
                b.Vy += gy;

                b.X += b.Vx;
                b.Y += b.Vy;

                // collide with axis-aligned walls
                if (b.X - b.R < -_width * 0.5f)
                {
                    b.X = -_width * 0.5f + b.R;
                    b.Vx = -b.Vx * _restitution;
                }

                if (b.X + b.R > _width * 0.5f)
                {
                    b.X = _width * 0.5f - b.R;
                    b.Vx = -b.Vx * _restitution;
                }

                if (b.Y - b.R < -_height * 0.5f)
                {
                    b.Y = -_height * 0.5f + b.R;
                    b.Vy = -b.Vy * _restitution;
                }

                if (b.Y + b.R > _height * 0.5f)
                {
                    b.Y = _height * 0.5f - b.R;
                    b.Vy = -b.Vy * _restitution;
                }

                // gentle damping
                b.Vx *= _damping;
                b.Vy *= _damping;
            }

            // draw container (rotated)
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ResetTransform();
            g.TranslateTransform(cx, cy);
            g.RotateTransform(angle);

            using (var pen = new Pen(Color.LightSteelBlue, 3))
                g.DrawRectangle(pen, -_width * 0.5f, -_height * 0.5f, _width, _height);

            // draw balls (still in rotated space)
            using (var bsh = new SolidBrush(Color.OrangeRed))
                foreach (var b in _balls)
                    g.FillEllipse(bsh, b.X - b.R, b.Y - b.R, b.R * 2, b.R * 2);
        }

        /* ----------------------------------------------------------- */
        // small helper: rotate vector
        private static (float x, float y) Rotate(float x, float y, float deg)
        {
            double rad = deg * Math.PI / 180.0;
            double c = Math.Cos(rad);
            double s = Math.Sin(rad);
            return ((float)(x * c - y * s), (float)(x * s + y * c));
        }

        /* ----------------------------------------------------------- */
        private class Ball
        {
            public float X, Y; // centre (local space)
            public float Vx, Vy; // velocity (local space)
            public float R; // radius
        }
    }
}