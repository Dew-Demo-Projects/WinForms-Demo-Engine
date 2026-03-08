using DemoSystem.Factories;
using DemoSystem.Library;

namespace DemoSystem
{
    public partial class DemoSystem : Form
    {
        private Demo _demo;
        private InputState _frameInput;

        public DemoSystem()
        {
            InitializeComponent();

            var factory = new MyDemoFactory();

            _demo = factory.CreateDemo();

            // Handle input
            MouseMove += (_, e) => _frameInput = _frameInput with { Mouse = e.Location };
            MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left) _frameInput = _frameInput with { LeftDown = true };
                if (e.Button == MouseButtons.Right) _frameInput = _frameInput with { RightDown = true };
            };
            MouseUp += (_, e) =>
            {
                if (e.Button == MouseButtons.Left) _frameInput = _frameInput with { LeftDown = false };
                if (e.Button == MouseButtons.Right) _frameInput = _frameInput with { RightDown = false };
            };
            MouseWheel += (_, e) =>
                _frameInput = _frameInput with { WheelDelta = _frameInput.WheelDelta + e.Delta };
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void DemoSystem_Paint(object sender, PaintEventArgs e)
        {
            _demo.Render(e.Graphics, Width, Height, _frameInput);
        }
    }
}