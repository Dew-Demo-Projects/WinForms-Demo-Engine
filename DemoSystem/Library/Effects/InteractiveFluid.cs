namespace DemoSystem.Library.Effects
{
    /// <summary>
    /// Real-time fluid simulation in a contained rectangle
    /// Cursor interacts with fluid as a collision object
    /// </summary>
    internal sealed class InteractiveFluid : IInteractiveEffect
    {
        private readonly List<FluidParticle> _particles = new();
        private readonly Random _rng = new();
        private readonly RectangleF _container;
        private readonly float _interactionRadius = 50f;
        private readonly float _viscosity = 0.96f;
        private readonly float _pressure = 0.8f;
        private readonly float _targetDensity = 2.0f;
        private readonly float _gravity = 0.2f;

        public InteractiveFluid(int particleCount = 400, float containerWidth = 400, float containerHeight = 300)
        {
            _container = new RectangleF(-containerWidth * 0.5f, -containerHeight * 0.5f, containerWidth,
                containerHeight);

            // Create fluid particles in a grid pattern
            int particlesPerRow = (int)Math.Sqrt(particleCount);
            float spacing = Math.Min(_container.Width, _container.Height) / (particlesPerRow + 1);

            for (int i = 0; i < particleCount; i++)
            {
                int row = i / particlesPerRow;
                int col = i % particlesPerRow;

                float x = _container.Left + spacing * (col + 1);
                float y = _container.Top + spacing * (row + 1);

                _particles.Add(new FluidParticle
                {
                    Position = new PointF(x, y),
                    Velocity = new PointF(0, 0),
                    Density = 0,
                    Pressure = 0,
                    NearDensity = 0
                });
            }
        }

        public void Render(Graphics g, int width, int height,
            int localFrame, int globalFrame, in InputState input)
        {
            float centerX = width * 0.5f;
            float centerY = height * 0.5f;

            // Convert mouse to world coordinates (centered)
            PointF mouseWorldPos = new PointF(
                input.Mouse.X - centerX,
                input.Mouse.Y - centerY
            );

            // Update fluid simulation
            UpdateFluid(mouseWorldPos, input);

            // Draw everything
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ResetTransform();
            g.TranslateTransform(centerX, centerY);

            // Draw fluid container
            DrawContainer(g);

            // Draw fluid particles with color based on pressure
            DrawFluid(g);

            // Draw cursor interaction visualization
            DrawCursorInteraction(g, mouseWorldPos, input);

            // Draw UI info
            DrawUI(g, width, height);
        }

        private void UpdateFluid(PointF mouseWorldPos, InputState input)
        {
            const float interactionRadiusSquared = 50f * 50f;
            const float smoothingRadius = 25f;
            const float smoothingRadiusSquared = smoothingRadius * smoothingRadius;

            // Reset densities
            foreach (var p in _particles)
            {
                p.Density = 0;
                p.NearDensity = 0;
            }

            // Calculate densities using SPH (Smoothed Particle Hydrodynamics)
            for (int i = 0; i < _particles.Count; i++)
            {
                var pi = _particles[i];

                for (int j = i + 1; j < _particles.Count; j++)
                {
                    var pj = _particles[j];

                    float dx = pi.Position.X - pj.Position.X;
                    float dy = pi.Position.Y - pj.Position.Y;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < smoothingRadiusSquared)
                    {
                        float distance = MathF.Sqrt(distanceSquared);
                        float influence = 1 - distance / smoothingRadius;
                        float density = influence * influence;

                        pi.Density += density;
                        pj.Density += density;

                        pi.NearDensity += density * density;
                        pj.NearDensity += density * density;
                    }
                }
            }

            // Calculate pressures and apply forces
            foreach (var p in _particles)
            {
                // Pressure force
                float densityError = p.Density - _targetDensity;
                p.Pressure = densityError * _pressure;

                // Gravity
                p.Velocity.Y += _gravity;

                // Viscosity
                p.Velocity.X *= _viscosity;
                p.Velocity.Y *= _viscosity;

                // Mouse interaction
                if (input.LeftDown || input.RightDown)
                {
                    float dx = mouseWorldPos.X - p.Position.X;
                    float dy = mouseWorldPos.Y - p.Position.Y;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < interactionRadiusSquared)
                    {
                        float distance = MathF.Sqrt(distanceSquared);
                        float force = input.RightDown ? -1.5f : 1.5f; // Right click pulls, left click pushes
                        float strength = (1 - distance / _interactionRadius) * force * 8f;

                        if (distance > 0.1f)
                        {
                            p.Velocity.X += (dx / distance) * strength;
                            p.Velocity.Y += (dy / distance) * strength;
                        }
                    }
                }

                // Mouse movement creates flow
                if (input.LeftDown)
                {
                    float dx = mouseWorldPos.X - p.Position.X;
                    float dy = mouseWorldPos.Y - p.Position.Y;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < interactionRadiusSquared * 2f)
                    {
                        // Create vortex-like flow around cursor
                        float crossX = -dy * 0.02f;
                        float crossY = dx * 0.02f;

                        p.Velocity.X += crossX;
                        p.Velocity.Y += crossY;
                    }
                }

                // Update position
                p.Position.X += p.Velocity.X;
                p.Position.Y += p.Velocity.Y;

                // Container collision with damping
                if (p.Position.X - 2 < _container.Left)
                {
                    p.Position.X = _container.Left + 2;
                    p.Velocity.X = -p.Velocity.X * 0.3f;
                }
                else if (p.Position.X + 2 > _container.Right)
                {
                    p.Position.X = _container.Right - 2;
                    p.Velocity.X = -p.Velocity.X * 0.3f;
                }

                if (p.Position.Y - 2 < _container.Top)
                {
                    p.Position.Y = _container.Top + 2;
                    p.Velocity.Y = -p.Velocity.Y * 0.3f;
                }
                else if (p.Position.Y + 2 > _container.Bottom)
                {
                    p.Position.Y = _container.Bottom - 2;
                    p.Velocity.Y = -p.Velocity.Y * 0.3f;
                }
            }

            // Apply pressure forces between particles
            for (int i = 0; i < _particles.Count; i++)
            {
                var pi = _particles[i];

                for (int j = i + 1; j < _particles.Count; j++)
                {
                    var pj = _particles[j];

                    float dx = pi.Position.X - pj.Position.X;
                    float dy = pi.Position.Y - pj.Position.Y;
                    float distanceSquared = dx * dx + dy * dy;

                    if (distanceSquared < smoothingRadiusSquared && distanceSquared > 0.1f)
                    {
                        float distance = MathF.Sqrt(distanceSquared);
                        float directionX = dx / distance;
                        float directionY = dy / distance;

                        // Pressure gradient
                        float pressureForce = (pi.Pressure + pj.Pressure) * 0.5f;
                        float influence = (1 - distance / smoothingRadius);
                        float force = pressureForce * influence * 0.1f;

                        pi.Velocity.X += directionX * force;
                        pi.Velocity.Y += directionY * force;
                        pj.Velocity.X -= directionX * force;
                        pj.Velocity.Y -= directionY * force;
                    }
                }
            }
        }

        private void DrawContainer(Graphics g)
        {
            using var containerPen = new Pen(Color.FromArgb(150, Color.LightBlue), 3f);
            using var containerBrush = new SolidBrush(Color.FromArgb(30, Color.Blue));

            g.FillRectangle(containerBrush, _container.X, _container.Y, _container.Width, _container.Height);
            g.DrawRectangle(containerPen, _container.X, _container.Y, _container.Width, _container.Height);
        }

        private void DrawFluid(Graphics graphics)
        {
            foreach (var p in _particles)
            {
                // Color based on pressure and velocity
                float pressureFactor = Math.Clamp(p.Pressure * 2f, -1f, 1f);
                float velocity = MathF.Sqrt(p.Velocity.X * p.Velocity.X + p.Velocity.Y * p.Velocity.Y);
                float velocityFactor = Math.Clamp(velocity * 2f, 0f, 1f);

                Color particleColor;
                if (pressureFactor > 0)
                {
                    // High pressure = red/orange
                    int r = 255;
                    int g = (int)(255 * (1 - pressureFactor));
                    int b = 0;
                    particleColor = Color.FromArgb(180, r, g, b);
                }
                else
                {
                    // Low pressure = blue/green
                    int r = 0;
                    int g = (int)(255 * (1 + pressureFactor));
                    int b = 255;
                    particleColor = Color.FromArgb(180, r, g, b);
                }

                // Add brightness based on velocity
                if (velocityFactor > 0.3f)
                {
                    particleColor = Color.FromArgb(
                        particleColor.A,
                        Math.Min(255, particleColor.R + (int)(50 * velocityFactor)),
                        Math.Min(255, particleColor.G + (int)(50 * velocityFactor)),
                        Math.Min(255, particleColor.B + (int)(50 * velocityFactor))
                    );
                }

                using var brush = new SolidBrush(particleColor);
                float size = 3f + velocityFactor * 2f;
                graphics.FillEllipse(brush, p.Position.X - size * 0.5f, p.Position.Y - size * 0.5f, size, size);
            }
        }

        private void DrawCursorInteraction(Graphics g, PointF mouseWorldPos, InputState input)
        {
            if (input.LeftDown || input.RightDown)
            {
                using var interactionPen = new Pen(
                    input.RightDown ? Color.Red : Color.Cyan,
                    2f
                );

                // Draw interaction radius
                g.DrawEllipse(interactionPen,
                    mouseWorldPos.X - _interactionRadius,
                    mouseWorldPos.Y - _interactionRadius,
                    _interactionRadius * 2,
                    _interactionRadius * 2);

                // Draw force direction indicators
                int rayCount = 8;
                for (int i = 0; i < rayCount; i++)
                {
                    float angle = i * (2 * MathF.PI / rayCount);
                    float cos = MathF.Cos(angle);
                    float sin = MathF.Sin(angle);

                    float startX = mouseWorldPos.X + cos * (_interactionRadius - 5);
                    float startY = mouseWorldPos.Y + sin * (_interactionRadius - 5);
                    float endX = mouseWorldPos.X + cos * (_interactionRadius + 5);
                    float endY = mouseWorldPos.Y + sin * (_interactionRadius + 5);

                    // Reverse direction for right click (pull)
                    if (input.RightDown)
                    {
                        var temp = (startX, startY);
                        startX = endX;
                        startY = endY;
                        endX = temp.startX;
                        endY = temp.startY;
                    }

                    g.DrawLine(interactionPen, startX, startY, endX, endY);
                }
            }
        }

        private void DrawUI(Graphics g, int width, int height)
        {
            g.ResetTransform(); // Back to screen coordinates

            string instructions = "LEFT: Push fluid | RIGHT: Pull fluid | MOVE: Create flow";
            string stats = $"Particles: {_particles.Count} | Viscosity: {_viscosity:F2}";

            using var font = new Font("Consolas", 9);
            using var brush = new SolidBrush(Color.White);
            using var bgBrush = new SolidBrush(Color.FromArgb(128, Color.Black));

            // Draw background for text
            g.FillRectangle(bgBrush, 5, 5, 400, 40);

            g.DrawString(instructions, font, brush, 10, 10);
            g.DrawString(stats, font, brush, 10, 25);
        }

        private class FluidParticle
        {
            public PointF Position;
            public PointF Velocity;
            public float Density;
            public float Pressure;
            public float NearDensity;
        }
    }
}