using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    using System;
    using System.Collections.Generic;

    
    /*
    public class NodeComparer : IComparer<Node>
    {
        public int Compare(Node? x, Node? y)
        {
            //if (x == null && y == null) return 0;
           // if (x == null) return -1;
           // if (y == null) return 1;

            // 只比较坐标，确保一致性
            int result = x.i.CompareTo(y.i);
            if (result == 0) result = x.j.CompareTo(y.j);
            return result;
        }
    }
    */

    public class Node : IComparable<Node>
    {
        public int i, j; // grid cell coordinates
        public int g; // f-, g- and h-values of the search node
        public double F, H;
        public Node parent; // backpointer to the predecessor node
        public int conflictsCount;

        public Node(int x = 0, int y = 0, Node? p = null, int g_ = 0, double H_ = 0, int conflictsCount_ = 0)
        {
            i = x;
            j = y;
            parent = p;
            g = g_;
            H = H_;
            F = (g_ == Constants.CN_INFINITY) ? g_ : g_ + H_; //(总代价/优先级值)
            conflictsCount = conflictsCount_;
        }

        public Node DeepCopy()
        {
            return new Node(
                x: this.i,
                y: this.j,
                p: this.parent, // 递归深拷贝 parent
                g_: this.g, //g (代价/到达成本)
                H_: this.H, //(启发值/估计成本)
                conflictsCount_: this.conflictsCount
            );
        }

        public override bool Equals(object obj)
        {
            if (obj is Node other)
            {
                return i == other.i && j == other.j;
            }
            return false;
        }

        public static bool operator ==(Node left, Node right)
        {
            if (ReferenceEquals(left, right)) return true; // 如果引用相等
            //if (left is null || right is null) return false; // 其中一个为 null
            return left.Equals(right); // 使用 Equals 方法判断逻辑相等
        }

        public static bool operator !=(Node left, Node right)
        {
            return !(left == right); // 使用 == 的逻辑反向判断
        }

        public override int GetHashCode()
        {
            return (i + j) * (i + j + 1) + j;
        }

        /*
        public int CompareTo(Node? other)
        {
            if (other == null) return 1;

            int result = i.CompareTo(other.i);
            if (result == 0) result = j.CompareTo(other.j);
            return result;
        }
        */
        public int CompareTo(Node? other)
        {
            // Compare by F, then by -g, then by i, then by j
            /*
            if (other == null)
            {
                // 如果当前对象为 null，应返回 -1，否则返回 1
                return 1; // 假设 null 比任何非 null 对象小
            }
            */
            int result = F.CompareTo(other.F);
            if (result == 0) result = (-g).CompareTo(-other.g);
            if (result == 0) result = i.CompareTo(other.i);
            if (result == 0) result = j.CompareTo(other.j);
            return result;
        }

        public virtual int convolution(int width, int height, bool withTime = false)
        {
            int res = withTime ? width * height * g : 0;
            return res + i * width + j;
        }

        public virtual int getHC()
        {
            return 0;
        }
    }

    /*
    public class NodeHash : IEqualityComparer<Node>
    {
        public bool Equals(Node? x, Node? y)
        {
            //if (x == null && y == null) return true;
            //if (x == null || y == null) return false;
            return x.Equals(y);
        }

        public int GetHashCode(Node node)
        {
            return node.GetHashCode();
        }
    }

    public class NodePairHash : IEqualityComparer<(Node, Node)>
    {
        public bool Equals((Node, Node) x, (Node, Node) y)
        {
            return x.Item1.Equals(y.Item1) && x.Item2.Equals(y.Item2);
        }

        public int GetHashCode((Node, Node) pair)
        {
            NodeHash nodeHash = new NodeHash();
            int hash1 = nodeHash.GetHashCode(pair.Item1);
            int hash2 = nodeHash.GetHashCode(pair.Item2);
            return (hash1 + hash2) * (hash1 + hash2 + 1) + hash2;
        }

    }
    */
}
