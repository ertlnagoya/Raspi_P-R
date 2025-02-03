using System;
using System.Collections.Generic;
using UnityEngine;

namespace LineTrace
{
    public class RouteSearch
    {
        public const float EdgeInitialDistance = 100000f;
        public const float InfiniteDistance = 100000000f;
        private const float EdgeFlagPenalty = 7f;
        private const float CrossFlagPenalty = 1f;
        private const float DirectionalPenalty = 1.5f;

        public int Size { get; private set; }
        public float[][] Edges { get; private set; }
        public bool[][] EdgeFlags {  get; set; }
        public bool[] CrossFlags { get; set; }

        // private static readonly Random random = new Random();

        public RouteSearch()
        {
            Size = 0;
            Edges = null;
            EdgeFlags = null;
            CrossFlags = null;
        }

        private bool IsInitialized => Edges != null && EdgeFlags != null && CrossFlags != null;

        public bool Available(int src, int dst)
        {
            if (!IsInitialized || !IsValidIndex(src) || !IsValidIndex(dst)) return false;
            return !(EdgeFlags[src][dst] || EdgeFlags[dst][src] || CrossFlags[src]);
        }

        public void MakeEdge(int i, int j, float length)
        {
            if (!IsInitialized || !IsValidIndex(i) || !IsValidIndex(j))
                throw new InvalidOperationException("The graph must be initialized before making edges.");

            Edges[i][j] = Edges[j][i] = length;
        }

        public void SetZero()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The graph is not initialized.");

            Array.Clear(CrossFlags, 0, Size);
            for (int i = 0; i < Size; i++)
            {
                Array.Clear(EdgeFlags[i], 0, Size);
            }
        }

        public struct Path
        {
            public int Cross1;
            public int Cross2;
            public float Length;
        }

        public void Init(int size, List<Path> edges)
        {
            Size = size;
            Edges = new float[size][];
            EdgeFlags = new bool[size][];
            CrossFlags = new bool[size];

            for (int i = 0; i < size; i++)
            {
                Edges[i] = new float[size];
                EdgeFlags[i] = new bool[size];
                for (int j = 0; j < size; j++)
                {
                    Edges[i][j] = EdgeInitialDistance;
                }
            }

            foreach (var edge in edges)
            {
                MakeEdge(edge.Cross1, edge.Cross2, edge.Length);
            }
        }

        public float DirectionWeight(int i, int j)
        {
            if (!IsInitialized || !IsValidIndex(i) || !IsValidIndex(j)) return EdgeInitialDistance;
            float weight = Edges[i][j];
            weight += EdgeFlags[i][j] ? DirectionalPenalty : 0;
            weight += EdgeFlags[j][i] ? EdgeFlagPenalty : 0;
            weight += CrossFlags[j] ? CrossFlagPenalty : 0;
            return weight;
        }

        public float FlagWeight(int i, int j)
        {
            if (!IsInitialized || !IsValidIndex(i) || !IsValidIndex(j)) return EdgeInitialDistance;
            float weight = Edges[i][j];
            weight += EdgeFlags[i][j] ? EdgeFlagPenalty : 0;
            weight += EdgeFlags[j][i] ? EdgeFlagPenalty : 0;
            weight += CrossFlags[j] ? CrossFlagPenalty : 0;
            return weight;
        }

        public float Length(int i, int j)
        {
            return IsInitialized && IsValidIndex(i) && IsValidIndex(j) ? Edges[i][j] : EdgeInitialDistance;
        }

        public int NaiveDijkstra(int start, int end) => Dijkstra(start, end, Length);
        public int FlagDijkstra(int start, int end) => Dijkstra(start, end, FlagWeight);
        public int DirectionDijkstra(int start, int end) => Dijkstra(start, end, DirectionWeight);

        // public int RandomDijkstra(int start)
        // {
        //     if (!IsInitialized || !IsValidIndex(start))
        //         throw new ArgumentOutOfRangeException(nameof(start));

        //     List<int> neighbors = new List<int>();
        //     for (int i = 0; i < Size; i++)
        //     {
        //         if (Edges[start][i] < EdgeInitialDistance) neighbors.Add(i);
        //     }

        //     if (neighbors.Count == 0)
        //         throw new InvalidOperationException("No available neighbors.");

        //     return neighbors[random.Next(neighbors.Count)];
        // }

        public int Dijkstra(int start, int end, Func<int, int, float> lengthFunc)
        {
            if (!IsInitialized || !IsValidIndex(start) || !IsValidIndex(end))
                throw new ArgumentOutOfRangeException();

            float[] costs = new float[Size];
            int[] precs = new int[Size];
            bool[] checks = new bool[Size];

            for (int i = 0; i < Size; i++)
            {
                costs[i] = InfiniteDistance;
                checks[i] = false;
            }
            costs[start] = 0;

            for (int l = 0; l < Size; l++)
            {
                float minCost = InfiniteDistance;
                int currentNode = -1;

                for (int k = 0; k < Size; k++)
                {
                    if (!checks[k] && costs[k] < minCost)
                    {
                        minCost = costs[k];
                        currentNode = k;
                    }
                }

                if (currentNode == -1)
                    throw new InvalidOperationException("No valid node found. Check the input data.");

                checks[currentNode] = true;
                for (int j = 0; j < Size; j++)
                {
                    float newCost = costs[currentNode] + lengthFunc(currentNode, j);
                    if (newCost < costs[j])
                    {
                        costs[j] = newCost;
                        precs[j] = currentNode;
                    }
                }
            }

            int tmp = end;
            while (start != precs[tmp])
            {
                tmp = precs[tmp];
            }
            return tmp;
        }

        public void Output(Action<string> logger)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The graph is not initialized.");

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (EdgeFlags[i][j])
                    {
                        logger?.Invoke($"{i} -> {j}");
                    }
                }
            }
        }

        private bool IsValidIndex(int index)
        {
            if (index < 0 || index >= Size)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of bounds.");
            return true;
        }

        public void PrintFlags()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("The graph is not initialized.");

            // Debug log for CrossFlags
            Debug.Log("CrossFlags:");
            if (CrossFlags != null)
            {
                Debug.Log(string.Join(",", CrossFlags));
            }
            else
            {
                Debug.Log("CrossFlags is null");
            }

            // Debug log for EdgeFlags
            Debug.Log("EdgeFlags:");
            if (EdgeFlags != null)
            {
                for (int i = 0; i < EdgeFlags.Length; i++)
                {
                    // 1行分の出力を格納するための変数
                    string rowOutput = $"Row {i}: ";
                    
                    if (EdgeFlags[i] != null)
                    {
                        for (int j = 0; j < EdgeFlags[i].Length; j++)
                        {
                            rowOutput += $"[{j}]: {EdgeFlags[i][j]} ";
                        }
                    }
                    else
                    {
                        rowOutput += "null";
                    }
                    
                    // 行全体を一度に出力
                    Debug.Log(rowOutput);
                }
            }
            else
            {
                Debug.Log("EdgeFlags is null");
            }
        }
    }
}
