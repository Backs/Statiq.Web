﻿using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.App;
using Statiq.Common;
using Statiq.Html;
using Statiq.Testing;
using Statiq.Web.Pipelines;

namespace Statiq.Web.Tests.Pipelines
{
    [TestFixture]
    [NonParallelizable]
    public class ContentFixture : BaseFixture
    {
        public class ExecuteTests : DataFixture
        {
            [Test]
            public async Task DefaultGatherHeadingsLevel()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper.Factory.CreateWeb(Array.Empty<string>());
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2" }, true);
            }

            [Test]
            public async Task GlobalGatherHeadingsLevelSetting()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GatherHeadingsLevel, 2);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }

            [Test]
            public async Task DocumentGatherHeadingsLevelSetting()
            {
                // Given
                Bootstrapper bootstrapper = Bootstrapper
                    .Factory
                    .CreateWeb(Array.Empty<string>())
                    .AddSetting(WebKeys.GatherHeadingsLevel, 3);
                TestFileProvider fileProvider = new TestFileProvider
                {
                    {
                        "/input/foo.html",
                        @"GatherHeadingsLevel: 2
---" + GatherHeadingsFile
                    }
                };

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(fileProvider);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                IDocument document = result.Outputs[nameof(Content)][Phase.Process].ShouldHaveSingleItem();
                document
                    .GetDocumentList(HtmlKeys.Headings)
                    .Flatten()
                    .Select(x => x.GetContentStringAsync().Result)
                    .ShouldBe(new[] { "1.1", "1.2", "2.1", "2.2", "2.3" }, true);
            }
        }

        public const string GatherHeadingsFile = @"
<html>
  <head>
  </head>
  <body>
    <div>a</div>
    <h1>1.1</h1>
    <div>b</div>
    <h2>2.1</h2>
    <div>b</div>
    <h2>2.2</h2>
    <div>c</div>
    <h1>1.2</h1>
    <div>d</div>
    <h2>2.3</h2>
    <div>e</div>
    <h3>3.1</h3>
    <div>f</div>
  </body>
</html>";
    }
}
