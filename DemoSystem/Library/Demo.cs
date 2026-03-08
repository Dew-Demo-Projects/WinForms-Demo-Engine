namespace DemoSystem.Library
{
    public class Demo
    {
        private int _frame = 0;
        private List<Moment> _moments = [];
        private int _currentMomentIndex = 0;

        public void AddMoment(Moment moment)
        {
            _moments.Add(moment);
        }

        public void Render(Graphics graphics, int width, int height, in InputState inputState)
        {
            var moment = _moments[_currentMomentIndex];
            bool momentContinues = moment.Render(graphics, width, height, _frame, inputState);

            if (!momentContinues)
            {
                _currentMomentIndex++;
                if (_currentMomentIndex >= _moments.Count)
                {
                    _currentMomentIndex = 0;
                }
            }

            _frame++;
        }
    }
}