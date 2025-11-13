using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PostNamazu.Common
{
    public delegate void OnExceptionEventHandler(Exception ex);
    
    internal class HttpServer : IDisposable
    {
        // 保持原始成员变量顺序，新增取消令牌相关变量
        private Task _serverTask;  // 替代原_thread，位置对应原始_serverThread
        private HttpListener _listener;  // 保持原始位置
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();  // 新增取消源

        public int Port { get; private set; }  // 保持原始位置

        public Action<string, string> PostNamazuDelegate = null;  // 保持原始位置
        public event OnExceptionEventHandler OnException;  // 保持原始位置

        #region Init  // 保留原始区域名
        /// <summary>
        /// 在指定端口启动监听
        /// </summary>
        /// <param name="port">要启动的端口</param>
        public HttpServer(int port)
        {
            Initialize(port);  // 保持原始调用逻辑
        }

        /// <summary>
        /// 在随机端口启动监听
        /// </summary>
        public HttpServer()
        {
            // 保持原始随机端口获取逻辑
            var l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            Initialize(port);
        }

        /// <summary>
        /// 初始化并启动监听
        /// </summary>
        /// <param name="port">监听的端口</param>
        private void Initialize(int port)
        {
            Port = port;  // 保持原始逻辑
            // 替换原始线程启动为任务启动，逻辑位置对应
            _serverTask = ListenAsync(_cts.Token);
        }

        /// <summary>
        /// 停止监听并释放资源
        /// </summary>
        public void Stop()
        {
            // 替换原始停止逻辑为取消令牌触发，保持方法位置
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
            _serverTask?.Wait(5000);  // 等待任务结束，对应原始线程终止
            Dispose();
        }
        #endregion


        // 保持Listen方法在Init区域之后，位置对应原始
        private async Task ListenAsync(CancellationToken cancellationToken)
        {
            try
            {
                // 保持监听器初始化逻辑，位置对应原始
                _listener = new HttpListener();
                _listener.Prefixes.Add("http://*:" + Port + "/");
                _listener.Start();
            }
            catch (Exception ex)
            {
                OnException?.Invoke(ex);  // 保持异常触发逻辑
                return;
            }

            // 注册取消回调，对应原始循环退出逻辑
            cancellationToken.Register(() => _listener?.Stop());

            try
            {
                // 保持循环监听逻辑，替换为异步+取消令牌
                while (!cancellationToken.IsCancellationRequested)
                {
                    HttpListenerContext context;
                    try
                    {
                        // 替换原始GetContext为异步版本
                        context = await _listener.GetContextAsync().ConfigureAwait(false);
                    }
                    catch (HttpListenerException ex) when (ex.ErrorCode == 995)
                    {
                        // 处理监听器停止异常，对应原始循环退出
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(ex);  // 保持异常处理位置
                        continue;
                    }

                    // 替换原始线程池处理为任务，逻辑对应
                    _ = Task.Run(() => DoAction(context), cancellationToken);
                }
            }
            finally
            {
                _listener?.Stop();  // 保持最终释放逻辑
            }
        }

        /// <summary>
        /// 根据HTTP请求内容执行对应的指令
        /// </summary>
        /// <param name="context">HTTP请求内容</param>
        // 保持DoAction方法在Listen之后，参数去掉ref（原始ref无必要）
        private void DoAction(HttpListenerContext context)
        {
            // 保持原始请求处理逻辑
            var payload = new StreamReader(context.Request.InputStream, Encoding.UTF8).ReadToEnd();

            PostNamazuDelegate?.Invoke(TrimUrl(context.Request.Url.AbsolutePath), payload);

            var buf = Encoding.UTF8.GetBytes(payload);
            context.Response.ContentLength64 = buf.Length;
            context.Response.OutputStream.Write(buf, 0, buf.Length);
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.OutputStream.Flush();
        }

        // 保持TrimUrl方法在最后，逻辑不变
        public string TrimUrl(string url)
        {
            return url.Trim(new char[] { '/' });
        }

        // 新增Dispose接口实现（放在最后，不影响原始结构顺序）
        public void Dispose()
        {
            _cts.Dispose();
            _listener?.Close();
        }
    }
}
