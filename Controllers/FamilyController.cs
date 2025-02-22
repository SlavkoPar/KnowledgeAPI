using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Knowledge.opera;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        private readonly string CosmosDbName = "Families";
        private readonly string CosmosDbContainerName = "Items";

        public FamilyController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private Container ContainerClient()
        {
            string CosmosDBAccountUri = Configuration["CosmosDBAccountUri"];
            string CosmosDBAccountPrimaryKey = Configuration["CosmosDBAccountPrimaryKey"];
            CosmosClient cosmosDbClient = new CosmosClient(CosmosDBAccountUri, CosmosDBAccountPrimaryKey);
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerName);
            return containerClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilies()
        {
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT VALUE { id: c.id, LastName: c.LastName, Address: c.Address } FROM c";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Family> queryResultSetIterator = container.GetItemQueryIterator<Model.Family>(queryDefinition);
                List<Family> families = new List<Family>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Family> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Family family in currentResultSet)
                    {
                        families.Add(family);
                    }
                }
                return Ok(families);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<FamilyController>/5
        [HttpGet("{id}")]
        public List<CategoryData> Get(int id)
        {
            using (StreamReader r = new StreamReader("InitialData/categories-questions.json"))
            {
                string json = r.ReadToEnd();
                CategoriesData categoriesData = JsonConvert.DeserializeObject<CategoriesData>(json);
                List<CategoryData> list = new List<CategoryData>();
                foreach (var categoryData in categoriesData!.Categories)
                {
                    list.Add(categoryData);
                    //await addCategory(categoryData)
                }

                //IEnumerable<SubCategoryDto> rows =
                //      categorys
                //         .Select(e => new SubCategoryDto
                //         {
                //             SubCategoryCode = e.SubCategoryCode,
                //             SubCategoryName = e.SubCategoryName
                //         })
                //         .ToList();

                //var rowsAtPage = rows
                //                      .Skip((query.Page - 1) * query.PageSize)
                //                      .Take(query.PageSize)
                //                      .AsEnumerable()
                //                      //.Select(item => new UserDtoSmall(item))
                //                      .ToList();
                return list;
            }
        }

        // POST api/<FamilyController>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<FamilyController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<FamilyController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
