namespace AndroidTvControl.Configuration_Manager
{
    public class TVEntity
    {
        public int TvId { get; set; }
        public string RoomName { get; set; }
        public string IpAddress { get; set; }
        public string Mac { get; set; }
        public string Certificate { get; set; }
        public bool IsPaired { get; set; }
    }
    public class TVApp
    {
        public int AppId { get; set; }
        public string AppName { get; set; }
        public string Url { get; set; }
    }
}
