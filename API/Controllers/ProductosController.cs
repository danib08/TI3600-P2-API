using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;


namespace BDAProy2.Controllers
{
    /// <summary>
    /// Controlador para el modelo Productos
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly IGraphClient _client;

        public ProductosController(IGraphClient client)
        {
            _client = client;
        }

        /// <summary>
        /// GET de todos los Productos existentes
        /// </summary>
        /// <returns> 
        /// Lista de todos los Productos
        /// </returns>
        /// <response code="200">Retorna la lista completa de Productos</response>
        [ProducesResponseType(200)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var productos = await _client.Cypher.Match("(x:Productos)")
                .Return(x => x.As<Productos>()).ResultsAsync;

            return Ok(productos);
        }

        /// <summary>
        /// GET de un Producto segun su identificador
        /// </summary>
        /// <param name="id">
        /// El identificador del Producto que se desea obtener
        /// </param>
        /// <returns> El objeto Producto deseado </returns>
        /// <response code="200">Retorna el Producto deseado</response>
        /// <response code="404">No existe un Producto con el identificador ingresado</response>
        [HttpGet("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(int id)
        {
            var productos = await _client.Cypher.Match("(x:Productos)")
                                               .Where((Productos x) => x.id == id)
                                               .Return(x => x.As<Productos>()).ResultsAsync;

            if (productos.Count() != 0) {
                return Ok(productos.LastOrDefault());
            }
            else {
                return NotFound();
            }
        }

        /// <summary>
        /// POST para crear un nuevo Producto
        /// </summary>
        /// <param name="producto">
        /// Objeto Producto a ser creado
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     POST api/Productos
        ///     {       
        ///       "id": "54",        
        ///       "nombre": "Piano",
        ///       "marca": "Yamaha",
        ///       "precio: 100
        ///     }
        /// </remarks>
        /// <response code="200">Se crea el nuevo Producto</response>
        /// <response code="500">El identificador ingresado ya pertenece a
        /// otro Producto </response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Productos producto)
        {
            await _client.Cypher.Create("(x:Productos $prod)")
                                .WithParam("prod", producto)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        /// <summary>
        /// PUT para modificar los atributos de un Producto
        /// </summary>
        /// <param name="id">
        /// Identificador del Producto a ser modificado
        /// </param>
        /// <param name="producto">
        /// Objeto con los nuevos atributos para el Producto
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     PUT api/Productos
        ///     {       
        ///       "id": "54",      
        ///       "nombre": "Piano",
        ///       "marca": "Yamaha",
        ///       "precio: 75
        ///     }
        /// </remarks>
        /// <response code="200">Se actualiza el nuevo Producto</response>
        /// <response code="500">El identificador ingresado ya pertenece a
        /// otro Producto </response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Productos producto)
        {
            await _client.Cypher.Match("(x:Productos)")
                                .Where((Productos x) => x.id == id)
                                .Set("x = $prod")
                                .WithParam("prod", producto)
                                .ExecuteWithoutResultsAsync();

            return Ok();
        }

        /// <summary>
        /// DELETE para eliminar a un Producto y todas sus relaciones
        /// </summary>
        /// <param name="id">
        /// Identificador del Producto a ser eliminado
        /// </param>
        /// <response code="200">Producto eliminado satisfactoriamente</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _client.Cypher.Match("(x:Productos)")
                                .Where((Productos x) => x.id == id)
                                .DetachDelete("x")
                                .ExecuteWithoutResultsAsync();
            
            // Eliminar compras asociadas al producto
            await _client.Cypher.Match("(x:Compras)")
                                .Where((Compras x) => x.idProducto == id)
                                .DetachDelete("x")
                                .ExecuteWithoutResultsAsync();
        
            return Ok();
        }

        /// <summary>
        /// Top 5 de Productos mas vendidos
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        /// 
        ///     GET api/Productos/Top5
        ///     [
        ///      {       
        ///       "idProducto": 54,  
        ///       "nombreProducto": "Piano",     
        ///       "cantidad": 100
        ///      },
        ///      {       
        ///       "idProducto": 7,  
        ///       "nombreProducto": "Guitarra",     
        ///       "cantidad": 85
        ///      }
        ///     ]
        /// </remarks>
        /// <returns></returns>
        [HttpGet("Top5")]
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
    }
}
