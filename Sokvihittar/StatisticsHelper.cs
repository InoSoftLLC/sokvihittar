using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sokvihittar.Controllers;

namespace Sokvihittar
{
    public static class StatisticsHelper
    {
        public static void WriteStatistics(SearchRequestStatiscs statistics, string logName)
        {
            var logDirectory = Path.GetDirectoryName(logName);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
                List<SearchRequestStatiscs> statisticList;
                if (File.Exists(logName))
                {
                    statisticList = JsonHelper.Deserialize<List<SearchRequestStatiscs>>(logName);
                    if (statisticList.Count > 100)
                        statisticList = statisticList.Skip(statisticList.Count - 100).ToList();
                    statisticList.Add(statistics);
                }
                else
                {
                    statisticList = new List<SearchRequestStatiscs> { statistics };
                }
                File.WriteAllText(logName, JsonHelper.Serialize(statisticList));
        }

        public static string ReadStatistics(string logName)
        {
            return File.Exists(logName) ? File.ReadAllText(logName) : "no results";
        }
    }
}