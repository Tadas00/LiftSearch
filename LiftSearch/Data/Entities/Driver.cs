﻿namespace LiftSearch.Data.Entities;

public class Driver
{
    public int Id { get; set; }
    public required int cancelledCountDriver { get; set; }
    public required DateTime registeredDriverDate { get; set; }
    public DateTime? lastTripDate { get; set; }
    public string? driverBio { get; set; }
    
    public required User user { get; set; }
}

