using MinibusTakip.Models;

namespace MinibusTakip.ViewModels
{
    public class YolcuPanelViewModel
    {
        public List<Stop> TumDuraklar { get; set; } = new();
        public int? SeciliStopId { get; set; }
        public string? SeciliStopAd { get; set; }
        public List<MinibusItemViewModel> Minibusler { get; set; } = new();
        public Stop? SeciliStop { get; set; }
    }

    public class MinibusItemViewModel
    {
        public double? Enlem { get; set; }
        public double? Boylam { get; set; }
        public int VehicleId { get; set; }
        public string Plaka { get; set; } = "";
        public string SoforAd { get; set; } = "";
        public string PlanlananSaat { get; set; } = "";
        public string TahminiSaat { get; set; } = "";
        public string? GecikmeAciklama { get; set; }
        public string KonumMetni { get; set; } = "";
        public string? Durum { get; set; }
        public DateTime? SonGuncellemeZamani { get; set; }
    }
}