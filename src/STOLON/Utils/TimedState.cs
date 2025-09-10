using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace STOLON
{
    public struct TimedState<T>
    {
        private T _lastValue;
        private int _time;
        public int Time => _time;
        public void Update(T currentValue, int elapsedMilliseconds)
        {
            _time = EqualityComparer<T>.Default.Equals(_lastValue, currentValue) ? _time + elapsedMilliseconds : 0;
            _lastValue = currentValue;
        }
        public void ResetTime() => _time = 0;
    }
    public static class TimedStateExtensions
    {
        public static void UpdatePositive(this ref TimedState<int> state, int currentValue, int elapsedMilliseconds)
        {
            state.Update(currentValue, elapsedMilliseconds);
            if (currentValue < 0) state.ResetTime();
        }
    }
}
