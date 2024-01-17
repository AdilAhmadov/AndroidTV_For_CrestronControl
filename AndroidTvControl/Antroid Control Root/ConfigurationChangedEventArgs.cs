using System;

namespace AndroidTvControl.Antroid_Control_Root
{
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public AndroidTVConfiguraton Configuration { get; private set; }

        public ConfigurationChangedEventArgs(AndroidTVConfiguraton configuration)
        {
            this.Configuration = configuration;
        }
    }
}
