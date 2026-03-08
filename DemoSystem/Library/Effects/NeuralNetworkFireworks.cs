namespace DemoSystem.Library.Effects
{
    /// <summary>
    /// Neural network that grows and activates based on mouse interactions
    /// Creates dynamic network patterns that pulse and evolve
    /// </summary>
    internal sealed class NeuralNetworkFireworks : IInteractiveEffect
    {
        private readonly List<Neuron> _neurons = new();
        private readonly List<Connection> _connections = new();
        private readonly Random _rng = new();
        private readonly float _activationThreshold = 0.7f;
        private float _learningRate = 0.1f;
        private int _nextNeuronId = 0;
        private PointF _lastMousePos;
        private bool _wasMouseDown = false;

        // Improved color gradients - less purple, more varied
        private readonly Color[] _neuronColors = new[]
        {
            Color.FromArgb(30, 30, 100), // Deep Blue
            Color.FromArgb(0, 100, 200), // Bright Blue
            Color.FromArgb(0, 200, 200), // Cyan
            Color.FromArgb(0, 200, 100), // Green
            Color.FromArgb(200, 200, 0), // Yellow
            Color.FromArgb(255, 150, 0), // Orange
            Color.FromArgb(255, 80, 0), // Red-Orange
            Color.FromArgb(200, 0, 100) // Magenta (limited use)
        };

        public NeuralNetworkFireworks(int initialNeurons = 20)
        {
            // Create initial random neurons
            for (int i = 0; i < initialNeurons; i++)
            {
                AddRandomNeuron();
            }

            // Create initial random connections
            for (int i = 0; i < initialNeurons * 2; i++)
            {
                TryAddRandomConnection();
            }
        }

        public void Render(Graphics g, int width, int height,
            int localFrame, int globalFrame, in InputState input)
        {
            float centerX = width * 0.5f;
            float centerY = height * 0.5f;

            // Convert mouse to world coordinates
            PointF mouseWorldPos = new PointF(
                input.Mouse.X - centerX,
                input.Mouse.Y - centerY
            );

            // Handle mouse interactions
            HandleMouseInteractions(mouseWorldPos, input);

            // Update neural network
            UpdateNetwork(mouseWorldPos, input);

            // Draw everything
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ResetTransform();
            g.TranslateTransform(centerX, centerY);

            // Draw connections first
            DrawConnections(g);

            // Draw neurons on top
            DrawNeurons(g);

            // Draw UI
            DrawUI(g, width, height, input);

            _lastMousePos = mouseWorldPos;
            _wasMouseDown = input.LeftDown || input.RightDown;
        }

        private void HandleMouseInteractions(PointF mouseWorldPos, InputState input)
        {
            // Left click: Add new neuron at mouse position
            if (input.LeftDown && !_wasMouseDown)
            {
                AddNeuronAt(mouseWorldPos);

                // Try to connect to nearby neurons
                ConnectToNearbyNeurons(_neurons[^1], 150f);
            }

            // Right click: Stimulate nearby neurons
            if (input.RightDown)
            {
                StimulateNearbyNeurons(mouseWorldPos, 100f, 0.5f); // Reduced from 0.8f to 0.5f
            }

            // Mouse wheel: Adjust learning rate
            if (input.WheelDelta != 0)
            {
                _learningRate += input.WheelDelta / 1200f;
                _learningRate = Math.Clamp(_learningRate, 0.01f, 0.5f);
            }

            // Mouse movement: Slight stimulation along path
            if (input.LeftDown)
            {
                // Create neurons along drag path
                float dx = mouseWorldPos.X - _lastMousePos.X;
                float dy = mouseWorldPos.Y - _lastMousePos.Y;
                float distance = MathF.Sqrt(dx * dx + dy * dy);

                if (distance > 30f && _neurons.Count < 500) // Limit total neurons
                {
                    int steps = (int)(distance / 30f);
                    for (int i = 1; i <= steps; i++)
                    {
                        float t = (float)i / steps;
                        PointF intermediatePos = new PointF(
                            _lastMousePos.X + dx * t,
                            _lastMousePos.Y + dy * t
                        );

                        if (_rng.NextSingle() < 0.3f) // 30% chance to add neuron
                        {
                            AddNeuronAt(intermediatePos);
                            ConnectToNearbyNeurons(_neurons[^1], 120f);
                        }
                    }
                }
            }
        }

        private void UpdateNetwork(PointF mouseWorldPos, InputState input)
        {
            // Update each neuron
            foreach (var neuron in _neurons)
            {
                // Use exponential decay that works regardless of current activation level
                float decayFactor = 0.85f; // 15% decay per frame - much more aggressive
                neuron.Activation *= decayFactor;

                // Ensure activation doesn't go below 0
                if (neuron.Activation < 0.01f)
                    neuron.Activation = 0f;

                // Apply input from connections
                float totalInput = 0f;
                int inputCount = 0;

                foreach (var conn in _connections)
                {
                    if (conn.ToNeuron == neuron && conn.FromNeuron.Activation > _activationThreshold)
                    {
                        totalInput += conn.FromNeuron.Activation * conn.Weight;
                        inputCount++;
                    }
                }

                if (inputCount > 0)
                {
                    neuron.Activation += totalInput / inputCount;
                    neuron.Activation = Math.Clamp(neuron.Activation, 0f, 1f);
                }

                // Hebbian learning: strengthen connections between active neurons
                if (neuron.Activation > _activationThreshold)
                {
                    foreach (var conn in _connections)
                    {
                        if (conn.FromNeuron.Activation > _activationThreshold &&
                            conn.ToNeuron.Activation > _activationThreshold)
                        {
                            conn.Weight += _learningRate * 0.03f; // Reduced from 0.05f
                            conn.Weight = Math.Clamp(conn.Weight, 0.1f, 2f);
                        }
                        else
                        {
                            // Weaken connections if not firing together
                            conn.Weight *= 0.998f;
                        }
                    }
                }

                // Update neuron position (slight drift)
                if (neuron.Activation < 0.1f)
                {
                    neuron.Velocity.X += (_rng.NextSingle() - 0.5f) * 0.2f;
                    neuron.Velocity.Y += (_rng.NextSingle() - 0.5f) * 0.2f;
                }

                // Apply velocity and damping
                neuron.Position.X += neuron.Velocity.X;
                neuron.Position.Y += neuron.Velocity.Y;
                neuron.Velocity.X *= 0.98f;
                neuron.Velocity.Y *= 0.98f;

                // Boundary constraints
                const float boundary = 400f;
                if (Math.Abs(neuron.Position.X) > boundary)
                {
                    neuron.Position.X = Math.Sign(neuron.Position.X) * boundary;
                    neuron.Velocity.X *= -0.5f;
                }

                if (Math.Abs(neuron.Position.Y) > boundary)
                {
                    neuron.Position.Y = Math.Sign(neuron.Position.Y) * boundary;
                    neuron.Velocity.Y *= -0.5f;
                }
            }

            // Randomly add/remove connections for dynamic network
            if (_rng.NextDouble() < 0.02f) // 2% chance per frame
            {
                TryAddRandomConnection();
            }

            if (_rng.NextDouble() < 0.01f && _connections.Count > 10) // 1% chance to remove weak connection
            {
                var weakConnections = _connections.Where(c => c.Weight < 0.2f).ToList();
                if (weakConnections.Count > 0)
                {
                    _connections.Remove(weakConnections[_rng.Next(weakConnections.Count)]);
                }
            }

            // Occasionally stimulate random neurons (but less frequently and weaker)
            if (_rng.NextDouble() < 0.02f && _neurons.Count > 0)
            {
                var randomNeuron = _neurons[_rng.Next(_neurons.Count)];
                if (randomNeuron.Activation < 0.2f) // Only stimulate inactive neurons
                {
                    randomNeuron.Activation += 0.2f;
                }
            }
        }

        private void AddRandomNeuron()
        {
            _neurons.Add(new Neuron
            {
                Id = _nextNeuronId++,
                Position = new PointF(
                    (_rng.NextSingle() - 0.5f) * 600f,
                    (_rng.NextSingle() - 0.5f) * 400f
                ),
                Velocity = new PointF(0, 0),
                Activation = _rng.NextSingle() * 0.3f,
                Radius = 4 + _rng.NextSingle() * 4
            });
        }

        private void AddNeuronAt(PointF position)
        {
            _neurons.Add(new Neuron
            {
                Id = _nextNeuronId++,
                Position = position,
                Velocity = new PointF(0, 0),
                Activation = 0.6f, // Reduced from 0.8f to prevent initial over-activation
                Radius = 5 + _rng.NextSingle() * 3
            });
        }

        private void ConnectToNearbyNeurons(Neuron newNeuron, float maxDistance)
        {
            float maxDistSq = maxDistance * maxDistance;
            int connectionsMade = 0;

            foreach (var existingNeuron in _neurons)
            {
                if (existingNeuron == newNeuron) continue;

                float dx = existingNeuron.Position.X - newNeuron.Position.X;
                float dy = existingNeuron.Position.Y - newNeuron.Position.Y;
                float distSq = dx * dx + dy * dy;

                if (distSq < maxDistSq && connectionsMade < 3) // Reduced from 5 to 3
                {
                    // Add bidirectional connections with lower initial weights
                    _connections.Add(new Connection
                    {
                        FromNeuron = newNeuron,
                        ToNeuron = existingNeuron,
                        Weight = 0.2f + _rng.NextSingle() * 0.3f // Lower initial weights
                    });

                    _connections.Add(new Connection
                    {
                        FromNeuron = existingNeuron,
                        ToNeuron = newNeuron,
                        Weight = 0.2f + _rng.NextSingle() * 0.3f
                    });

                    connectionsMade++;
                }
            }
        }

        private void StimulateNearbyNeurons(PointF position, float radius, float strength)
        {
            float radiusSq = radius * radius;

            foreach (var neuron in _neurons)
            {
                float dx = neuron.Position.X - position.X;
                float dy = neuron.Position.Y - position.Y;
                float distSq = dx * dx + dy * dy;

                if (distSq < radiusSq)
                {
                    float distanceFactor = 1f - (MathF.Sqrt(distSq) / radius);
                    neuron.Activation += strength * distanceFactor;
                    neuron.Activation = Math.Clamp(neuron.Activation, 0f, 1f);
                }
            }
        }

        private void TryAddRandomConnection()
        {
            if (_neurons.Count < 2) return;

            var neuron1 = _neurons[_rng.Next(_neurons.Count)];
            var neuron2 = _neurons[_rng.Next(_neurons.Count)];

            if (neuron1 != neuron2 && !ConnectionExists(neuron1, neuron2))
            {
                _connections.Add(new Connection
                {
                    FromNeuron = neuron1,
                    ToNeuron = neuron2,
                    Weight = 0.1f + _rng.NextSingle() * 0.2f // Lower initial weights
                });
            }
        }

        private bool ConnectionExists(Neuron from, Neuron to)
        {
            return _connections.Any(c => c.FromNeuron == from && c.ToNeuron == to);
        }

        private void DrawConnections(Graphics g)
        {
            foreach (var conn in _connections)
            {
                // Use the lower activation of the two neurons for color
                float minActivation = Math.Min(conn.FromNeuron.Activation, conn.ToNeuron.Activation);
                float alpha = Math.Clamp(conn.Weight * 80f, 20f, 120f); // Reduced alpha
                float thickness = Math.Clamp(conn.Weight * 2f, 0.3f, 3f); // Reduced thickness

                // Color based on the activation
                Color connectionColor = GetColorForActivation(minActivation, (int)alpha);

                using var pen = new Pen(connectionColor, thickness);
                g.DrawLine(pen, conn.FromNeuron.Position, conn.ToNeuron.Position);
            }
        }

        private void DrawNeurons(Graphics g)
        {
            foreach (var neuron in _neurons)
            {
                Color neuronColor = GetColorForActivation(neuron.Activation, 200);
                float glowSize = neuron.Activation * 8f; // Reduced glow
                float size = neuron.Radius + neuron.Activation * 2f; // Reduced size increase

                // Draw glow for activated neurons
                if (neuron.Activation > 0.3f)
                {
                    using var glowBrush = new SolidBrush(Color.FromArgb(
                        (int)(neuron.Activation * 60), // Reduced glow alpha
                        neuronColor
                    ));
                    g.FillEllipse(glowBrush,
                        neuron.Position.X - glowSize,
                        neuron.Position.Y - glowSize,
                        glowSize * 2,
                        glowSize * 2);
                }

                // Draw neuron body
                using var neuronBrush = new SolidBrush(neuronColor);
                g.FillEllipse(neuronBrush,
                    neuron.Position.X - size * 0.5f,
                    neuron.Position.Y - size * 0.5f,
                    size, size);

                // Draw outline
                using var outlinePen = new Pen(Color.FromArgb(150, Color.Black), 1f); // Black outline for contrast
                g.DrawEllipse(outlinePen,
                    neuron.Position.X - size * 0.5f,
                    neuron.Position.Y - size * 0.5f,
                    size, size);
            }
        }

        private Color GetColorForActivation(float activation, int alpha)
        {
            int colorIndex = (int)(activation * (_neuronColors.Length - 1));
            colorIndex = Math.Clamp(colorIndex, 0, _neuronColors.Length - 1);
            Color baseColor = _neuronColors[colorIndex];
            return Color.FromArgb(alpha, baseColor);
        }

        private void DrawUI(Graphics g, int width, int height, InputState input)
        {
            g.ResetTransform();

            string instructions = "LEFT: Add neurons | RIGHT: Stimulate | WHEEL: Learning rate";
            string stats =
                $"Neurons: {_neurons.Count} | Connections: {_connections.Count} | Learning: {_learningRate:F2}";

            using var font = new Font("Consolas", 9);
            using var brush = new SolidBrush(Color.FromArgb(220, 220, 220)); // Light grey instead of white
            using var bgBrush = new SolidBrush(Color.FromArgb(200, 20, 20, 20)); // Darker background

            SizeF instructionsSize = g.MeasureString(instructions, font);
            SizeF statsSize = g.MeasureString(stats, font);

            float maxWidth = Math.Max(instructionsSize.Width, statsSize.Width) + 20; // +20 for padding
            float totalHeight = instructionsSize.Height + statsSize.Height + 15; // +15 for spacing between lines

            // Draw background for text - now properly sized
            g.FillRectangle(bgBrush, 5, 5, maxWidth, totalHeight);

            g.DrawString(instructions, font, brush, 10, 10);
            g.DrawString(stats, font, brush, 10, 10 + instructionsSize.Height + 5);

            // Draw activation key
            if (_neurons.Count > 0)
            {
                DrawActivationKey(g, width, height);
            }
        }

        private void DrawActivationKey(Graphics g, int width, int height)
        {
            using var font = new Font("Consolas", 8);
            using var brush = new SolidBrush(Color.FromArgb(220, 220, 220)); // Light grey text

            int keyX = width - 120;
            int keyY = 10;

            using var bgBrush = new SolidBrush(Color.FromArgb(200, 20, 20, 20));
            g.FillRectangle(bgBrush, keyX - 5, keyY - 5, 115, 120);

            g.DrawString("Activation:", font, brush, keyX, keyY);

            for (int i = 0; i < _neuronColors.Length; i++)
            {
                float activation = (float)i / (_neuronColors.Length - 1);
                using var colorBrush = new SolidBrush(_neuronColors[i]);
                g.FillRectangle(colorBrush, keyX, keyY + 15 + i * 12, 10, 10);

                // Black outline for the color squares
                using var outlinePen = new Pen(Color.Black, 1f);
                g.DrawRectangle(outlinePen, keyX, keyY + 15 + i * 12, 10, 10);

                g.DrawString($"{activation:F1}", font, brush, keyX + 15, keyY + 15 + i * 12);
            }
        }

        private class Neuron
        {
            public int Id;
            public PointF Position;
            public PointF Velocity;
            public float Activation;
            public float Radius;
        }

        private class Connection
        {
            public Neuron FromNeuron;
            public Neuron ToNeuron;
            public float Weight;
        }
    }
}