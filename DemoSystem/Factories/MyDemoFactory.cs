using DemoSystem.Library;
using DemoSystem.Library.Effects;

namespace DemoSystem.Factories
{
    internal class MyDemoFactory : IDemoFactory
    {
        public Demo CreateDemo()
        {
            var demo = new Demo();


            var moment1 = new Moment(frameCount: 60, timeMultiplier: 1f);
            moment1.AddEffect(new ElectricArcs(2));

            var moment2 = new Moment(frameCount: 200);
            moment2.AddEffect(new RotatingGravityBox(500));

            var moment3 = new Moment(frameCount: 300, timeMultiplier: 1f);
            moment3.AddInteractiveEffect(new MagneticParticles(200));

            var moment4 = new Moment(frameCount: 400, timeMultiplier: 1);
            moment4.AddInteractiveEffect(new NeuralNetworkFireworks());

            var moment5 = new Moment(frameCount: 300, timeMultiplier: 1);
            moment5.AddInteractiveEffect(new InteractiveFluid(200));

            demo.AddMoment(moment1);
            demo.AddMoment(moment2);
            demo.AddMoment(moment3);
            demo.AddMoment(moment4);
            demo.AddMoment(moment5);

            return demo;
        }
    }
}