using MinibusTakip.Models;

namespace MinibusTakip.ViewModels
{
    public class SoforPanelViewModel
    {
        public Vehicle? Minibus { get; set; }
        public VehicleStatus? Durum { get; set; }
        public List<Stop> TumDuraklar { get; set; } = new();
        public List<Schedule> Tarifeler { get; set; } = new();
        public List<DriverMessage> SonMesajlar { get; set; } = new();
    }
}