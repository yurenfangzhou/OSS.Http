﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OSS.Common.Authrization;
using OSS.Common.ComModels;
using OSS.Common.Plugs;
using OSS.Http.Mos;

namespace OSS.Http.Extention
{
    /// <summary>
    /// 通用请求结果扩展
    /// </summary>
    public static class OsRestExtention
    {
        /// <summary>
        /// 处理远程请求方法，并返回需要的实体
        /// </summary>
        /// <typeparam name="T">需要返回的实体类型</typeparam>
        /// <param name="request">远程请求组件的request基本信息</param>
        /// <param name="formatFunc">处理内容委托</param>
        /// <param name="client">自定义httpclient</param>
        /// <param name="moduleName">模块名称</param>
        /// <returns>实体类型</returns>
        public static async Task<T> RestCommon<T>(this OsHttpRequest request,
            Func<HttpResponseMessage, Task<T>> formatFunc, HttpClient client = null,
            string moduleName = ModuleNames.Default)
            where T : ResultMo, new()
        {
            var resp = await request.RestSend(client);
            var t = await formatFunc(resp);

            return t ?? new T() {ret = -1, msg = "未发现结果"};
        }


        /// <summary>
        ///  使用json格式化内容方法
        /// </summary>
        /// <typeparam name="TResp"></typeparam>
        /// <param name="resp"></param>
        /// <returns></returns>
        private static async Task<TResp> JsonFormat<TResp>(HttpResponseMessage resp)
            where TResp : ResultMo, new()
        {
            if (!resp.IsSuccessStatusCode)
                return new TResp()
                {
                    ret = -(int) resp.StatusCode,
                    msg = resp.ReasonPhrase
                };
            var contentStr = await resp.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TResp>(contentStr);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResp"></typeparam>
        /// <param name="request"></param>
        /// <param name="client"></param>
        /// <param name="moduleName"></param>
        /// <returns></returns>
        public static async Task<TResp> RestCommonJson<TResp>(this OsHttpRequest request, HttpClient client = null,
            string moduleName = ModuleNames.Default)
            where TResp : ResultMo, new()
        {
            return await RestCommon(request, JsonFormat<TResp>, client, moduleName);
        }

    }
}
