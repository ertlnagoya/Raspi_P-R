using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using Microsoft.VisualBasic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO.Compression;

namespace Mission
{
    public class Map
    {
        private int height, width;
        private int emptyCellCount;
        private double cellSize;
        private int[][] grid;
        private Dictionary<int, (int cross_i, int cross_j)> crosses;

        public Map()
        {
            height = -1;
            width = -1;
            grid = null;
            cellSize = 1;
            crosses = new Dictionary<int, (int, int)>();
        }

        public bool GetMap(string fileName)
        {
            int grid_i = 0, grid_j = 0;
            emptyCellCount = 0;

            bool hasGrid = false, hasHeight = false, hasWidth = false;

            
            // 加载XML文件
            XDocument doc;
            
            //doc = XDocument.Load(fileName);
            
            try
            {
                doc = XDocument.Load(fileName);
            }
            catch (Exception)
            {
                Debug.LogError("Error opening XML file!");
                return false;
            }
            
            // 获取根元素
            XElement root = doc.Element(Constants.CNS_TAG_ROOT);
            if (root == null)
            {
                Debug.LogError("Error! No 'CNS_TAG_ROOT' tag found in XML file!");
                return false;
            }
            
            // 获取地图元素
            XElement map = root.Element(Constants.CNS_TAG_MAP);
            if (map == null)
            {
                Debug.LogError("Error! No 'CNS_TAG_MAP' tag found in XML file!");
                return false;
            }

            // 遍历 map 的子元素
            foreach (XElement mapNode in map.Elements())
            {
                string value = mapNode.Name.LocalName.ToLower();

                if (value == Constants.CNS_TAG_GRID)
                {
                    hasGrid = true;

                    // 获取高度
                    if (int.TryParse(mapNode.Attribute(Constants.CNS_TAG_HEIGHT)?.Value, out height) && height >= 0)
                    {
                        hasHeight = true;
                    }
                    else
                    {
                        Debug.LogError("Warning! Invalid value of 'CNS_TAG_HEIGHT' attribute encountered.");
                    }

                    // 获取宽度
                    if (int.TryParse(mapNode.Attribute(Constants.CNS_TAG_WIDTH)?.Value, out width) && width > 0)
                    {
                        hasWidth = true;
                    }
                    else
                    {
                        Debug.LogError("Warning! Invalid value of 'CNS_TAG_WIDTH' attribute encountered.");
                    }

                    if (!(hasHeight && hasWidth))
                    {
                        Debug.LogError("Error! No 'CNS_TAG_WIDTH' or 'CNS_TAG_HEIGHT' attribute in 'CNS_TAG_GRID' tag!");
                        return false;
                    }

                    // 初始化网格
                    //Debug.Log("height is " + height);
                    //Debug.Log("width is " + width);
                    grid = new int[height][];
                    for (int i = 0; i < height; i++)
                    {
                        grid[i] = new int[width];
                    }

                    XElement element = mapNode.Element(Constants.CNS_TAG_ROW);
                    while (grid_i < height)
                    {
                        if (element == null)
                        {
                            Debug.LogError($"Error! Not enough 'CNS_TAG_ROW' tags inside 'CNS_TAG_GRID' tag.");
                            return false;
                        }

                        // 解析行数据
                        List<string> elems = element.Value.Split(' ').ToList();
                        grid_j = 0;
                        int val;

                        for (grid_j = 0; grid_j < width; grid_j++)
                        {
                            if (grid_j >= elems.Count) break;
                            if (int.TryParse(elems[grid_j], NumberStyles.Integer, CultureInfo.InvariantCulture, out val))
                            {
                                grid[grid_i][grid_j] = val;
                                if (val == Constants.CN_GC_NOOBS)
                                {
                                    emptyCellCount++;
                                }
                            }
                            else
                            {
                                Debug.LogError($"Invalid value on grid in row {grid_i + 1}");
                                return false;
                            }
                        }

                        if (grid_j != width)
                        {
                            Debug.LogError($"Invalid value on 'CNS_TAG_GRID' in row {grid_i + 1}");
                            return false;
                        }

                        grid_i++;
                        element = element.ElementsAfterSelf(Constants.CNS_TAG_ROW).FirstOrDefault();

                    }
                }
                if (value == Constants.CNS_TAG_CROSS)
                {
                    if (int.TryParse(mapNode.Attribute("id")?.Value, out int id) &&
                        int.TryParse(mapNode.Attribute("grid_i")?.Value, out int cross_i) &&
                        int.TryParse(mapNode.Attribute("grid_j")?.Value, out int cross_j))
                    {
                        // 添加到字典
                        if (grid[cross_i][cross_j] == 0 && getCellDegree(cross_i, cross_j) >= 3)
                        {
                            crosses.Add(id, (cross_i, cross_j));
                        }
                        else 
                        {
                            Debug.LogError($"Warning! Invalid position of cross ({cross_i},{cross_j}) attribute encountered.");
                            return false;

                        }
                        
                    }
                    else
                    {
                        Debug.LogError("Warning! Invalid value of 'CNS_TAG_CROSS' attribute encountered.");
                        return false;
                    }
                }
            }

            if (!hasGrid)
            {
                Debug.LogError("Error! There is no 'grid' tag in XML file!");
                return false;
            }
            
            return true;
        }
        public void ShowMap()
        {
            // 遍历行和列
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    Debug.Log(grid[i][j] + " "); // 输出每个元素，使用Tab分隔
                }
                Debug.Log("\n"); // 每行结束换行
            }
        }

        public bool CellIsTraversable(int i, int j, HashSet<Node> occupiedNodes)
        {
            return (grid[i][j] == Constants.CN_GC_NOOBS) && !occupiedNodes.Contains(new Node(i, j));
        }

        public bool CellIsObstacle(int i, int j)
        {
            return (grid[i][j] != Constants.CN_GC_NOOBS);
        }

        public bool CellOnGrid(int i, int j)
        {
            return (i < height && i >= 0 && j < width && j >= 0);
        }
        /*
        public Node RandomConnectingCell(Node conflicPosition, HashSet<Node> goalList)
        {
            int i = conflicPosition.i;
            int j = conflicPosition.j;
            
            // 检查上边
            if (i - 1 >= 0 && grid[i - 1][ j] == 0)
                return new Node(i - 1, j);

            // 检查下边
            if (i + 1 < height && grid[i + 1][ j] == 0)
                return new Node(i + 1, j);

            // 检查左边
            if (j - 1 >= 0 && grid[i][ j - 1] == 0)
                return new Node(i, j - 1);

            // 检查右边
            if (j + 1 < width && grid[i][ j + 1] == 0)
                return new Node(i, j + 1);
            return null;
        }
        */

        public Node RandomConnectingCell(Node conflicPosition, HashSet<Node> goalList)
        {
            List<Node> candidates = new List<Node>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            FindCandidates(conflicPosition.i, conflicPosition.j, goalList, candidates, visited);

            if (candidates.Count > 0)
            {
                System.Random rand = new System.Random();
                return candidates[rand.Next(candidates.Count)];
            }

            return null; // 理论上不会执行到这里
        }

        private void FindCandidates(int i, int j, HashSet<Node> goalList, List<Node> candidates, HashSet<(int, int)> visited)
        {
            if (visited.Contains((i, j))) return;
            visited.Add((i, j));

            // 依次检查四个方向
            if (i - 1 >= 0 && grid[i - 1][j] == 0 && !goalList.Any(goal => goal.i == i - 1 && goal.j == j))
                candidates.Add(new Node(i - 1, j));
            if (i + 1 < height && grid[i + 1][j] == 0 && !goalList.Any(goal => goal.i == i + 1 && goal.j == j))
                candidates.Add(new Node(i + 1, j));
            if (j - 1 >= 0 && grid[i][j - 1] == 0 && !goalList.Any(goal => goal.i == i && goal.j == j - 1))
                candidates.Add(new Node(i, j - 1));
            if (j + 1 < width && grid[i][j + 1] == 0 && !goalList.Any(goal => goal.i == i && goal.j == j + 1))
                candidates.Add(new Node(i, j + 1));

            // 如果找到了至少一个候选点，则停止
            if (candidates.Count > 0) return;

            // 否则，递归查找四个方向的相邻点
            if (i - 1 >= 0 && grid[i - 1][j] == 0) FindCandidates(i - 1, j, goalList, candidates, visited);
            if (i + 1 < height && grid[i + 1][j] == 0) FindCandidates(i + 1, j, goalList, candidates, visited);
            if (j - 1 >= 0 && grid[i][j - 1] == 0) FindCandidates(i, j - 1, goalList, candidates, visited);
            if (j + 1 < width && grid[i][j + 1] == 0) FindCandidates(i, j + 1, goalList, candidates, visited);
        }

        public int getValue(int i, int j)
        {
            if (i < 0 || i >= height)
                return -1;

            if (j < 0 || j >= width)
                return -1;

            return grid[i][j];
        }

        public int getMapHeight()
        {
            return height;
        }

        public int getMapWidth()
        {
            return width;
        }

        public int getEmptyCellCount()
        {
            return emptyCellCount;
        }

        public double getCellSize()
        {
            return cellSize;
        }

        public int getCellDegree(int i, int j)
        {
            int degree = 0;
            for (int di = -1; di <= 1; ++di)
            {
                for (int dj = -1; dj <= 1; ++dj)
                {
                    if ((di == 0) ^ (dj == 0))
                    {
                        if (CellOnGrid(i + di, j + dj) && !CellIsObstacle(i + di, j + dj))
                        {
                            ++degree;
                        }
                    }
                }
            }
            //Console.WriteLine($"degree of {i} , {j} is {degree}");
            return degree;
        }

        public (int i, int j) getRandomAvalibleCell()
        {
            System.Random random = new System.Random();
            int randomCell = random.Next(0, emptyCellCount) + 1;
            int count = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (grid[i][j] == 0)
                    {
                        count++;
                        if (count == randomCell)
                        {
                            return (i, j); // 返回第 targetIndex 个 0 的坐标
                        }
                    }
                }
            }
            return (0, 0);
        }

        public (int i, int j) getRandomAvalibleCross()
        {
            System.Random random = new System.Random();
            int randomCross = random.Next(0, crosses.Count - 1);
            if (crosses.TryGetValue(randomCross, out var coordinates))
            {
                if (grid[coordinates.cross_i][coordinates.cross_j] == 0)
                {
                    //Console.WriteLine($"Random Cross: ID = {randomCross}, Grid Coordinates = (i: {coordinates.cross_i}, j: {coordinates.cross_j})");
                    return (coordinates.cross_i, coordinates.cross_j);
                }
                else
                {
                    //Debug.Log($"Cross ID {randomCross} invalid.");
                    return (0, 0);
                }

            }
            else
            {
               // Debug.Log($"Cross ID {randomCross} not found.");
                return (0, 0);
            }
        }
    }
}


