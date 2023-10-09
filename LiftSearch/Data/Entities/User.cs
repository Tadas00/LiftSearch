using LiftSearch.Data.Entities.Enums;

namespace LiftSearch.Data.Entities;

public class User
{
    public int Id { get; set; }
    
    public required string name { get; set; }
    public required string lastname { get; set; }
    public required string email { get; set; }
    public required string phone { get; set; }
    
    public required DriverStatus driverStatus { get; set; }
    
    public required DateTime registrationDate { get; set; }
    public required int cancelledCountTraveler { get; set; }
}

