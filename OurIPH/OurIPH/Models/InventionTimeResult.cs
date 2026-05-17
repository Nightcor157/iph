namespace OurIPH.Models
{
    public sealed class InventionTimeResult
    {
        public double CopyTimeSeconds { get; set; }
        public double InventionTimeSeconds { get; set; }

        public double ExtraTimeSeconds
        {
            get { return CopyTimeSeconds + InventionTimeSeconds; }
        }
    }
}
