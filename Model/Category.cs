using Azure;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        public int level { get; set; }
        public IList<string>? variations { get; set; }
        public int numOfQuestions { get; set; }
        public bool hasSubCategories { get; set; }
        public WhoWhen created { get; set; }
        public WhoWhen? modified { get; set; }
        public WhoWhen? archived { get; set; }
        public IList<Question>? questions { get; set; }
        public bool? hasMoreQuestions { get; set; }

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
            this.questions = null;

        }

        internal static async Task<List<Category>> GetAllCategories()
        {
            if (Category.container == null)
            {
                Category.container = await Category.Db!.GetContainer(Category.containerId);
            }
            // OR c.parentCategory = ''
            var sqlQuery = "SELECT * FROM c WHERE c.type = 'category' AND IS_NULL(c.archived) ORDER BY c.Title ASC";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
            FeedIterator<Category> queryResultSetIterator = container.GetItemQueryIterator<Category>(queryDefinition);
            //List<CategoryDto> subCategories = new List<CategoryDto>();
            List<Category> subCategories = new List<Category>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Category> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Category category in currentResultSet)
                {
                    //subCategories.Add(new CategoryDto(category));
                    subCategories.Add(category);
                }
            }
            return subCategories;
        }


        internal static async Task<List<Category>> GetSubCategories(string partitionKey, string parentCategory)
        {
            if (Category.container == null)
            {
                Category.container = await Category.Db!.GetContainer(Category.containerId);
            }
            // OR c.parentCategory = ''
            var sqlQuery = $"SELECT * FROM c WHERE c.type = 'category' AND IS_NULL(c.archived) AND " + (
                (partitionKey == "null")
                    ? $""
                    : $" c.partitionKey = '{partitionKey}' AND "
            )
            + (
                (parentCategory == "null")
                    ? $" IS_NULL(c.parentCategory)"
                    : $" c.parentCategory = '{parentCategory}'"
            );
            QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
            FeedIterator<Category> queryResultSetIterator = container.GetItemQueryIterator<Category>(queryDefinition);
            //List<CategoryDto> subCategories = new List<CategoryDto>();
            List<Category> subCategories = new List<Category>();
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<Category> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (Category category in currentResultSet)
                {
                    //subCategories.Add(new CategoryDto(category));
                    subCategories.Add(category);
                }
            }
            return subCategories;
        }


 
        public static async Task<Category> GetCategory(string partitionKey, string id, bool hidrate, int? pageSize, string includeQuestionId)
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
                if (hidrate == true && category != null && category.numOfQuestions > 0)
                {
                    QuestionsMore questionsMore = await Question.GetQuestions(id, 0, (int)pageSize, includeQuestionId);
                    category.questions = questionsMore.questions;
                    category.hasMoreQuestions = questionsMore.hasMoreQuestions;
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
                    if (categoryData.id == "DOMAIN") {
                        for (var i=0; i<500; i++)
                            categoryData.questions.Add(new QuestionData(category.Id, $"Demo data for DOMAIN {i}"));
                    }

                    foreach (var questionData in categoryData.questions)
                    {
                        questionData.parentCategory = category.Id;
                        questionData.PartitionKey = category.Id;
                        await question.AddQuestion(questionData);
                    }
                    //if (categoryData.id == "DOMAIN")
                    //{
                        //for(var i=0; i<500; i++)
                        //{
                        //    var questionData = new QuestionData(category.Id, $"Demo data for DOMAIN {i}");
                        //    Question q = new Question(questionData);
                        //    await question.AddQuestion(q);
                        //}
                    //}
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



