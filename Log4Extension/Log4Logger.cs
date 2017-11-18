using System;
using log4net.Core;
using Microsoft.Extensions.Logging;

namespace Log4Extension
{
    /// <summary>
    /// log4net日志组件
    /// </summary>
    public class Log4Logger : Microsoft.Extensions.Logging.ILogger, IDisposable
    {
        //log4日志组件实例
        private log4net.Core.ILogger _logger;
        private Func<string, LogLevel, bool> _filter;

        /// <summary>
        /// 日志消息过滤器
        /// </summary>
        /// <value>The filter.</value>
        public Func<string, LogLevel, bool> Filter
        {
            get
            {
                return this._filter;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }
                this._filter = value;
            }
        }

        /// <summary>
        /// 是否包含作用域
        /// </summary>
        /// <value><c>true</c> if include scopes; otherwise, <c>false</c>.</value>
        public bool IncludeScopes
        {
            get;
            set;
        }

        /// <summary>
        /// 本Logger的名称空间
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// 初始化一个Log4net组件
        /// </summary>
        public Log4Logger(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.Name = name;
            //从log4net管理组件中获取日志组件
            //是否创建新实例由传入的名称决定

            try
            {
                this._logger = LoggerManager.GetLogger(Log4LoggerProvider.REPOSITORY_NAME, name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 使用指定的信息初始化日志组件
        /// </summary>
        /// <param name="name">日志组件名称空间.</param>
        /// <param name="filter">过滤器.</param>
        /// <param name="includeScopes">是否包含作用域.</param>
        public Log4Logger(string name, Func<string, LogLevel, bool> filter, bool includeScopes)
            : this(name)
        {
            this.Filter = filter;
            this.IncludeScopes = includeScopes;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return this;
        }

        /// <summary>
        /// 该方法是MS ILogger的成员，用于判定是否需要过滤指定级别的日志，Log4net不负责级别处理
        /// </summary>
        /// <returns><c>true</c>, if enabled was ised, <c>false</c> otherwise.</returns>
        /// <param name="logLevel">Log level.</param>
        public bool IsEnabled(LogLevel logLevel)
        {
            //日志级别控制由MS.Logging类进行，Log4net不再处理，接到即表示需要输出
            return this.Filter.Invoke(this.Name, logLevel);
        }

        /// <summary>
        /// 日志记录方法
        /// </summary>
        /// <param name="logLevel">日志级别.</param>
        /// <param name="eventId">日志事件编号.</param>
        /// <param name="state">State.</param>
        /// <param name="exception">Exception.</param>
        /// <param name="formatter">Formatter.</param>
        /// <typeparam name="TState">The 1st type parameter.</typeparam>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                //暂时还为完成Provider名称与配置文件中名称的映射，此处还是需要多一层判断
                return;
            }

            this._logger.Log(null,      //日志调研堆栈的边界类型，即调用者
                             LevelMapping(logLevel),    //日志级别
                             formatter.Invoke(state, exception),         //日志的内容 
                             exception);        //如果是异常日志则传入异常对象
        }

        /// <summary>
        /// 回收日志组件资源
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:DIDemo.Tools.Log4Logger"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:DIDemo.Tools.Log4Logger"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:DIDemo.Tools.Log4Logger"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:DIDemo.Tools.Log4Logger"/> was occupying.</remarks>
        public void Dispose()
        {
            //回收Log4net日志组件
            LoggerManager.Exists(this._logger.Repository.Name, this._logger.Name);
        }

        /// <summary>
        /// log4net日志级别与MS Logging日志级别映射
        /// </summary>
        /// <returns>log4net日志级别.</returns>
        /// <param name="msLogLevel">MS Logging日志级别.</param>
        private Level LevelMapping(LogLevel msLogLevel)
        {
            Level log4level = Level.Verbose;

            switch (msLogLevel)
            {
                case LogLevel.None:
                    log4level = Level.Verbose;
                    break;
                case LogLevel.Trace:
                    log4level = Level.Trace;
                    break;
                case LogLevel.Debug:
                    log4level = Level.Debug;
                    break;
                case LogLevel.Information:
                    log4level = Level.Info;
                    break;
                case LogLevel.Warning:
                    log4level = Level.Warn;
                    break;
                case LogLevel.Error:
                    log4level = Level.Error;
                    break;
                case LogLevel.Critical:
                    log4level = Level.Critical;
                    break;
                default:
                    log4level = Level.Verbose;
                    break;
            }

            return log4level;
        }
    }
}
