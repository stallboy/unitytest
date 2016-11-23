using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BuildSystem
{
    public class ABChecker
    {
        private class DepInfo
        {
            public HashSet<string> containingABs = new HashSet<string>();

            public int memsize;
        }

        private static Dictionary<string, DepInfo> depasset2bundles;
        private static Dictionary<string, string> asset2bundle;

        private static void collectDep(string asset, string bundle, int depth)
        {
            /*
            if (depth == 0)
            {
                Debug.Log(asset + " --- " + bundle);
            }
            else if (depth < 10)
            {
                string res = "";
                for (int i = 0; i < depth; i++)
                {
                    res += "    ";
                }
                Debug.Log(res + asset);
            }
            */

            foreach (var depasset in AssetDatabase.GetDependencies(asset, false))
            {
                if (!depasset.EndsWith(".cs") && !asset2bundle.ContainsKey(depasset))
                {
                    DepInfo res;
                    if (depasset2bundles.TryGetValue(depasset, out res))
                    {
                        res.containingABs.Add(bundle);
                    }
                    else
                    {
                        res = new DepInfo();
                        res.containingABs.Add(bundle);
                        depasset2bundles.Add(depasset, res);
                    }
                    collectDep(depasset, bundle, depth + 1);
                }
            }
        }

        [MenuItem("Tool/Check AssetBundle Duplicate")]
        public static void CheckDuplicate()
        {
            asset2bundle = new Dictionary<string, string>();
            foreach (var bundle in AssetDatabase.GetAllAssetBundleNames())
            {
                foreach (var asset in AssetDatabase.GetAssetPathsFromAssetBundle(bundle))
                {
                    asset2bundle.Add(asset, bundle);
                }
            }

            Debug.Log("asset count=" + asset2bundle.Count);
            depasset2bundles = new Dictionary<string, DepInfo>();
            foreach (var kv in asset2bundle)
            {
                collectDep(kv.Key, kv.Value, 0);
            }

            int canSaveMemSize = 0;
            foreach (var kv in depasset2bundles)
            {
                var depinfo = kv.Value;
                if (depinfo.containingABs.Count > 1)
                {
                    depinfo.memsize = calcSize(kv.Key);

                    Debug.Log(kv.Key + " count=" + depinfo.containingABs.Count + ", memsize=" +
                              depinfo.memsize);
                    foreach (var containingAB in depinfo.containingABs)
                    {
                        Debug.Log("    " + containingAB);
                    }
                    canSaveMemSize += depinfo.memsize*(depinfo.containingABs.Count - 1);
                }
            }

            Debug.Log("can save mem size=" + canSaveMemSize);
        }

        private static int calcSize(string asset)
        {
            var objs = AssetDatabase.LoadAllAssetsAtPath(asset);
            var allsize = 0;
            foreach (var obj in objs)
            {
                var size = Profiler.GetRuntimeMemorySize(obj);
                allsize += size;
            }
            return allsize;
        }
    }
}