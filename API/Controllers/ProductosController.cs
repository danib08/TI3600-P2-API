using BDAProy2.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;


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

        [HttpGet("prodsPorCliente/{nombre}/{apellido}")]
        public async Task<IActionResult> GetProdsByClient(string nombre, string apellido)
        {
            var clientes = await _client.Cypher.Match("(c:Clientes), (p:Productos), (x:Compras)")
                                               .Where((Clientes c, Productos p, Compras x) => 
                                               (c.first_name == nombre & c.last_name == apellido &
                                               c.id == x.idCliente & p.id == x.idProducto))
                                               .Return(x => new
                                              {
                                                nombreProducto = Return.As<string>("p.nombre"),
                                                marcaProducto = Return.As<string>("p.marca"),
                                                cantidadProducto = Return.As<int>("x.cantidad"),
                                              })
                                              .ResultsAsync;
            return Ok(clientes);
        }

        [HttpGet("clienteProductoComun/{nombreProducto}")]
        public async Task<IActionResult> ClientCommonProd(string nombreProducto)
        {
            var clientes = await _client.Cypher.Match("(c:Clientes), (p:Productos), (x:Compras)")
                                               .Where((Clientes c, Productos p, Compras x) => 
                                               (p.id == x.idProducto & c.id == x.idCliente &
                                               nombreProducto == p.nombre))
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
