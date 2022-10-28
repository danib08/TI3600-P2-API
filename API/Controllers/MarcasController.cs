using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;


namespace BDAProy2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarcasController : ControllerBase
    {

        private readonly IGraphClient _client;

        public MarcasController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var marcas = await _client.Cypher.Match("(x:Marcas)")
                .Return(x => x.As<Marcas>()).ResultsAsync;

            return Ok(marcas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var marcas = await _client.Cypher.Match("(x:Marcas)")
                                               .Where((Marcas x) => x.id == id)
                                               .Return(x => x.As<Marcas>()).ResultsAsync;
            return Ok(marcas.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Marcas mar)
        {
            await _client.Cypher.Create("(x:Marcas $mar)")
                                .WithParam("mar", mar)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Marcas mar)
        {
            await _client.Cypher.Match("(x:Marcas)")
                                .Where((Marcas x) => x.id == id)
                                .Set("x = $mar")
                                .WithParam("mar", mar)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(x:Marcas)")
                                .Where((Marcas x) => x.id == id)
                                .Delete("x")
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpGet("topFiveMarcas")]
        public async Task<IActionResult> GetTopFiveMarcas()
        {
            var compras = await _client.Cypher.Match("(m:Marcas)<-[:esMarca]-(p:Productos)-[:prodCompra]->(c:Compras)")

                                              .Return(x => new
                                              {

                                                  nombreMarca = Return.As<string>("m.nombre"),
                                                  paisMarca = Return.As<string>("m.pais"),
                                                  cantidad = Return.As<int>("SUM(c.cantidad)")
                                              })
                                              .OrderByDescending("cantidad")
                                              .Limit(5).ResultsAsync;


            return Ok(compras);
        }

    }
}
