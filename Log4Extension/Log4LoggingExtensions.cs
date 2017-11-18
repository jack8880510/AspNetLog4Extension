using System;
using System.IO;
using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Log4Extension
{
    /// <summary>
    /// 用于扩展ILoggingBuilder使其支持log4net
    /// </summary>
    public static class Log4LoggingExtensions
    {
        /// <summary>
        /// 为ILoggingBuilder添加扩展方法，实现对log4net日志提供器的支持
        /// </summary>
        /// <param name="loggerBuilder">Logger builder.</param>
        /// <param name="configPath">log4net日志文件的地址.</param>
        /// <param name="sourceFilterFunc">内容过滤器，以命名控件进行过滤.</param>
        public static ILoggingBuilder AddLog4net(this ILoggingBuilder loggerBuilder, string configPath = "log4net.config")
        {
            //将依赖注入关系注册到IOC容器
            ServiceCollectionServiceExtensions.AddSingleton<ILoggerProvider, Log4LoggerProvider>(loggerBuilder.Services);
            return loggerBuilder;
        }

        /// <summary>
        /// 向日志工厂扩展Log4net组件提供器
        /// </summary>
        /// <returns>Core日志工厂.</returns>
        /// <param name="factory">Core日志工厂.</param>
        /// <param name="configuration">Core日志组件.</param>
        public static ILoggerFactory AddLog4net(this ILoggerFactory factory, IConfiguration configuration)
        {
            //使用设置对象创建组件提供程序
            return factory.AddLog4net(new ConfigurationLog4LoggerSettings(configuration));
        }

        /// <summary>
        /// 向日志工厂扩展Log4net组件提供器
        /// </summary>
        /// <returns>Core日志工厂.</returns>
        /// <param name="factory">Core日志工厂.</param>
        /// <param name="settings">设置对象.</param>
        public static ILoggerFactory AddLog4net(this ILoggerFactory factory, ILog4LoggerSettings settings)
        {
            factory.AddProvider(new Log4LoggerProvider(settings));
            return factory;
        }
    }
}
