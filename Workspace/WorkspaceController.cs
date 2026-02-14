using Knowledge.Services;
using KnowledgeAPI.A.Workspaces.Model;
using Microsoft.AspNetCore.Mvc;


// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KnowledgeAPI.A.Workspaces
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class WorkspaceController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        DbService dbService { get; set; }

        public WorkspaceController(IConfiguration configuration)
        {
            dbService = new DbService(configuration);
            dbService.Initialize.Wait();
            Configuration = configuration;
        }

        [HttpPost]
        //[Authorize]
        [Route("create")]
        public async Task<IActionResult> Post([FromBody] WorkspaceDto workspaceDto)
        {
            try
            {
                Console.WriteLine("===>>> CreateWorkspace: {0} \n", workspaceDto);
                var workspaceService = new WorkspaceService(dbService, "");
                WorkspaceEx workspaceEx = await workspaceService.CreateWorkspace(workspaceDto);
                return Ok(new WorkspaceDtoEx(workspaceEx));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        //[Authorize]
        [Route("get")]
        public async Task<IActionResult> Post([FromBody] WorkspaceKey workspaceKey)
        {
            try
            {
                Console.WriteLine("===>>> GetWorkspace: {0} \n", workspaceKey);
                var workspaceService = new WorkspaceService(dbService, "workspace");
                WorkspaceEx workspaceEx = await workspaceService.GetWorkspace(workspaceKey);
                WorkspaceDtoEx wsDtoEx = new WorkspaceDtoEx(workspaceEx);
                return Ok(wsDtoEx);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
