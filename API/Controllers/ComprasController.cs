using BDAProy2.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ComprasController : ControllerBase
    {

        private readonly IGraphClient _client;

        public ComprasController(IGraphClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var compras = await _client.Cypher.Match("(x:Compras)")
                .Return(x => x.As<Compras>()).ResultsAsync;

            return Ok(compras);
        }

        [HttpGet("{idCliente}/{idProducto}")]
        public async Task<IActionResult> GetById(int idCliente, int idProd)
        {
            var compras = await _client.Cypher.Match("(x:Compras)")
                                               .Where((Compras x) => x.idCliente == idCliente & x.idProducto == idProd)
                                               .Return(x => x.As<Compras>()).ResultsAsync;
            return Ok(compras.LastOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Compras compras)
        {
            await _client.Cypher.Create("(x:Compras $compras)")
                                .WithParam("compras", compras)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpPut("{idCliente}/{idProducto}")]
        public async Task<IActionResult> Update(int idCliente, int idProd, [FromBody] Compras compras)
        {
            await _client.Cypher.Match("(x:Compras)")
                                .Where((Compras x) => x.idCliente == idCliente & x.idProducto == idProd)
                                .Set("x = $compras")
                                .WithParam("compras", compras)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        [HttpDelete("{idCliente}/{idProducto}")]
        public async Task<IActionResult> Delete(int idCliente, int idProd)
        {
            await _client.Cypher.Match("(x:Compras)")
                                .Where((Compras x) => x.idCliente == idCliente & x.idProducto == idProd)
                                .Delete("x")
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }


        [HttpGet("topFiveProds")]
        public async Task<IActionResult> GetTopFiveProds()
        {
            var compras = await _client.Cypher.Match("(x:Compras), (p:Productos)")
                                              .Where((Compras x, Productos p) => x.idProducto == p.id)
                                              .Return(x => new
                                              {
                                                  idProducto = Return.As<int>("p.id"),
                                                  nombreProducto = Return.As<string>("p.nombre"),
                                                  cantidad = Return.As<int>("SUM(x.cantidad)")
                                              })
                                              .OrderByDescending("cantidad")
                                              .Limit(5).ResultsAsync;
                                              

            return Ok(compras);
        }

        [HttpGet("topFiveClientes")]
        public async Task<IActionResult> GetTopFiveClientes()
        {
            var clientes = await _client.Cypher.Match("(x:Compras), (c:Clientes)")
                                              .Where((Compras x, Clientes c) => x.idCliente == c.id)
                                              .Return(x => new
                                              {
                                                  idCliente = Return.As<string>("x.idCliente"),
                                                  nombreCliente = Return.As<string>("c.first_name"),
                                                  cantidad = Return.As<int>("SUM(x.cantidad)")
                                              })
                                              .OrderByDescending("cantidad")
                                              .Limit(5).ResultsAsync;


            return Ok(clientes);
        }

         

        [HttpPost("registrarCompras")]
        public async Task<IActionResult> Create([FromBody] RegistroCompra registro)
        {
            int clientId = registro.idCliente;

            foreach (Productos prod in registro.listaProductos)
            {
                var exists = await _client.Cypher.Match("(x:Compras)")
                                              .Where((Compras x) => (x.idCliente == clientId && x.idProducto == prod.id))
                                              .Return(x => new
                                              {

                                                  idProducto = Return.As<int>("x.idProducto"),
                                                  idCliente = Return.As<int>("x.idCliente"),
                                                  cantidad = Return.As<int>("x.cantidad")
                                              })
                                              .ResultsAsync;
            }
            return Ok();
        }
    }
}
