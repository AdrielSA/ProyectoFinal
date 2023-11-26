using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ProyectoFinal.Api.Models;

namespace ProyectoFinal.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CandidatosController : ControllerBase
    {
        /// <summary>
        /// Este método recibe un archivo de Excel con los datos de los candidatos y devuelve un informe con los resultados de la votación.
        /// </summary>
        /// <param name="excel">Archivo en formato excel que contiene la lista de candidatos</param>
        /// <returns>Devuelve el informe requerido en base a los candidatos</returns>
        [HttpPost("ObtenerInformeCandidatos")]
        public IActionResult Post([FromForm]IFormFile excel)
        {
            try
            {
                // Verificar si el archivo es nulo y tiene la extensión correcta
                if (excel == null || Path.GetExtension(excel.FileName) != ".xlsx")
                    return BadRequest("No se ha enviado el archivo o el formato es incorrecto.");

                // Obtener la lista de candidatos en base al excel
                var candidatos = ObtenerCandidatos(excel);

                int totalVotos = candidatos.Sum(c => c.Votos);
                int votosHombres = candidatos.Where(c => c.Sexo == "M").Sum(c => c.Votos);
                int votosMujeres = candidatos.Where(c => c.Sexo == "F").Sum(c => c.Votos);
                var candidatoGanador = candidatos.OrderByDescending(c => c.Votos).FirstOrDefault();

                // Crear el objeto Informe
                var informe = new Informe
                {
                    Ganador = candidatoGanador.Nombre,
                    TotalVotos = totalVotos,
                    VotosPorGenero =
                    [
                        new Dictionary<string, int> { { "Hombres", votosHombres } },
                        new Dictionary<string, int> { { "Mujeres", votosMujeres } }
                    ],
                    PorcentajePorCandidato = candidatos.Select(c => new Dictionary<string, double>
                    {
                        { c.Nombre, Math.Round((c.Votos / (double)totalVotos) * 100, 2) }
                    }).ToList()
                };

                return Ok(informe);
            }
            catch (Exception ex)
            {
                // Devolver un error 500 con el mensaje de error en caso de que ocurra una excepción
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        
        [ApiExplorerSettings(IgnoreApi = true)]
        public List<Candidato> ObtenerCandidatos(IFormFile excel)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var listaCandidatos = new List<Candidato>();

            // Leer el archivo Excel
            using (var stream = excel.OpenReadStream())
            {
                using (var package = new ExcelPackage(stream))
                {
                    // Suponiendo que los datos están en la primera hoja
                    var worksheet = package.Workbook.Worksheets[0];

                    // Iterar sobre las filas y crear objetos Candidato
                    for (int i = 2; i < worksheet.Dimension.Rows; i++)
                    {
                        var nombre = worksheet.Cells[i, 1].Text;
                        var votos = worksheet.Cells[i, 2].Text;
                        var sexo = worksheet.Cells[i, 3].Text;

                        // Validar que los datos no estén vacíos
                        if (!string.IsNullOrEmpty(nombre) && 
                            !string.IsNullOrEmpty(votos) && !string.IsNullOrEmpty(sexo))
                        {
                            // Crear el objeto Candidato y agregarlo a la lista
                            var candidato = new Candidato
                            {
                                Nombre = nombre,
                                Votos = int.Parse(votos),
                                Sexo = sexo
                            };
                            listaCandidatos.Add(candidato);
                        }
                    }
                }
            }
            return listaCandidatos;
        }
    }
}
