#define RANDOM_GENERATION // 定义条件变量

using System;
using Mission;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;



namespace Mission
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Error! Pathfinding task file is not specified!");
                return;
            }
            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);
            string fileName = Path.Combine("Examples", args[0]);
            Console.WriteLine(fileName);
            MissionSearch mission = new MissionSearch(fileName);



            Console.WriteLine("Parsing the map from XML:");

            if (!mission.GetMap())
            {
                Console.WriteLine("Incorrect map! Program halted!");
            }
            else
            {
                mission.ShowMap();
                Console.WriteLine("Map OK!");
                Console.WriteLine("Parsing configurations (algorithm, log) from XML:");

                if (!mission.GetConfig())
                    Console.WriteLine("Incorrect configurations! Program halted!");
                else
                {
                    Console.WriteLine("Configurations OK!");
                    Console.WriteLine("Creating log channel:");

                    if (!mission.CreateLog())
                        Console.WriteLine("Log channel has not been created! Program halted!");
                    else
                    {
                        Console.WriteLine("Log OK!");
                        Console.WriteLine("Start searching");
                    }
                    
                   mission.CreateAlgorithm();
                    
                   int tasksCount = mission.getSingleExecution() ? 1 : mission.getTasksCount();
                    Console.WriteLine("Count of task is" + tasksCount);
                   //Debugger.Break();
                   for (int i = 0; i < tasksCount; i++)
                   {
#if RANDOM_GENERATION
                       string agentsFile = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                       if (!mission.creatTask())
#else
                       string agentsFile = mission.getAgentsFile() + "-" + (i + mission.getFirstTask()+1).ToString() + ".xml";
                       if (!mission.GetAgents(agentsFile))
#endif
                            Console.WriteLine("Agent set has not been created! Program halted!");
                       else if (mission.checkAgentsCorrectness(agentsFile))
                       {
                           Console.WriteLine("Starting search for agents file " + agentsFile);
                           mission.startSearch(agentsFile);
                       }
                   }

                    //Console.WriteLine("mission.getSingleExecution() is " + mission.getSingleExecution());
                    if (!mission.getSingleExecution())
                   {
                        /*
                       if (mission.getSaveAggregatedResults())
                       {
                           mission.gaveAggregatedResultsToLog();
                       }
                       
                       else
                       {
                           mission.saveSeparateResultsToLog();
                       }
                       */
                    }

                    Console.WriteLine("All searches are finished!");
               
               
                }
            }
        
        }
    }

}

