using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Knowledge.Model;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Knowledge.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly string containerId = "Questions";

        public CatController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

    
        [HttpGet("{partitionKey}/{id}/{upTheTree}")]
        public async Task<IActionResult> GetCategories(string partitionKey, string id, bool upTheTree)
        {
            try
            {
                Category.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                List<CategoryDto> list = new List<CategoryDto>();
                Category category = await Category.GetCategory(partitionKey, id, false, 0, null);
                if (category != null)
                {
                    list.Add(new CategoryDto(category));
                    var parentCategory = category.parentCategory;
                    while (parentCategory != null)
                    {
                        Category c = await Category.GetCategory(partitionKey, parentCategory, false, 0, null);
                        if (c != null)
                        {
                            list.Add(new CategoryDto(c));
                            parentCategory = c.parentCategory;
                        }
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


        [HttpGet("{partitionKey}/{id}")]
        public async Task<IActionResult> GetCategory(string partitionKey, string id)
        {
            try
            {
                // TODO 1. ovo 2. what does  /partitionKey mean?
                Category.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                Category category = await Category.GetCategory(partitionKey, id, false, 0, null);
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
    }
}
