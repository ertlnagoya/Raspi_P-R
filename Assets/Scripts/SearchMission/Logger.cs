using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using Mission;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public interface ILogger
    {
        bool GetLog(string fileName, string[] logParams);
        
        void SaveLog();
        void WriteToLogMap(Map map, List<Node> path);
        void WriteToLogPath(List<Node> path);
        void WriteToLogHPPath(List<Node> path);
        void WriteToLogAgentsPaths(AgentSet agentSet,
                                   List<List<Node>> agentsPaths,
                                   string agentsFile, double time,
                                   double makespan, double flowtime,
                                   int hlExpansions, int hlNodes,
                                   int hlExpansionsStart, int hlNodesStart,
                                   double llExpansions, double llNodes);
        void WriteToLogNotFound();
        void WriteToLogAggregatedResults(Dictionary<int, int> successCount,
                                         TestingResults res,
                                         string agentsFile = "");
        void WriteToLogSummary(uint numberOfSteps, uint nodesCreated, float length, double time, double cellSize);
        
    }
    public class XmlLogger : ILogger
    {
        public string logLevel;
        public XmlDocument doc = new XmlDocument();
        public string logFileName;

        public XmlLogger(string logLevel)
        {
            this.logLevel = logLevel;
        }

        public bool GetLog(string fileName, string[] logParams)
        {
            if (logLevel == "NOPE") return true;

            try
            {
                doc.Load(fileName);
            }
            catch
            {
                Console.WriteLine("Error opening input XML file");
                return false;
            }

            if (string.IsNullOrEmpty(logParams[0]) && string.IsNullOrEmpty(logParams[1]))
            {
                logFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_log{Path.GetExtension(fileName)}";
            }
            else if (string.IsNullOrEmpty(logParams[0]))
            {
                logFileName = $"{fileName.Substring(0, fileName.LastIndexOf("\\") + 1)}{logParams[1]}";
            }
            else if (string.IsNullOrEmpty(logParams[1]))
            {
                logFileName = $"{logParams[0]}\\{Path.GetFileNameWithoutExtension(fileName)}_log{Path.GetExtension(fileName)}";
            }
            else
            { 
                logFileName = $"{logParams[0]}\\{logParams[1]}";
            }

            XmlElement root = doc.DocumentElement;
            if (root == null)
            {
                Console.WriteLine($"No '{root.Name}' element found in XML file");
                Console.WriteLine("Cannot create log");
                return false;
            }

            XmlElement logElement = doc.CreateElement("log");
            root.AppendChild(logElement);
            if (logLevel != "NOPE")
            {
                XmlElement mapFnElement = doc.CreateElement("MapFileName");
                mapFnElement.InnerText = fileName;
                logElement.AppendChild(mapFnElement);
            }

            if (logLevel == "FULL" || logLevel == "MEDIUM")
            {
                XmlElement lowLevelElement = doc.CreateElement("LowLevel");
                logElement.AppendChild(lowLevelElement);
            }
            Console.WriteLine($"Log file will be saved at: {logFileName}");
            return true;
        }

        public void SaveLog()
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD)
            {
                return;
            }
            string relativePath = Path.Combine("python", logFileName);
            string fullPath = Path.GetFullPath(relativePath);
            doc.Save(fullPath);
        }

        public void WriteToLogMap(Map map, List<Node> path)
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD || logLevel == Constants.CN_LP_LEVEL_TINY_WORD)
            {
                return;
            }

            XmlNode mapTag = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}/{Constants.CNS_TAG_PATH}");

            if (mapTag == null)
            {
                return; // 确保 mapTag 存在
            }

            int iterate = 0;
            string str = string.Empty;

            for (int i = 0; i < map.getMapHeight(); ++i)
            {
                XmlElement element = doc.CreateElement(Constants.CNS_TAG_ROW);
                element.SetAttribute(Constants.CNS_TAG_ATTR_NUM, iterate.ToString());

                for (int j = 0; j < map.getMapWidth(); ++j)
                {
                    bool inPath = path.Any(node => node.i == i && node.j == j);

                    if (!inPath)
                    {
                        str += map.getValue(i, j).ToString();
                    }
                    else
                    {
                        str += Constants.CNS_OTHER_PATHSELECTION;
                    }

                    str += Constants.CNS_OTHER_MATRIXSEPARATOR;
                }

                element.InnerText = str;
                mapTag.AppendChild(element);
                str = string.Empty; // 清空字符串
                iterate++;
            }
        }

        public void WriteToLogAgentsPaths(
                                            AgentSet agentSet,
                                            List<List<Node>> agentsPaths,
                                            string agentsFile,
                                            double time,
                                            double makespan,
                                            double flowtime,
                                            int HLExpansionsStart,
                                            int HLNodesStart,
                                            int HLExpansions,
                                            int HLNodes,
                                            double LLExpansions,
                                            double LLNodes)
        {
            XmlNode log = doc.SelectSingleNode($"/{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}");

            if (log == null)
            {
                throw new InvalidOperationException("Log element not found in the XML document.");
            }

            XmlElement taskFileElement = doc.CreateElement(Constants.CNS_TAG_TASKFN);
            taskFileElement.InnerText = agentsFile;
            log.AppendChild(taskFileElement);

            XmlElement summaryElement = doc.CreateElement(Constants.CNS_TAG_SUMMARY);
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_COUNT, agentsPaths.Count.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_TIME, time.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_MAKESPAN, makespan.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_FLOWTIME, flowtime.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_HLE, HLExpansions.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_HLN, HLNodes.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_HLES, HLExpansionsStart.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_HLNS, HLNodesStart.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_LLE, LLExpansions.ToString());
            summaryElement.SetAttribute(Constants.CNS_TAG_ATTR_LLN, LLNodes.ToString());
            log.AppendChild(summaryElement);

            for (int i = 0; i < agentsPaths.Count; ++i)
            {
                Agent agent = agentSet.getAgent(i);
                XmlElement agentElement = doc.CreateElement(Constants.CNS_TAG_AGENT);
                agentElement.SetAttribute(Constants.CNS_TAG_ATTR_ID, i.ToString());
                agentElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTX, agent.getStart_j().ToString());
                agentElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTY, agent.getStart_i().ToString());
                agentElement.SetAttribute(Constants.CNS_TAG_ATTR_GOALX, agent.getGoal_j().ToString());
                agentElement.SetAttribute(Constants.CNS_TAG_ATTR_GOALY, agent.getGoal_i().ToString());

                XmlElement pathElement = doc.CreateElement(Constants.CNS_TAG_PATH);
                pathElement.SetAttribute(Constants.CNS_TAG_ATTR_PATH_FOUND, "true");

                for (int j = 0; j < agentsPaths[i].Count - 1; ++j)
                {
                    XmlElement sectionElement = doc.CreateElement(Constants.CNS_TAG_SECTION);
                    Node curNode = agentsPaths[i][j];
                    Node nextNode = agentsPaths[i][j + 1];
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_ID, j.ToString());
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTX, curNode.j.ToString());
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_STARTY, curNode.i.ToString());
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_GOALX, nextNode.j.ToString());
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_GOALY, nextNode.i.ToString());
                    sectionElement.SetAttribute(Constants.CNS_TAG_ATTR_DUR, "1");
                    pathElement.AppendChild(sectionElement);
                }
                agentElement.AppendChild(pathElement);
                log.AppendChild(agentElement);
            }
        }


        public void WriteToLogAggregatedResults(
                                                Dictionary<int, int> successCount,
                                                TestingResults res,
                                                string agentsFile)
        {
            XmlNode log = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}");

            if (log == null)
            {
                throw new InvalidOperationException("Log element not found in the XML document.");
            }

            XmlElement results = doc.CreateElement(Constants.CNS_TAG_RESULTS);

            if (!string.IsNullOrEmpty(agentsFile))
            {
                results.SetAttribute(Constants.CNS_TAG_AGENTS_FILE, agentsFile);
            }

            List<string> keys = res.getKeys();
            foreach (var pair in successCount)
            {
                XmlElement result = doc.CreateElement(Constants.CNS_TAG_RESULT);
                result.SetAttribute(Constants.CNS_TAG_ATTR_COUNT, pair.Key.ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_SC, pair.Value.ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_TN, res.finalTotalNodes[pair.Key].ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_FHLN, res.finalHLNodes[pair.Key].ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_FHLE, res.finalHLExpansions[pair.Key].ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_FHLNS, res.finalHLNodesStart[pair.Key].ToString());
                result.SetAttribute(Constants.CNS_TAG_ATTR_FHLES, res.finalHLExpansionsStart[pair.Key].ToString());

                for (int i = 0; i < res.data[Constants.CNS_TAG_FOCAL_W][pair.Key].Count; ++i)
                {
                    XmlElement iteration = doc.CreateElement(Constants.CNS_TAG_ITERATION);
                    foreach (var key in keys)
                    {
                        iteration.SetAttribute(key, res.data[key][pair.Key][i].ToString());
                    }
                    result.AppendChild(iteration);
                }
                results.AppendChild(result);
            }

            log.AppendChild(results);
        }

        public void WriteToLogHPPath(List<Node> hppath)
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD ||
                logLevel == Constants.CN_LP_LEVEL_TINY_WORD ||
                hppath == null || hppath.Count == 0)
            {
                return;
            }

            int partnumber = 0;

            XmlNode hplevel = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}/{Constants.CNS_TAG_HPLEVEL}");
            if (hplevel == null)
            {
                throw new InvalidOperationException("HPLevel element not found in the XML document.");
            }

            for (int i = 0; i < hppath.Count - 1; ++i)
            {
                XmlElement part = doc.CreateElement(Constants.CNS_TAG_SECTION);
                part.SetAttribute(Constants.CNS_TAG_ATTR_NUM, partnumber.ToString());
                part.SetAttribute(Constants.CNS_TAG_ATTR_STX, hppath[i].j.ToString());
                part.SetAttribute(Constants.CNS_TAG_ATTR_STY, hppath[i].i.ToString());
                part.SetAttribute(Constants.CNS_TAG_ATTR_FINX, hppath[i + 1].j.ToString());
                part.SetAttribute(Constants.CNS_TAG_ATTR_FINY, hppath[i + 1].i.ToString());
                part.SetAttribute(Constants.CNS_TAG_ATTR_LENGTH, (hppath[i + 1].g - hppath[i].g).ToString());
                hplevel.AppendChild(part);

                ++partnumber;
            }
        }

        public void WriteToLogSummary(uint numberofsteps, uint nodescreated, float length, double time, double cellSize)
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD)
            {
                return;
            }

            XmlNode summary = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}/{Constants.CNS_TAG_SUM}");
            if (summary == null)
            {
                throw new InvalidOperationException("Summary element not found in the XML document.");
            }

            XmlElement element = (XmlElement)summary;
            element.SetAttribute(Constants.CNS_TAG_ATTR_NUMOFSTEPS, numberofsteps.ToString());
            element.SetAttribute(Constants.CNS_TAG_ATTR_NODESCREATED, nodescreated.ToString());
            element.SetAttribute(Constants.CNS_TAG_ATTR_LENGTH, length.ToString());
            element.SetAttribute(Constants.CNS_TAG_ATTR_LENGTH_SCALED, (length * cellSize).ToString());
            element.SetAttribute(Constants.CNS_TAG_ATTR_TIME, time.ToString());
        }

        public void WriteToLogNotFound()
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD)
            {
                return;
            }

            XmlNode node = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}/{Constants.CNS_TAG_PATH}");
            if (node == null)
            {
                throw new InvalidOperationException("Path element not found in the XML document.");
            }

            XmlText notFoundText = doc.CreateTextNode("Path NOT found!");
            node.AppendChild(notFoundText);
        }

        public void WriteToLogPath(List<Node> path)
        {
            if (logLevel == Constants.CN_LP_LEVEL_NOPE_WORD ||
                logLevel == Constants.CN_LP_LEVEL_TINY_WORD ||
                path == null || path.Count == 0)
            {
                return;
            }

            int iterate = 0;

            XmlNode lplevel = doc.SelectSingleNode($"{Constants.CNS_TAG_ROOT}/{Constants.CNS_TAG_LOG}/{Constants.CNS_TAG_LPLEVEL}");
            if (lplevel == null)
            {
                throw new InvalidOperationException("LPLevel element not found in the XML document.");
            }

            foreach (var node in path)
            {
                XmlElement element = doc.CreateElement(Constants.CNS_TAG_POINT);
                element.SetAttribute(Constants.CNS_TAG_ATTR_X, node.j.ToString());
                element.SetAttribute(Constants.CNS_TAG_ATTR_Y, node.i.ToString());
                element.SetAttribute(Constants.CNS_TAG_ATTR_NUM, iterate.ToString());
                lplevel.AppendChild(element);
                iterate++;
            }
        }


    }




}

