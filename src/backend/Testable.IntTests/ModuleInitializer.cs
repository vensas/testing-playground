using System.Runtime.CompilerServices;

namespace Testable.IntTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Initialize() =>
        VerifierSettings.InitializePlugins();
}