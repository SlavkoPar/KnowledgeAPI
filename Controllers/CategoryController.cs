using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Knowledge.Operations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IConfiguration Configuration;

        public CategoryController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

       

        // GET api/<FamilyController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<CategoryData> list = new List<CategoryData>();
            try
            {
                var db = new DB(Configuration);
                var container = await db.getContainer();
                using (StreamReader r = new StreamReader("InitialData/categories-questions.json"))
                {
                    string json = r.ReadToEnd();
                    CategoriesData categoriesData = JsonConvert.DeserializeObject<CategoriesData>(json);
                    foreach (var categoryData in categoriesData!.Categories)
                    {
                        categoryData.parentCategory = null;
                        list.Add(categoryData);
                        await db.AddCategory(categoryData);
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
                }

                return Ok(list);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategories(int id)
        {
            try
            {
                var db = new DB(Configuration);
                var container = await db.getContainer();

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
