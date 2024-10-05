// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the License.txt file in the project root for more information.

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Fakes;
using Microsoft.HttpRepl.IntegrationTests.Utilities;
using Microsoft.HttpRepl.Preferences;

namespace Microsoft.HttpRepl.IntegrationTests
{
    public class BaseIntegrationTest
    {
        private static readonly Regex _dateRegex;
        private static readonly string _dateReplacement;

        static BaseIntegrationTest()
        {
            if (Environment.NewLine == "\r\n")
            {
                _dateRegex = new Regex("^Date: [A-Za-z]{3}, \\d{2} [A-Za-z]{3} \\d{4} \\d{2}:\\d{2}:\\d{2} GMT\r$", RegexOptions.Compiled | RegexOptions.Multiline);
                _dateReplacement = "Date: [Date]\r";
            }
            else
            {
                _dateRegex = new Regex("^Date: [A-Za-z]{3}, \\d{2} [A-Za-z]{3} \\d{4} \\d{2}:\\d{2}:\\d{2} GMT$", RegexOptions.Compiled | RegexOptions.Multiline);
                _dateReplacement = "Date: [Date]";
            }
        }

        protected static string NormalizeOutput(string output, string baseUrl)
        {
            // The console implementation uses trailing whitespace when a new line's text is shorter than the previous
            // line.  For example (the trailing * represent spaces):
            // Line 1: (Disconnected)> run C:\path\to\a\test\script\file.txt
            // Line 2: (Disconnected)> set base http://localhost:12345******
            // This having this whitespace makes it harder to read/write test baselines, so here we'll trim each line
            string result = string.Join(Environment.NewLine, output.Split(Environment.NewLine).Select(l => l.TrimEnd()));

            // next, normalize the base URL from the test fixture
            if (!string.IsNullOrEmpty(baseUrl))
            {
                result = result.Replace(baseUrl, "[BaseUrl]");
            }

            // next, normalize the date
            result = _dateRegex.Replace(result, _dateReplacement);

            // strip ansi begin/end formatting (bold, color)
            result = Regex.Replace(result, @"\u001b\[[0-9]*m", string.Empty);

            return result;
        }

        protected static async Task<string> RunTestScript(string scriptText, string baseAddress, IPreferences preferences = null)
        {
            LoggingConsoleManagerDecorator console = new LoggingConsoleManagerDecorator(new NullConsoleManager());
            preferences ??= new NullPreferences();

            using (var script = new TestScript(scriptText))
            {
                await Program.Start($"run {script.FilePath}".Split(' '), console, preferences);
            }

            string output = console.LoggedOutput;
            // remove the first line because it has the randomly generated script file name.
            output = output.Substring(output.IndexOf(Environment.NewLine) + Environment.NewLine.Length);
            output = NormalizeOutput(output, baseAddress);

            return output;
        }
    }
}
