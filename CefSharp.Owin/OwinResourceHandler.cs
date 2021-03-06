﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace CefSharp.Owin
{
    //Shorthand for Owin pipeline func
    using AppFunc = Func<IDictionary<string, object>, Task>;


    /// <summary>
    /// Loosly based on https://github.com/eVisionSoftware/Harley/blob/master/src/Harley.UI/Owin/OwinSchemeHandlerFactory.cs
    /// New instance is instanciated for every request
    /// </summary>
    public class OwinResourceHandler : ResourceHandler
    {
        private static readonly Dictionary<int, string> StatusCodeToStatusTextMapping = new Dictionary<int, string>
        {
            {200, "OK"},
            {301, "Moved Permanently"},
            {304, "Not Modified"},
            {404, "Not Found"}
        };

        private readonly AppFunc _appFunc;

        public OwinResourceHandler(AppFunc appFunc)
        {
            _appFunc = appFunc;
        }

        /// <summary>
        /// Read the request, then process it through the OWEN pipeline
        /// then populate the response properties.
        /// </summary>
        /// <param name="request">request</param>
        /// <param name="callback">callback</param>
        /// <returns>always returns true as we'll handle all requests this handler is registered for.</returns>
        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            // PART 1 - Read the request - here we read the request and create a dictionary
            // that follows the OWEN standard

            var responseStream = new MemoryStream();
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

            //TODO: Implement cancellation token
            //var cancellationTokenSource = new CancellationTokenSource();
            //var cancellationToken = cancellationTokenSource.Token;
            var uri = new Uri(request.Url);
            var requestHeaders = request.Headers.ToDictionary();
            //Add Host header as per http://owin.org/html/owin.html#5-2-hostname
            requestHeaders.Add("Host", new[] { uri.Host + (uri.Port > 0 ? (":" + uri.Port) : "") });

            //http://owin.org/html/owin.html#3-2-environment
            //The Environment dictionary stores information about the request,
            //the response, and any relevant server state.
            //The server is responsible for providing body streams and header collections for both the request and response in the initial call.
            //The application then populates the appropriate fields with response data, writes the response body, and returns when done.
            //Keys MUST be compared using StringComparer.Ordinal.
            var owinEnvironment = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                //Request http://owin.org/html/owin.html#3-2-1-request-data
                {"owin.RequestBody", requestBody},
                {"owin.RequestHeaders", requestHeaders},
                {"owin.RequestMethod", request.Method},
                {"owin.RequestPath", uri.AbsolutePath},
                {"owin.RequestPathBase", "/"},
                {"owin.RequestProtocol", "HTTP/1.1"},
                {"owin.RequestQueryString", uri.Query},
                {"owin.RequestScheme", uri.Scheme},
                //Response http://owin.org/html/owin.html#3-2-2-response-data
                {"owin.ResponseHeaders", new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)},
                {"owin.ResponseBody", responseStream},
                //Other Data
                {"owin.Version", "1.0.0"},
                //{"owin.CallCancelled", cancellationToken}
            };

            //PART 2 - Spawn a new task to execute the OWIN pipeline
            //We execute this in an async fashion and return true so other processing
            //can occur
            Task.Run(async () =>
            {
                //Call into the OWEN pipeline
                await _appFunc(owinEnvironment);

                //Response has been populated - reset the position to 0 so it can be read
                responseStream.Position = 0;

                int statusCode;

                if (owinEnvironment.ContainsKey("owin.ResponseStatusCode"))
                {
                    statusCode = Convert.ToInt32(owinEnvironment["owin.ResponseStatusCode"]);
                    //TODO: Improve status code mapping - see if CEF has a helper function that can be exposed
                    //StatusText = StatusCodeToStatusTextMapping[response.StatusCode];
                }
                else
                {
                    statusCode = (int)HttpStatusCode.OK;
                    //StatusText = "OK";
                }

                //Grab a reference to the ResponseHeaders
                var responseHeaders = (Dictionary<string, string[]>)owinEnvironment["owin.ResponseHeaders"];

                //Populate the response properties
                Stream = responseStream;
                ResponseLength = responseStream.Length;
                StatusCode = statusCode;
                MimeType = responseHeaders.ContainsKey("Content-Type") ? responseHeaders["Content-Type"].First() : "text/plain";

                //Add the response headers from OWEN to the Headers NameValueCollection
                foreach (var responseHeader in responseHeaders)
                {
                    //It's possible for headers to have multiple values
                    Headers.Add(responseHeader.Key, string.Join(";", responseHeader.Value));
                }

                //Once we've finished populating the properties we execute the callback
                //Callback wraps an unmanaged resource, so let's explicitly Dispose when we're done    
                using (callback)
                {
                    callback.Continue();
                }

            });

            return CefReturnValue.ContinueAsync;
        }
    }
}
