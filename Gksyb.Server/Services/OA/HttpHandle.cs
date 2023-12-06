namespace Gksyb.Server.Services.OA
{
    public class HttpHandle
    {

        public static async Task<string> HttpPostAsync(string Url, string postDataStr, Dictionary<string, string> dict)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url);

                request.Content = new StringContent(postDataStr, Encoding.UTF8, "application/x-www-form-urlencoded");

                if (dict != null)
                {
                    foreach (var item in dict)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string retString = await response.Content.ReadAsStringAsync();
                    return retString;
                }
                else
                {
                    return "error:" + (int)response.StatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle other exceptions or specific error scenarios as needed
                Console.WriteLine("Exception: " + ex.Message);
                return "error:-1";
            }
        }

        /// <summary>
        /// json格式post
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public async Task<string> PostJSONAsync(string url, string postData)
        {
            string result;

            try
            {
                // 创建一个 HttpClient 实例用于发送 HTTP 请求
                HttpClient httpClient = new HttpClient();

                // 准备包含 JSON 数据的 POST 请求内容
                var content = new StringContent(postData, Encoding.UTF8, "application/json");

                // 异步发送 POST 请求到指定的 URL
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // 如果响应成功，异步读取响应内容
                    result = await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // 处理不成功的响应（例如非 200 状态码）
                    result = "错误：" + (int)response.StatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                // 处理 HTTP 请求可能出现的异常
                Console.WriteLine("异常：" + ex.Message);
                result = "错误：-1";
            }

            // 返回 POST 请求的结果（响应内容或错误消息）
            return result;
        }

        /// <summary>
        /// 非json格式 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public async Task<string> PostAsync(string url, string postData, Dictionary<string, string> headers)
        {
            try
            {
                // 创建 HttpClient 实例用于发送 HTTP 请求
                using HttpClient httpClient = new HttpClient();

                // 设置请求头
                if (headers != null)
                {
                    foreach (var item in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(item.Key, item.Value);
                    }
                }

                // 准备包含表单数据的请求内容
                var content = new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded");

                // 发送 POST 请求到指定的 URL
                HttpResponseMessage response = await httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    // 如果响应成功，读取响应内容
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    // 处理不成功的响应（例如非 200 状态码）
                    return "错误：" + (int)response.StatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                // 处理 HTTP 请求可能出现的异常
                Console.WriteLine("异常：" + ex.Message);
                return "错误：-1";
            }
        }

        /// <summary>
        /// GET方式
        /// </summary>
        /// <param name="urlString"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(string urlString)
        {
            try
            {
                // 创建 HttpClient 实例用于发送 HTTP 请求
                using HttpClient httpClient = new HttpClient();

                // 发送 GET 请求到指定的 URL
                HttpResponseMessage response = await httpClient.GetAsync(urlString);

                if (response.IsSuccessStatusCode)
                {
                    // 如果响应成功，读取响应内容
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
                else
                {
                    // 处理不成功的响应（例如非 200 状态码）
                    return "错误：" + (int)response.StatusCode;
                }
            }
            catch (HttpRequestException ex)
            {
                // 处理 HTTP 请求可能出现的异常
                Console.WriteLine("异常：" + ex.Message);
                return "错误：-1";
            }
        }

    }
}
