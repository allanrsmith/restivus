﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Restivus
{
    public interface IRestClient
    {
        IWebApi WebApi { get; }
        IReadOnlyCollection<IHttpRequestMiddleware> RequestMiddlewares { get; }
        IHttpRequestSender RequestSender { get; }
    }

    public static class RestClientExtensions
    {
        public static HttpRequestMessage CreateRequestMessage(this IRestClient client,
            HttpMethod method,
            string path)
        {
            var message = new HttpRequestMessage(
                method,
                client.WebApi.UriForAbsolutePath(path)
            );

            return client.RequestMiddlewares.Aggregate(
                message,
                (msg, middleware) => middleware?.Run(message)
            );
        }

        public static Task<T> SendAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            Action<HttpRequestMessage> mutateRequestMessage,
            Func<HttpResponseMessage, T> deserializeResponse)
        {
            var message = client.CreateRequestMessage(method, absolutePath);

            mutateRequestMessage(message);

            return client.RequestSender.SendAsync(message, deserializeResponse);
        }

        public static Task<T> SendAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            Action<HttpRequestMessage> mutateRequestMessage,
            Func<HttpResponseMessage, Task<T>> deserializeResponseAsync)
        {
            var message = client.CreateRequestMessage(method, absolutePath);

            mutateRequestMessage(message);

            return client.RequestSender.SendAsync(message, deserializeResponseAsync);
        }

        public static Task<T> SendJsonAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            Func<T> getPayload,
            Func<HttpResponseMessage, T> deserializeResponse)
        {
            return client.SendAsync(
                method,
                absolutePath,
                message => message.Content = JsonConvert.SerializeObject(getPayload()).AsJsonContent(),
                deserializeResponse
            );
        }

        public static Task<T> SendJsonAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            Func<T> getPayload,
            Func<HttpResponseMessage, Task<T>> deserializeResponseAsync)
        {
            return client.SendAsync(
                method,
                absolutePath,
                message => message.Content = JsonConvert.SerializeObject(getPayload()).AsJsonContent(),
                deserializeResponseAsync
            );
        }

        public static Task<T> SendJsonAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            T payload,
            Func<HttpResponseMessage, T> deserializeResponse)
        {
            return client.SendJsonAsync(
                method,
                absolutePath,
                () => payload,
                deserializeResponse
            );
        }

        public static Task<T> SendJsonAsync<T>(this IRestClient client,
            HttpMethod method,
            string absolutePath,
            T payload,
            Func<HttpResponseMessage, Task<T>> deserializeResponseAsync)
        {
            return client.SendJsonAsync(
                method,
                absolutePath,
                () => payload,
                deserializeResponseAsync
            );
        }
    }
}
