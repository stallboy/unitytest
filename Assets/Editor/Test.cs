using System.Text;
using UnityEditor;
using UnityEngine;

public class Test
{
    [MenuItem("Tool/Build AssetBundle")]
    public static void BuildAssetBundle()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets",
            BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.UncompressedAssetBundle,
            BuildTarget.StandaloneWindows);
    }

    [MenuItem("Tool/PrintDep")]
    public static void TestPrintDep()
    {
        Debug.Log("texture size=" +
                  Profiler.GetRuntimeMemorySize(
                      AssetDatabase.LoadAssetAtPath<Object>("Assets/testassetbundlebuildrule/texture.psd")));
        Debug.Log("mat size=" +
                  Profiler.GetRuntimeMemorySize(
                      AssetDatabase.LoadAssetAtPath<Object>("Assets/testassetbundlebuildrule/mat.mat")));

        printDep("Assets/testassetbundlebuildrule/mat.mat", 0);

        printDep("Assets/testassetbundlebuildrule/prefab2.prefab", 0);
    }

    private static void printDep(string asset, int depth)
    {
        //var obj = AssetDatabase.LoadAssetAtPath<Object>(asset);
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < depth; i++)
        {
            sb.Append("    ");
        }
        sb.Append(asset);
        sb.Append(", ");
        var objs = AssetDatabase.LoadAllAssetsAtPath(asset);
        var allsize = 0;
        foreach (var obj in objs)
        {
            var size = Profiler.GetRuntimeMemorySize(obj);
            allsize += size;
            sb.Append("obj=" + obj + ",size=" + size + "\n");
        }
        
        
        Debug.Log(sb);
        
        foreach (var depasset in AssetDatabase.GetDependencies(asset, false))
        {
            printDep(depasset, depth + 1);
        }
    }
}