﻿namespace CoreRankingInfra.Model;

public record QuadraKill : Multikill
{
    public double Time { get; set; }
    public int Points { get; set; }
    public List<string> Messages { get; set; }
}