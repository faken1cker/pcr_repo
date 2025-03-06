using System.Collections.Generic;

namespace powercontrolRNDdesign.psu
{
    public class PsuCfg
    {
        public string setting { get; set; }
        public string regex { get; set; }
        public int baudrate { get; set; }
        public List<ChannelCfg> channel { get; set; }
    }
}
