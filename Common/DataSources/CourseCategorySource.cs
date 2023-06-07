using Common.Model;

namespace Common.DataSources
{
    public class CourseCategorySource : ICourseCategorySource
    {
        IEnumerable<CourseCategory> ICourseCategorySource.GetCourseCategories()
        {
            return new List<CourseCategory>()
                {
                    new CourseCategory("Onlab BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Onlab"),
                    new CourseCategory("Témalabor BProf", "https://www.aut.bme.hu/Education/BProf/Temalabor"),
                    new CourseCategory("Önlab BSc Villany", "https://www.aut.bme.hu/Education/BScVillany/Onlab"),
                    new CourseCategory("Szakdolgozat BSc Villany", "https://www.aut.bme.hu/Education/BScVillany/Szakdolgozat"),
                    new CourseCategory("Témalabor BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Temalabor"),
                    new CourseCategory("Önlab BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Onlab"),
                    new CourseCategory("Szakdolgozat BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat"),
                    new CourseCategory("Önlab MSc Villany", "https://www.aut.bme.hu/Education/MScVillany/Onlab"),
                    new CourseCategory("Dipterv MSc Villany", "https://www.aut.bme.hu/Education/MScVillany/Diploma"),
                    new CourseCategory("Önlab MSc Info", "https://www.aut.bme.hu/Education/MScInfo/Onlab"),
                    new CourseCategory("Dipterv MSc Info", "https://www.aut.bme.hu/Education/MScInfo/Diploma"),
                    new CourseCategory("Önlab MSc Mecha", "https://www.aut.bme.hu/Education/MScMechatronika/Onlab"),
                    new CourseCategory("Dipterv MSc Mecha", "https://www.aut.bme.hu/Education/MScMechatronika/Diploma")
                };
        }
    }
}
