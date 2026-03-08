namespace DemoSystem.Library.Effects
{
    internal class GrowingCircle(Brush brush, int xOffset = 0, int yOffset = 0) : IEffect
    {
        private Brush _brush = brush;
        private int _xOffset = xOffset;
        private int _yOffset = yOffset;

        public void Render(Graphics g, int width, int height, int localFrame, int globalFrame)
        {
            g.FillEllipse(
                _brush,
                width / 2 - localFrame + _xOffset,
                height / 2 - localFrame + _yOffset,
                localFrame * 2,
                localFrame * 2
            );
        }
    }
}