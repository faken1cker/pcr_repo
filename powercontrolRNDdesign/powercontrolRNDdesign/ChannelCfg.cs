namespace powercontrolRNDdesign.psu
{
    public class ChannelCfg
    {
        public int id { get; set; }
        public string usage { get; set; }
        public double defaultVout { get; set; }
        public double defaultImax { get; set; }
        public bool defaultOn { get; set; }
    }
}
