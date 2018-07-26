﻿
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OSS.Http.Mos;

namespace OSS.Http.Extention
{
    /// <summary>
    ///  请求基类
    /// </summary>
    public static class HttpClientExtention
    {
        private const string _lineBreak = "\r\n";
        /// <summary>
        ///   编码格式
        /// </summary>
        public static Encoding Encoding { get; set; } = Encoding.UTF8;
        //private static readonly Dictionary<string,Action<HttpContentHeaders,string>> _notCanAddContentHeaderDics
        //    =new Dictionary<string, Action<HttpContentHeaders, string>>();

        #region   扩展方法

        /// <summary>
        ///  执行请求方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> RestSend(this HttpClient client, OsHttpRequest request)
        {
            return RestSend(client, request, HttpCompletionOption.ResponseContentRead, CancellationToken.None);
        }
        
        /// <summary>
        ///  执行请求方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="completionOption"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> RestSend(this HttpClient client, OsHttpRequest request,
            HttpCompletionOption completionOption)
        {
           return  RestSend(client, request, completionOption, CancellationToken.None);
        }
        
        /// <summary>
        ///  执行请求方法
        /// </summary>
        /// <param name="client"></param>
        /// <param name="request"></param>
        /// <param name="completionOption"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task<HttpResponseMessage> RestSend(this HttpClient client, OsHttpRequest request,
            HttpCompletionOption completionOption ,
            CancellationToken cancellationToken)
        {
            var reqMsg = ConfigureReqMsg(request);

            if (request.TimeOutMilSeconds > 0)
                client.Timeout = TimeSpan.FromMilliseconds(request.TimeOutMilSeconds);
            
            return client.SendAsync(reqMsg, completionOption, cancellationToken);
        }


        #endregion

        #region  配置 ReqMsg信息

        /// <summary>
        /// 配置请求
        /// </summary>
        /// <returns></returns>
        public static HttpRequestMessage ConfigureReqMsg(OsHttpRequest request)
        {
            var reqMsg = new HttpRequestMessage
            {
                RequestUri = string.IsNullOrEmpty(request.AddressUrl) ? request.Uri : new Uri(request.AddressUrl),
                Method = request.HttpMethod
            };
            ConfigReqContent(reqMsg, request); //  配置内容
            return reqMsg;
        }
        

        /// <summary>
        ///  配置使用的cotent
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        private static void ConfigReqContent(HttpRequestMessage reqMsg, OsHttpRequest req)
        {
            if (req.HttpMethod == HttpMethod.Get)
            {
                req.RequestSet?.Invoke(reqMsg);
                return;
            }

            if (req.HasFile)
            {
                var boundary =GetBoundary();

                var memory=new MemoryStream();
                WriteMultipartFormData(memory, req, boundary);
                memory.Seek(0, SeekOrigin.Begin);//设置指针到起点
                
                reqMsg.Content = new StreamContent(memory);
                req.RequestSet?.Invoke(reqMsg);  

                reqMsg.Content.Headers.Remove("Content-Type");
                reqMsg.Content.Headers.TryAddWithoutValidation("Content-Type", $"multipart/form-data;boundary={boundary}");
            }
            else
            {
                var data = GetNormalFormData(req);
               
                reqMsg.Content = new StringContent(data);
                req.RequestSet?.Invoke(reqMsg);
            }

          
        }

        #endregion


        #region   请求数据的 内容 处理

        #region 处理带文件上传的数据处理
    
       
        /// <summary>
        /// 写入 Form 的内容值 【 非文件参数 + 文件头 + 文件参数（内部完成） + 请求结束符 】
        /// </summary> 
        /// <param name="memory"></param>
        /// <param name="request"></param>
        /// <param name="boundary"></param>
        private static void WriteMultipartFormData(Stream memory, OsHttpRequest request, string boundary)
        {
            foreach (var param in request.FormParameters)
            {
                WriteStringTo(memory, GetMultipartFormData(param, boundary));
            }
            foreach (var file in request.FileParameters)
            {
                //文件头
                WriteStringTo(memory, GetMultipartFileHeader(file, boundary));
                //文件内容
                file.Writer(memory);
                //文件结尾
                WriteStringTo(memory, _lineBreak);
            }
            //写入整个请求的底部信息
            WriteStringTo(memory, GetMultipartFooter(boundary));
        }

        /// <summary>
        /// 写入 Form 的内容值（文件头）
        /// </summary>
        /// <param name="file"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private static string GetMultipartFileHeader(FileParameter file, string boundary)
        {
            var conType = file.ContentType ?? "application/octet-stream";
            return $"--{boundary}{_lineBreak}Content-Disposition: form-data; name=\"{file.Name}\"; filename=\"{file.FileName}\"{_lineBreak}Content-Type: {conType}{_lineBreak}{_lineBreak}";
        }
        /// <summary>
        /// 写入 Form 的内容值（非文件参数）
        /// </summary>
        /// <param name="param"></param>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private static string GetMultipartFormData(FormParameter param, string boundary)
        {
            return
                $"--{boundary}{_lineBreak}Content-Disposition: form-data; name=\"{param.Name}\"{_lineBreak}{_lineBreak}{param.Value}{_lineBreak}";
        }

        /// <summary>
        /// 写入 Form 的内容值  （请求结束符）
        /// </summary>
        /// <param name="boundary"></param>
        /// <returns></returns>
        private static string GetMultipartFooter(string boundary)
        {
            return $"--{boundary}--{_lineBreak}";
        }

        #endregion

        #region 不包含文件的数据处理（正常 get/post 请求）
        /// <summary>
        /// 写入请求的内容信息 （非文件上传请求）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string GetNormalFormData(OsHttpRequest request)
        {
            var formstring = new StringBuilder();
            foreach (var p in request.FormParameters)
            {
                if (formstring.Length > 1)
                    formstring.Append("&");
                formstring.AppendFormat(p.ToString());
            }
            if (string.IsNullOrEmpty(request.CustomBody)) return formstring.ToString();

            if (formstring.Length > 1)
                formstring.Append("&");
            formstring.Append(request.CustomBody);
            return formstring.ToString();
        }
        #endregion

        #endregion


        #region 请求辅助方法
        /// <summary>
        /// 写入数据方法（将数据写入  webrequest）
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="toWrite"></param>
        /// <returns>写入的字节数量</returns>
        private static void WriteStringTo(Stream stream, string toWrite)
        {
            var bytes = Encoding.GetBytes(toWrite);
            stream.Write(bytes, 0, bytes.Length);
        }
        
        /// <summary>
        /// 创建 请求 分割界限
        /// </summary>
        /// <returns></returns>
        private static string GetBoundary()
        {
            const string pattern = "abcdefghijklmnopqrstuvwxyz0123456789";
            var boundaryBuilder = new StringBuilder();
            var rnd = new Random();
            for (var i = 0; i < 10; i++)
            {
                var index = rnd.Next(pattern.Length);
                boundaryBuilder.Append(pattern[index]);
            }
            return $"-------{boundaryBuilder}";
        }

        #endregion
    }
}
