using Common.Model;

namespace Common.DataSources
{
    public class CourseCategorySource : ICourseCategorySource
    {
        IEnumerable<CourseCategory> ICourseCategorySource.GetCourseCategories()
        {
            return new CourseCategory[]
                {
                    new CourseCategory("Témalabor BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Temalabor", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Témalabor BProf", "https://www.aut.bme.hu/Education/BProf/Temalabor", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.BProf),
                    new CourseCategory("Önlab BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Onlab", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Önlab BSc Villany", "https://www.aut.bme.hu/Education/BScVillany/Onlab", CourseCategory.MajorEnum.Villany, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Szakdolgozat BSc Info", "https://www.aut.bme.hu/Education/BScInfo/Szakdolgozat", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Szakdolgozat BSc Villany", "https://www.aut.bme.hu/Education/BScVillany/Szakdolgozat", CourseCategory.MajorEnum.Villany, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Szakdolgozat BSc Mecha", "https://www.aut.bme.hu/Education/BScMechatronika/Szakdolgozat", CourseCategory.MajorEnum.Mecha, CourseCategory.LevelEnum.BSc),
                    new CourseCategory("Önlab MSc Info", "https://www.aut.bme.hu/Education/MScInfo/Onlab", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.MSc),
                    new CourseCategory("Önlab MSc Villany", "https://www.aut.bme.hu/Education/MScVillany/Onlab", CourseCategory.MajorEnum.Villany, CourseCategory.LevelEnum.MSc),
                    new CourseCategory("Önlab MSc Mecha", "https://www.aut.bme.hu/Education/MScMechatronika/Onlab", CourseCategory.MajorEnum.Mecha, CourseCategory.LevelEnum.MSc),
                    new CourseCategory("Dipterv MSc Info", "https://www.aut.bme.hu/Education/MScInfo/Diploma", CourseCategory.MajorEnum.Info, CourseCategory.LevelEnum.MSc),
                    new CourseCategory("Dipterv MSc Villany", "https://www.aut.bme.hu/Education/MScVillany/Diploma", CourseCategory.MajorEnum.Villany, CourseCategory.LevelEnum.MSc),
                    new CourseCategory("Dipterv MSc Mecha", "https://www.aut.bme.hu/Education/MScMechatronika/Diploma", CourseCategory.MajorEnum.Mecha, CourseCategory.LevelEnum.MSc)
                };
        }
    }
}
