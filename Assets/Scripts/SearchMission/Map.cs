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
            doc = XDocument.Load(fileName);
            try
            {
                doc = XDocument.Load(fileName);
            }
            catch (Exception)
            {
                Console.WriteLine("Error opening XML file!");
                return false;
            }

            // 获取根元素
            XElement root = doc.Element(Constants.CNS_TAG_ROOT);
            if (root == null)
            {
                Console.WriteLine("Error! No 'CNS_TAG_ROOT' tag found in XML file!");
                return false;
            }

            // 获取地图元素
            XElement map = root.Element(Constants.CNS_TAG_MAP);
            if (map == null)
            {
                Console.WriteLine("Error! No 'CNS_TAG_MAP' tag found in XML file!");
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
                        Console.WriteLine("Warning! Invalid value of 'CNS_TAG_HEIGHT' attribute encountered.");
                    }

                    // 获取宽度
                    if (int.TryParse(mapNode.Attribute(Constants.CNS_TAG_WIDTH)?.Value, out width) && width > 0)
                    {
                        hasWidth = true;
                    }
                    else
                    {
                        Console.WriteLine("Warning! Invalid value of 'CNS_TAG_WIDTH' attribute encountered.");
                    }

                    if (!(hasHeight && hasWidth))
                    {
                        Console.WriteLine("Error! No 'CNS_TAG_WIDTH' or 'CNS_TAG_HEIGHT' attribute in 'CNS_TAG_GRID' tag!");
                        return false;
                    }

                    // 初始化网格
                    Console.WriteLine("height is " + height);
                    Console.WriteLine("width is " + width);
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
                            Console.WriteLine($"Error! Not enough 'CNS_TAG_ROW' tags inside 'CNS_TAG_GRID' tag.");
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
                                Console.WriteLine($"Invalid value on grid in row {grid_i + 1}");
                                return false;
                            }
                        }

                        if (grid_j != width)
                        {
                            Console.WriteLine($"Invalid value on 'CNS_TAG_GRID' in row {grid_i + 1}");
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
                            Console.WriteLine($"Warning! Invalid position of cross ({cross_i},{cross_j}) attribute encountered.");
                            return false;

                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Warning! Invalid value of 'CNS_TAG_CROSS' attribute encountered.");
                        return false;
                    }
                }
            }

            if (!hasGrid)
            {
                Console.WriteLine("Error! There is no 'grid' tag in XML file!");
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
                    Console.Write(grid[i][j] + " "); // 输出每个元素，使用Tab分隔
                }
                Console.WriteLine(); // 每行结束换行
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
            Random random = new Random();
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
            Random random = new Random();
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
                    Console.WriteLine($"Cross ID {randomCross} invalid.");
                    return (0, 0);
                }

            }
            else
            {
                Console.WriteLine($"Cross ID {randomCross} not found.");
                return (0, 0);
            }
        }
    }
}


