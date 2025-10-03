using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public class Config
    {
        /*
            SearchParams (double[])：用于存储搜索算法的参数值，例如权重、成本等，可能用于自定义算法的行为。
            LogParams (string[])：日志相关参数，通常用于指定日志路径、日志文件名等。
            N (uint)：一般用于表示搜索或路径规划中的节点数或代理数的上限。
            searchType (int)：搜索算法类型的标识符，例如 CBS（Conflict-Based Search）或 A*。
            lowLevel (int)：低级搜索算法类型，可能代表不同的子路径查找算法（如 A*、SIPP 等）。
            minAgents (int)：代理的最小数量。
            maxAgents (int)：代理的最大数量，默认为 -1 表示无上限。
            maxTime (int)：搜索或路径规划的最大运行时间，单位可能为秒或毫秒。
            agentsFile (string)：包含代理信息的文件路径，例如代理的初始位置和目标位置。
            tasksCount (int)：任务数量，用于表示需要分配的代理任务的总数。
            withCAT (bool)：是否启用冲突避免表（Conflict Avoidance Table），用于优化多代理路径规划。
            withPerfectHeuristic (bool)：是否启用完美启发式函数，可能用于更精确的路径估计。
            ppOrder (int)：启发式处理时的优先级顺序或任务执行顺序。
            parallelizePaths1 (bool)：是否在一级路径搜索上并行化。
            parallelizePaths2 (bool)：是否在二级路径搜索上并行化。
            singleExecution (bool)：是否执行单次搜索运行，可能用于调试或特定的路径查找模式。
            withCardinalConflicts (bool)：是否考虑关键冲突（Cardinal Conflicts），用于识别需优先处理的冲突。
            withBypassing (bool)：是否启用路径绕行，允许绕过某些障碍或拥堵。
            withMatchingHeuristic (bool)：是否使用匹配启发式函数，增强路径优化。
            storeConflicts (bool)：是否存储冲突信息，可能用于记录和处理未来的冲突。
            withDisjointSplitting (bool)：是否使用独立分裂方法，用于处理冲突时将路径拆分成独立部分。
            withFocalSearch (bool)：是否启用焦点搜索，通常用于多代理搜索优化。
            genSuboptFromOpt (bool)：是否从最优解生成次优解，可能用于生成备用路径或实现子路径。
            saveAggregatedResults (bool)：是否保存聚合结果，例如多个搜索运行的总结。
            useCatAtRoot (bool)：是否在根节点使用冲突避免表。
            restartFrequency (int)：搜索的重启频率，表示每隔多少步重新启动搜索。
            lowLevelRestartFrequency (int)：低级搜索的重启频率，适用于低级搜索算法。
            withReplanning (bool)：是否启用重新规划，允许在发现冲突时重新生成路径。
            cutIrrelevantConflicts (bool)：是否剪裁掉不相关的冲突信息，以减少无效计算。
            focalW (double)：焦点搜索权重，用于调整搜索时焦点节点的优先级。
            agentsStep (int)：代理步长，每次增加的代理数量，用于增量式的路径规划。
            firstTask (int)：起始任务编号，用于任务调
        或分配。
         */
        public double[]? SearchParams;
        public string[]? LogParams;
        public uint N;
        public int searchType = Constants.CN_ST_PR;
        public int lowLevel;
        public int minAgents = 1;
        public int maxAgents = -1;
        public int maxTime = 1000;
        public string? agentsFile;
        public int tasksCount = 1;
        public bool withCAT = false;
        public bool withPerfectHeuristic = false;
        public int ppOrder = 0;
        public bool parallelizePaths1 = false;
        public bool parallelizePaths2 = false;
        public bool singleExecution = false;
        public bool withCardinalConflicts = false;
        public bool withBypassing = false;
        public bool withMatchingHeuristic = false;
        public bool storeConflicts = false;
        public bool withDisjointSplitting = false;
        public bool withFocalSearch = false;
        public bool genSuboptFromOpt = false;
        public bool saveAggregatedResults = true;
        public bool useCatAtRoot = true;
        public int restartFrequency = 1000;
        public int lowLevelRestartFrequency = 10000000;
        public bool withReplanning = false;
        public bool cutIrrelevantConflicts = false;
        public double focalW = 1.0;
        public int agentsStep = 1;
        public int firstTask = 1;

        public Config()
        {
        }

        public bool GetConfig(string fileName)
        {
            string value = string.Empty;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                Console.WriteLine("Error opening XML file!");
                return false;
            }

            XmlNode? root = doc.SelectSingleNode(Constants.CNS_TAG_ROOT);
            if (root == null)
            {
                Console.WriteLine("Error! No 'Constants.CNS_TAG_ROOT' element found in XML file!");
                return false;
            }


            XmlNode options = GetChild(root, Constants.CNS_TAG_OPT, true);
            if (options == null) return false;

            foreach (XmlNode child in options.ChildNodes)
            {
                //Console.WriteLine(child.Name);
            }

            LogParams = new string[3];
            LogParams[Constants.CN_LP_PATH] = "";
            LogParams[Constants.CN_LP_NAME] = "";


            XmlNode? element = options.SelectSingleNode(Constants.CNS_TAG_LOGPATH);
            if (element == null || element.InnerText == "")
            {
                //Console.WriteLine("Warning! Value of " + Constants.CNS_TAG_LOGPATH + "tag was defined to 'current directory'.");
            }
            else
            {
                LogParams[Constants.CN_LP_PATH] = element.InnerText;
            }

            element = options.SelectSingleNode(Constants.CNS_TAG_LOGFN);
            if (element == null || element.InnerText == "")
            {
                //Console.WriteLine("Warning! Value of " + Constants.CNS_TAG_LOGFN + " tag was defined to default.");
            }
            else
            {
                LogParams[Constants.CN_LP_NAME] = element.InnerText;
            }

            bool success = true;
            success &= GetText(options, Constants.CNS_TAG_AGENTS_FILE, ref agentsFile);


            XmlNode range = GetChild(options, Constants.CNS_TAG_AGENTS_RANGE, false);
            if (range != null)
            {
                GetIntFromAttribute(range, Constants.CNS_TAG_MIN, "int", ref minAgents);
                GetIntFromAttribute(range, Constants.CNS_TAG_MAX, "int", ref maxAgents);
            }

            if (minAgents < 1 || (maxAgents != -1 && maxAgents < minAgents))
            {
                Console.WriteLine("Error! 'Constants.CNS_TAG_MIN' or 'Constants.CNS_TAG_MAX' value is incorrect.");
                return false;
            }

            GetIntFromText(options, Constants.CNS_TAG_TASKS_COUNT, "int", ref tasksCount);
            GetIntFromText(options, Constants.CNS_TAG_MAXTIME, "int", ref maxTime);
            GetBoolFromText(options, Constants.CNS_TAG_SINGLE_EX, "bool", ref singleExecution);
            GetBoolFromText(options, Constants.CNS_TAG_AR, "bool", ref saveAggregatedResults);

            XmlNode algorithm = GetChild(root, Constants.CNS_TAG_ALG, true);
            if (algorithm == null) return false;

            string lowLevelSearch = string.Empty;
            lowLevel = Constants.CN_SP_ST_ASTAR;

            GetIntFromText(algorithm, Constants.CNS_TAG_PP_ORDER, "int", ref ppOrder);
            GetBoolFromText(algorithm, Constants.CNS_TAG_PAR_PATHS_1, "bool", ref parallelizePaths1);
            GetBoolFromText(algorithm, Constants.CNS_TAG_PAR_PATHS_2, "bool", ref parallelizePaths2);
            GetBoolFromText(algorithm, Constants.CNS_TAG_BYPASSING, "bool", ref withBypassing);
            GetDoubleFromText(algorithm, Constants.CNS_TAG_FOCAL_W, "double", ref focalW);

            parallelizePaths1 = parallelizePaths1 || parallelizePaths2;
            storeConflicts = withFocalSearch || withBypassing || withMatchingHeuristic || withDisjointSplitting;
            withCardinalConflicts = withCardinalConflicts || withMatchingHeuristic || withDisjointSplitting;
            return true;

        }

    

    
    /*
    private bool GetValueFromText(XmlNode elem, string name, string typeName, ref field)
    {
        XmlNode child = elem.SelectSingleNode(name);
        if (child == null)
        {
            Console.WriteLine("Warning! No '{0}' tag found in XML file! Using default value.", name);
            return false;
        }

        if (typeName == "int")
        {
            field = int.Parse(child.InnerText);
        }
        else if (typeName == "bool")
        {
            field = bool.Parse(child.InnerText);
        }
        else if (typeName == "double")
        {
            field = double.Parse(child.InnerText);
        }

        return true;
    }
  */

        private bool GetBoolFromText(XmlNode elem, string name, string typeName, ref bool field)
        {
            XmlNode? child = elem.SelectSingleNode(name);
            if (child == null)
            {
                Console.WriteLine("Warning! No '{0}' tag found in XML file! Using default value.", name);
                return false;
            }
            else if (typeName == "bool")
            {
                field = bool.Parse(child.InnerText);
            }
            return true;
        }

        private bool GetIntFromText(XmlNode elem, string name, string typeName, ref int field)
        {
            XmlNode? child = elem.SelectSingleNode(name);
            if (child == null)
            {
                Console.WriteLine("Warning! No '{0}' tag found in XML file! Using default value.", name);
                return false;
            }

            if (typeName == "int")
            {
                field = int.Parse(child.InnerText);
            }
            return true;
        }

        private bool GetDoubleFromText(XmlNode elem, string name, string typeName, ref double field)
        {
            XmlNode? child = elem.SelectSingleNode(name);
            if (child == null)
            {
                Console.WriteLine("Warning! No '{0}' tag found in XML file! Using default value.", name);
                return false;
            }
            else if (typeName == "double")
            {
                field = double.Parse(child.InnerText);
            }

            return true;
        }

        private bool GetIntFromAttribute(XmlNode elem, string attrName, string typeName, ref int field)
    {
        XmlAttribute? attribute = elem.Attributes[attrName];
        if (attribute == null)
        {
            Console.WriteLine("Warning! Couldn't get value from '{0}' attribute! Using default value.", attrName);
            return false;
        }

        if (typeName == "int")
        {
            field = int.Parse(attribute.Value);
        }
        return true;
    }

        private bool GetBoolFromAttribute(XmlNode elem, string attrName, string typeName, ref bool field)
        {
            XmlAttribute attribute = elem.Attributes[attrName];
            if (attribute == null)
            {
                Console.WriteLine("Warning! Couldn't get value from '{0}' attribute! Using default value.", attrName);
                return false;
            }
        
            else if (typeName == "bool")
            {
                field = bool.Parse(attribute.Value);
            }
      
            return true;
        }

        private bool GetDoubleFromAttribute(XmlNode elem, string attrName, string typeName, ref double field)
        {
            XmlAttribute attribute = elem.Attributes[attrName];
            if (attribute == null)
            {
                Console.WriteLine("Warning! Couldn't get value from '{0}' attribute! Using default value.", attrName);
                return false;
            }

            else if (typeName == "double")
            {
                field = double.Parse(attribute.Value);
            }
            
            return true;
        }

        private XmlNode GetChild(XmlNode elem, string name, bool printError)
    {
        XmlNode child = elem.SelectSingleNode(name);
        if (child == null && printError)
        {
            Console.WriteLine("Error! No '{0}' tag found in XML file!", name);
        }
        return child;
    }
        
    private bool GetText(XmlNode elem, string name, ref string field)
    {
        XmlNode child = elem.SelectSingleNode(name);
        if (child == null)
        {
            Console.WriteLine("Warning! No '{0}' tag found in XML file!", name);
            return false;
        }
        field = child.InnerText;
        return true;
    }
        
    }

}



