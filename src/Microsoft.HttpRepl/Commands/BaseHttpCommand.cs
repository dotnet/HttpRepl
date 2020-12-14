// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Formatting;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.Resources;
using Microsoft.HttpRepl.Suggestions;
using Microsoft.HttpRepl.Telemetry;
using Microsoft.HttpRepl.Telemetry.Events;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;
using Microsoft.Repl.Parsing;
using Microsoft.Repl.Suggestions;
using Newtonsoft.Json.Linq;

namespace Microsoft.HttpRepl.Commands
{
    public abstract class BaseHttpCommand : CommandWithStructuredInputBase<HttpState, ICoreParseResult>
    {
        private const string HeaderOption = nameof(HeaderOption);
        private const string ResponseHeadersFileOption = nameof(ResponseHeadersFileOption);
        private const string ResponseBodyFileOption = nameof(ResponseBodyFileOption);
        private const string BodyFileOption = nameof(BodyFileOption);
        private const string NoBodyOption = nameof(NoBodyOption);
        private const string NoFormattingOption = nameof(NoFormattingOption);
        private const string StreamingOption = nameof(StreamingOption);
        private const string BodyContentOption = nameof(BodyContentOption);
        private static readonly char[] HeaderSeparatorChars = new[] { '=', ':' };
        private static readonly Dictionary<string, string> FileExtensionLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "application/json", ".json" },
            { "text/json", ".json" },
            { "application/xml", ".xml" },
            { "text/xml", ".xml" },
        };

        private CommandInputSpecification _inputSpec;

        private readonly IFileSystem _fileSystem;
        private readonly IPreferences _preferences;
        private readonly ITelemetry _telemetry;

        public override string Name => Verb;

        protected abstract string Verb { get; }

        protected abstract bool RequiresBody { get; }

        protected BaseHttpCommand(IFileSystem fileSystem, IPreferences preferences, ITelemetry telemetry)
        {
            _fileSystem = fileSystem;
            _preferences = preferences;
            _telemetry = telemetry;
        }

        public override CommandInputSpecification InputSpec
        {
            get
            {
                if (_inputSpec != null)
                {
                    return _inputSpec;
                }

                CommandInputSpecificationBuilder builder = CommandInputSpecification.Create(Verb)
                    .MaximumArgCount(1)
                    .WithOption(new CommandOptionSpecification(HeaderOption, requiresValue: true, forms: new[] { "--header", "-h" }))
                    .WithOption(new CommandOptionSpecification(ResponseHeadersFileOption, requiresValue: true, maximumOccurrences: 1, forms: new[] { "--response:headers", }))
                    .WithOption(new CommandOptionSpecification(ResponseBodyFileOption, requiresValue: true, maximumOccurrences: 1, forms: new[] { "--response:body", }))
                    .WithOption(new CommandOptionSpecification(NoFormattingOption, maximumOccurrences: 1, forms: new[] { "--no-formatting", "-F" }))
                    .WithOption(new CommandOptionSpecification(StreamingOption, maximumOccurrences: 1, forms: new[] { "--streaming", "-s" }));

                if (RequiresBody)
                {
                    builder = builder.WithOption(new CommandOptionSpecification(NoBodyOption, maximumOccurrences: 1, forms: "--no-body"))
                        .WithOption(new CommandOptionSpecification(BodyFileOption, requiresValue: true, maximumOccurrences: 1, forms: new[] { "--file", "-f" }))
                        .WithOption(new CommandOptionSpecification(BodyContentOption, requiresValue: true, maximumOccurrences: 1, forms: new[] { "--content", "-c" }));
                }

                _inputSpec = builder.Finish();
                return _inputSpec;
            }
        }

        protected override async Task ExecuteAsync(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, CancellationToken cancellationToken)
        {
            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

            shellState = shellState ?? throw new ArgumentNullException(nameof(shellState));

            if (programState.BaseAddress == null && (commandInput.Arguments.Count == 0 || !Uri.TryCreate(commandInput.Arguments[0].Text, UriKind.Absolute, out _)))
            {
                shellState.ConsoleManager.Error.WriteLine(Strings.Error_NoBasePath.SetColor(programState.ErrorColor));
                return;
            }

            SendTelemetry(commandInput);

            if (programState.SwaggerEndpoint != null)
            {
                await CreateDirectoryStructureForSwaggerEndpointAsync(shellState, programState, cancellationToken).ConfigureAwait(false);
            }

            Dictionary<string, string> thisRequestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (InputElement header in commandInput.Options[HeaderOption])
            {
                int equalsIndex = header.Text.IndexOfAny(HeaderSeparatorChars);

                if (equalsIndex < 0)
                {
                    shellState.ConsoleManager.Error.WriteLine(Strings.BaseHttpCommand_Error_HeaderFormatting.SetColor(programState.ErrorColor));
                    return;
                }

                thisRequestHeaders[header.Text.Substring(0, equalsIndex)] = header.Text.Substring(equalsIndex + 1);
            }

            Uri effectivePath = programState.GetEffectivePath(commandInput.Arguments.Count > 0 ? commandInput.Arguments[0].Text : string.Empty);
            using (HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(Verb.ToUpperInvariant()), effectivePath))
            {
                if (RequiresBody)
                {
                    if (!HandleRequiresBody(commandInput, shellState, programState, request, thisRequestHeaders))
                    {
                        // HandleRequiresBody can fail if there is a problem with the specified file or the specified editor,
                        // in which case we should bail out before trying to send the request.
                        return;
                    }
                }

                foreach (KeyValuePair<string, IEnumerable<string>> header in programState.Headers)
                {
                    // We only want to add headers that are not content headers
                    if (!WellKnownHeaders.ContentHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                foreach (KeyValuePair<string, string> header in thisRequestHeaders)
                {
                    // We only want to add headers that are not content headers
                    if (!WellKnownHeaders.ContentHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                InputElement responseHeadersFileOption = commandInput.Options[ResponseHeadersFileOption].Any() ? commandInput.Options[ResponseHeadersFileOption][0] : null;
                InputElement responseBodyFileOption = commandInput.Options[ResponseBodyFileOption].Any() ? commandInput.Options[ResponseBodyFileOption][0] : null;

                string headersTarget = responseHeadersFileOption?.Text;
                string bodyTarget = responseBodyFileOption?.Text;

                if (!string.IsNullOrWhiteSpace(headersTarget) &&
                    string.Equals(headersTarget, bodyTarget, StringComparison.OrdinalIgnoreCase))
                {
                    shellState.ConsoleManager.Error.WriteLine(Strings.BaseHttpCommand_Error_SameBodyAndHeaderFileName.SetColor(programState.ErrorColor));
                    return;
                }

                try
                {
                    HttpResponseMessage response = await programState.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                    await HandleResponseAsync(programState, commandInput, shellState.ConsoleManager, response, programState.EchoRequest, headersTarget, bodyTarget, cancellationToken).ConfigureAwait(false);
                }
                catch (HttpRequestException httpRequestException)
                {
                    shellState.ConsoleManager.Error.WriteLine(httpRequestException.Message.SetColor(programState.ErrorColor));
                }
                catch (OperationCanceledException)
                {
                    // We just want to eat this exception because the cancellation actually occurs for the entire command,
                    // not just the HTTP Request. So the cancellation is handled further down the stack by inspecting
                    // the CancellationToken.IsCancellationRequested property
                }
            }
        }

        private async Task CreateDirectoryStructureForSwaggerEndpointAsync(IShellState shellState, HttpState programState, CancellationToken cancellationToken)
        {
            string swaggerRequeryBehaviorSetting = _preferences.GetValue(WellKnownPreference.SwaggerRequeryBehavior, "auto");

            if (swaggerRequeryBehaviorSetting.StartsWith("auto", StringComparison.OrdinalIgnoreCase))
            {
                ApiConnection apiConnection = new ApiConnection(programState, _preferences, shellState.ConsoleManager, false)
                {
                    BaseUri = programState.BaseAddress,
                    SwaggerUri = programState.SwaggerEndpoint,
                    AllowBaseOverrideBySwagger = false
                };
                await apiConnection.SetupHttpState(programState, performAutoDetect: false, persistHeaders: true, persistPath: true, cancellationToken).ConfigureAwait(false);
            }
        }

        private bool HandleRequiresBody(DefaultCommandInput<ICoreParseResult> commandInput,
            IShellState shellState,
            HttpState programState,
            HttpRequestMessage request,
            Dictionary<string, string> requestHeaders)
        {
            string filePath = null;
            string bodyContent = null;
            bool deleteFile = false;
            bool noBody = commandInput.Options[NoBodyOption].Count > 0;

            if (!requestHeaders.TryGetValue("content-type", out string contentType) && programState.Headers.TryGetValue("content-type", out IEnumerable<string> contentTypes))
            {
                contentType = contentTypes.FirstOrDefault();
            }

            if (!noBody)
            {
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = "application/json";
                }

                if (commandInput.Options[BodyFileOption].Count > 0)
                {
                    filePath = commandInput.Options[BodyFileOption][0].Text;

                    if (!_fileSystem.FileExists(filePath))
                    {
                        shellState.ConsoleManager.Error.WriteLine(string.Format(Strings.BaseHttpCommand_Error_ContentFileDoesNotExist, filePath).SetColor(programState.ErrorColor));
                        return false;
                    }
                }
                else if (commandInput.Options[BodyContentOption].Count > 0)
                {
                    bodyContent = commandInput.Options[BodyContentOption][0].Text;
                }
                else
                {
                    string defaultEditorCommand = _preferences.GetValue(WellKnownPreference.DefaultEditorCommand, null);
                    if (string.IsNullOrWhiteSpace(defaultEditorCommand))
                    {
                        shellState.ConsoleManager.Error.WriteLine(string.Format(Strings.BaseHttpCommand_Error_DefaultEditorNotConfigured, WellKnownPreference.DefaultEditorCommand).SetColor(programState.ErrorColor));
                        return false;
                    }
                    else if (!_fileSystem.FileExists(defaultEditorCommand))
                    {
                        shellState.ConsoleManager.Error.WriteLine(string.Format(Strings.BaseHttpCommand_Error_DefaultEditorDoesNotExist, defaultEditorCommand).SetColor(programState.ErrorColor));
                        return false;
                    }

                    deleteFile = true;
                    filePath = _fileSystem.GetTempFileName(GetFileExtensionFromContentType(contentType));

                    string exampleBody = GetExampleBody(commandInput.Arguments.Count > 0 ? commandInput.Arguments[0].Text : string.Empty, ref contentType, Verb, programState);

                    if (!string.IsNullOrEmpty(exampleBody))
                    {
                        _fileSystem.WriteAllTextToFile(filePath, exampleBody);
                    }

                    string defaultEditorArguments = _preferences.GetValue(WellKnownPreference.DefaultEditorArguments, null) ?? "";
                    string original = defaultEditorArguments;
                    string pathString = $"\"{filePath}\"";

                    defaultEditorArguments = defaultEditorArguments.Replace("{filename}", pathString, StringComparison.OrdinalIgnoreCase);

                    if (string.Equals(defaultEditorArguments, original, StringComparison.Ordinal))
                    {
                        defaultEditorArguments = (defaultEditorArguments + " " + pathString).Trim();
                    }

                    ProcessStartInfo info = new ProcessStartInfo(defaultEditorCommand, defaultEditorArguments);

                    Process.Start(info)?.WaitForExit();
                }
            }

            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "application/json";
            }

            byte[] data = noBody
                ? Array.Empty<byte>()
                : string.IsNullOrEmpty(bodyContent)
                    ? _fileSystem.ReadAllBytesFromFile(filePath)
                    : Encoding.UTF8.GetBytes(bodyContent);

            HttpContent content = new ByteArrayContent(data);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            request.Content = content;

            if (deleteFile)
            {
                _fileSystem.DeleteFile(filePath);
            }

            AddHttpContentHeaders(content, programState, requestHeaders);

            return true;
        }

        private static string GetFileExtensionFromContentType(string contentType)
        {
            if (FileExtensionLookup.TryGetValue(contentType, out string extension))
            {
                return extension;
            }
            return ".tmp";
        }

        private static void AddHttpContentHeaders(HttpContent content, HttpState programState, Dictionary<string, string> requestHeaders)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in programState.Headers)
            {
                // We only want to add content headers, except for Content-Type, which is handled elsewhere
                if (WellKnownHeaders.ContentHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase) &&
                    !string.Equals(WellKnownHeaders.ContentType, header.Key, StringComparison.OrdinalIgnoreCase))
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (KeyValuePair<string, string> header in requestHeaders)
            {
                // We only want to add content headers, except for Content-Type, which is handled elsewhere
                if (WellKnownHeaders.ContentHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase) &&
                    !string.Equals(WellKnownHeaders.ContentType, header.Key, StringComparison.OrdinalIgnoreCase))
                {
                    content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        private async Task HandleResponseAsync(HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, IConsoleManager consoleManager, HttpResponseMessage response, bool echoRequest, string headersTargetFile, string bodyTargetFile, CancellationToken cancellationToken)
        {
            string protocolInfo;

            if (echoRequest)
            {
                RequestConfig requestConfig = new RequestConfig(_preferences);
                string hostString = response.RequestMessage.RequestUri.Scheme + "://" + response.RequestMessage.RequestUri.Host + (!response.RequestMessage.RequestUri.IsDefaultPort ? ":" + response.RequestMessage.RequestUri.Port : "");
                await HandleEchoRequest(commandInput, consoleManager, programState, response, requestConfig, hostString, cancellationToken);

                // Only need to write out this separator if we've echoed the request
                consoleManager.WriteLine();
                consoleManager.WriteLine($"Response from {hostString}...".SetColor(requestConfig.AddressColor));
                consoleManager.WriteLine();
            }

            ResponseConfig responseConfig = new ResponseConfig(_preferences);

            protocolInfo = $"{"HTTP".SetColor(responseConfig.ProtocolNameColor)}{"/".SetColor(responseConfig.ProtocolSeparatorColor)}{response.Version.ToString().SetColor(responseConfig.ProtocolVersionColor)}";
            string status = ((int)response.StatusCode).ToString().SetColor(responseConfig.StatusCodeColor) + " " + response.ReasonPhrase.SetColor(responseConfig.StatusReasonPhraseColor);

            consoleManager.WriteLine($"{protocolInfo} {status}");

            IEnumerable<KeyValuePair<string, IEnumerable<string>>> responseHeaders = response.Headers;

            if (response.Content != null)
            {
                responseHeaders = responseHeaders.Union(response.Content.Headers);
            }

            List<string> headerFileOutput = null;
            List<string> bodyFileOutput = null;

            if (headersTargetFile != null)
            {
                headerFileOutput = new List<string>();
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in responseHeaders.OrderBy(x => x.Key))
            {
                string headerKey = header.Key.SetColor(responseConfig.HeaderKeyColor);
                string headerSep = ":".SetColor(responseConfig.HeaderSeparatorColor);
                string headerValue = string.Join(";".SetColor(responseConfig.HeaderValueSeparatorColor), header.Value.Select(x => x.Trim().SetColor(responseConfig.HeaderValueColor)));
                consoleManager.WriteLine($"{headerKey}{headerSep} {headerValue}");
                headerFileOutput?.Add($"{header.Key}: {string.Join(";", header.Value.Select(x => x.Trim()))}");
            }

            if (bodyTargetFile != null)
            {
                bodyFileOutput = new List<string>();
            }

            consoleManager.WriteLine();

            if (response.Content != null)
            {
                await FormatBodyAsync(commandInput, programState, consoleManager, response.Content, bodyFileOutput, _preferences, cancellationToken).ConfigureAwait(false);
            }

            if (headersTargetFile != null && headerFileOutput != null)
            {
                _fileSystem.WriteAllLinesToFile(headersTargetFile, headerFileOutput);
            }

            if (bodyTargetFile != null && bodyFileOutput != null)
            {
                _fileSystem.WriteAllLinesToFile(bodyTargetFile, bodyFileOutput);
            }

            consoleManager.WriteLine();
        }

        private async Task HandleEchoRequest(DefaultCommandInput<ICoreParseResult> commandInput,
            IConsoleManager consoleManager,
            HttpState programState,
            HttpResponseMessage response,
            RequestConfig requestConfig,
            string hostString,
            CancellationToken cancellationToken)
        {
            consoleManager.WriteLine($"Request to {hostString}...".SetColor(requestConfig.AddressColor));
            consoleManager.WriteLine();

            string method = response.RequestMessage.Method.ToString().ToUpperInvariant().SetColor(requestConfig.MethodColor);
            string pathAndQuery = response.RequestMessage.RequestUri.PathAndQuery.SetColor(requestConfig.AddressColor);
            string protocolInfo = $"{"HTTP".SetColor(requestConfig.ProtocolNameColor)}{"/".SetColor(requestConfig.ProtocolSeparatorColor)}{response.Version.ToString().SetColor(requestConfig.ProtocolVersionColor)}";

            consoleManager.WriteLine($"{method} {pathAndQuery} {protocolInfo}");
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> requestHeaders = response.RequestMessage.Headers;

            if (response.RequestMessage.Content != null)
            {
                requestHeaders = requestHeaders.Union(response.RequestMessage.Content.Headers);
            }

            foreach (KeyValuePair<string, IEnumerable<string>> header in requestHeaders.OrderBy(x => x.Key))
            {
                string headerKey = header.Key.SetColor(requestConfig.HeaderKeyColor);
                string headerSep = ":".SetColor(requestConfig.HeaderSeparatorColor);
                string headerValue = string.Join(";".SetColor(requestConfig.HeaderValueSeparatorColor), header.Value.Select(x => x.Trim().SetColor(requestConfig.HeaderValueColor)));
                consoleManager.WriteLine($"{headerKey}{headerSep} {headerValue}");
            }

            consoleManager.WriteLine();

            List<string> responseOutput = new List<string>();

            if (response.RequestMessage.Content != null)
            {
                await FormatBodyAsync(commandInput, programState, consoleManager, response.RequestMessage.Content, responseOutput, _preferences, cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task FormatBodyAsync(DefaultCommandInput<ICoreParseResult> commandInput, HttpState programState, IConsoleManager consoleManager, HttpContent content, List<string> bodyFileOutput, IPreferences preferences, CancellationToken cancellationToken)
        {
            if (commandInput.Options[StreamingOption].Count > 0)
            {
                Memory<char> buffer = new Memory<char>(new char[2048]);

#if NET5_0
                Stream s = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
                Stream s = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

                using (StreamReader reader = new StreamReader(s))
                {
                    consoleManager.WriteLine(Resources.Strings.BaseHttpCommand_FormatBodyAsync_Streaming.SetColor(programState.WarningColor));

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            Task<int> readTask = reader.ReadAsync(buffer, cancellationToken).AsTask();
                            if (await WaitForCompletionAsync(readTask, cancellationToken).ConfigureAwait(false))
                            {
                                if (readTask.Result == 0)
                                {
                                    break;
                                }

                                string str = new string(buffer.Span.Slice(0, readTask.Result));
                                consoleManager.Write(str);
                                bodyFileOutput.Add(str);
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    }
                }

                return;
            }

            await FormatResponseContentAsync(commandInput, consoleManager, content, bodyFileOutput, preferences);
        }

        private static async Task FormatResponseContentAsync(DefaultCommandInput<ICoreParseResult> commandInput,
            IConsoleManager consoleManager,
            HttpContent content,
            List<string> bodyFileOutput,
            IPreferences preferences)
        {
            string contentType = null;
            if (content.Headers.TryGetValues("Content-Type", out IEnumerable<string> contentTypeValues))
            {
                contentType = contentTypeValues.FirstOrDefault()?.Split(';').FirstOrDefault();
            }

            contentType = contentType?.ToUpperInvariant() ?? "text/plain";

            if (commandInput.Options[NoFormattingOption].Count == 0)
            {
                if (contentType.EndsWith("/JSON", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("-JSON", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("+JSON", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("/JAVASCRIPT", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("-JAVASCRIPT", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("+JAVASCRIPT", StringComparison.OrdinalIgnoreCase))
                {
                    if (await FormatJsonAsync(consoleManager, content, bodyFileOutput, preferences))
                    {
                        return;
                    }
                }
                else if (contentType.EndsWith("/HTML", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("-HTML", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("+HTML", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("/XML", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("-XML", StringComparison.OrdinalIgnoreCase)
                    || contentType.EndsWith("+XML", StringComparison.OrdinalIgnoreCase))
                {
                    if (await FormatXmlAsync(consoleManager, content, bodyFileOutput))
                    {
                        return;
                    }
                }
            }

            string responseContent = await content.ReadAsStringAsync().ConfigureAwait(false);
            bodyFileOutput?.Add(responseContent);
            consoleManager.WriteLine(responseContent);
        }

        private static async Task<bool> WaitForCompletionAsync(Task<int> readTask, CancellationToken cancellationToken)
        {
            while (!readTask.IsCompleted && !cancellationToken.IsCancellationRequested && !Console.KeyAvailable)
            {
                await Task.Delay(1, cancellationToken).ConfigureAwait(false);
            }

            if (Console.KeyAvailable)
            {
                Console.ReadKey(false);
                return false;
            }

            return readTask.IsCompleted;
        }

        private static async Task<bool> FormatXmlAsync(IWritable consoleManager, HttpContent content, List<string> bodyFileOutput)
        {
            string responseContent = await content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                XDocument body = XDocument.Parse(responseContent);
                consoleManager.WriteLine(body.ToString());
                bodyFileOutput?.Add(body.ToString());
                return true;
            }
            catch
            {
            }

            return false;
        }

        private static async Task<bool> FormatJsonAsync(IWritable outputSink, HttpContent content, List<string> bodyFileOutput, IPreferences preferences)
        {
            string responseContent = await content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                JsonConfig config = new JsonConfig(preferences);
                string formatted = JsonVisitor.FormatAndColorize(config, responseContent);
                outputSink.WriteLine(formatted);
                bodyFileOutput?.Add(JToken.Parse(responseContent).ToString());
                return true;
            }
            catch
            {
            }

            return false;
        }

        protected override string GetHelpDetails(IShellState shellState, HttpState programState, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult)
        {
            StringBuilder helpText = new StringBuilder();
            helpText.Append(Strings.Usage.Bold());
            helpText.AppendLine($"{Verb.ToUpperInvariant()} [Options]");
            helpText.AppendLine();
            helpText.AppendLine($"Issues a {Verb.ToUpperInvariant()} request.");

            if (RequiresBody)
            {
                helpText.AppendLine("Your default editor will be opened with a sample body if no options are provided.");
            }

            return helpText.ToString();
        }

        public override string GetHelpSummary(IShellState shellState, HttpState programState)
        {
#pragma warning disable CA1308 // Normalize strings to uppercase
            return $"{Verb.ToLowerInvariant()} - Issues a {Verb.ToUpperInvariant()} request";
#pragma warning restore CA1308 // Normalize strings to uppercase
        }

        protected override IEnumerable<string> GetArgumentSuggestionsForText(IShellState shellState, HttpState programState, ICoreParseResult parseResult, DefaultCommandInput<ICoreParseResult> commandInput, string normalCompletionString)
        {
            programState = programState ?? throw new ArgumentNullException(nameof(programState));

            List<string> results = new List<string>();

            if (programState.Structure is object && programState.BaseAddress is object)
            {
                parseResult = parseResult ?? throw new ArgumentNullException(nameof(parseResult));

                //If it's an absolute URI, nothing to suggest
                if (Uri.TryCreate(parseResult.Sections[1], UriKind.Absolute, out Uri _))
                {
                    return null;
                }

                normalCompletionString = normalCompletionString ?? throw new ArgumentNullException(nameof(normalCompletionString));

                string path = normalCompletionString.Replace('\\', '/');
                int searchFrom = normalCompletionString.Length - 1;
                int lastSlash = path.LastIndexOf('/', searchFrom);
                string prefix;

                if (lastSlash < 0)
                {
                    path = string.Empty;
                    prefix = normalCompletionString;
                }
                else
                {
                    path = path.Substring(0, lastSlash + 1);
                    prefix = normalCompletionString.Substring(lastSlash + 1);
                }

                IDirectoryStructure s = programState.Structure.TraverseTo(programState.PathSections.Reverse()).TraverseTo(path);

                foreach (string child in s.DirectoryNames)
                {
                    if (child.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(path + child);
                    }
                }
            }

            return results;
        }

        protected override IEnumerable<string> GetOptionValueCompletions(IShellState shellState, HttpState programState, string optionId, DefaultCommandInput<ICoreParseResult> commandInput, ICoreParseResult parseResult, string normalizedCompletionText)
        {
            if (string.Equals(optionId, BodyFileOption, StringComparison.Ordinal) ||
                string.Equals(optionId, ResponseBodyFileOption, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(optionId, ResponseHeadersFileOption, StringComparison.OrdinalIgnoreCase))
            {
                return FileSystemCompletion.GetCompletions(normalizedCompletionText);
            }

            if (string.Equals(optionId, HeaderOption, StringComparison.Ordinal))
            {
                commandInput = commandInput ?? throw new ArgumentNullException(nameof(commandInput));

                normalizedCompletionText = normalizedCompletionText ?? throw new ArgumentNullException(nameof(normalizedCompletionText));

                HashSet<string> alreadySpecifiedHeaders = new HashSet<string>(StringComparer.Ordinal);
                IReadOnlyList<InputElement> options = commandInput.Options[HeaderOption];
                for (int i = 0; i < options.Count; ++i)
                {
                    if (options[i] == commandInput.SelectedElement)
                    {
                        continue;
                    }

                    string elementText = options[i].Text;
                    string existingHeaderName = elementText.Split(HeaderSeparatorChars)[0];
                    alreadySpecifiedHeaders.Add(existingHeaderName);
                }

                //Check to see if the selected element is in a header name or value
                int equalsIndex = normalizedCompletionText.IndexOfAny(HeaderSeparatorChars);
                string path = commandInput.Arguments.Count > 0 ? commandInput.Arguments[0].Text : string.Empty;

                if (equalsIndex < 0)
                {
                    IEnumerable<string> headerNameOptions = HeaderCompletion.GetCompletions(alreadySpecifiedHeaders, normalizedCompletionText);

                    List<string> allSuggestions = new List<string>();
                    foreach (string suggestion in headerNameOptions.Select(x => x))
                    {
                        allSuggestions.Add(suggestion + ":");

                        IEnumerable<string> suggestions = HeaderCompletion.GetValueCompletions(Verb, path, suggestion, string.Empty, programState);

                        if (suggestions != null)
                        {
                            foreach (string valueSuggestion in suggestions)
                            {
                                allSuggestions.Add(suggestion + ":" + valueSuggestion);
                            }
                        }
                    }

                    return allSuggestions;
                }
                else
                {
                    //Didn't exit from the header name check, so must be a value
                    string headerName = normalizedCompletionText.Substring(0, equalsIndex);
                    IEnumerable<string> suggestions = HeaderCompletion.GetValueCompletions(Verb, path, headerName, normalizedCompletionText.Substring(equalsIndex + 1), programState);

                    if (suggestions == null)
                    {
                        return null;
                    }

                    return suggestions.Select(x => normalizedCompletionText.Substring(0, equalsIndex + 1) + x);
                }
            }

            return null;
        }

        private static string GetExampleBody(string path, ref string contentType, string method, HttpState httpState)
        {
            Uri effectivePath = httpState.GetEffectivePath(path);
            string rootRelativePath = effectivePath.LocalPath.Substring(httpState.BaseAddress.LocalPath.Length).TrimStart('/');
            IDirectoryStructure structure = httpState.Structure?.TraverseTo(rootRelativePath);
            return structure?.RequestInfo?.GetRequestBodyForContentType(ref contentType, method);
        }

        private void SendTelemetry(DefaultCommandInput<ICoreParseResult> commandInput)
        {
            HttpCommandEvent httpCommandEvent = new HttpCommandEvent(
                method:                         Verb.ToUpperInvariant(),
                isPathSpecified:                commandInput.Arguments.Count > 0,
                isHeaderSpecified:              commandInput.Options[HeaderOption].Any(),
                isResponseHeadersFileSpecified: commandInput.Options[ResponseHeadersFileOption].Any(),
                isResponseBodyFileSpecified:    commandInput.Options[ResponseBodyFileOption].Any(),
                isNoFormattingSpecified:        commandInput.Options[NoFormattingOption].Any(),
                isStreamingSpecified:           commandInput.Options[StreamingOption].Any(),
                isNoBodySpecified:              RequiresBody && commandInput.Options[NoBodyOption].Any(),
                isRequestBodyFileSpecified:     RequiresBody && commandInput.Options[BodyFileOption].Any(),
                isRequestBodyContentSpecified:  RequiresBody && commandInput.Options[BodyContentOption].Any()
            );

            _telemetry.TrackEvent(httpCommandEvent);
        }
    }
}
