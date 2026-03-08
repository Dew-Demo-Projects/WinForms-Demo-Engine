using DemoSystem.Library.Effects;

namespace DemoSystem.Library
{
    public class Moment(int frameCount, float timeMultiplier = 1)
    {
        private int _localFrame;
        private List<IEffect> _effects = [];
        private List<IInteractiveEffect> _interactiveEffects = [];
        private float _timeMultiplier = timeMultiplier;

        public bool Render(Graphics graphics, int width, int height,
            int globalFrame, in InputState input = default)
        {
            var finalLocalFrame = (int)(_localFrame * _timeMultiplier);
            var finalGlobalFrame = (int)(globalFrame * _timeMultiplier);
            foreach (var effect in _effects)
            {
                effect.Render(graphics, width, height, finalLocalFrame, finalGlobalFrame);
            }

            foreach (var interactiveEffect in _interactiveEffects)
            {
                interactiveEffect.Render(graphics, width, height, finalLocalFrame, finalGlobalFrame, input);
            }

            _localFrame++;

            if (_localFrame < frameCount) return _localFrame < frameCount;
            _localFrame = 0;
            return false;
        }

        internal void AddEffect(IEffect effect)
        {
            _effects.Add(effect);
        }

        internal void AddInteractiveEffect(IInteractiveEffect interactiveEffect)
        {
            _interactiveEffects.Add(interactiveEffect);
        }
    }
}