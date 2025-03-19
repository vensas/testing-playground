namespace Testable.IntTests;

public abstract class VerifyTestBase
{
    protected VerifyTestBase()
    {
        VerifySettings = new VerifySettings();
        VerifySettings.UseDirectory("./snapshots");
        VerifySettings.ScrubInlineGuids();
    }
    
    public VerifySettings VerifySettings { get; }
}