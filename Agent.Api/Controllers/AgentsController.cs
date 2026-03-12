using Agent.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AgentsController : ControllerBase
    {
        private static List<Agent> _agents = new List<Agent>
        {
            new() { Id = "1", Name = "Agent One", Status = "running" },
            new() { Id = "2", Name = "Agent Two", Status = "stopped" }
        };

        [HttpGet]
        public ActionResult<PagedResponse<Agent>> GetAgents([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var items = _agents.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Ok(new PagedResponse<Agent>
            {                
                Items = items,
                Page = page,
                PageSize = pageSize,
                Total = _agents.Count
            });
        }

        [HttpGet("{id}")]
        public ActionResult<Agent> GetAgent(string id)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            return Ok(agent);
        }

        [HttpPost]
        public ActionResult<Agent> CreateAgent([FromBody] AgentCreateRequest request)
        {
            var newAgent = new Agent
            {
                Id = (_agents.Count + 1).ToString(),
                Name = request.Name,
                Status = "stopped"
            };
            _agents.Add(newAgent);
            return Ok(newAgent);
        }

        [HttpPut("{id}")]
        public ActionResult<Agent> UpdateAgent(string id, [FromBody] AgentUpdateRequest request)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            agent.Name = request.Name;
            // puedes actualizar más campos
            return Ok(agent);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAgent(string id)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            _agents.Remove(agent);
            return NoContent();
        }

        [HttpPost("{id}/start")]
        public ActionResult<Agent> StartAgent(string id)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            agent.Status = "running";
            return Ok(agent);
        }

        [HttpPost("{id}/stop")]
        public ActionResult<Agent> StopAgent(string id)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == id);
            if (agent == null) return NotFound();
            agent.Status = "stopped";
            return Ok(agent);
        }
    }

    // Mock Models
    public class Agent
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
    
    public class AgentCreateRequest
    {
        public string Name { get; set; }
    }

    public class AgentUpdateRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}