using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using pGina.Shared.Settings;
using log4net;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace pGina.Plugin.Cbits
{
    class Settings
    {
        private static dynamic m_settings = new pGinaDynamicSettings(PluginImpl.PluginUuid);
        private static ILog m_logger = LogManager.GetLogger("CbitsSettings");
        private const string url = "https://pgina.cbits.net/api/v1/auth/";
        private static string DEFAULT_DOMAIN = "cbits.net";

        static Settings()
        {
            try
            {
                m_settings.SetDefault("Domain", @DEFAULT_DOMAIN);
            }
            catch (Exception)
            {
                // do nothing
            }
        }

        public static dynamic Store
        {
            get { return m_settings; }
        }

        public static string resolveSettings()
        {
            string domain = _urlByEnvVar();
            if (domain == null)
            {
                // try to get URL from DNS
                try
                {
                    domain = m_settings.Domain;
                    m_logger.DebugFormat("Login domain from GinaSettings: {0}", domain);
                }
                catch (KeyNotFoundException)
                {
                    domain = DEFAULT_DOMAIN;
                    m_logger.DebugFormat("Default domain server url: {0}", domain);
                }
            }
            else
            {
                m_logger.DebugFormat("Default domain from ENVVar: {0}", domain);
                _persist(domain);
            }
            return url + domain;
        }

        private static void _persist(string domain)
        {
            try
            {
                m_settings.SetSetting("Domain", domain);
            }
            catch (Exception e)
            {
                m_logger.ErrorFormat("Cannot save settings: {0}", e.ToString());
            }
        }

        /*
         * returns CBITSLOGINDOMAIN environment variable content if set, otherwise null.
         * Setting by environment variable allows easy override of login endpoint address.
         */
        private static string _urlByEnvVar()
        {
            try
            {
                return Environment.GetEnvironmentVariable("CBITSLOGINDOMAIN");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
