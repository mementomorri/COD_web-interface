using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace COD.Models
{
    public struct sandJson
    {

        public string id { get; set; }

        public string feature { get; set; }

        public string atribute { get; set; }

        public object value { get; set; }

    }

    public struct chart
    {

        public string label { get; set; }

        public List<point> data { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

    }

    public struct ChartPack 
    {
        public string startDate { get; set; }
        public string endDate { get; set; }

        public ChartData data { get; set; } 
    }

    public struct ChartData
    {

        public List<FullPoint> day { get; set; }

        public List<FullPoint> week { get; set; }

        public List<FullPoint> month { get; set; }

    }


    public struct point
    {
        public object x { get; set; }

        public object y { get; set; }

    }

    public struct FullPoint
    {
        public string label { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public List<point> data { get; set; }

    }

    public struct iChart {
        public string Tag_name { get; set; }
        public float Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string quality { get; set; }
        public int type { get; set; }

    }

    // Define an enumeration to represent student grades.
    public enum WebTypeParam { Temperatura = 0, Pressure = 1};

    public enum ChartSize { day = 0, week = 1, month  = 2};

    public struct HistData
    {

        public string id { get; set; }
        public string tag { get; set; }

        public object value { get; set; }

        public DateTime timestamp { get; set; }

        public string quality { get; set; }

    }

    public struct param
    {

        public string par { get; set; }
        public string val { get; set; }


    }

}
