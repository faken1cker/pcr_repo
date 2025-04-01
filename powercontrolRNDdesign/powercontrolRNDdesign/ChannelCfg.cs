namespace powercontrolRNDdesign.psu
{
    // Represents a single PSU channel's configuration settings.
    // Another dev reading this file should note how each channel's usage,
    // default voltage/current, and defaultOn state are stored here,
    // then applied in Controller or ControllerCmd.
    public class ChannelCfg
    {
        public int id { get; set; }         // Numeric identifier for the channel, e.g. 1..4
        public string usage { get; set; }   // Brief label describing how this channel is used (e.g., "vocom")
        public double defaultVout { get; set; } // Default voltage to apply at startup or applySetting
        public double defaultImax { get; set; } // Default current limit for the channel
        public bool defaultOn { get; set; }      // If true, channel is enabled by default (at startup or applySetting)
    }
}
