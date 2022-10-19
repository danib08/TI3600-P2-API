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
        public async Task<IActionResult> ClientCommonBuy(string nombreCliente, string apellidoCliente)
        {
            var clientes = await _client.Cypher.Match("(c:Clientes)-[:clienteCompra]->(x:Compras)<-[:prodCompra]-(p:Productos)-[:prodCompra]->(co:Compras)<-[:clienteCompra]-(a:Clientes)")
                                               .With("Count(a.id) as prodQuant, a as ci, c as cl")
                                               .Where("cl.first_name=\"" + nombreCliente + 
                                               "\" and cl.last_name=\"" + apellidoCliente + 
                                               "\" and prodQuant > 1")
                                               .Return(x => new
                                              {
                                                nombreCliente = Return.As<string>("ci.first_name"),
                                                apellidoCliente = Return.As<string>("ci.last_name")
                                              })
                                              .ResultsAsync;

            //List<ComprasComun> lista = new List<ComprasComun>();
            //List<string> list1 = new List<string>();
            //var nombre = clientes.ElementAt(0).nombreCliente;

            //foreach (var item in clientes)
            //{
                var productos = await _client.Cypher.Match("(c:Clientes)-[:clienteCompra]->(x:Compras)<-[:prodCompra]-(p:Productos)-[:prodCompra]->(co:Compras)<-[:clienteCompra]-(a:Clientes)")
                                               .Where("c.first_name=\"" + nombreCliente + 
                                               "\" and c.last_name=\"" + apellidoCliente + 
                                               "\" and a.first_name=\"" + clientes.ElementAt(0).nombreCliente + 
                                               "\" and a.last_name=\"" + clientes.ElementAt(0).apellidoCliente + "\"")
                                               .Return(x => new
                                              {
                                                nombreProducto = Return.As<string>("p.nombre")                                               
                                              })
                                              .ResultsAsync;  

                /*ComprasComun comprasComun = new ComprasComun();
                comprasComun.nombreCliente = item.nombreCliente;
                comprasComun.apellidoCliente = item.apellidoCliente;
                foreach (var prod in productos)
                {
                    comprasComun.listaProductos.Append(prod.nombreProducto);
                }
                lista.Append(comprasComun);

                list1.Add(productos.ElementAt(0).nombreProducto);*/
            //}

            return Ok(productos);
        }
    }
}