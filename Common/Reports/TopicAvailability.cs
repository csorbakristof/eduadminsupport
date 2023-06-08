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
        public void ShowFreeAndTotalAndRequiredSeatsPerCourseCategory(Context context)
        {
            var counters = new Dictionary<CourseCategory, (int HunTotal, int EngTotal, int HunFree, int EngFree, int HunRequired, int EngRequired)>();

            
            foreach (var cat in context.CourseCategories)
            {
                int enrollmentCountHun = 0;
                int enrollmentCountEng = 0;
                foreach (var c in cat.Courses)
                {
                    if (c.IsEnglish)
                        enrollmentCountEng += c.EnrolledStudentCountInNeptun.Value;
                    else
                        enrollmentCountHun += c.EnrolledStudentCountInNeptun.Value;
                }
                counters.Add(cat, (0, 0, 0, 0, enrollmentCountHun, enrollmentCountEng));
            }

            foreach (var t in context.Topics)
            {
                foreach (var cat in t.CourseCategories)
                {
                    (int HunTotal, int EngTotal, int HunFree, int EngFree, int HunRequired, int EngRequired) = counters[cat];
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
                    counters[cat] = (HunTotal, EngTotal, HunFree, EngFree, HunRequired, EngRequired);
                }
            }

            Console.WriteLine("Available and total number of seats in topics");
            foreach (var cat in context.CourseCategories)
            {
                (int HunTotal, int EngTotal, int HunFree, int EngFree, int HunRequired, int EngRequired) = counters[cat];
                Console.WriteLine($"- {cat.Title}: HUN {HunFree}/{HunTotal}/{HunRequired}, ENG {EngFree}/{EngTotal}/{EngRequired}");
            }
        }
    }
}
