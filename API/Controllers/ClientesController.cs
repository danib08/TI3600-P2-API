using BDAProy2.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace BDAProy2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IGraphClient _client;

        public ClientesController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var clientes = await _client.Cypher.Match("(x:Clientes)")
                .Return(x => x.As<Clientes>()).ResultsAsync;

            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var clientes = await _client.Cypher.Match("(x:Clientes)")
                                               .Where((Clientes x) => x.id == id)
                                               .Return(x => x.As<Clientes>()).ResultsAsync;
            return Ok(clientes.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]Clientes cl)
        {
            await _client.Cypher.Create("(x:Clientes $cl)")
                                .WithParam("cl", cl)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]Clientes cl)
        {
            await _client.Cypher.Match("(x:Clientes)")
                                .Where((Clientes x) => x.id == id)
                                .Set("x = $cl")
                                .WithParam("cl", cl)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(x:Clientes)")
                                .Where((Clientes x) => x.id == id)
                                .Delete("x")
                                .ExecuteWithoutResultsAsync();
            
            return Ok();
        }

        [HttpGet("clienteCompraComun/{nombreCliente}/{apellidoCliente}")]
        public async Task<IActionResult> ClientCommonProd(string nombreCliente, string apellidoCliente)
        {
            var clientes = await _client.Cypher.Match("(c:Clientes), (p:Productos), (x:Compras)")
                                               .Where((Clientes c, Productos p, Compras x) => 
                                               (nombreCliente == c.first_name & 
                                                apellidoCliente == c.last_name &
                                                c.id == x.idCliente))
                                               .Return(x => new
                                              {
                                                nombreCliente = Return.As<string>("c.first_name"),
                                                apellidoCliente = Return.As<string>("c.last_name")
                                              })
                                              .ResultsAsync;
            return Ok(clientes);
        }
    }
}