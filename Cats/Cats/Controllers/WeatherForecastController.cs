using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Net.Http;

namespace Cats.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KittyController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IMemoryCache _catCache;



        public KittyController(IHttpClientFactory clientFactory, IMemoryCache catCache)
        {
            _clientFactory = clientFactory;
            _catCache = catCache;
        }
        
    

        [HttpPost]
        public IActionResult ProcessForm([FromForm] CatFormModel catForm)
        {
            string catUri = catForm.CatUri;
            Stream kittyImage;
            HttpResponseMessage httpResponse;
            HttpClient client = _clientFactory.CreateClient();
            int httpStatusCode;

            try
            {
                httpStatusCode = (int)client.Send(new HttpRequestMessage(HttpMethod.Get, catUri)).StatusCode;
                httpResponse = client.Send(new HttpRequestMessage(HttpMethod.Get, $"https://http.cat/{httpStatusCode}.jpg"));
                if (!_catCache.TryGetValue(httpStatusCode, out kittyImage))
                {
                    _catCache.Set(
                        httpStatusCode,
                        httpResponse.Content.ReadAsStream(),
                        new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(90) });
                    return File(httpResponse.Content.ReadAsStream(), "image/jpg");
                }
                return File(kittyImage, "image/jpg");
            }
            catch (Exception exception)
            {
                httpResponse = client.Send(new HttpRequestMessage(HttpMethod.Get, $"https://http.cat/404.jpg"));
                return File(httpResponse.Content.ReadAsStream(), "image/jpg");
            }
        }
    }
}