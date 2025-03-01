using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;

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
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                Category.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                List<Category> subCategories = await Category.GetAllCategories();
                if (subCategories != null)
                {
                    List<CategoryDto> list = new List<CategoryDto>();
                    foreach (Category category in subCategories)
                    {
                        list.Add(new CategoryDto(category));
                    }
                    return Ok(list);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET api/<FamilyController>
        [HttpGet("{parentCategory}")]
        public async Task<IActionResult> GetSubCategories(string parentCategory)
        {
            try
            {
                Category.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                List<Category> subCategories = await Category.GetSubCategories(parentCategory);
                if (subCategories != null)
                {
                    List<CategoryDto> list = new List<CategoryDto>();
                    foreach (Category category in subCategories)
                    {
                        list.Add(new CategoryDto(category));
                    }
                    return Ok(list);
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{partitionKey}/{id}/{hidrate}/{pageSize}")]
        public async Task<IActionResult> Get(string partitionKey, string id, bool hidrate, int pageSize)
        {
            try
            {
                Category.Db = new Db(this.Configuration);
               // var container = await Db.GetContainer(this.containerId);
                Category category = await Category.GetCategory(partitionKey, id, hidrate, pageSize);
                if (category != null)
                {
                    return Ok(new CategoryDto(category));
                }
                return NotFound();
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
