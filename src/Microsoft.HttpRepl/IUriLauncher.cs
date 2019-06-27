using System;
using System.Threading.Tasks;

namespace Microsoft.HttpRepl
{
    public interface IUriLauncher
    {
        Task LaunchUriAsync(Uri uri);
    }
}
