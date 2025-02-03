using System;

namespace LineTrace
{
    [Serializable]
    public struct Demand
    {
        public int Id;  //Demand ID
        public int Src; //Source node
        public int Dst; //Destination node
        public int Goal; //Goal node
        public bool Re; //Re-request flag
    }

    [Serializable]
    public struct Demand_CSoS
    {
        public int Id;  //Demand ID
        public int Src; //Source node
        public int Dst; //Destination node
        public int Next; //Next destination node
        public int Goal; //Goal node
        public bool Re; //Re-request flag
    }

    [Serializable]
    public struct Demand_Resource
    {
        public int Id;  //Demand ID
    }
    
    [Serializable]
    public class Demand_Reply
    {
        public bool ok;
        public bool[] edgeFlags;
        public bool[] crossFlags;
        public int edgeFlagRow;
    }
}