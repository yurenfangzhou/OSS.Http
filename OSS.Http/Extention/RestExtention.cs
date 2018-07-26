﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OSS.Http.Mos;

namespace OSS.Http.Extention
{
    /// <summary>
    /// http请求辅助类
    /// </summary>
    public static class RestExtention
    {
     
        #region   扩展方法

        /// <summary>
        /// 同步的请求方式
        /// </summary>
        /// <param name="request">请求的参数</param>
        /// <param name="client"></param>
        /// <returns>自定义的Response结果</returns>
        public static Task<HttpResponseMessage> RestSend(this OsHttpRequest request, HttpClient client = null)
        {
            return RestSend(request, HttpCompletionOption.ResponseContentRead, CancellationToken.None, client);
        }

        /// <summary>
        /// 同步的请求方式
        /// </summary>
        /// <param name="request">请求的参数</param>
        /// <param name="completionOption"></param>
        /// <param name="client"></param>
        /// <returns>自定义的Response结果</returns>
        public static Task<HttpResponseMessage> RestSend(this OsHttpRequest request,
            HttpCompletionOption completionOption, HttpClient client = null)
        {
            return RestSend(request, completionOption, CancellationToken.None, client);
        }

        /// <summary>
        /// 同步的请求方式
        /// </summary>
        /// <param name="request">请求的参数</param>
        /// <param name="completionOption"></param>
        /// <param name="token"></param>
        /// <param name="client"></param>
        /// <returns>自定义的Response结果</returns>
        public static Task<HttpResponseMessage> RestSend(this OsHttpRequest request,
            HttpCompletionOption completionOption,
            CancellationToken token,
            HttpClient client = null)
        {
            return (client ?? GetDefaultClient()).RestSend(request, completionOption, token);
        }

        #endregion

        private static HttpClient _Client=null;
        /// <summary>
        /// 配置请求处理类
        /// </summary>
        /// <returns></returns>
        private static HttpClient GetDefaultClient()
        {
            if (_Client != null) return _Client;

            return _Client = new HttpClient();
        }
    }
}