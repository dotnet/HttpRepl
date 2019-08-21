﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.HttpRepl.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.HttpRepl.Resources.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Append the given directory to the currently selected path, or move up a path when using `cd ..`.
        /// </summary>
        internal static string ChangeDirectoryCommand_HelpSummary {
            get {
                return ResourceManager.GetString("ChangeDirectoryCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning: The &apos;{0}&apos; endpoint is not present in the Swagger metadata.
        /// </summary>
        internal static string ChangeDirectoryCommand_Warning_UnknownEndpoint {
            get {
                return ResourceManager.GetString("ChangeDirectoryCommand_Warning_UnknownEndpoint", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Removes all text from the shell.
        /// </summary>
        internal static string ClearCommand_HelpSummary {
            get {
                return ResourceManager.GetString("ClearCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configures the directory structure and base address of the api server.
        /// </summary>
        internal static string ConnectCommand_Description {
            get {
                return ResourceManager.GetString("ConnectCommand_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The base address must be a valid absolute url or relative url. If it is a relative url, the root address must be specified.
        /// </summary>
        internal static string ConnectCommand_Error_InvalidBase {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_InvalidBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The swagger address must be a valid absolute url or relative url. If it is a relative url, the root address must be specified.
        /// </summary>
        internal static string ConnectCommand_Error_InvalidSwagger {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_InvalidSwagger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If no root address is specified, the base address must be an absolute url, including scheme.
        /// </summary>
        internal static string ConnectCommand_Error_NoRootNoAbsoluteBase {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_NoRootNoAbsoluteBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If no root address is specified, the swagger address must be an absolute url, including scheme.
        /// </summary>
        internal static string ConnectCommand_Error_NoRootNoAbsoluteSwagger {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_NoRootNoAbsoluteSwagger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You must specify either a root address or a base address and a swagger address.
        /// </summary>
        internal static string ConnectCommand_Error_NothingSpecified {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_NothingSpecified", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If specified, the root address must be a valid absolute url, including scheme.
        /// </summary>
        internal static string ConnectCommand_Error_RootAddressNotValid {
            get {
                return ResourceManager.GetString("ConnectCommand_Error_RootAddressNotValid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configures the directory structure and base address of the api server based on the arguments and options specified. At least one of [rootAddress], [--base baseAddress] or [--swagger swaggerAddress] must be specified.
        /// </summary>
        internal static string ConnectCommand_HelpDetails_Line1 {
            get {
                return ResourceManager.GetString("ConnectCommand_HelpDetails_Line1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [rootAddress] will be used to automatically determine the base address and swagger address.
        /// </summary>
        internal static string ConnectCommand_HelpDetails_Line2 {
            get {
                return ResourceManager.GetString("ConnectCommand_HelpDetails_Line2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [--base baseAddress] and [--swagger swaggerAddress] allow you to explicitly set those addresses and skip auto detection.
        /// </summary>
        internal static string ConnectCommand_HelpDetails_Line3 {
            get {
                return ResourceManager.GetString("ConnectCommand_HelpDetails_Line3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using a base address of {0}.
        /// </summary>
        internal static string ConnectCommand_Status_Base {
            get {
                return ResourceManager.GetString("ConnectCommand_Status_Base", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to determine a base address.
        /// </summary>
        internal static string ConnectCommand_Status_NoBase {
            get {
                return ResourceManager.GetString("ConnectCommand_Status_NoBase", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to find a swagger definition.
        /// </summary>
        internal static string ConnectCommand_Status_NoSwagger {
            get {
                return ResourceManager.GetString("ConnectCommand_Status_NoSwagger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using swagger definition at {0}.
        /// </summary>
        internal static string ConnectCommand_Status_Swagger {
            get {
                return ResourceManager.GetString("ConnectCommand_Status_Swagger", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Turns request echoing on or off, show the request that was made when using request commands.
        /// </summary>
        internal static string EchoCommand_HelpSummary {
            get {
                return ResourceManager.GetString("EchoCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &apos;set base {url}&apos; must be called before issuing requests to a relative path.
        /// </summary>
        internal static string Error_NoBasePath {
            get {
                return ResourceManager.GetString("Error_NoBasePath", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot start the REPL when output is being redirected.
        /// </summary>
        internal static string Error_OutputRedirected {
            get {
                return ResourceManager.GetString("Error_OutputRedirected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Exit the shell.
        /// </summary>
        internal static string ExitCommand_HelpSummary {
            get {
                return ResourceManager.GetString("ExitCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Arguments:.
        /// </summary>
        internal static string Help_Arguments {
            get {
                return ResourceManager.GetString("Help_Arguments", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   {0} - The initial base address for the REPL..
        /// </summary>
        internal static string Help_BaseAddress {
            get {
                return ResourceManager.GetString("Help_BaseAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to   {0} - Show help information..
        /// </summary>
        internal static string Help_Help {
            get {
                return ResourceManager.GetString("Help_Help", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Options:.
        /// </summary>
        internal static string Help_Options {
            get {
                return ResourceManager.GetString("Help_Options", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Once the REPL starts, these commands are valid:.
        /// </summary>
        internal static string Help_REPLCommands {
            get {
                return ResourceManager.GetString("Help_REPLCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: .
        /// </summary>
        internal static string Help_Usage {
            get {
                return ResourceManager.GetString("Help_Usage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to REPL Customization Commands:.
        /// </summary>
        internal static string HelpCommand_Core_CustomizationCommands {
            get {
                return ResourceManager.GetString("HelpCommand_Core_CustomizationCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use these commands to customize the REPL behavior.
        /// </summary>
        internal static string HelpCommand_Core_CustomizationCommands_Description {
            get {
                return ResourceManager.GetString("HelpCommand_Core_CustomizationCommands_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use `help &lt;COMMAND&gt;` for more detail on an individual command. e.g. `help get`.
        /// </summary>
        internal static string HelpCommand_Core_Details_Line1 {
            get {
                return ResourceManager.GetString("HelpCommand_Core_Details_Line1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to For detailed tool info, see https://aka.ms/http-repl-doc.
        /// </summary>
        internal static string HelpCommand_Core_Details_Line2 {
            get {
                return ResourceManager.GetString("HelpCommand_Core_Details_Line2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP Commands:.
        /// </summary>
        internal static string HelpCommand_Core_HttpCommands {
            get {
                return ResourceManager.GetString("HelpCommand_Core_HttpCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use these commands to execute requests against your application.
        /// </summary>
        internal static string HelpCommand_Core_HttpCommands_Description {
            get {
                return ResourceManager.GetString("HelpCommand_Core_HttpCommands_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Navigation Commands:.
        /// </summary>
        internal static string HelpCommand_Core_NavigationCommands {
            get {
                return ResourceManager.GetString("HelpCommand_Core_NavigationCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The REPL allows you to navigate your URL space and focus on specific APIs that you are working on.
        /// </summary>
        internal static string HelpCommand_Core_NavigationCommands_Description {
            get {
                return ResourceManager.GetString("HelpCommand_Core_NavigationCommands_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Setup Commands:.
        /// </summary>
        internal static string HelpCommand_Core_SetupCommands {
            get {
                return ResourceManager.GetString("HelpCommand_Core_SetupCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use these commands to configure the tool for your API server.
        /// </summary>
        internal static string HelpCommand_Core_SetupCommands_Description {
            get {
                return ResourceManager.GetString("HelpCommand_Core_SetupCommands_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Shell Commands:.
        /// </summary>
        internal static string HelpCommand_Core_ShellCommands {
            get {
                return ResourceManager.GetString("HelpCommand_Core_ShellCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use these commands to interact with the REPL shell.
        /// </summary>
        internal static string HelpCommand_Core_ShellCommands_Description {
            get {
                return ResourceManager.GetString("HelpCommand_Core_ShellCommands_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to If {0} is not an absolute URI, {1} must be specified..
        /// </summary>
        internal static string HttpState_Error_NoAbsoluteUriNoBaseAddress {
            get {
                return ResourceManager.GetString("HttpState_Error_NoAbsoluteUriNoBaseAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No base address has been set, so there is nothing to list. Use the &quot;set base&quot; command to set a base address..
        /// </summary>
        internal static string ListCommand_Error_NoBaseAddress {
            get {
                return ResourceManager.GetString("ListCommand_Error_NoBaseAddress", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No directory structure has been set, so there is nothing to list. Use the &quot;set swagger&quot; command to set a directory structure based on a swagger definition..
        /// </summary>
        internal static string ListCommand_Error_NoDirectoryStructure {
            get {
                return ResourceManager.GetString("ListCommand_Error_NoDirectoryStructure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Show all endpoints for the current path.
        /// </summary>
        internal static string ListCommand_HelpSummary {
            get {
                return ResourceManager.GetString("ListCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} does not have a configured value.
        /// </summary>
        internal static string PrefCommand_Error_NoConfiguredValue {
            get {
                return ResourceManager.GetString("PrefCommand_Error_NoConfiguredValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Whether to get or set a preference must be specified.
        /// </summary>
        internal static string PrefCommand_Error_NoGetOrSet {
            get {
                return ResourceManager.GetString("PrefCommand_Error_NoGetOrSet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The preference to set must be specified.
        /// </summary>
        internal static string PrefCommand_Error_NoPreferenceName {
            get {
                return ResourceManager.GetString("PrefCommand_Error_NoPreferenceName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error saving preferences.
        /// </summary>
        internal static string PrefCommand_Error_Saving {
            get {
                return ResourceManager.GetString("PrefCommand_Error_Saving", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Configured value: {0}.
        /// </summary>
        internal static string PrefCommand_Get_ConfiguredValue {
            get {
                return ResourceManager.GetString("PrefCommand_Get_ConfiguredValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current Preferences:.
        /// </summary>
        internal static string PrefCommand_HelpDetails_CurrentPreferences {
            get {
                return ResourceManager.GetString("PrefCommand_HelpDetails_CurrentPreferences", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Current Default Preferences:.
        /// </summary>
        internal static string PrefCommand_HelpDetails_DefaultPreferences {
            get {
                return ResourceManager.GetString("PrefCommand_HelpDetails_DefaultPreferences", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - Gets the value of the specified preference or lists all preferences if no preference is specified.
        /// </summary>
        internal static string PrefCommand_HelpDetails_GetSyntax {
            get {
                return ResourceManager.GetString("PrefCommand_HelpDetails_GetSyntax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - Sets (or clears if value is not specified) the value of the specified preference.
        /// </summary>
        internal static string PrefCommand_HelpDetails_SetSyntax {
            get {
                return ResourceManager.GetString("PrefCommand_HelpDetails_SetSyntax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} - Get or sets a preference to a particular value.
        /// </summary>
        internal static string PrefCommand_HelpDetails_Syntax {
            get {
                return ResourceManager.GetString("PrefCommand_HelpDetails_Syntax", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Allows viewing or changing preferences, e.g. &apos;pref set editor.command.default &apos;C:\\Program Files\\Microsoft VS Code\\Code.exe&apos;`.
        /// </summary>
        internal static string PrefCommand_HelpSummary {
            get {
                return ResourceManager.GetString("PrefCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Could not find script file {0}.
        /// </summary>
        internal static string RunCommand_CouldNotFindScriptFile {
            get {
                return ResourceManager.GetString("RunCommand_CouldNotFindScriptFile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to run {path to script}
        ///
        ///Runs the specified script.
        ///A script is a text file containing one CLI command per line. Each line will be run as if it was typed into the CLI.
        ///
        ///When +history option is specified, commands specified in the text file will be added to command history..
        /// </summary>
        internal static string RunCommand_HelpDetails {
            get {
                return ResourceManager.GetString("RunCommand_HelpDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Runs the script at the given path. A script is a set of commands that can be typed with one command per line.
        /// </summary>
        internal static string RunCommand_HelpSummary {
            get {
                return ResourceManager.GetString("RunCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Warning: HEAD request to the specified address was unsuccessful {0}.
        /// </summary>
        internal static string SetBaseCommand_HEADRequestUnSuccessful {
            get {
                return ResourceManager.GetString("SetBaseCommand_HEADRequestUnSuccessful", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Set the base URI. e.g. `set base http://locahost:5000`.
        /// </summary>
        internal static string SetBaseCommand_HelpSummary {
            get {
                return ResourceManager.GetString("SetBaseCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must specify a server.
        /// </summary>
        internal static string SetBaseCommand_MustSpecifyServerError {
            get {
                return ResourceManager.GetString("SetBaseCommand_MustSpecifyServerError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Using swagger metadata from .
        /// </summary>
        internal static string SetBaseCommand_SwaggerMetadataUriLocation {
            get {
                return ResourceManager.GetString("SetBaseCommand_SwaggerMetadataUriLocation", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sets or clears a header. When [value] is empty the header is cleared..
        /// </summary>
        internal static string SetHeaderCommand_HelpDetails {
            get {
                return ResourceManager.GetString("SetHeaderCommand_HelpDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sets or clears a header for all requests. e.g. `set header content-type application/json`.
        /// </summary>
        internal static string SetHeaderCommand_HelpSummary {
            get {
                return ResourceManager.GetString("SetHeaderCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sets the swagger document to use for information about the current server.
        /// </summary>
        internal static string SetSwaggerCommand_Description {
            get {
                return ResourceManager.GetString("SetSwaggerCommand_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Set the URI, relative to your base if set, of the Swagger document for this API. e.g. `set swagger /swagger/v1/swagger.json`.
        /// </summary>
        internal static string SetSwaggerCommand_HelpSummary {
            get {
                return ResourceManager.GetString("SetSwaggerCommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must specify a valid swagger document.
        /// </summary>
        internal static string SetSwaggerCommand_InvalidSwaggerUri {
            get {
                return ResourceManager.GetString("SetSwaggerCommand_InvalidSwaggerUri", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must specify a swagger document.
        /// </summary>
        internal static string SetSwaggerCommand_SpecifySwaggerDocument {
            get {
                return ResourceManager.GetString("SetSwaggerCommand_SpecifySwaggerDocument", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ui - Launches the Swagger UI page (if available) in the default browser.
        /// </summary>
        internal static string UICommand_Description {
            get {
                return ResourceManager.GetString("UICommand_Description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Displays the Swagger UI page, if available, in the default browser.
        /// </summary>
        internal static string UICommand_HelpSummary {
            get {
                return ResourceManager.GetString("UICommand_HelpSummary", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Must be connected to a server to launch Swagger UI.
        /// </summary>
        internal static string UICommand_NotConnectedToServerError {
            get {
                return ResourceManager.GetString("UICommand_NotConnectedToServerError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unable to launch {0}.
        /// </summary>
        internal static string UICommand_UnableToLaunchUriError {
            get {
                return ResourceManager.GetString("UICommand_UnableToLaunchUriError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Usage: .
        /// </summary>
        internal static string Usage {
            get {
                return ResourceManager.GetString("Usage", resourceCulture);
            }
        }
    }
}
