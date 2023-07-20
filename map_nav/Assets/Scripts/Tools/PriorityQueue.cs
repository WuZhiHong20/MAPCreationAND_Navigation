using System;
using System.Collections.Generic;
using UnityEditor;

namespace Tools
{
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> queue;

        public PriorityQueue()
        {
            queue = new List<T>();
        }

        public PriorityQueue(List<T> q)
        {
            queue = q;
            int n = queue.Count;
            n -= 2;
            n /= 2;
            for (int i = n; i >= 0; --i)
            {
                Down(i);
            }
        }

        public int Count()
        {
            return queue.Count;
        }

        public PriorityQueue<T> Add(T item)
        {
            queue.Add(item);
            int son = queue.Count;
            --son;
            int father = (son-1) / 2;
            while (son > 0)
            {
                if (item.CompareTo(queue[father]) == 1)
                {
                    break;
                }

                queue[son] = queue[father];
                son = father;
                --father;
                father /= 2;
            }

            queue[son] = item;
            return this;
        }

        /// <summary>
        /// 弹出最顶的元素
        /// </summary>
        /// <returns></returns>
        public PriorityQueue<T> Pop()
        {
            queue[0] = queue[queue.Count - 1];
            queue.RemoveAt(queue.Count - 1);
            Down(0);
            return this;
        }

        private void Down(int pos)
        {
            int n = queue.Count - 1;
            int t;
            while (pos <= n)
            {
                t = pos;
                if (pos * 2 + 1 <= n && queue[t].CompareTo(queue[pos * 2 + 1]) == 1)
                {
                    t = pos * 2 + 1;
                }

                if (pos * 2 + 2 <= n && queue[t].CompareTo(queue[pos * 2 + 2]) == 1)
                {
                    t = pos * 2 + 2;
                }

                if (pos == t) break;

                (queue[t], queue[pos]) = (queue[pos], queue[t]);
                pos = t;
            }
        }

        public T GetTop()
        {
            return queue[0];
        }
    }
}