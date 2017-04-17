using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TShockAPI;

namespace Utils
{
    public static class Extensions
    {

        public static void FillToCapacity<T>(this List<T> l, T obj)
        {
            while (l.Count < l.Capacity)
            {
                l.Add(obj);
            }
        }

        public static int AddFirstDefault<T>(this List<T> l, T obj)
        {
            for (var i = 0; i < l.Count; i++)
            {
                if ( EqualityComparer<T>.Default.Equals(l[i], default(T)) )
                {
                    l[i] = obj;
                    return i;
                }
            }
            l.Add(obj);
            return l.Count - 1;
        }

        public static void RemoveNoShift<T>(this List<T> l, int i)
        {
            l[i] = default(T);
            if (i == l.Count - 1)
                l.RemoveAt(i);
        }

        private static Regex tsform = new Regex(@"((\d+)(\D+))+",RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex tsform2 = new Regex(@"(\d+)(\D+)", RegexOptions.Compiled | RegexOptions.Singleline);
        public static TimeSpan ToTimeSpan(this string str)
        {
            Match m = tsform.Match(str);

            if (!m.Success)
                return TimeSpan.Zero;

            var caps = m.Groups[1].Captures;

            TimeSpan ts = TimeSpan.Zero;

            foreach (Capture cap in caps)
            {
                Match a = tsform2.Match(cap.Value);
                if (!a.Success)
                    continue;

                double val = double.Parse(a.Groups[1].Value);
                string unit = a.Groups[2].Value.ToLower();

                switch (unit)
                {
                    case "t":
                        ts += TimeSpan.FromTicks(long.Parse(a.Groups[1].Value));
                        break;
                    case "ms":
                        ts += TimeSpan.FromMilliseconds(val);
                        break;
                    case "s":
                        ts += TimeSpan.FromSeconds(val);
                        break;
                    case "m":
                        ts += TimeSpan.FromMinutes(val);
                        break;
                    case "h":
                        ts += TimeSpan.FromHours(val);
                        break;
                    case "d":
                        ts += TimeSpan.FromDays(val);
                        break;
                }

            }
            return ts;
        }

        public static string ToCustomFormat(this TimeSpan ts)
        {
            string outp = "";

            int days = ts.Days;
            int hrs = ts.Hours;
            int mins = ts.Minutes;
            int secs = ts.Seconds;
            int mils = ts.Milliseconds;
            long ticks = ts.Ticks 
                - (TimeSpan.TicksPerDay*days)
                - (TimeSpan.TicksPerHour*hrs)
                - (TimeSpan.TicksPerMinute*mins)
                - (TimeSpan.TicksPerSecond*secs)
                - (TimeSpan.TicksPerMillisecond*mils); // Ticks component

            if (days != 0) outp += days + "d";
            if (hrs != 0) outp += hrs + "h";
            if (mins != 0) outp += mins + "m";
            if (secs != 0) outp += secs + "s";
            if (mils != 0) outp += mils + "ms";
            if (ticks != 0) outp += ticks + "t";

            return outp;
        }

        public static string JoinObject<T>(this string sep, T[] os)
        {
            string s = "";
            foreach (T o in os)
            {
                s += o.ToString() + sep;
            }
            return s.Substring(0, s.Length - sep.Length);
        }

        public static string ToReadable(this TimeSpan ts)
        {
            // formats and its cutoffs based on totalseconds
            var cutoff = new SortedList<long, string> {
                {1, "{4:L}" },
                {60, "{3:S}, {4:L}" },
                {60*60, "{2:M}, {3:S}"},
                {24*60*60, "{1:H}, {2:M}"},
                {Int64.MaxValue , "{0:D}, {1:H}"}
            };

            // find nearest best match
            var find = cutoff.Keys.ToList()
              .BinarySearch((long)ts.TotalSeconds);
            // negative values indicate a nearest match
            var near = find < 0 ? Math.Abs(find) - 1 : find;
            // use custom formatter to get the string
            return string.Format(
                new Util.HMSFormatter(),
                cutoff[cutoff.Keys[near]],
                ts.Days,
                ts.Hours,
                ts.Minutes,
                ts.Seconds,
                ts.Milliseconds);
        }
    }
}
