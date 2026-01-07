using System;
using System.Collections.Generic;

namespace Rookiss
{
	public class PriorityQueue<T> where T : IComparable<T>
	{
		private List<T> heap = new List<T>();

		public void Enqueue(T item)
		{
			heap.Add(item);
			int i = heap.Count - 1;
			while (i > 0)
			{
				int parent = (i - 1) / 2;
				if (heap[i].CompareTo(heap[parent]) >= 0) break;
				Swap(i, parent);
				i = parent;
			}
		}

		public T Dequeue()
		{
			if (heap.Count == 0) throw new System.InvalidOperationException();
			T result = heap[0];
			int last = heap.Count - 1;
			heap[0] = heap[last];
			heap.RemoveAt(last);
			int i = 0;
			while (true)
			{
				int left = 2 * i + 1, right = 2 * i + 2;
				int smallest = i;
				if (left < heap.Count && heap[left].CompareTo(heap[smallest]) < 0) smallest = left;
				if (right < heap.Count && heap[right].CompareTo(heap[smallest]) < 0) smallest = right;
				if (smallest == i) break;
				Swap(i, smallest);
				i = smallest;
			}
			return result;
		}

		public bool Contains(T item)
		{
			return heap.Contains(item);
		}

		public int Count => heap.Count;

		private void Swap(int i, int j)
		{
			T temp = heap[i];
			heap[i] = heap[j];
			heap[j] = temp;
		}
	}

}