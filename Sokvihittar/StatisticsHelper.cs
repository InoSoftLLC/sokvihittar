using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Sokvihittar.Controllers
{
    public static class StatisticsHelper
    {
        public static void WriteStatistics(SearchRequestStatiscs statistics, string logName)
        {
            lock (logName)
            {
                var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), logName);
                List<SearchRequestStatiscs> statisticList;
                if (File.Exists(fileName))
                {
                    statisticList = JsonHelper.Deserialize<List<SearchRequestStatiscs>>(fileName);
                    if (statisticList.Count > 100)
                        statisticList = statisticList.Skip(statisticList.Count - 100).ToList();
                    statisticList.Add(statistics);
                }
                else
                {
                    statisticList = new List<SearchRequestStatiscs> { statistics };
                }
                File.WriteAllText(fileName, JsonHelper.Serialize(statisticList));
            }
        }

        public static string ReadStatistics(string logName)
        {
            var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), logName);
            List<SearchRequestStatiscs> statisticList;
            if (File.Exists(fileName))
            {
                return File.ReadAllText(fileName);
            }
            return "no results";
        }
    }
}