using Library.Interfaces;
using Library.Models;
using Library.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace RMSAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttContoller : ControllerBase
    {
        private readonly MqttService _mqttService;
        private readonly IServiceProvider _serviceProvider;
        public MqttContoller(MqttService mqttService, IServiceProvider serviceProvider)
        {
            _mqttService = mqttService;
            _serviceProvider = serviceProvider;
        }
        [HttpGet("connect")]
        public async Task<IActionResult> Connect()
        {
            await _mqttService.ConnectAsync();
            return  Ok("Connected to MQTT broker.");
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] MqttPublishRequest request)
        {
            await _mqttService.PublishAsync(request.Topic, request.Payload);
            return Ok("Message published.");
        }

        [HttpGet("connected-device-count")]
        public IActionResult GetConnectedDeviceCount()
        {
            var count = _mqttService.GetConnectedDeviceCount();
            return Ok(count);
        }
        [HttpGet("GetDataBase")]
        public async Task<ActionResult<IEnumerable<Device>>> GetDataBase(string DeviceName, DateTime StartTime,DateTime endTime)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var data = await context.Devices.Where(d => d.DeviceName == DeviceName && d.LastUpdated >= StartTime && d.LastUpdated < endTime).ToListAsync();


                    return Ok(data);
                }
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetErrorLog")]
        public async Task<ActionResult<IEnumerable<ErrorLog>>> GetErrorLog(DateTime StartTime, DateTime endTime)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var data = await context.ErrorLogs.Where(d => d.LogDateTime >= StartTime && d.LogDateTime <= endTime.AddDays(1)).ToListAsync();

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("PostDeviceData")]
        public IActionResult PostDeviceData([FromBody] string data)
        {
            try
            {               
                if(string.IsNullOrEmpty(data))
                {
                    return  BadRequest("Invalid device data");
                }
                _mqttService.Signaldevice = data;                
                return Ok();
            }
            catch (Exception ex)
            { 
                return BadRequest();
            }            
        }        
        [HttpGet("GetDeviceName")]
        public async Task<ActionResult<IEnumerable<string>>> GetUniqueDataBase() 
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();
                    var data = await context.Devices.Select(d=> d.DeviceName).Distinct().ToListAsync();

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {                
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost("SetDeviceValue")]
        public IActionResult SetDeviceValue([FromBody] string data)
        {
            try
            {                
                if (string.IsNullOrEmpty(data))
                {
                    return BadRequest("Invalid device data");
                }
                _mqttService.PublishAsync(_mqttService.Signaldevice, data);
                return Ok();
            }
            catch (Exception ex)
            {  
                return BadRequest();
            }
        }


        public class MqttPublishRequest
        {
            public string Topic { get; set; }
            public string Payload { get; set; }
        }
    }
}
