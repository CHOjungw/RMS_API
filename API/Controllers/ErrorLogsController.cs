using Library.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RMSAPI.ErrorHub;
using static Library.Models.ErrorViewModel;
using Library.Models;

namespace RMSAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ErrorLogsController : ControllerBase
    {
        private readonly IErrorLogService _errorLogService;        

        public ErrorLogsController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
            
        }

        [HttpPost]
        public async Task<IActionResult> AddErrorLog([FromBody] ErrorLog errorLog)
        {
            if (errorLog == null)
            {
                return BadRequest("Error log is null.");
            }

            await _errorLogService.AddErrorLog(errorLog);
            return Ok();
        }
        
     
    }
}
