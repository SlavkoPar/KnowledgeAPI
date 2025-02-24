﻿using System.Diagnostics.Metrics;


namespace Knowledge.Model
{
    public class QuestionData
    {
        public string title { get; set; }
        public IList<int>? assignedAnswers { get; set; }
        //public int? source { get; set; }
        //public int? status { get; set; }
    }

    public class CategoryData
    {
        public string? parentCategory { get; set; }
        public string id { get; set; }
        public string? PartitionKey { get; set; }
        public string title { get; set; }
        public int kind { get; set; }
        public IList<string>? variations { get; set; }
        public IList<CategoryData>? categories { get; set; }
        public IList<QuestionData>? questions { get; set; }
    }

    public class CategoriesData
    {
        public List<CategoryData> Categories { get; set; }
    }
}
