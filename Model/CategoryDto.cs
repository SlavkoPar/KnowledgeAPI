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
        public IList<QuestionDto>? questions { get; set; }

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

            if (category.questions == null)
            {
                this.questions = null;
            }
            else
            {
                IList<QuestionDto> list = new List<QuestionDto>();
                foreach (var q in category.questions)
                    list.Add(new QuestionDto(q));
                this.questions = list;
            }
        }

        //public CategoryDto(Category category, IList<Question> questions)
        //    : this(category)
        //{
        //    this.questions = questions;

        //}

    }
}



