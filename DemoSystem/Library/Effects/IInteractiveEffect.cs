namespace DemoSystem.Library.Effects;

public interface IInteractiveEffect
{
    void Render(Graphics g, int width, int height, int localFrame, int globalFrame, in InputState inputState);
}