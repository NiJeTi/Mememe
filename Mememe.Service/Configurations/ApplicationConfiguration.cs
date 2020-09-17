using System;

namespace Mememe.Service.Configurations
{
    public class ApplicationConfiguration
    {
        public int ContentAmount { get; set; } = 1;
        public TimeSpan RepeatEvery { get; set; } = TimeSpan.FromMinutes(1);
    }
}