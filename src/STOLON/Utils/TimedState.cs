namespace STOLON
{
    public struct TimedState
    {
        private int _lastValue;
        private int _time;
        public int Time => _time;
        public void Update(int currentValue, int elapsedMilliseconds)
        {
            _time = EqualityComparer<int>.Default.Equals(_lastValue, currentValue) ? _time + elapsedMilliseconds : 0;
            _lastValue = currentValue;
        }
        public void ResetTime() => _time = 0;
        public void UpdatePositive(int currentValue, int elapsedMilliseconds)
        {
            Update(currentValue, elapsedMilliseconds);
            if (currentValue < 0) ResetTime();
        }
    }
}
