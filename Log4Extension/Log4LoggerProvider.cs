using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Log4Extension
{
    /// <summary>
    /// 日志组件提供器，需要注册到LoggingFactory来使用
    /// </summary>
    public class Log4LoggerProvider : ILoggerProvider, IDisposable
    {
        /// <summary>
        /// log4net配置仓库的名称
        /// </summary>
        public const string REPOSITORY_NAME = "NETCoreRepository";

        /// <summary>
        /// 默认过滤器
        /// </summary>
        private static readonly Func<string, LogLevel, bool> trueFilter = (arg1, arg2) => true;

        static Log4LoggerProvider()
        {
            //创建Log4net配置仓库
            LoggerManager.CreateRepository(REPOSITORY_NAME);
        }

        /// <summary>
        /// 该日志提供器所产出的日志组件集合
        /// </summary>
        private readonly ConcurrentDictionary<string, Log4Logger> _loggers = new ConcurrentDictionary<string, Log4Logger>();
        private ILog4LoggerSettings _settings;

        /// <summary>
        /// 使用指Core配置对象初始化提供器
        /// </summary>
        /// <param name="configuration">Core配置对象.</param>
        public Log4LoggerProvider(IConfiguration configuration)
            : this(new Log4LoggerSettings(configuration))
        {
        }

        /// <summary>
        /// 初始化日志组件提供器
        /// </summary>
        public Log4LoggerProvider(ILog4LoggerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            this._settings = settings;
            //获取Log4net配置仓库
            var logRps = LoggerManager.GetRepository(REPOSITORY_NAME);
            //根据路径中的配置文件来初始化仓库，如果文件不存在不会报错，但不会记录日志文件
            XmlConfigurator.Configure(logRps, new FileInfo(this._settings.Log4netConfigPath));
            if (this._settings.ChangeToken != null)
            {
                //订阅配置文件变更事件，重新初始化配置对象
                this._settings.ChangeToken.RegisterChangeCallback(new Action<object>(this.OnConfigurationReload), null);
            }
        }

        /// <summary>
        /// 创建新的日志组件
        /// </summary>
        /// <returns>日志组件实例.</returns>
        /// <param name="name">日志组件名称.</param>
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string name)
        {
            //根据组件名称创建或获取一个日志组件
            return _loggers.GetOrAdd(name, new Func<string, Log4Logger>(this.CreateLoggerImplementation));
        }

        /// <summary>
        /// 用于创建日志组件
        /// </summary>
        /// <returns>日志组件.</returns>
        /// <param name="name">日志组件的名称空间.</param>
        private Log4Logger CreateLoggerImplementation(string name)
        {
            bool includeScopes = (this._settings != null) ? this._settings.IncludeScopes : false;
            return new Log4Logger(name, this.GetFilter(name, this._settings), includeScopes);
        }


        /// <summary>
        /// 回收日志组件提供器的资源
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:DIDemo.Tools.Log4LoggerProvider"/>.
        /// The <see cref="Dispose"/> method leaves the <see cref="T:DIDemo.Tools.Log4LoggerProvider"/> in an unusable
        /// state. After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:DIDemo.Tools.Log4LoggerProvider"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:DIDemo.Tools.Log4LoggerProvider"/> was occupying.</remarks>
        public void Dispose()
        {
            //清空所有日志组件，转为非托管状态
            _loggers.Clear();
        }

        /// <summary>
        /// 根据名称空间，从设置对象中获取对应的过滤器
        /// </summary>
        /// <returns>过滤器信息.</returns>
        /// <param name="name">需要获取过滤器的名称空间.</param>
        /// <param name="settings">设置对象.</param>
        private Func<string, LogLevel, bool> GetFilter(string name, ILog4LoggerSettings settings)
        {
            if (settings == null)
            {
                //如果没有相关设置，则默认全部允许记录
                return Log4LoggerProvider.trueFilter;
            }

            //分解命名空间，逐个寻找对应的过滤器
            foreach (var partName in this.GetKeyPrefixes(name))
            {
                LogLevel level;
                if (settings.TryGetSwitch(partName, out level))
                {
                    //如果在配置中存在对应的名称空间的配置，则使用该配置的级别来进行验证
                    return (string n, LogLevel l) => l >= level;
                }
            }

            //没有找到过滤器则也使用全部通过的过滤器
            return Log4LoggerProvider.trueFilter;
        }

        /// <summary>
        /// 根据命名空间整个继承链中可用的名称信息
        /// </summary>
        /// <returns>可用名称集合.</returns>
        /// <param name="name">需要分析的名称空间.</param>
        private IEnumerable<string> GetKeyPrefixes(string name)
        {
            //如果名称存在则开始解析名称
            while (!string.IsNullOrEmpty(name))
            {
                //返回名称给迭代器
                yield return name;

                //获取名称中.运算符的位置
                int num = name.LastIndexOf('.');
                if (num == -1)
                {
                    //如果.运算符不存在则返回一个Default
                    yield return "Default";
                    //结束迭代
                    break;
                }
                //根据.来截断字符串
                name = name.Substring(0, num);
            }
        }

        /// <summary>
        /// 处理配置改变事件
        /// </summary>
        /// <param name="state">State.</param>
        private void OnConfigurationReload(object state)
        {
            try
            {
                //重新载入日志设置信息
                this._settings = this._settings.Reload();

                //获取includeScopes设置
                bool includeScopes = this._settings != null && this._settings.IncludeScopes;

                //迭代所有的日志器
                foreach (var logger in this._loggers.Values)
                {
                    logger.Filter = this.GetFilter(logger.Name, this._settings);
                    logger.IncludeScopes = includeScopes;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Error while loading configuration changes.{0}{1}", Environment.NewLine, ex));
            }
            finally
            {
                if (((this._settings != null) ? this._settings.ChangeToken : null) != null)
                {
                    this._settings.ChangeToken.RegisterChangeCallback(new Action<object>(this.OnConfigurationReload), null);
                }
            }
        }
    }
}
