using Agent.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MonitoringController : ControllerBase
    {
        // Mock logs
        private static List<LogEntry> _logs = new List<LogEntry>
        {
            new LogEntry { Id = "1", Timestamp = DateTime.UtcNow.AddMinutes(-15), Message = "System started", Level = "Info" },
            new LogEntry { Id = "2", Timestamp = DateTime.UtcNow.AddMinutes(-5), Message = "Agent running", Level = "Info" },
            new LogEntry { Id = "3", Timestamp = DateTime.UtcNow.AddMinutes(-2), Message = "CPU spike detected", Level = "Warning" }
        };

        // Mock metrics
        private static MetricSnapshot _latestMetrics = new MetricSnapshot
        {
            Timestamp = DateTime.UtcNow,
            CpuUsage = 17.4,
            MemoryUsage = 927.6,
            ActiveAgents = 5
        };

        // GET: /logs?page=1&pageSize=50
        [HttpGet("/logs")]
        public ActionResult<PagedResponse<LogEntry>> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var items = _logs.OrderByDescending(l => l.Timestamp)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();
            return Ok(new PagedResponse<LogEntry>
            {                
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = _logs.Count
            });
        }

        // GET: /metrics/latest
        [HttpGet("/metrics/latest")]
        public ActionResult<MetricSnapshot> GetLatestMetrics()
        {
            return Ok(_latestMetrics);
        }
    }

    // Mock Models
    public class LogEntry
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
    }

    public class MetricSnapshot
    {
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public int ActiveAgents { get; set; }
    }    
}
