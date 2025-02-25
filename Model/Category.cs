using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

namespace Knowledge.Model
{
    public class Category
    {
        public string type { get; set; }

        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string title { get; set; }
        public int kind { get; set; }
        public string? parentCategory { get; set; }
        // public IList<string> words { get; set; }
        public int level { get; set; }
        public IList<string>? variations { get; set; }
        public int numOfQuestions { get; set; }
        public bool hasSubCategories { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }
        public IList<Question>? questions { get; set; }

        public static Db? Db { get; set; } = null;

        private static readonly string containerId = "Questions";
        public static Container? container { get; set; } = null;

        public static string? partitionKey { get; set; } = null;

        public Category()
        {
        }

        public Category(IConfiguration configuration)
        {
            Category.Db = new Db(configuration);
        }

        public Category(CategoryData categoryData)
        {
            this.type = "category";
            this.Id = categoryData.id;
            this.PartitionKey = categoryData.PartitionKey!;
            this.title = categoryData.title;
            //this.words =
            //    categoryData.title
            //        .ToLower()
            //        .Replace("?", "")
            //        .Split(' ', StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries)
            //        .Where(w => w.Length > 1)
            //        .ToList();
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


        public static async Task<Category> GetCategory(string partitionKey, string id, bool hidrate)
        {
            if (Category.container == null)
            {
                Category.container = await Category.Db!.GetContainer(Category.containerId);
            }
            try
            {
                // Read the item to see if it exists.  
                //ItemResponse<Category> aResponse =
                Category category = await Category.container.ReadItemAsync<Category>(id, new PartitionKey(partitionKey));
                if (category != null && category.numOfQuestions > 0 && hidrate)
                {
                    // OR c.parentCategory = ''
                    var sqlQuery = "SELECT * FROM c WHERE c.type = 'question' AND IS_NULL(c.archived)" +
                        $" AND c.parentCategory = '{id}'";
                    QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                    FeedIterator<Question> queryResultSetIterator = container.GetItemQueryIterator<Question>(queryDefinition);
                    List<Question> questions = new List<Question>();
                    while (queryResultSetIterator.HasMoreResults)
                    {
                        FeedResponse<Question> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                        foreach (Question question in currentResultSet)
                        {
                            questions.Add(question);
                        }
                    }
                    category.questions = questions;
                }
                return category;
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
            return null;
        }

        public async Task AddCategory(CategoryData categoryData)
        {
            if (Category.container == null)
            {
                Category.container = await Category.Db!.GetContainer(Category.containerId);
            }

            if (categoryData.parentCategory == null)
            {
                Category.partitionKey = categoryData.id;
                categoryData.PartitionKey = Category.partitionKey;
            }
            // Create a category object 
            Category category = new Category(categoryData);
            try
            {
                // Read the item to see if it exists.  
                ItemResponse<Category> aResponse =
                    await Category.container.ReadItemAsync<Category>(
                        category.Id,
                        new PartitionKey(Category.partitionKey)
                    );
                Console.WriteLine("Item in database with id: {0} already exists\n", aResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                // Create an item in container.Note we provide the value of the partition key for this item
                ItemResponse<Category> aResponse =
                    await Category.container.CreateItemAsync<Category>(
                        category,
                        new PartitionKey(Category.partitionKey)
                    );

                if (categoryData.categories != null)
                {
                    foreach (var subCategoryData in categoryData.categories)
                    {
                        subCategoryData.parentCategory = category.Id;
                        subCategoryData.PartitionKey = Category.partitionKey;
                        await this.AddCategory(subCategoryData);
                    }
                }
                // questions
                if (categoryData.questions != null)
                {
                    Question question = new Question(Category.Db!);   
                    foreach (var questionData in categoryData.questions)
                    {
                        questionData.parentCategory = category.Id;
                        questionData.PartitionKey = category.Id; // Category.partitionKey;
                        await question.AddQuestion(questionData);
                    }
                }
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", aResponse.Resource.Id, aResponse.RequestCharge);
            }
            catch (Exception ex)
            {
                // Note that after creating the item, we can access the body of the item with the Resource property off the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
                Console.WriteLine(ex.Message);
            }
        }
    }
}



