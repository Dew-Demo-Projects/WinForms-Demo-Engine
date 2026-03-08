namespace DemoSystem.Library.Effects
{
    internal interface IEffect
    {
        void Render(Graphics g, int width, int height, int localFrame, int globalFrame);
    }
}