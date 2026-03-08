namespace DemoSystem.Library.Effects
{
    /// <summary>
    /// Magnetic particles that attract/repel based on mouse input
    /// Left click = attraction, Right click = repulsion
    /// Mouse wheel adjusts magnetic strength
    /// </summary>
    internal sealed class MagneticParticles : IInteractiveEffect
    {
        private readonly List<Particle> _particles = new();
        private readonly Random _rng = new();
        private float _magneticStrength = 1.0f;

        private readonly Color[] _particleColors = new[]
        {
            Color.Cyan,
            Color.Magenta,
            Color.Yellow,
            Color.Lime,
            Color.Orange,
            Color.Violet
        };

        public MagneticParticles(int particleCount = 150)
        {
            // Create particles with random properties
            for (int i = 0; i < particleCount; i++)
            {
                _particles.Add(new Particle
                {
                    Position = new PointF(
                        _rng.NextSingle() * 1000 - 500,
                        _rng.NextSingle() * 1000 - 500
                    ),
                    Velocity = new PointF(
                        (_rng.NextSingle() - 0.5f) * 4f,
                        (_rng.NextSingle() - 0.5f) * 4f
                    ),
                    Radius = 2 + _rng.NextSingle() * 4,
                    Color = _particleColors[_rng.Next(_particleColors.Length)],
                    Trail = new Queue<PointF>(),
                    MaxTrailLength = 10 + _rng.Next(20)
                });
            }
        }

        public void Render(Graphics g, int width, int height,
            int localFrame, int globalFrame, in InputState input)
        {
            float centerX = width * 0.5f;
            float centerY = height * 0.5f;

            // Adjust magnetic strength with mouse wheel
            _magneticStrength += input.WheelDelta / 1200f;
            _magneticStrength = Math.Clamp(_magneticStrength, 0.1f, 3.0f);

            // Convert mouse to world coordinates (centered)
            PointF mouseWorldPos = new PointF(
                input.Mouse.X - centerX,
                input.Mouse.Y - centerY
            );

            foreach (var p in _particles)
            {
                // Add current position to trail (for drawing later)
                p.Trail.Enqueue(p.Position);
                while (p.Trail.Count > p.MaxTrailLength)
                    p.Trail.Dequeue();

                // Calculate distance to mouse
                float dx = mouseWorldPos.X - p.Position.X;
                float dy = mouseWorldPos.Y - p.Position.Y;
                float distance = MathF.Sqrt(dx * dx + dy * dy);

                // Apply magnetic forces based on input
                if (distance > 0.1f && distance < 400f) // Only affect particles within range
                {
                    float forceMagnitude = 0f;

                    if (input.LeftDown) // Attraction
                    {
                        forceMagnitude = _magneticStrength * 800f / (distance * distance + 100f);
                    }
                    else if (input.RightDown) // Repulsion
                    {
                        forceMagnitude = -_magneticStrength * 1200f / (distance * distance + 100f);
                    }

                    if (forceMagnitude != 0f)
                    {
                        // Normalize direction and apply force
                        float invDistance = 1f / distance;
                        p.Velocity.X += dx * invDistance * forceMagnitude;
                        p.Velocity.Y += dy * invDistance * forceMagnitude;
                    }
                }

                // Apply velocity
                p.Position.X += p.Velocity.X;
                p.Position.Y += p.Velocity.Y;

                // Apply damping
                p.Velocity.X *= 0.98f;
                p.Velocity.Y *= 0.98f;

                // Boundary checking with wrap-around
                const float boundary = 600f;
                if (p.Position.X < -boundary) p.Position.X = boundary;
                if (p.Position.X > boundary) p.Position.X = -boundary;
                if (p.Position.Y < -boundary) p.Position.Y = boundary;
                if (p.Position.Y > boundary) p.Position.Y = -boundary;
            }

            // Draw everything
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ResetTransform();
            g.TranslateTransform(centerX, centerY);

            // Draw trails first (so particles appear on top)
            foreach (var p in _particles)
            {
                DrawParticleTrail(g, p);
            }

            // Draw particles
            foreach (var p in _particles)
            {
                DrawParticle(g, p);
            }

            // Draw UI info
            DrawUI(g, width, height, input);
        }

        private void DrawParticleTrail(Graphics g, Particle p)
        {
            if (p.Trail.Count < 2) return;

            var trailArray = p.Trail.ToArray();
            using var trailPen = new Pen(Color.FromArgb(80, p.Color), 1f);

            for (int i = 1; i < trailArray.Length; i++)
            {
                // Fade trail based on position
                float alpha = (float)i / trailArray.Length * 80f;
                trailPen.Color = Color.FromArgb((int)alpha, p.Color);

                g.DrawLine(trailPen, trailArray[i - 1], trailArray[i]);
            }
        }

        private void DrawParticle(Graphics g, Particle p)
        {
            using var brush = new SolidBrush(p.Color);
            g.FillEllipse(brush,
                p.Position.X - p.Radius,
                p.Position.Y - p.Radius,
                p.Radius * 2,
                p.Radius * 2);

            // Add a subtle glow
            using var glowPen = new Pen(Color.FromArgb(100, Color.White), 1f);
            g.DrawEllipse(glowPen,
                p.Position.X - p.Radius - 1,
                p.Position.Y - p.Radius - 1,
                (p.Radius + 1) * 2,
                (p.Radius + 1) * 2);
        }

        private void DrawUI(Graphics g, int width, int height, InputState input)
        {
            g.ResetTransform(); // Back to screen coordinates

            string instructions = "LEFT: Attract | RIGHT: Repel | WHEEL: Strength";
            string strengthText = $"Strength: {_magneticStrength:F2}";
            string particleCount = $"Particles: {_particles.Count}";

            using var font = new Font("Consolas", 10);
            using var brush = new SolidBrush(Color.White);
            using var bgBrush = new SolidBrush(Color.FromArgb(128, Color.Black));

            // Draw background for text
            g.FillRectangle(bgBrush, 5, 5, 300, 70);

            g.DrawString(instructions, font, brush, 10, 10);
            g.DrawString(strengthText, font, brush, 10, 30);
            g.DrawString(particleCount, font, brush, 10, 50);

            // Draw magnetic field visualization when active
            if (input.LeftDown || input.RightDown)
            {
                DrawMagneticField(g, input, width, height);
            }
        }

        private void DrawMagneticField(Graphics g, InputState input, int width, int height)
        {
            var center = new PointF(width * 0.5f, height * 0.5f);
            float fieldRadius = 150f * _magneticStrength;

            using var fieldPen = new Pen(
                    input.LeftDown ? Color.Cyan : Color.Magenta,
                    2f
                )
                { DashStyle = DashStyle.Dash };

            g.DrawEllipse(fieldPen,
                input.Mouse.X - fieldRadius,
                input.Mouse.Y - fieldRadius,
                fieldRadius * 2,
                fieldRadius * 2);

            // Draw force direction indicators
            float arrowSize = 8f;
            for (int i = 0; i < 8; i++)
            {
                float angle = i * MathF.PI / 4f;
                float cos = MathF.Cos(angle);
                float sin = MathF.Sin(angle);

                PointF start = new PointF(
                    input.Mouse.X + cos * (fieldRadius - 10),
                    input.Mouse.Y + sin * (fieldRadius - 10)
                );

                PointF end = new PointF(
                    input.Mouse.X + cos * (fieldRadius + 10),
                    input.Mouse.Y + sin * (fieldRadius + 10)
                );

                // Reverse arrow direction for repulsion
                if (input.RightDown)
                {
                    var temp = start;
                    start = end;
                    end = temp;
                }

                g.DrawLine(fieldPen, start, end);

                // Draw arrow head
                DrawArrowHead(g, fieldPen, start, end, arrowSize);
            }
        }

        private void DrawArrowHead(Graphics g, Pen pen, PointF start, PointF end, float size)
        {
            float angle = MathF.Atan2(end.Y - start.Y, end.X - start.X);

            PointF[] arrowHead = new PointF[]
            {
                new PointF(
                    end.X - size * MathF.Cos(angle - MathF.PI / 6),
                    end.Y - size * MathF.Sin(angle - MathF.PI / 6)
                ),
                end,
                new PointF(
                    end.X - size * MathF.Cos(angle + MathF.PI / 6),
                    end.Y - size * MathF.Sin(angle + MathF.PI / 6)
                )
            };

            g.DrawLines(pen, arrowHead);
        }

        private class Particle
        {
            public PointF Position;
            public PointF Velocity;
            public float Radius;
            public Color Color;
            public Queue<PointF> Trail;
            public int MaxTrailLength;
        }
    }
}