using UnityEditor;

public class Test  {

    [MenuItem("Tool/Build AssetBundle")]
    static void BuildAssetBundle ()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.DeterministicAssetBundle, BuildTarget.StandaloneWindows);
    }
	
}
