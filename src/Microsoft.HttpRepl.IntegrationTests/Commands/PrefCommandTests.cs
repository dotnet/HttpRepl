using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.IntegrationTests.Mocks;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl.Parsing;
using Xunit;

namespace Microsoft.HttpRepl.IntegrationTests.Commands
{
    public class PrefCommandTests
    {
        [Fact]
        public async Task ExecuteAsync_UppercaseGetCommand_CorrectOutput()
        {
            MockedShellState shellState = new MockedShellState();
            MockedFileSystem fileSystem = new MockedFileSystem();
            PreferencesProvider preferencesProvider = new PreferencesProvider(fileSystem, new UserProfileDirectoryProvider());
            HttpState httpState = new HttpState(fileSystem, preferencesProvider);
            ICoreParseResult parseResult = CoreParseResultHelper.Create($"pref GET {WellKnownPreference.ProtocolColor}");

            PrefCommand command = new PrefCommand();

            await command.ExecuteAsync(shellState, httpState, parseResult, CancellationToken.None);

            Assert.Single(shellState.Output);
            Assert.Equal($"Configured value: {httpState.Preferences[WellKnownPreference.ProtocolColor]}", shellState.Output[0]);
        }
    }
}
