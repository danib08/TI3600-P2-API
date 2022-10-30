using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;
using Newtonsoft.Json;

namespace BDAProy2.Controllers
{
    /// <summary>
    /// Controlador para el modelo Clientes
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly IGraphClient _client;

        public ClientesController(IGraphClient client)
        {
            _client = client;
        }

        /// <summary>
        /// GET de todos los Clientes existentes
        /// </summary>
        /// <returns> 
        /// Lista de todos los Clientes
        /// </returns>
        /// <response code="200">Retorna la lista completa de Clientes</response>
        [HttpGet]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Get()
        {
            var clientes = await _client.Cypher.Match("(x:Clientes)")
                .Return(x => x.As<Clientes>()).ResultsAsync;

            return Ok(clientes);
        }

        /// <summary>
        /// GET de un Cliente segun su identificador
        /// </summary>
        /// <param name="id">
        /// El identificador del Cliente que se desea obtener
        /// </param>
        /// <returns> El objeto Cliente deseado </returns>
        /// <response code="200">Retorna el Cliente deseado</response>
        /// <response code="404">No existe un Cliente con el identificador ingresado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var clientes = await _client.Cypher.Match("(x:Clientes)")
                                               .Where((Clientes x) => x.id == id)
                                               .Return(x => x.As<Clientes>()).ResultsAsync;
            if (clientes.Count() != 0) {
                return Ok(clientes.LastOrDefault());
            }
            else {
                return NotFound();
            }
        }

        /// <summary>
        /// POST para crear un nuevo Cliente
        /// </summary>
        /// <param name="cliente">
        /// Objeto Cliente a ser creado
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     POST api/Clientes
        ///     {       
        ///       "id": "54"         
        ///       "first_name": "Mike",
        ///       "last_name": "Andrew"
        ///     }
        /// </remarks>
        /// <response code="200">Se crea el nuevo Cliente</response>
        /// <response code="500">El identificador ingresado ya pertenece a
        /// otro Cliente </response>
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody]Clientes cliente)
        {
            var clientes = await _client.Cypher.Create("(x:Clientes $cl)")
                                .WithParam("cl", cliente)
                                .Return(x => x.As<Clientes>()).ResultsAsync;

            return Ok();
        }

        /// <summary>
        /// PUT para modificar los atributos de un Cliente
        /// </summary>
        /// <param name="id">
        /// Identificador del Cliente a ser modificado
        /// </param>
        /// <param name="cliente">
        /// Objeto con los nuevos atributos para el Cliente
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     PUT api/Clientes
        ///     {       
        ///       "id": "54"         
        ///       "first_name": "Taylor",
        ///       "last_name": "Andrew"
        ///     }
        /// </remarks>
        
        /// <response code="200">Se actualiza el nuevo Cliente</response>
        /// <response code="500">El identificador ingresado ya pertenece a
        /// otro Cliente </response>
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody]Clientes cliente)
        {
            await _client.Cypher.Match("(x:Clientes)")
                                .Where((Clientes x) => x.id == id)
                                .Set("x = $cl")
                                .WithParam("cl", cliente)
                                .ExecuteWithoutResultsAsync();
            return Ok();
        }

        /// <summary>
        /// DELETE para eliminar a un Cliente y todas sus relaciones
        /// </summary>
        /// <param name="id">
        /// Identificador del Cliente a ser eliminado
        /// </param>
        /// <response code="200">Cliente eliminado satisfactoriamente</response>
        [ProducesResponseType(200)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(x:Clientes)")
                                .Where((Clientes x) => x.id == id)
                                .DetachDelete("x")
                                .ExecuteWithoutResultsAsync();

            // Eliminar compras asociadas al cliente
            await _client.Cypher.Match("(x:Compras)")
                                .Where((Compras x) => x.idCliente == id)
                                .DetachDelete("x")
                                .ExecuteWithoutResultsAsync();
        
            return Ok();
        }

        /// <summary>
        /// Top 5 de Clientes con mas Compras
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     GET api/Clientes/Top5
        ///     [
        ///      {       
        ///       "idCliente": "1",  
        ///       "nombreCliente": "Ollie Dourin",     
        ///       "cantidad": 67
        ///      },
        ///      {       
        ///       "idCliente": "7",  
        ///       "nombreCliente": "Ariana Grande",     
        ///       "cantidad": 59
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpGet("Top5")]
        public async Task<IActionResult> GetTopFiveClientes()
        {
            var clientes = await _client.Cypher.Match("(x:Compras), (c:Clientes)")
                                              .Where((Compras x, Clientes c) => x.idCliente == c.id)
                                              .Return(x => new
                                              {
                                                  idCliente = Return.As<string>("x.idCliente"),
                                                  nombreCliente = Return.As<string>("COALESCE(c.first_name,\"\") + \" \" + COALESCE(c.last_name,\"\")"),
                                                  cantidad = Return.As<int>("SUM(x.cantidad)")
                                              })
                                              .OrderByDescending("cantidad")
                                              .Limit(5).ResultsAsync;

            return Ok(clientes);
        }

        /// <summary>
        /// Busqueda de un Cliente para mostrar sus Compras
        /// </summary>
        /// <param name="nombre">
        /// Nombre del Cliente a ser buscado
        /// </param>
        /// <param name="apellido">
        /// Apellido del Cliente a ser buscado
        /// </param>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     GET api/Clientes/Busqueda/{nombre}/{apellido}
        ///     [
        ///      {       
        ///       "nombreProducto": "Piano",        
        ///       "marcaProducto": "Yamaha",
        ///       "cantidadProducto": 1
        ///      },
        ///      {       
        ///       "nombreProducto": "Wii U",        
        ///       "marcaProducto": "Nintendo",
        ///       "cantidadProducto": 2
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpGet("Busqueda/{nombre}/{apellido}")]
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

        /// <summary>
        /// Clientes Compras en comun
        /// </summary>
        /// <param name="nombreCliente">
        /// Nombre del Cliente deseado
        /// </param>
        /// <param name="apellidoCliente">
        /// Apellido del Cliente deseado
        /// </param>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     GET api/Clientes/CompraComun/{nombre}/{apellido}
        ///     [
        ///      {       
        ///       "nombreCliente": "Ollie",        
        ///       "apellidoCliente": "Dourin",
        ///       "listaProductos": ["Piano", "Wii U"]
        ///      },
        ///      {       
        ///       "nombreCliente": "Ariana",        
        ///       "apellidoCliente": "Grande",
        ///       "listaProductos": ["Piano", "Wii U"]
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpGet("CompraComun/{nombreCliente}/{apellidoCliente}")]
        public  async Task<IActionResult> ClientCommonBuy(string nombreCliente, string apellidoCliente)
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

            List<ComprasComun> lista = new List<ComprasComun>();

            foreach (var item in clientes)
            {
                var productos =  await _client.Cypher.Match("(c:Clientes)-[:clienteCompra]->(x:Compras)<-[:prodCompra]-(p:Productos)-[:prodCompra]->(co:Compras)<-[:clienteCompra]-(a:Clientes)")
                                               .Where("c.first_name=\"" + nombreCliente + 
                                               "\" and c.last_name=\"" + apellidoCliente + 
                                               "\" and a.first_name=\"" + item.nombreCliente + 
                                               "\" and a.last_name=\"" + item.apellidoCliente + "\"")
                                               .Return(x => new
                                              {
                                                nombreProducto = Return.As<string>("p.nombre")                                               
                                              })
                                              .ResultsAsync;

                ComprasComun comprasComun = new ComprasComun();
                comprasComun.nombreCliente = item.nombreCliente;
                comprasComun.apellidoCliente = item.apellidoCliente;
                foreach (var prod in productos)
                {
                    comprasComun.listaProductos.Add(prod.nombreProducto);
                }
                lista.Add(comprasComun);

            }

            var json = JsonConvert.SerializeObject(lista);

            return Ok(json);
        }

        /// <summary>
        /// Clientes y Producto comun
        /// </summary>
        /// <param name="nombreProducto">
        /// Nombre del Producto deseado
        /// </param>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     GET api/Clientes/ProductoComun/{nombreProducto}
        ///     [
        ///      {       
        ///       "nombreCliente": "Ollie",        
        ///       "apellidoCliente": "Dourin"
        ///      },
        ///      {       
        ///       "nombreCliente": "Ariana",        
        ///       "apellidoCliente": "Grande"
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpGet("ProductoComun/{nombreProducto}")]
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