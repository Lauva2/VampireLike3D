namespace VampireLike
{
    // Panel-specific subclasses own presentation data, never View objects or AA handles.
    public abstract class UIModel
    {
        public abstract void Refresh(GameManager game);
        public virtual void OnClose() { }
    }
}
