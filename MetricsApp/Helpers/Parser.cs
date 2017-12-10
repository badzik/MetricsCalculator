using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MetricsApp.Helpers
{
    public class Parser
    {
        public static TimeSpan EffortParser(string effort)
        {
            TimeSpan effortTime=new TimeSpan();
            if (effort.Contains("h"))
            {
                if (effort.Contains("min"))
                {
                    effort = effort.Replace("min", "");
                    effort = effort.Replace("h", ":");
                    effortTime = TimeSpan.ParseExact(effort,"H:MM",null);
                }
                else
                {
                    effort = effort.Replace("h", "");
                    effortTime = TimeSpan.FromHours(Double.Parse(effort));
                }
            }
            else
            {
                effort = effort.Replace("min", "");
                effortTime = TimeSpan.FromMinutes(Double.Parse(effort));
            }


            return effortTime;
        }
    }

}