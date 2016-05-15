namespace Assets.Scripts.InMaze.UI.Mapify
{
    public interface IMiniMapEvents
    {
        void SyncUpdate(float x, float y);
        void BeginNextChunk();
    }
}