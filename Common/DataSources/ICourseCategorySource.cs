﻿using Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataSources
{
    public interface ICourseCategorySource
    {
        List<CourseCategory> GetCourseCategories();
    }
}
