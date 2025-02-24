﻿using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;
using System.Net;

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

        public static Db? Db { get; set; } = null;

        private readonly string containerId = "Items";
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
            this.Id = categoryData.id;
            this.PartitionKey = categoryData.PartitionKey!;
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

        public async Task AddCategory(CategoryData categoryData)
        {
            if (Category.container == null)
            {
                Category.container = await Category.Db!.GetContainer(this.containerId);
            }

            if (categoryData.parentCategory == null)
            {
                Category.partitionKey = categoryData.id;
                categoryData.PartitionKey = Category.partitionKey;
            }
            // Create a category object 
            Category category = new Category(categoryData: categoryData);
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



