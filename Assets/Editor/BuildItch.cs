using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build WebGL prête à envoyer sur itch.io.
//
// Dans Unity : menu Build > WebGL pour itch.io.
// En ligne de commande :
//   Unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod BuildItch.BuildFromCommandLine
//
// Résultat : Builds/WebGL (le jeu) et Builds/Imposteur-itch.zip (à envoyer sur itch.io).
public static class BuildItch
{
    private const string BuildFolder = "Builds/WebGL";
    private const string ZipPath = "Builds/Imposteur-itch.zip";

    [MenuItem("Build/WebGL pour itch.io")]
    public static void BuildFromMenu()
    {
        if (Build())
            EditorUtility.RevealInFinder(ZipPath);
    }

    public static void BuildFromCommandLine()
    {
        EditorApplication.Exit(Build() ? 0 : 1);
    }

    public static void ApplySettings()
    {
        // Modèle de page du projet : format portrait, chargement, bouton plein écran.
        PlayerSettings.WebGL.template = "PROJECT:Imposteur";

        // itch.io ne renvoie pas l'en-tête "Content-Encoding" pour les fichiers .br/.gz :
        // sans "decompression fallback", le jeu ne se charge pas (page blanche).
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
        PlayerSettings.WebGL.nameFilesAsHashes = false;

        // L'hôte d'une partie en ligne doit continuer à tourner si la page perd le focus.
        PlayerSettings.runInBackground = true;

        // Taille par défaut de la page (portrait), reprise sur la page itch.io.
        PlayerSettings.defaultWebScreenWidth = 540;
        PlayerSettings.defaultWebScreenHeight = 960;

        AssetDatabase.SaveAssets();
    }

    private static bool Build()
    {
        ApplySettings();

        if (Directory.Exists(BuildFolder))
            Directory.Delete(BuildFolder, true);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = BuildFolder,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"[BuildItch] Échec de la build : {report.summary.result}");
            return false;
        }

        // itch.io attend un zip avec index.html à la racine.
        if (File.Exists(ZipPath))
            File.Delete(ZipPath);

        ZipFile.CreateFromDirectory(BuildFolder, ZipPath, System.IO.Compression.CompressionLevel.Optimal, false);

        long size = new FileInfo(ZipPath).Length;
        Debug.Log($"[BuildItch] Build terminée : {Path.GetFullPath(ZipPath)} ({size / (1024f * 1024f):0.0} Mo)");

        return true;
    }
}
