// Old Skull Games
// Bernard Barthelemy
// Thursday, December 6, 2018

using System;
using System.Collections.Generic;

namespace OSG.Core
{
    public class Accumulator<T>
    {
        private readonly Func<T, T, T> add;
        private readonly Func<T, T, T> sub;
        private readonly Func<T, int, T> divide;

        private Queue<T> queue;

        private Accumulator()
        {
            throw new Exception("Nope.");
        }

        protected Accumulator(Func<T,T,T> _add, Func<T, T, T> _sub, Func<T,int,T> _divide, int capacity)
        {
            add = _add;
            sub = _sub;
            divide = _divide;
            sum = default(T);
            this.capacity = capacity;
            queue = new Queue<T>(capacity);
        }
        private T sum;
        private T average;
        private readonly int capacity;

        public T Sum => sum;
        public T Average => average;
        public int Count => queue.Count;

        public void AddValue(T value)
        {
            if(queue.Count>=capacity)
            {
                sum = sub(sum, queue.Dequeue());
            }
            queue.Enqueue(value);
            sum = add(value, sum);
            average = divide(sum, queue.Count);
        }

        public void Reset()
        {
            sum = default(T);
            average = sum;
            queue.Clear();
        }

        public static implicit operator T(Accumulator<T> acc)
        {
            return acc.Average;
        }
    }

    public class FloatAccumulator : Accumulator<float>
    {
        public FloatAccumulator(int capacity)
            : base((f1, f2) => f1 + f2,
                (f1, f2) => f1 - f2,
                (f, i) => f / i, capacity)
        {
        }
    }

}