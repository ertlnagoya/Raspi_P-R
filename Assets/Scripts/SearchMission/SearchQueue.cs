using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mission
{
    using System;
    using System.Collections.Generic;

    public class SearchQueue
    {
        public SortedSet<Node> sortByKey;
        public Dictionary<int, Node> sortByIndex;
        public Func<Node, Node, bool> cmp;

        public SearchQueue(Func<Node, Node, bool> cmp = null)
        {
            this.cmp = cmp ?? ((lhs, rhs) => lhs.CompareTo(rhs) < 0);
            //sortByKey = new SortedSet<Node>(Comparer<Node>.Create((lhs, rhs) => this.cmp(lhs, rhs) ? -1 : 1));
            sortByKey = new SortedSet<Node>(Comparer<Node>.Create((lhs, rhs) => {
                if (lhs.CompareTo(rhs) == -1) return -1; // lhs 小于 rhs
                if (rhs.CompareTo(lhs) == -1) return 1;  // rhs 小于 lhs
                return 0;                   // 两者相等
            }));
            sortByIndex = new Dictionary<int, Node>();
        }

        public bool Insert(Map map, Node node, bool withTime, bool withOld = false, Node? old = null)
        {
            if (!withOld)  //true
            {
                old = GetByIndex(map, node, withTime);  //
            }

            if (old == null || cmp(node, old)) //old == null 之前没插入过，且
            {
                if (old != null)
                {
                    sortByKey.Remove(old);
                }
                int index = node.convolution(map.getMapWidth(), map.getMapHeight(), withTime);
                sortByIndex[index] = node;
                sortByKey.Add(node);
                return true;
            }
            return false;
        }

        public void Erase(Map map, Node node, bool withTime)
        {            
            sortByKey.Remove(node);
            int index = node.convolution(map.getMapWidth(), map.getMapHeight(), withTime);
            sortByIndex.Remove(index);            
        }

        public Node GetByIndex(Map map, Node node, bool withTime)
        {
            int index = node.convolution(map.getMapWidth(), map.getMapHeight(), withTime);
            if (sortByIndex.TryGetValue(index, out Node value))
            {
                return value;
            }
            return null;
        }

        public void MoveByUpperBound(SearchQueue other, double threshold, Map map, SortedSet<double> otherF, bool withTime = false)
        {
            var toRemove = new List<Node>();

            foreach (var it in sortByKey)
            {
                if (it.F > threshold)
                    break;

                other.Insert(map, it, withTime);
                otherF.Add(it.F);
                int index = it.convolution(map.getMapWidth(), map.getMapHeight(), withTime);
                sortByIndex.Remove(index);
                toRemove.Add(it);
            }

            foreach (var node in toRemove)
            {
                sortByKey.Remove(node);
            }
        }

        public void MoveByLowerBound(SearchQueue other, double threshold, Map map, SortedSet<double> FValues, bool withTime = false)
        {
            var toRemove = new List<Node>();

            foreach (var it in sortByKey)
            {
                if (it.F <= threshold)
                    continue;

                other.Insert(map, it, withTime);
                int index = it.convolution(map.getMapWidth(), map.getMapHeight(), withTime);
                sortByIndex.Remove(index);
                FValues.Remove(it.F);
                toRemove.Add(it);
            }

            foreach (var node in toRemove)
            {
                sortByKey.Remove(node);
            }
        }

        public Node GetFront()
        {
            return sortByKey.Min;
        }

        public bool IsEmpty()
        {
            return sortByKey.Count == 0;
        }

        public int Size()
        {
            return sortByKey.Count;
        }

        public void Clear()
        {
            sortByKey.Clear();
            sortByIndex.Clear();
        }
    }


}

