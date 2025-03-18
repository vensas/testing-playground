namespace Testable.Tests;

public abstract class VerifyTestBase
{
    protected VerifyTestBase()
    {
        VerifySettings = new VerifySettings();
        VerifySettings.UseDirectory("./snapshots");
    }
    
    public VerifySettings VerifySettings { get; }
}