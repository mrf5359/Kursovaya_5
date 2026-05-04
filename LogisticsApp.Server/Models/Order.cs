namespace LogisticsApp.Server.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string BriefDescription { get; set; } = string.Empty;

        // Данные по водному транспорту
        public string WatercraftModel { get; set; } = string.Empty;
        public string WatercraftNumber { get; set; } = string.Empty;

        // Детали по маршруту та грузу
        public DateTime? DateStart { get; set; }
        public DateTime? DateFinish { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string TypeOfCargo { get; set; } = string.Empty;
        public double WeightTons { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public double RouteLengthKm { get; set; }

        // Соляра
        public double FuelOnStart { get; set; }
        public double FuelOnFinish { get; set; }
        public double FuelConsumption { get; set; }
        public int NumberOfRefills { get; set; }
        public double AmountOfFuelPoured { get; set; }

        //Экипаж (Один-к-Многим)
        public List<CrewMember> Crew { get; set; } = new();
    }
    // Люди
    public class CrewMember
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}