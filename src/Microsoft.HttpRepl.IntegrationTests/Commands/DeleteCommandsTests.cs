using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.IntegrationTests.SampleApi;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class DeleteCommandTests : HttpCommandTests<DeleteCommand>, IClassFixture<HttpCommandsFixture<DeleteCommandsConfig>>
    {
        private static readonly IFileSystem _fileSystem = new MockedFileSystem();

        private readonly DeleteCommandsConfig _config;
        
        public DeleteCommandTests(HttpCommandsFixture<DeleteCommandsConfig> deleteCommandsFixture)
            : base(new DeleteCommand(_fileSystem))
        {
            _config = deleteCommandsFixture.Config;
        }

        [Fact]
        public async Task ExecuteAsync_WithNoBasePath_VerifyError()
        {
            await VerifyErrorMessage(commandText: "DELETE",
                                     baseAddress: null,
                                     path: null,
                                     expectedErrorMessage: Resources.Strings.Error_NoBasePath);
        }

        [Fact]
        public async Task ExecuteAsync_WithMultipartRoute_VerifyOutput()
        {
            await VerifyResponse(commandText: "DELETE",
                                 baseAddress: _config.BaseAddress,
                                 path: "a/file/path.txt",
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "File path delete received successfully.");
        }

        [Fact]
        public async Task ExecuteAsync_WithOnlyBaseAddress_VerifyOutput()
        {
            await VerifyResponse(commandText: "DELETE",
                                 baseAddress: _config.BaseAddress,
                                 path: null,
                                 expectedResponseLines: 5,
                                 expectedResponseContent: "Root delete received successfully.");
        }
    }

    public class DeleteCommandsConfig : SampleApiServerConfig
    {
        public DeleteCommandsConfig()
        {
            Routes.Add(new StaticSampleApiServerRoute("DELETE", "", "Root delete received successfully."));
            Routes.Add(new StaticSampleApiServerRoute("DELETE", "a/file/path.txt", "File path delete received successfully."));
        }
    }
}
