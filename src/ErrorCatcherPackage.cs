using Microsoft.VisualStudio.Shell;
using System;
using System.Runtime.InteropServices;

namespace ErrorCatcher
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid("a6ea2ef8-a48a-4ebd-89d5-16b1ba16f5e3")]
    public sealed class ErrorCatcherPackage : AsyncPackage
    {
    }
}
