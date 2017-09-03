﻿using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Moq;
using NUnit.Framework;
using WebApi.OutputCache.Core.Cache;

namespace WebApi.OutputCache.V2.Tests
{
    [TestFixture]
    public class InvalidateTests : IDisposable
    {
        [SetUp]
        public void init()
        {
            Thread.CurrentPrincipal = null;

            _cache = new Mock<IApiOutputCache>();

            var conf = new HttpConfiguration();
            var builder = new ContainerBuilder();
            builder.RegisterInstance(_cache.Object);

            conf.DependencyResolver = new AutofacWebApiDependencyResolver(builder.Build());
            conf.Routes.MapHttpRoute(
                "DefaultApi",
                "api/{controller}/{action}/{id}",
                new {id = RouteParameter.Optional}
            );

            _server = new HttpServer(conf);
        }

        [TearDown]
        public void fixture_dispose()
        {
            if (_server != null) _server.Dispose();
        }

        private HttpServer _server;
        private readonly string _url = "http://www.strathweb.com/api/sample/";
        private Mock<IApiOutputCache> _cache;

        private void SetupCacheForAutoInvalidate()
        {
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s ==
                         "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_s50_exclude_fakecallback")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                s => s == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100"))).Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_s50_exclude_fakecallback")))
                .Returns(true);
            _cache.Setup(x => x.Contains(It.Is<string>(
                    s => s ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100_array")))
                .Returns(true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~InvalidateTests()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_server != null)
                {
                    _server.Dispose();
                    _server = null;
                }
        }

        [Test]
        public void controller_level_invalidate_on_three_methods_works_on_delete()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.DeleteAsync("http://www.strathweb.com/api/autoinvalidate/Delete").Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
        }

        [Test]
        public void controller_level_invalidate_on_three_methods_works_on_post()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PostAsync("http://www.strathweb.com/api/autoinvalidate/Post",
                new StringContent(string.Empty)).Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
        }

        [Test]
        public void controller_level_invalidate_on_three_methods_works_on_put()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PutAsync("http://www.strathweb.com/api/autoinvalidate/Put",
                new StringContent(string.Empty)).Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatecontroller-etag_match_304")),
                Times.Exactly(1));
        }

        [Test]
        public void controller_level_invalidate_with_type_check_does_not_invalidate_on_no_type_match()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PostAsync("http://www.strathweb.com/api/autoinvalidatewithtype/Post",
                new StringContent(string.Empty)).Result;

            Assert.True(result2.IsSuccessStatusCode);
            _cache.Verify(s => s.Contains(It.IsAny<string>()), Times.Never());
            _cache.Verify(s => s.RemoveStartsWith(It.IsAny<string>()), Times.Never());
        }

        [Test]
        public void controller_level_invalidate_with_type_check_invalidates_only_methods_with_types_matched()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PostAsync("http://www.strathweb.com/api/autoinvalidatewithtype/PostString", "hi",
                new JsonMediaTypeFormatter()).Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100_array")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_s50_exclude_fakecallback")),
                Times.Never());
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_c100_s100_array")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.autoinvalidatewithtypecontroller-get_s50_exclude_fakecallback")),
                Times.Never());
        }

        [Test]
        public void regular_invalidate_on_two_methods_works_on_post()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PostAsync(_url + "Post_2_invalidates", new StringContent(string.Empty)).Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x ==
                         "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_s50_exclude_fakecallback")),
                Times.Exactly(1));
        }

        [Test]
        public void regular_invalidate_works_on_post()
        {
            SetupCacheForAutoInvalidate();
            var client = new HttpClient(_server);

            var result2 = client.PostAsync(_url + "Post", new StringContent(string.Empty)).Result;

            _cache.Verify(
                s => s.Contains(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")),
                Times.Exactly(1));
            _cache.Verify(
                s => s.RemoveStartsWith(It.Is<string>(
                    x => x == "webapi.outputcache.v2.tests.testcontrollers.samplecontroller-get_c100_s100")),
                Times.Exactly(1));
        }
    }
}