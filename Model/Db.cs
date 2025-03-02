
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;

namespace Knowledge.Model
{
    public class Db
    {
        
        private readonly IConfiguration Configuration;
        private readonly string CosmosDBAccountUri;
        private readonly string CosmosDBAccountPrimaryKey;

        private CosmosClient cosmosClient;
        private Database? database;
        Dictionary<string, Container> containers = new Dictionary<string, Container>();

        // The name of the database and container we will create
        private readonly string databaseId = "Knowledge";

        public Db(IConfiguration configuration)
        {
            this.Configuration = configuration;
            CosmosDBAccountUri = configuration["CosmosDBAccountUri"];
            CosmosDBAccountPrimaryKey = configuration["CosmosDBAccountPrimaryKey"];

            this.cosmosClient = new CosmosClient(
                CosmosDBAccountUri,
                CosmosDBAccountPrimaryKey,
                new CosmosClientOptions()
                {
                    ApplicationName = "KnowledgeCosmos"
                }
            );
        }

        private async Task<Boolean> CreateDatabaseAsync()
        {
            // Create a new database
            DatabaseResponse response = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(this.databaseId);
            this.database = response.Database;
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
            return response.StatusCode == HttpStatusCode.Created;
        }

        // <CreateContainerAsync>
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>
        /// <returns></returns>
        private async Task<Container> CreateContainerAsync(string containerId)
        {
            // Create a new container
            var container = await this.database!.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            this.containers.Add(containerId, container);
            Console.WriteLine("Created Container: {0}\n", this.containers[containerId].Id);
            return container;
        }
        // </CreateContainerAsync>


        // <ScaleContainerAsync>
        /// <summary>
        /// Scale the throughput provisioned on an existing Container.
        /// You can scale the throughput (RU/s) of your container up and down to meet the needs of the workload. Learn more: https://aka.ms/cosmos-request-units
        /// </summary>
        /// <returns></returns>
        private async Task ScaleContainerAsync(string containerId)
        {
            // Read the current throughput
            try
            {
                Container container = this.containers[containerId];
                int? throughput = await container.ReadThroughputAsync();
                if (throughput.HasValue)
                {
                    Console.WriteLine("Current provisioned throughput : {0}\n", throughput.Value);
                    int newThroughput = throughput.Value + 100;
                    // Update throughput
                    await container.ReplaceThroughputAsync(newThroughput);
                    Console.WriteLine("New provisioned throughput : {0}\n", newThroughput);
                }
            }
            catch (CosmosException cosmosException) when (cosmosException.StatusCode == HttpStatusCode.BadRequest)
            {
                Console.WriteLine("Cannot read container throuthput.");
                Console.WriteLine(cosmosException.ResponseBody);
            }

        }
        // </ScaleContainerAsync>

        private async Task<bool> AddInitialData()
        {
            List<CategoryData> list = new List<CategoryData>();
            try
            {
                var category = new Category(this.Configuration);
                using (StreamReader r = new StreamReader("InitialData/categories-questions.json"))
                {
                    string json = r.ReadToEnd();
                    CategoriesData categoriesData = JsonConvert.DeserializeObject<CategoriesData>(json);
                    foreach (var categoryData in categoriesData!.Categories)
                    {
                        categoryData.parentCategory = null;
                        list.Add(categoryData);
                        await category.AddCategory(categoryData);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<Container> GetContainer(string containerId)
        {
            if (this.database == null)
            {
                bool created = await this.CreateDatabaseAsync();
                if (created)
                {
                    await this.AddInitialData();
                }
            }
            Container? container = null;
            if (this.containers.ContainsKey(containerId))
            {
                container = this.containers[containerId];
            }
            else 
            {
                container = await this.CreateContainerAsync(containerId);
                //await this.ScaleContainerAsync();
            }
            return container;
        }

        // <GetStartedAsync>
        /// <summary>
        /// Entry point to call methods that operate on Azure Cosmos DB resources in this sample
        /// </summary>
        public async Task GetStartedAsync()
        {
            // Create a new instance of the Cosmos Client
            this.cosmosClient = new CosmosClient(
                CosmosDBAccountUri,
                CosmosDBAccountPrimaryKey,
                new CosmosClientOptions()
                {
                    ApplicationName = "KnowledgeCosmos"
                }
            );

            //await this.CreateDatabaseAsync();
            //await this.CreateContainerAsync();
            //await this.ScaleContainerAsync();
            //await this.AddItemsToContainerAsync();
            //await this.QueryItemsAsync();
            //await this.ReplaceFamilyItemAsync();
            // await this.DeleteFamilyItemAsync();
            // await this.DeleteDatabaseAndCleanupAsync();
        }
            
        // </GetStartedDemoAsync>

    }
}
