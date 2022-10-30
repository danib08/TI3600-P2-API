using API.Models;
using Microsoft.AspNetCore.Mvc;
using Neo4jClient;
using Neo4jClient.Cypher;

namespace BDAProy2.Controllers {

    /// <summary>
    /// Controlador para la carga de los archivos .csv
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CargaController : ControllerBase{

        private readonly IGraphClient _client;

        public CargaController(IGraphClient client)
        {
            _client = client;
        }


        /// <summary>
        /// Carga de archivos a la base de datos
        /// </summary>
        /// <param name="link">
        /// Objeto JSON con enlaces a los .csv hosteados en la web
        /// </param>
        /// <remarks>
        /// Ejemplo de body:
        /// 
        ///     POST api/Carga/
        ///     {       
        ///      "clientesLink": "https://docs.google.com/spreadsheets/d/e/2PACX,
        ///      "productosLink": "https://docs.google.com/spreadsheets/d/e/2PACX,
        ///      "marcasLink": "https://docs.google.com/spreadsheets/d/e/2PACX,
        ///      "comprasLink": "https://docs.google.com/spreadsheets/d/e/2PACX
        ///     }
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Load([FromBody]Link link)
        {
            Uri uriClientes = new Uri(link.clientesLink);
            await _client.Cypher.LoadCsv(uriClientes, "csvLine", true)
                                .Create("(c:Clientes {id: toInteger(csvLine.id), first_name: csvLine.first_name, last_name: csvLine.last_name})")
                                .ExecuteWithoutResultsAsync();
            

            Uri uriProductos = new Uri(link.productosLink);
            await _client.Cypher.LoadCsv(uriProductos, "csvLine", true)
                                .Create("(p:Productos {id: toInteger(csvLine.id), nombre: csvLine.nombre, marca: csvLine.marca, precio: toInteger(csvLine.precio)})")
                                .ExecuteWithoutResultsAsync();

            Uri uriMarcas = new Uri(link.marcasLink);
            await _client.Cypher.LoadCsv(uriMarcas, "csvLine", true)
                                .Create("(c:Marcas {id: toInteger(csvLine.id), nombre: csvLine.nombre, pais: csvLine.pais})")
                                .ExecuteWithoutResultsAsync();

            Uri uriCompras = new Uri(link.comprasLink);
            await _client.Cypher.LoadCsv(uriCompras, "csvLine", true)
                                .Create("(c:Compras {idCliente: toInteger(csvLine.idCliente), idProducto: toInteger(csvLine.idProducto), cantidad: toInteger(csvLine.cantidad)})")
                                .ExecuteWithoutResultsAsync();

            await _client.Cypher.Match("(p:Productos), (m:Marcas)")
                                .Where("p.marca = m.nombre")
                                .Create("(p)-[r:esMarca]->(m)")
                                .ExecuteWithoutResultsAsync();

            await _client.Cypher.Match("(cl:Clientes), (c:Compras)")
                                .Where("cl.id = c.idCliente")
                                .Create("(cl)-[r:clienteCompra]->(c)")
                                .ExecuteWithoutResultsAsync();

            await _client.Cypher.Match("(p:Productos), (c:Compras)")
                                .Where("p.id = c.idProducto")
                                .Create("(p)-[r:prodCompra]->(c)")
                                .ExecuteWithoutResultsAsync();            
            return Ok();
        }
    }
}