public class UIBase
{
    protected UITKEventHelper EventHelper;

    /// <summary>
    /// Base Setup will initiate instance of the eventhelper that is used to register and unregister callbacks!
    /// </summary>
    public virtual void Setup()
    {
        EventHelper ??= new UITKEventHelper();
    }

    /// <summary>
    /// Disposes all callbacks of the eventHelper, this is used for general cleanup and to prevent triggering requests twice through
    /// double clicking.
    /// </summary>
    public void DisposeCallbacks()
    {
        EventHelper?.UnregisterAllCallbacks();
    }

    /// <summary>
    /// General disposal that is necessary to cleanup ui controllers
    /// </summary>
    public virtual void Dispose()
    {
        DisposeCallbacks();
    }
}
