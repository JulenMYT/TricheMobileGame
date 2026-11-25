using System.Runtime.InteropServices;

// Force l'enregistrement des fichiers de Application.persistentDataPath.
// Utile uniquement dans un navigateur (WebGL) : ailleurs, l'écriture est immédiate.
public static class PersistentFiles
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SyncPersistentFiles();
#endif

    public static void Flush()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SyncPersistentFiles();
#endif
    }
}
