using System;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Mission
{
    public static class Constants
    {
        // Mathematical Constants
        public const double CN_PI = 3.14159265359;
        public const double CN_SQRT_TWO = 1.41421356237;
        public const int CN_INFINITY = 1000000000;
        public const double CN_RESOLUTION = 1e-6;
        public const double CN_EPSILON = 1e-9;

        // XML Tags
        public const string CNS_TAG_ROOT = "root";

        public const string CNS_TAG_MAP = "map";
        public const string CNS_TAG_CROSS = "cross";
        public const string CNS_TAG_CELLSIZE = "cellsize";
        public const string CNS_TAG_WIDTH = "width";
        public const string CNS_TAG_HEIGHT = "height";
        public const string CNS_TAG_STX = "startx";
        public const string CNS_TAG_STY = "starty";
        public const string CNS_TAG_FINX = "finishx";
        public const string CNS_TAG_FINY = "finishy";
        public const string CNS_TAG_GRID = "grid";
        public const string CNS_TAG_ROW = "row";

        public const string CNS_TAG_ALG = "algorithm";
        public const string CNS_TAG_ST = "searchtype";
        public const string CNS_TAG_HW = "hweight";
        public const string CNS_TAG_MT = "metrictype";
        public const string CNS_TAG_BT = "breakingties";
        public const string CNS_TAG_AS = "allowsqueeze";
        public const string CNS_TAG_AD = "allowdiagonal";
        public const string CNS_TAG_CC = "cutcorners";

        public const string CNS_TAG_OPT = "options";
        public const string CNS_TAG_LOGLVL = "loglevel";
        public const string CNS_TAG_LOGPATH = "logpath";
        public const string CNS_TAG_LOGFN = "logfilename";
        public const string CNS_TAG_PLANNER = "planner";
        public const string CNS_TAG_MAXTIME = "maxtime";
        public const string CNS_TAG_AGENTS_FILE = "agents_file";
        public const string CNS_TAG_WITH_CAT = "with_cat";
        public const string CNS_TAG_WITH_PH = "with_perfect_h";
        public const string CNS_TAG_CARD_CONF = "with_card_conf";
        public const string CNS_TAG_BYPASSING = "with_bypassing";
        public const string CNS_TAG_WITH_MH = "with_matching_h";
        public const string CNS_TAG_WITH_DS = "with_disjoint_splitting";
        public const string CNS_TAG_FOCAL_W = "focal_w";
        public const string CNS_TAG_WEIGHT = "weight";
        public const string CNS_TAG_SFO = "gen_subopt_from_opt";
        public const string CNS_TAG_RC = "use_cat_at_root";
        public const string CNS_TAG_RFS = "restart_frequency";
        public const string CNS_TAG_LL_RFS = "low_level_restart_frequency";
        public const string CNS_TAG_CIC = "cut_irrelevant_conflicts";
        public const string CNS_TAG_AR = "aggregated_results";
        public const string CNS_TAG_LOW_LEVEL = "low_level";
        public const string CNS_TAG_PP_ORDER = "pp_order";
        public const string CNS_TAG_PAR_PATHS_1 = "parallelize_paths_1";
        public const string CNS_TAG_PAR_PATHS_2 = "parallelize_paths_2";
        public const string CNS_TAG_SINGLE_EX = "single_execution";
        public const string CNS_TAG_TASKS_COUNT = "tasks_count";
        public const string CNS_TAG_AGENTS_RANGE = "agents_range";
        public const string CNS_TAG_MIN = "min";
        public const string CNS_TAG_MAX = "max";
        public const string CNS_TAG_AGENTS_STEP = "agents_step";
        public const string CNS_TAG_FIRST_TASK = "first_task";

        public const string CNS_TAG_LOG = "log";
        public const string CNS_TAG_MAPFN = "mapfilename";
        public const string CNS_TAG_TASKFN = "taskfilename";
        public const string CNS_TAG_SUMMARY = "summary";
        public const string CNS_TAG_SUM = "summary";
        public const string CNS_TAG_PATH = "path";
        public const string CNS_TAG_RESULTS = "results";
        public const string CNS_TAG_RESULT = "result";
        public const string CNS_TAG_ITERATION = "iteration";
        public const string CNS_TAG_AGENT = "agent";
        public const string CNS_TAG_LPLEVEL = "lplevel";
        public const string CNS_TAG_HPLEVEL = "hplevel";
        public const string CNS_TAG_SECTION = "section";
        public const string CNS_TAG_LOWLEVEL = "lowlevel";
        public const string CNS_TAG_STEP = "step";
        public const string CNS_TAG_OPEN = "open";
        public const string CNS_TAG_POINT = "node";
        public const string CNS_TAG_CLOSE = "close";

        // XML Tags' Attributes
        public const string CNS_TAG_ATTR_NUMOFSTEPS = "numberofsteps";
        public const string CNS_TAG_ATTR_NODESCREATED = "nodescreated";
        public const string CNS_TAG_ATTR_LENGTH = "length";
        public const string CNS_TAG_ATTR_LENGTH_SCALED = "length_scaled";
        public const string CNS_TAG_ATTR_TIME = "time";
        public const string CNS_TAG_ATTR_HLES = "start_HL_expansions";
        public const string CNS_TAG_ATTR_HLNS = "start_HL_nodes";
        public const string CNS_TAG_ATTR_HLE = "HL_expansions";
        public const string CNS_TAG_ATTR_HLN = "HL_nodes";
        public const string CNS_TAG_ATTR_LLE = "LL_avg_expansions";
        public const string CNS_TAG_ATTR_LLN = "LL_avg_nodes";
        public const string CNS_TAG_ATTR_TN = "total_nodes";
        public const string CNS_TAG_ATTR_FHLN = "final_HL_nodes";
        public const string CNS_TAG_ATTR_FHLNS = "final_HL_nodes_start";
        public const string CNS_TAG_ATTR_FHLE = "final_HL_expansions";
        public const string CNS_TAG_ATTR_FHLES = "final_HL_expansions_start";
        public const string CNS_TAG_ATTR_FILENAME = "filename";
        public const string CNS_TAG_ATTR_X = "x";
        public const string CNS_TAG_ATTR_Y = "y";
        public const string CNS_TAG_ATTR_NUM = "number";
        public const string CNS_TAG_ATTR_ID = "id";
        public const string CNS_TAG_ATTR_STARTX = "start.x";
        public const string CNS_TAG_ATTR_STARTY = "start.y";
        public const string CNS_TAG_ATTR_GOALX = "goal.x";
        public const string CNS_TAG_ATTR_GOALY = "goal.y";
        public const string CNS_TAG_ATTR_DUR = "duration";
        public const string CNS_TAG_ATTR_PATH_FOUND = "pathfound";
        public const string CNS_TAG_ATTR_COUNT = "agents_count";
        public const string CNS_TAG_ATTR_MAKESPAN = "makespan";
        public const string CNS_TAG_ATTR_FLOWTIME = "flowtime";
        public const string CNS_TAG_ATTR_SC = "success_count";
        public const string CNS_TAG_ATTR_F = "F";
        public const string CNS_TAG_ATTR_G = "g";
        public const string CNS_TAG_ATTR_PARX = "parent_x";
        public const string CNS_TAG_ATTR_PARY = "parent_y";
        public const string CNS_TAG_ATTR_STX = "start.x";
        public const string CNS_TAG_ATTR_STY = "start.y";
        public const string CNS_TAG_ATTR_FINX = "finish.x";
        public const string CNS_TAG_ATTR_FINY = "finish.y";

        // Search Parameters
        public const string CNS_ST_CBS = "cbs";
        public const string CNS_ST_PR = "push_and_rotate";
        public const string CNS_ST_PP = "prioritized_planning";
        public const string CNS_ST_ECBS = "ecbs";
        public const string CNS_ST_ACBS = "anytime_cbs";
        public const string CNS_ST_AECBS = "anytime_ecbs";

        public const int CN_ST_CBS = 0;
        public const int CN_ST_PR = 1;
        public const int CN_ST_PP = 2;
        public const int CN_ST_ECBS_CT = 3;
        public const int CN_ST_ACBS = 4;
        public const int CN_ST_AECBS = 5;

        public const string CNS_SP_ST_ASTAR = "astar";
        public const string CNS_SP_ST_FS = "focal_search";
        public const string CNS_SP_ST_SIPP = "sipp";
        public const string CNS_SP_ST_ZSCIPP = "zero_scipp";
        public const string CNS_SP_ST_SCIPP = "scipp";
        public const string CNS_SP_ST_FLPASTAR = "focal_lpastar";
        public const string CNS_SP_ST_RASTAR = "replanning_astar";
        public const string CNS_SP_ST_RFS = "replanning_focal_search";

        public const int CN_SP_ST_ASTAR = 0;
        public const int CN_SP_ST_FS = 2;
        public const int CN_SP_ST_SIPP = 3;
        public const int CN_SP_ST_ZSCIPP = 4;
        public const int CN_SP_ST_SCIPP = 5;
        public const int CN_SP_ST_FLPASTAR = 6;
        public const int CN_SP_ST_RASTAR = 7;
        public const int CN_SP_ST_RFS = 8;

        // Search Parameters
        public const int CN_SP_AD = 1; // AllowDiagonal
        public const int CN_SP_CC = 2; // CutCorners
        public const int CN_SP_AS = 3; // AllowSqueeze
        public const int CN_SP_MT = 4; // MetricType

        public const string CNS_SP_MT_DIAG = "diagonal";
        public const string CNS_SP_MT_MANH = "manhattan";
        public const string CNS_SP_MT_EUCL = "euclidean";
        public const string CNS_SP_MT_CHEB = "chebyshev";

        public const int CN_SP_MT_DIAG = 0;
        public const int CN_SP_MT_MANH = 1;
        public const int CN_SP_MT_EUCL = 2;
        public const int CN_SP_MT_CHEB = 3;

        public const int CN_SP_HW = 5; // HeuristicWeight
        public const int CN_SP_BT = 6; // BreakingTies

        public const string CNS_SP_BT_GMIN = "g-min";
        public const string CNS_SP_BT_GMAX = "g-max";

        public const bool CN_SP_BT_GMIN = false;
        public const bool CN_SP_BT_GMAX = true;

        // Log Configuration
        public const int CN_LP_LEVEL = 0;

        public const string CN_LP_LEVEL_NOPE_VALUE = "0";
        public const string CN_LP_LEVEL_NOPE_WORD = "none";
        public const string CN_LP_LEVEL_TINY_VALUE = "0.5";
        public const string CN_LP_LEVEL_TINY_WORD = "tiny";
        public const string CN_LP_LEVEL_SHORT_VALUE = "1";
        public const string CN_LP_LEVEL_SHORT_WORD = "short";
        public const string CN_LP_LEVEL_MEDIUM_VALUE = "1.5";
        public const string CN_LP_LEVEL_MEDIUM_WORD = "medium";
        public const string CN_LP_LEVEL_FULL_VALUE = "2";
        public const string CN_LP_LEVEL_FULL_WORD = "full";

        public const int CN_LP_PATH = 1;
        public const int CN_LP_NAME = 2;

        // Grid Cell
        public const int CN_GC_NOOBS = 0;
        public const int CN_GC_OBS = 1;

        // Other
        public const string CNS_OTHER_PATHSELECTION = "*";
        public const char CNS_OTHER_MATRIXSEPARATOR = ' ';
        public const char CNS_OTHER_POSITIONSEPARATOR = ',';
    }


}
