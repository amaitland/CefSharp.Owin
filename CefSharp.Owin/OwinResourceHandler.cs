using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Net;

namespace CefSharp.Owin
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Loosly based on https://github.com/eVisionSoftware/Harley/blob/master/src/Harley.UI/Owin/OwinSchemeHandlerFactory.cs
    /// </summary>
    public class OwinResourceHandler : IResourceHandler, IDisposable
    {
        private readonly AppFunc _appFunc;
        private readonly Stream _responseStream = new MemoryStream();
        private Dictionary<string, object> _owinEnvironment;

        public OwinResourceHandler(AppFunc appFunc)
        {
            _appFunc = appFunc;
        }

        public bool ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var requestHeaders = request.Headers.ToDictionary();
            var requestBody = Stream.Null;

            if (request.Method == "POST")
            {
                using (var postData = request.PostData)
                {
                    if (postData != null)
                    {
                        var postDataElements = postData.Elements;


                        var firstPostDataElement = postDataElements.First();

                        var bytes = firstPostDataElement.Bytes;

                        requestBody = new MemoryStream(bytes, 0, bytes.Length);

                        //TODO: Investigate how to process multi part POST data
                        //var charSet = request.GetCharSet();
                        //foreach (var element in elements)
                        //{
                        //    if (element.Type == PostDataElementType.Bytes)
                        //    {
                        //        var body = element.GetBody(charSet);
                        //    }
                        //}
                    }
                }
            }
            
            var uri = new Uri(request.Url);

            _owinEnvironment = new Dictionary<string, object>
            {
                {"owin.RequestBody", requestBody},
                {"owin.RequestHeaders", requestHeaders},
                {"owin.RequestMethod", request.Method},
                {"owin.RequestPath", uri.AbsolutePath},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestProtocol", "HTTP/1.1"},
                {"owin.RequestQueryString", uri.Query},
                {"owin.RequestScheme", "HTTP/1.1"},
                {"owin.ResponseHeaders", new Dictionary<string, string[]>()},
                {"owin.ResponseBody", _responseStream}
            };

            //Spawn a new task to execute the own pipeline - need to return ASAP so other processing can occur
            Task.Run(async () =>
            {
                await _appFunc(_owinEnvironment);
                
                //Callback wraps an unmanaged resource, so let's Dispose when we're done
                using (callback)
                {
                    callback.Continue();
                }
            });

            return true;
        }

        public Stream GetResponse(IResponse response, out long responseLength, out string redirectUrl)
        {
            responseLength = _responseStream.Length;
            redirectUrl = null;

            if (_owinEnvironment.ContainsKey("owin.ResponseStatusCode"))
            {
                response.StatusCode = Convert.ToInt32(_owinEnvironment["owin.ResponseStatusCode"]);
                //TODO: May status code to StatusText
                response.StatusText = "OK";
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusText = "OK";
            }

            //Copy the response headers
            var responseHeaders = (Dictionary<string, string[]>)_owinEnvironment["owin.ResponseHeaders"];
            foreach (var responseHeader in responseHeaders)
            {
                response.ResponseHeaders.Add(responseHeader.Key, string.Join(";", responseHeader.Value));
            }
            
            response.MimeType = responseHeaders.ContainsKey("Content-Type") ? response.ResponseHeaders["Content-Type"] : "text/plain";

            if (string.IsNullOrEmpty(response.MimeType))
            {
                response.MimeType = "text/plain";
            }

            //Response has been populated - reset the position to 0 so it can be read
            _responseStream.Position = 0;

            return _responseStream;
        }

        public void Dispose()
        {
            
        }
    }
}
