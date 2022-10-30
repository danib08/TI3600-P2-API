using API.Models;
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
        public async Task<IActionResult> GetById(int idCliente, int idProducto)
        {
            var compras = await _client.Cypher.Match("(x:Compras)")
                                               .Where((Compras x) => x.idCliente == idCliente & x.idProducto == idProducto)
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
        public async Task<IActionResult> Delete(int idCliente, int idProducto)
        {
            await _client.Cypher.Match("(x:Compras)")
                                .Where((Compras x) => x.idCliente == idCliente & x.idProducto == idProducto)
                                .DetachDelete("x")
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }
 
        /// <summary>
        /// POST para registrar las Compras de un Cliente
        /// </summary>
        /// <param name="registro">
        /// Objeto con los productos comprados y su cantidad
        /// </param>
        /// <param name="idCliente">
        /// Identificador del Cliente que realiza la compra
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     POST api/Compras/registroDeCompras
        ///     [
        ///      {       
        ///       "idProducto": 1,        
        ///       "cantidad": 5,
        ///      },
        ///      {       
        ///       "idProducto": 7,        
        ///       "cantidad": 3,
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpPost("registroDeCompras/{idCliente}")]
        public async Task<IActionResult> Create([FromBody] RegistroCompra[] registro, int idCliente)
        {
            int clientId = idCliente;

            foreach (RegistroCompra item in registro)
            {
                // Crea nuevo nodo Compra si el cliente nunca antes habia comprado dicho Producto,
                // de lo contrario, actualiza la "cantidad" del nodo existente
                var merge_nodes = await _client.Cypher.Merge("(c:Compras {idCliente:" + idCliente + ", idProducto:" + item.idProducto + "})")
                                              .OnCreate()
                                              .Set("c.cantidad=" + item.cantidad)
                                              .OnMatch()
                                              .Set("c.cantidad=c.cantidad+" + item.cantidad)
                                              .Return(x => new
                                              {
                                                  idProducto = Return.As<int>("c.idProducto")
                                              })
                                              .ResultsAsync;

                // Si no existe, crea la relacion ClienteCompra
                var merge_relat = await _client.Cypher.Match("(cl:Clientes {id:" + idCliente + "}), (c:Compras {idCliente:" + idCliente + ", idProducto:" + item.idProducto + "})")
                                             .Merge("(cl)-[r:ClienteCompra]->(c)")
                                             .Return(x => new
                                              {
                                                  idProducto = Return.As<int>("c.idProducto")
                                              })
                                              .ResultsAsync;

                // Si no existe, crea la relacion prodCompra
                var merge_relat2 = await _client.Cypher.Match("(p:Productos {id:" + item.idProducto + "}), (c:Compras {idCliente:" + idCliente + ", idProducto:" + item.idProducto + "})")
                                             .Merge("(p)-[r:prodCompra]->(c)")
                                             .Return(x => new
                                              {
                                                  idProducto = Return.As<int>("c.idProducto")
                                              })
                                              .ResultsAsync;
            }
            return Ok();
        }
    }
}
