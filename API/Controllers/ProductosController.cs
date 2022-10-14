using BDAProy2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDAProy2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {

        private readonly IGraphClient _client;

        public ProductosController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var productos = await _client.Cypher.Match("(x:Productos)")
                .Return(x => x.As<Productos>()).ResultsAsync;

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var productos = await _client.Cypher.Match("(x:Productos)")
                                               .Where((Productos x) => x.id == id)
                                               .Return(x => x.As<Productos>()).ResultsAsync;
            return Ok(productos.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Productos prod)
        {
            await _client.Cypher.Create("(x:Productos $prod)")
                                .WithParam("prod", prod)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Productos prod)
        {
            await _client.Cypher.Match("(x:Productos)")
                                .Where((Productos x) => x.id == id)
                                .Set("x = $prod")
                                .WithParam("prod", prod)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(x:Productos)")
                                .Where((Productos x) => x.id == id)
                                .Delete("x")
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

    }
}
