using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Reports
{
    public class TopicAvailability
    {
        public void ShowReport(Context context)
        {
            var counters = new Dictionary<CourseCategory, (int HunTotal, int EngTotal, int HunFree, int EngFree)>();
            foreach (var cat in context.CourseCategories)
                counters.Add(cat, (0, 0, 0, 0));
            foreach (var t in context.Topics)
            {
                foreach (var cat in t.CourseCategories)
                {
                    (int HunTotal, int EngTotal, int HunFree, int EngFree) = counters[cat];
                    if (t.IsForEnglishStudents)
                    {
                        EngTotal += t.SeatCount;
                        EngFree += t.SeatCount - (t.RegisteredStudents?.Count ?? 0);
                    }
                    else
                    {
                        HunTotal += t.SeatCount;
                        HunFree += t.SeatCount - (t.RegisteredStudents?.Count ?? 0);
                    }
                    counters[cat] = (HunTotal, EngTotal, HunFree, EngFree);
                }
            }

            Console.WriteLine("Available and total number of seats in topics");
            foreach (var cat in context.CourseCategories)
            {
                (int HunTotal, int EngTotal, int HunFree, int EngFree) = counters[cat];
                Console.WriteLine($"- {cat.Title}: HUN {HunFree}/{HunTotal}, ENG {EngFree}/{EngTotal}");
            }
        }
    }
}
