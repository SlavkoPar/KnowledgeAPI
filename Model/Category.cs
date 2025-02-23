using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Knowledge.Model
{
    public class Category
    {

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string title { get; set; }
        public int kind { get; set; }
        public string? parentCategory { get; set; }
        public IList<string> words { get; set; }
        public int level { get; set; }
        public IList<string>? variations { get; set; }
        public int numOfQuestions { get; set; }
        public bool hasSubCategories { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }

        public Category()
        {

        }
        public Category(CategoryData categoryData)
        {
            this.Id = categoryData.id;
            this.PartitionKey = categoryData.PartitionKey;
            this.title = categoryData.title;
            this.words =
                categoryData.title
                    .ToLower()
                    .Replace("?", "")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
                    .Where(w => w.Length > 1)
                    .ToList();
            this.kind = categoryData.kind;
            this.parentCategory = categoryData.parentCategory;
            this.level = 1;
            this.variations = categoryData.variations ?? [];
            this.numOfQuestions = categoryData.questions == null ? 0 : categoryData.questions.Count;
            this.hasSubCategories = categoryData.categories == null ? false : categoryData.categories.Count > 0;
            this.created = new WhoWhen("Admin");
            this.modified = null;
            this.archived = null;
        }
    }
}



