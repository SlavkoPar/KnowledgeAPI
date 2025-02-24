using System.Diagnostics.Metrics;


namespace Knowledge.Model
{
    public class QuestionData
    {
        public string? parentCategory { get; set; }
        public string id { get; set; }
        public string? PartitionKey { get; set; }

        public string title { get; set; }
        public IList<int>? assignedAnswers { get; set; }
        //public int? source { get; set; }
        //public int? status { get; set; }
    }


    public class QuestionsData
    {
        public List<CategoryData> Questions { get; set; }
    }
}
