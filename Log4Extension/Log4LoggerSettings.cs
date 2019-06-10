using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Log4Extension
{
    public class Log4LoggerSettings : ILog4LoggerSettings
    {
        //
        // Fields
        //
        private readonly IConfiguration _configuration;

        //
        // Properties
        //
        public IChangeToken ChangeToken
        {
            get;
            private set;
        }

        public bool IncludeScopes
        {
            get
            {
                string text = this._configuration.GetSection("Logging").GetValue<string>("IncludeScopes");
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                bool result;
                if (bool.TryParse(text, out result))
                {
                    return result;
                }
                throw new InvalidOperationException(string.Format("Configuration value '{0}' for setting '{1}' is not supported.", text, "IncludeScopes"));
            }
        }

        public string Log4netConfigPath
        {
            get
            {
                
                string text = this._configuration.GetSection("Logging:Log4").GetValue<string>("ConfigPath");
                if (string.IsNullOrEmpty(text) || (text != null && text.ToUpper() == "DEFAULT"))
                {
                    //获取当前程序集所在的目录位置
                    FileInfo assemblyFile = new FileInfo(this.GetType().Assembly.Location);

                    //默认情况下采用本程序集编译产生的配置文件，可以拷贝该文件到项目任何其他目录，然后在appsetting.json中单独配置
                    return assemblyFile.DirectoryName + "/log4net.config";
                }
                return text;
            }
        }

        //
        // Constructors
        //
        public Log4LoggerSettings(IConfiguration configuration)
        {
            this._configuration = configuration;
            this.ChangeToken = configuration.GetReloadToken();
        }

        //
        // Methods
        //
        public ILog4LoggerSettings Reload()
        {
            this.ChangeToken = null;
            return new Log4LoggerSettings(this._configuration);
        }

        public bool TryGetSwitch(string name, out LogLevel level)
        {
            IConfigurationSection section = this._configuration.GetSection("Logging:Log4:LogLevel");
            if (section == null)
            {
                level = LogLevel.None;
                return false;
            }
            string text = section.GetValue<string>(name);
            if (string.IsNullOrEmpty(text))
            {
                level = LogLevel.None;
                return false;
            }
            if (Enum.TryParse<LogLevel>(text, true, out level))
            {
                return true;
            }
            throw new InvalidOperationException(string.Format("Configuration value '{0}' for category '{1}' is not supported.", text, name));
        }
    }
}
