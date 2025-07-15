using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace WpfAppProlab1.Services
{
    public class PriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private List<(TElement Element, TPriority Priority)> elements = new List<(TElement, TPriority)>();

        public int Count => elements.Count;

        public void Enqueue(TElement item, TPriority priority)
        {
            elements.Add((item, priority));
            elements.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        public TElement Dequeue()
        {
            if (elements.Count == 0)
                throw new InvalidOperationException("PriorityQueue is empty.");

            var bestItem = elements[0].Element;
            elements.RemoveAt(0);
            return bestItem;
        }
    }
}

