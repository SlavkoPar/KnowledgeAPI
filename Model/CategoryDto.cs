using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class CategoryDto
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string PartitionKey { get; set; }
        public string title { get; set; }
        public int kind { get; set; }
        public string? parentCategory { get; set; }
        public int level { get; set; }
        public IList<string>? variations { get; set; }
        public int numOfQuestions { get; set; }
        public bool hasSubCategories { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }
        public IList<QuestionDto>? questions { get; set; }

        public  CategoryDto(QuestionsMore questionsMore)
        {
            this.questions = this.Questions2Dto(questionsMore.questions);
        }

        public CategoryDto(Category category)
        {
            this.Id = category.Id;
            this.PartitionKey = category.PartitionKey!;
            this.title = category.title;
            this.kind = category.kind;
            this.parentCategory = category.parentCategory;
            this.level = 1;
            this.variations = category.variations;
            this.numOfQuestions = category.numOfQuestions;
            this.hasSubCategories = category.hasSubCategories;
            this.created = category.created;
            this.modified = category.modified;
            this.archived = category.archived;
            if (category.questions == null)
            {
                this.questions = null;
            }
            else
            {
                //IList<QuestionDto> questions = new List<QuestionDto>();
                //foreach (var question in category.questions)
                //    questions.Add(new QuestionDto(question));
                this.questions = this.Questions2Dto(category.questions);
            }

        }

        public IList<QuestionDto> Questions2Dto(IList<Question> questions)
        {
            IList<QuestionDto> list = new List<QuestionDto>();
            foreach (var question in questions)
            {
                list.Add(new QuestionDto(question));
            }
            return list;
        }

        //public CategoryDto(Category category, IList<Question> questions)
        //    : this(category)
        //{
        //    if (questions == null)
        //    {
        //        this.questions = null;
        //    }
        //    else
        //    {
        //        IList<QuestionDto> list = new List<QuestionDto>();
        //        foreach (var q in questions)
        //            list.Add(new QuestionDto(q));
        //        this.questions = list;
        //    }
        //}

        //public CategoryDto(Category category, IList<Question> questions)
        //    : this(category)
        //{
        //    this.questions = questions;

        //}

    }
}



