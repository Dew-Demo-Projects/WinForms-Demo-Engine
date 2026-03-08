namespace DemoSystem.Library.Effects
{
    internal class PulsingRings : IEffect
    {
        private readonly Color[] _colors;
        private readonly int _ringCount;
        private readonly float _maxRingSize;
        private readonly float _pulseSpeed;
        private readonly int _xOffset;
        private readonly int _yOffset;

        public PulsingRings(Color[] colors, int ringCount = 5, float maxRingSize = 200f,
            float pulseSpeed = 0.1f, int xOffset = 0, int yOffset = 0)
        {
            _colors = colors.Length > 0 ? colors : new[] { Color.Red, Color.Blue, Color.Green };
            _ringCount = ringCount;
            _maxRingSize = maxRingSize;
            _pulseSpeed = pulseSpeed;
            _xOffset = xOffset;
            _yOffset = yOffset;
        }

        public PulsingRings(int xOffset = 0, int yOffset = 0)
            : this(new[] { Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.Indigo, Color.Violet },
                7, 250f, 0.08f, xOffset, yOffset)
        {
        }

        public void Render(Graphics g, int width, int height, int localFrame, int globalFrame)
        {
            var centerX = width / 2 + _xOffset;
            var centerY = height / 2 + _yOffset;

            // Calculate pulsing factor (0 to 1)
            var pulse = (float)((Math.Sin(localFrame * _pulseSpeed) + 1) / 2);

            for (int i = 0; i < _ringCount; i++)
            {
                // Calculate ring properties with phase offset
                var phaseOffset = (float)i / _ringCount;
                var ringPulse = (float)((Math.Sin(localFrame * _pulseSpeed + phaseOffset * Math.PI) + 1) / 2);

                // Calculate ring size with pulsing
                var ringSize = 20f + ringPulse * _maxRingSize;

                // Calculate ring thickness with pulsing
                var thickness = 3f + pulse * 8f;

                // Get color with cycling based on frame and ring index
                var colorIndex = (i + globalFrame / 10) % _colors.Length;
                var color = _colors[colorIndex];

                // Create pen with alpha transparency that also pulses
                var alpha = (int)(150 + 105 * ringPulse);
                using var pen = new Pen(Color.FromArgb(alpha, color), thickness);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // Draw the ring
                g.DrawEllipse(pen,
                    centerX - ringSize / 2,
                    centerY - ringSize / 2,
                    ringSize,
                    ringSize);
            }

            // Draw a bright center point that pulses
            var centerPulse = (float)((Math.Sin(localFrame * _pulseSpeed * 2) + 1) / 2);
            var centerSize = 5f + centerPulse * 15f;
            var centerColor = Color.FromArgb(200 + (int)(55 * centerPulse), Color.White);

            using var centerBrush = new SolidBrush(centerColor);
            g.FillEllipse(centerBrush,
                centerX - centerSize / 2,
                centerY - centerSize / 2,
                centerSize,
                centerSize);
        }
    }
}