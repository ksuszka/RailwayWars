﻿namespace RailwayWars.Contracts
{
    public class PlayerPublicInfoDTO
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int TotalScore { get; set; }
        public int? LastCommandTimeMilliseconds { get; set; }
    }
}
