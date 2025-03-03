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
    public class QuestionController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly string containerId = "Questions";

        public QuestionController(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        [HttpGet("{parentCategory}/{startCursor}/{pageSize}/{includeQuestionId}")]
        public async Task<IActionResult> GetQuestions(string parentCategory, int startCursor, int pageSize, string? includeQuestionId)
        {
            try
            {
                Question.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                QuestionsMore questionsMore = await Question.GetQuestions(parentCategory, startCursor, pageSize, includeQuestionId);
                var categoryDto = new CategoryDto(questionsMore);
                return Ok(categoryDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{parentCategory}/{id}")]
        public async Task<IActionResult> GetQuestion(string parentCategory, string id)
        {
            try
            {
                Question.Db = new Db(this.Configuration);
                // var container = await Db.GetContainer(this.containerId);
                Question question = await Question.GetQuestion(parentCategory, id);
                if (question != null)
                {
                    return Ok(new QuestionDto(question));
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
