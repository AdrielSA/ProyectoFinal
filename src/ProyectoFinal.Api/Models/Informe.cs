namespace ProyectoFinal.Api.Models
{
    public class Informe
    {
        public string Ganador { get; set; }
        public int TotalVotos { get; set; }
        public List<Dictionary<string, int>> VotosPorGenero { get; set; }
        public List<Dictionary<string, double>> PorcentajePorCandidato { get; set; }
    }
}
