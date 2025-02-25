using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class QuestionDto
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string title { get; set; }
        public string? parentCategory { get; set; }
        public List<long>? assignedAnswers { get; set; }
        public int source { get; set; }
        public int status { get; set; }

        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }

        public QuestionDto(Question question)
        {
            this.Id = question.Id;
            this.title = question.title;
            this.parentCategory = question.parentCategory;
            this.source = question.source;
            this.status = question.status;
            this.created = question.created;
            this.modified = question.modified;
            this.archived = question.archived;
        }

    }
}



