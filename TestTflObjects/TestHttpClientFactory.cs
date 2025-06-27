using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TestTflObjects
{
    public sealed class TestHttpClientFactory : IHttpClientFactory, IDisposable
    {
        private readonly Lazy<HttpMessageHandler> _handlerLazy = new(() => new HttpClientHandler());

        public HttpClient CreateClient(string name) =>
            new HttpClient(_handlerLazy.Value, disposeHandler: false);

        public void Dispose()
        {
            if (_handlerLazy.IsValueCreated)
                _handlerLazy.Value.Dispose();
        }
    }

    public class HttpClientFactoryFixture : IDisposable
    {
        public IServiceProvider Services { get; }

        public HttpClientFactoryFixture()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHttpClientFactory, TestHttpClientFactory>();
            Services = services.BuildServiceProvider();
        }

        public void Dispose()
        {
            if (Services is IDisposable disposable)
                disposable.Dispose();
        }
    }

    [CollectionDefinition("HttpClientFactory collection")]
    public class HttpClientFactoryCollection : ICollectionFixture<HttpClientFactoryFixture> { }
}
