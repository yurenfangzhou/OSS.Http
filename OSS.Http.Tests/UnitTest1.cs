using System.Net.Http;
using System.Threading.Tasks;
using OSS.Common.Authrization;
using OSS.Common.ComModels;
using OSS.Http.Extention;
using OSS.Http.Mos;
using Xunit;

namespace OSS.Http.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async void TestRestCommonJson()
        {
            var req = new OsHttpRequest
            {
                AddressUrl = "http://localhost:62936",
                HttpMethod = HttpMethod.Get
            };
            MemberShiper.SetAppAuthrizeInfo(new AppAuthorizeInfo());
            var res = await req.RestCommonJson<ResultMo>();
            Assert.True(res.IsSuccess());
        }


        [Fact]
        public async void Test1()
        {
            var res = await GetTest1();
            Assert.True(res.IsSuccessStatusCode);
        }

        private static async Task<HttpResponseMessage> GetTest1()
        {
            var req = new OsHttpRequest
            {
                AddressUrl =  "http://www.baidu.com",
                HttpMethod = HttpMethod.Get
            };
            return await req.RestSend();
        }

        private static async Task<HttpResponseMessage> GetTest()
        {
            var req = new OsHttpRequest
            {
                AddressUrl =
                    "https://api.weixin.qq.com/sns/oauth2/access_token?appid=wxaa9e6cb3f03afa97&secret=0fc0c6f735a90fda1df5fc840e010144&code=ssss&grant_type=authorization_code",
                HttpMethod = HttpMethod.Get
            };

            return await req.RestSend();
        }
        private static async Task<HttpResponseMessage> Test()
        {
            //OsHttpRequest req = new OsHttpRequest();

            //req.AddressUrl = "http://www.baidu.com";
            //req.HttpMothed = HttpMothed.GET;
            //return await req.RestSend();

            var req = new OsHttpRequest();
            req.AddressUrl = "http://localhost:59489/";
            req.HttpMethod = HttpMethod.Post;

            //  文件上传测试
            //var imageFile = new FileStream("E:\\111.png", FileMode.Open, FileAccess.Read);
            //req.FileParameters.Add(new FileParameter("media", imageFile, "111.png", "image/jpeg")); 
            // 表单参数测试
            //req.FormParameters.Add(new FormParameter("description", "测试"));
            return await req.RestSend();
        }
    }
}
