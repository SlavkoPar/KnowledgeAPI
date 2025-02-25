using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly string containerId = "Questions";

        public CategoryController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
       

        // GET api/<FamilyController>
        [HttpGet]
        public async Task<IActionResult> GetCategories(string parentCategory)
        {
            try
            {
                var Db = new Db(this.Configuration);
                var container = await Db.GetContainer(containerId);

                // OR c.parentCategory = ''
                var sqlQuery = "SELECT * FROM c WHERE c.type = 'category' AND " + (
                    (parentCategory == "null")
                        ? "IS_NULL(c.parentCategory)" 
                        : $"c.parentCategory = '{parentCategory}'"
                );
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Category> queryResultSetIterator = container.GetItemQueryIterator<Category>(queryDefinition);
                List<CategoryDto> items = new List<CategoryDto>();
                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Category> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Category category in currentResultSet)
                    {
                        items.Add(new CategoryDto(category));
                    }
                }
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            List<CategoryDto> list = new List<CategoryDto>();
            try
            {
                var category = new Category(this.Configuration);

                //using() {
                //}

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
