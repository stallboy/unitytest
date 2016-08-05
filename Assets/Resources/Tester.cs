using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class Tester : MonoBehaviour
{
    private static float last_logtime;

    private int _col;
    private int _row;

    private Object abLoadAsyncAsset;

    private Dictionary<string, int> oldObjectStrs;
    private Dictionary<string, int> oldResourceStrs;

    private void Start()
    {
        Log("Start ==================================");
        diff(0);
        DontDestroyOnLoad(this);
    }

    private void LateUpdate()
    {
        diff(2);
    }

    private void OnGUI()
    {
        _col = 0;
        _row = 0;


        btn("load prefab", prefab_load);
        btn("load prefab instantiate", prefab_load_instantiate);
        btn("loadasync prefab instantiate", prefab_loadasync_instantiate);
        btn("load prefab unload",
            "报错，失败。提示UnloadAsset may only be used on individual assets and can not be used on GameObject's / Components or AssetBundles",
            prefab_load_unload);
        btn("load prefab destroy",
            "报错，失败。提示Destroying assets is not permitted to avoid data loss.",
            prefab_load_destroy);

        next();
        btn("find gameobject destroy", "destroy之后那些Prefab和其依赖的资源也都还留着，然后再点击UnloadUnusedAssets才能删除Prefab和其依赖的资源", find_object_destroy);
        btn("find gameobject instantiate", find_object_instanitate);

        empty();

        btn("UnloadUnusedAssets", UnloadUnusedAssets);
        btn("dump mytest", dump_mytest);
        btn("dump Dialog", dump_dialog);
        btn("dump stat", dump_stat);
        btn("dump all", dump_all);
        btn("switch scene", switch_scene);
        btn("test null", "unity重载了Object的==", testnull);
        btn("test gc", "GC.Collect的确是同步的", testgc);
        btn("test WWW resource limited?", "还是设置个限制吧，在window测试，当启动1000个WWW，可能会启动200个左右的线程，会提示too many thread错误", test_WWW_resouce_limited);
        
        nextcol();

        btn("load texture", texture_load);
        btn("load texture instantiate",
            "报错，但成功clone，提示Instantiating a non-readable 'MyTestTexture' texture is not allowed! Please mark the texture readable in the inspector or don't instantiate it.",
            texture_load_instantiate);
        btn("load texture unload",
            "成功，之前加载的go贴图丢失",
            texture_load_unload);
        
        btn("find texture object destroy",
            "成功，因为会成功clone啊，必然会成功destroy的",
            find_texture_object_destroy);


        next();
        btn("test clip", clip_test);
        btn("play clip", clip_play);
        btn("set clip null", clip_null);


        nextcol();

        btn("www", loadwww);
        btn("www assetBundle",
            "如果没有调用AssetBundle.Unload(false), 所以如果调用多次，第二次会报错，同时返回www.assetBundle为null，The AssetBundle 'xxx' can't be loaded because another AssetBundle with the same files are already loaded",
            loadwww_assetBundle);
        btn("www assetBundle unload", loadwww_assetBundle_Unload);
        btn("www assetBundle loadAssetAsync unload",
            "连续调多次会加载多份asset！！！也就是说从不同bundle实例（但同一个bundle文件）中load出来的asset, unity认为不同。因为依赖texture的bundle，所以要先加载texture bundle，然后再加载这个，保存asset，然后把这两个bundle Unload，用asset来Instantiate就OK了",
            loadwww_assetBundle_LoadAssetAsync_Unload);

        btn("saved AB asset instantiate", abLoadAsyncAsset_instantiate);
        btn("saved AB asset set null", abLoadAsyncAsset_null);

        next();
        btn("find assetBundle unload", find_assetBundle_Unload);
        btn("dump ab manifest", dump_ab_manifest);
        empty();
        empty();
        btn("black assetBundle loadAssetAsync unload",
            "这个bundle是个完整的，不依赖其他。实验得出AssetBundle.LoadAsset的策略是尽力解析，如果能找到就直接加载了依赖",
            black_assetBundle_LoadAssetAsync_Unload);

        nextcol();

        btn("www texture assetBundle", loadwww_texture_assetBundle);
        btn("www texture assetBundle loadAssetAsync unload", loadwww_texture_assetBundle_loadAssetAsync_Unload);

        next();
        btn("find texture assetBundle unload", find_texture_assetBundle_Unload);

        GUI.Label(new Rect(360, 500, 600, 100), GUI.tooltip);
    }

    private bool btn(string text, string tooltip = null)
    {
        var w = 300;
        var rect = new Rect(20 + (w + 40)*_col, 40 + _row*30, w, 20);
        _row++;
        return tooltip == null ? GUI.Button(rect, text) : GUI.Button(rect, new GUIContent(text + "?", tooltip));
    }

    private void empty()
    {
        _row++;
    }

    private void next()
    {
        _row = 7;
    }

    private void nextcol()
    {
        _col++;
        _row = 0;
    }

    private void btn(string text, Action action)
    {
        if (btn(text))
        {
            Log("========== " + text + " START ==========");
            action();
            Log("========== " + text + " END ==========");
        }
    }

    private void btn(string text, string tooltip, Action action)
    {
        if (btn(text, tooltip))
        {
            Log("========== " + text + " START ==========");
            action();
            Log("========== " + text + " END ==========");
        }
    }

    private AudioClip clip;

    void clip_test()
    {
        StartCoroutine(loadclip());
    }

    IEnumerator loadclip()
    {
        Debug.Log("load audiotest ab");
        var www = new WWW("file:///" + Application.dataPath + "/StreamingAssets/audiotest");
        yield return www;
        var ab = www.assetBundle;
        Debug.Log("load audiotest ab done");
        www.Dispose();

        string asset = "";
        foreach (var allAssetName in ab.GetAllAssetNames())
        {
            Debug.Log("    asset: " + allAssetName);
            asset = allAssetName;
        }

        var req = ab.LoadAssetAsync<AudioClip>(asset);
        yield return req;
        clip = req.asset as AudioClip;
        Debug.Log("loaded " + clip);
        ab.Unload(false);

        var audiosource = GetComponent<AudioSource>();
        audiosource.PlayOneShot(clip);

    }


    void clip_play()
    {
        var audiosource = GetComponent<AudioSource>();
        audiosource.PlayOneShot(clip);
    }

    void clip_null()
    {
        clip = null;
    }



    private void loadwww()
    {
        StartCoroutine(do_loadwww(0, "mytestprefabbundle"));
    }

    private void loadwww_assetBundle()
    {
        StartCoroutine(do_loadwww(1, "mytestprefabbundle"));
    }

    private void find_assetBundle_Unload()
    {
        find_ab_unload("Assets/Resources/MyTestPrefab.prefab");
    }

    private void test_WWW_resouce_limited()
    {
        for (int i = 0; i < 1000; i++)
        {
            StartCoroutine(do_loadwww(0, "mytestprefabbundle"));
        }
    }

    private void loadwww_assetBundle_Unload()
    {
        StartCoroutine(do_loadwww(2, "mytestprefabbundle"));
    }

    private void loadwww_assetBundle_LoadAssetAsync_Unload()
    {
        StartCoroutine(do_loadwww(3, "mytestprefabbundle", true));
    }


    private void black_assetBundle_LoadAssetAsync_Unload()
    {
        StartCoroutine(do_loadwww(3, "mytestblackbundle", true));
    }

    private void loadwww_texture_assetBundle()
    {
        StartCoroutine(do_loadwww(1, "mytesttexturebundle"));
    }

    private void loadwww_texture_assetBundle_loadAssetAsync_Unload()
    {
        StartCoroutine(do_loadwww(3, "mytesttexturebundle"));
    }

    private void find_texture_assetBundle_Unload()
    {
        find_ab_unload("Assets/Resources/MyTestTexture.psd");
    }

    private void find_ab_unload(string asset)
    {
        foreach (var ab in Resources.FindObjectsOfTypeAll<AssetBundle>())
        {
            if (ab.GetAllAssetNames().Contains(asset.ToLower()))
            {
                Log("find AssetBundle that contains {0} START Unload", asset);
                ab.Unload(false);
                break;
            }
        }

        Log("assetbundle contains {0} NOT FOUND", asset);
    }

    private void dump_ab_manifest()
    {
        StartCoroutine(do_loadwww(2, "StreamingAssets"));
    }

    private IEnumerator do_loadwww(int mode, string bundle, bool saveAsset = false)
    {
        var path = Path.Combine(Application.streamingAssetsPath, bundle);
        if (!path.Contains("://"))
        {
            path = "file:///" + path; //文档上说windows要3个/，我实验是2个，3个都ok
        }
        Log("loadwww START {0}", path);
        using (var www = new WWW(path))
        {
            yield return www;
            if (mode == 0)
            {
                Log("loadwww DONE isDone={0}, error={1}, bytes.Count={2}, size={3}", www.isDone, www.error,
                    www.bytes.Length, www.size);
            }
            else if (mode == 1)
            {
                Log("loadwww_assetBundle DONE isDone={0}, error={1}, bytes.Count={2}, size={3}, assetBundle={4}",
                    www.isDone, www.error, www.bytes.Length, www.size, www.assetBundle);
                //没有调用unload，下次再用会报错。并且返回的www.assetBundle为null
            }
            else if (mode == 2)
            {
                Log(
                    "loadwww_assetBundle_Unload DONE isDone={0}, error={1}, bytes.Count={2}, size={3}, assetBundle={4}",
                    www.isDone, www.error, www.bytes.Length, www.size, www.assetBundle);
                var ab = www.assetBundle;
                foreach (var assetName in ab.GetAllAssetNames())
                {
                    Log("include {0}", assetName);

                    if (assetName == "assetbundlemanifest")
                    {
                        var manifest = ab.LoadAsset<AssetBundleManifest>(assetName);
                        foreach (var bn in manifest.GetAllAssetBundles())
                        {
                            Log( "manifest " + bn + " dep: " +manifest.GetAllDependencies(bn).Aggregate("", (a, d) => a + "," + d));
                        }
                    }
                }
                ab.Unload(false);
            }
            else if (mode == 3)
            {
                Log(
                    "loadwww_assetBundle_LoadAssetAync_Unload DONE isDone={0}, error={1}, bytes.Count={2}, size={3}, assetBundle={4}",
                    www.isDone, www.error, www.bytes.Length, www.size, www.assetBundle);
                var ab = www.assetBundle;
                foreach (var assetName in ab.GetAllAssetNames())
                {
                    StartCoroutine(do_ab_loadassetasync(ab, assetName, saveAsset));
                    break;
                }
            }
        }
    }

    //private int loadcount = 1;
    private IEnumerator do_ab_loadassetasync(AssetBundle ab, string assetName, bool saveAsset)
    {
        Log("LoadAssetAsync START {0}", assetName);
        var a = ab.LoadAssetAsync(assetName);
        yield return a;

        //if (loadcount++ < 3)
        //    StartCoroutine(do_ab_loadassetasync(ab, assetName, saveAsset)); //多次LoadAssetAsync，是ok的。不会多加载

        Log("LoadAssetAsync DONE {0} = {1}", assetName, a.asset);
        if (saveAsset)
        {
            abLoadAsyncAsset = a.asset;
            Log("Saved AB asset {0}", abLoadAsyncAsset);
        }
        ab.Unload(false);
    }

    private void abLoadAsyncAsset_instantiate()
    {
        if (abLoadAsyncAsset != null)
        {
            Log("Saved AB asset Instantiate {0}", abLoadAsyncAsset);
            Instantiate(abLoadAsyncAsset);
        }
    }

    private void abLoadAsyncAsset_null()
    {
        if (abLoadAsyncAsset != null)
        {
            Log("Saved AB asset Set NULL {0}", abLoadAsyncAsset);

            abLoadAsyncAsset = null;
        }
    }

    private IEnumerator do_ab_loadassetasync_instantiate(AssetBundle ab, string assetName)
    {
        Log("LoadAssetAsync START {0}", assetName);
        var a = ab.LoadAssetAsync(assetName);
        yield return a;
        var asset = a.asset;
        Log("LoadAssetAsync DONE {0} = {1}, Instantiate START", assetName, asset);
        ab.Unload(false);
        Instantiate(asset);
    }


    private void prefab_load()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
    }

    private void prefab_load_instantiate()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
        Instantiate(go);
    }

    private void prefab_loadasync_instantiate()
    {
        StartCoroutine(do_prefab_loadasync_instantiate());
    }

    private IEnumerator do_prefab_loadasync_instantiate()
    {
        var go = Resources.LoadAsync<GameObject>("MyTestPrefab");
        Log("loadasync prefab START {0}", go);
        yield return go;
        Log("loadasync prefab DONE {0}", go.asset);
        Instantiate(go.asset);
    }

    private void find_object_destroy()
    {
        var gos = FindObjectsOfType<GameObject>();
        foreach (var go in gos)
        {
            if (go.name.StartsWith("MyTestPrefab"))
            {
                Log("find gameobject {0}", go);
                Destroy(go); //成功
                break;
            }
        }
    }

    private void find_object_instanitate()
    {
        var gos = FindObjectsOfType<GameObject>();
        foreach (var go in gos)
        {
            if (go.name.StartsWith("MyTestPrefab"))
            {
                Log("find gameobject {0}", go);
                Instantiate(go); //成功
                break;
            }
        }
    }


    private void prefab_load_unload()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
        Resources.UnloadAsset(go); //会报异常，然后失败
    }


    private void prefab_load_destroy()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
        Object.Destroy(go); //会报异常，然后失败
    }


    private void texture_load()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
    }

    private void texture_load_instantiate()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
        Instantiate(tex); //会报异常，但会成功，不要这么用
    }

    private void find_texture_object_destroy()
    {
        var gos = FindObjectsOfType<Texture>();
        foreach (var go in gos)
        {
            if (go.name.StartsWith("MyTestTexture"))
            {
                Log("find texture {0}", go);
                Destroy(go); //这个会成功的
                break;
            }
        }
    }

    private void texture_load_unload()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
        Resources.UnloadAsset(tex); //这个会卸载掉loadprefab加载进来的gameobject的贴图。
    }

    private void switch_scene()
    {
        Log("switch scene START ==================================");
        Application.LoadLevel("unitytest2");
        Log("switch scene DONE");
    }

    private void UnloadUnusedAssets()
    {
        StartCoroutine(do_unload());
    }

    private IEnumerator do_unload()
    {
        Log("UnloadUnusedAssets START");
        yield return Resources.UnloadUnusedAssets();
        Log("UnloadUnusedAssets DONE");
    }

    private void dump_mytest()
    {
        diff(1, "MyTest");
    }
    private void dump_dialog()
    {
        diff(1, "Dialog");
    }

    private void dump_stat()
    {
        diff(1);
    }

    private void dump_all()
    {
        diff(1, "");
    }

    private void diff(int mode, string filter = null)
    {
        var objects = new Dictionary<Object, Object>();
        var objectStrs = new Dictionary<string, int>();

        foreach (var o in FindObjectsOfType<Object>())
        {
            objects[o] = o;
            var str = o.ToString();
            int oldcount;
            objectStrs.TryGetValue(str, out oldcount);
            objectStrs[str] = oldcount + 1;
        }

        var resources = new Dictionary<Object, Object>();
        var resourceStrs = new Dictionary<string, int>();

        foreach (var o in Resources.FindObjectsOfTypeAll<Object>())
        {
            if (!objects.ContainsKey(o))
            {
                resources[o] = o;
                var str = o.ToString();
                var oldcount = 0;
                resourceStrs.TryGetValue(str, out oldcount);
                resourceStrs[str] = oldcount + 1;
            }
        }

        if (mode == 0)
        {
            dump("initial obj", objects.Keys, false);
            dump("initial res", resources.Keys, false);

            oldObjectStrs = objectStrs;
            oldResourceStrs = resourceStrs;
        }
        else if (mode == 1)
        {
            dump("initial obj", objects.Keys, false);
            dump("initial res", resources.Keys, false);
            if (filter != null)
            {
                Log("dump filter = {0} START", filter);
                foreach (var o in objectStrs)
                {
                    if (o.Key.ToLower().StartsWith(filter.ToLower()) || o.Key.StartsWith("yizi01"))
                    {
                        Log("dump obj {0} = {1}", o.Key, o.Value);
                    }
                }
                foreach (var o in resourceStrs)
                {
                    if (o.Key.ToLower().StartsWith(filter.ToLower()) || o.Key.StartsWith("yizi01"))
                    {
                        Log("dump res {0} = {1}", o.Key, o.Value);
                    }
                }
                foreach (var ab in Resources.FindObjectsOfTypeAll<AssetBundle>())
                {
                    //Log("dump ab {0}", ab.name);
                    foreach (var assetName in ab.GetAllAssetNames())
                    {
                        if (assetName.ToLower().Contains(filter.ToLower()) || assetName.Contains("yizi01"))
                        {
                            Log("dump res asset {0} in ab {1}", assetName, ab);
                        }
                    }
                }
                Log("dump filter = {0} DONE", filter);
            }
        }
        else
        {
            diffdump("++++ res", resourceStrs, oldResourceStrs);
            diffdump("---- res", oldResourceStrs, resourceStrs);
            diffdump("++++ obj", objectStrs, oldObjectStrs);
            diffdump("---- obj", oldObjectStrs, objectStrs);

            oldObjectStrs = objectStrs;
            oldResourceStrs = resourceStrs;
        }
    }

    private void diffdump(string prefix, Dictionary<string, int> cur, Dictionary<string, int> old)
    {
        var dif = new Dictionary<string, int>();
        foreach (var o in cur)
        {
            var obj = o.Key;
            var newCount = o.Value;
            int oldCount;
            old.TryGetValue(obj, out oldCount);

            if (newCount > oldCount && !obj.StartsWith("TextMesh"))
            {
                dif[obj] = newCount - oldCount;
            }
        }
        if (dif.Count > 0)
        {
            dump(prefix, null, true, dif);
        }
    }


    private void dump(string prefix, ICollection<Object> objs, bool verbose, Dictionary<string, int> obj2strs = null)
    {
        if (objs != null)
        {
            if (verbose)
            {
                foreach (var o in objs)
                {
                    Log(prefix + " {0}", o);
                }
            }
            else
            {
                logStat(prefix, objs);
            }
        }

        if (obj2strs != null)
        {
            if (verbose)
            {
                foreach (var o in obj2strs)
                {
                    Log(prefix + " {0} = {1}", o.Key, o.Value);
                }
            }
            else
            {
                var topstr = obj2strs.OrderBy(t => t.Value)
                    .Reverse()
                    .Take(20)
                    .Aggregate(", ====top: ",
                        (old, kv) =>
                            old + ", " + (kv.Key == "AssetBundle" ? "<color=yellow>AssetBundle</color>" : kv.Key) +
                            "=<color=yellow>" + kv.Value + "</color>");
                Log(prefix + " str=" + obj2strs.Count + topstr);
            }
        }
    }

    private void logStat(string prefix, ICollection<Object> objs)
    {
        var namemap = from o in objs
            group o by o.name
            into g
            select new {name = g.Key, count = g.Count()};

        var typemap = from o in objs
            group o by o.GetType()
            into g
            select new {type = g.Key, count = g.Count()};

        var topname = namemap.OrderBy(t => t.count)
            .Reverse()
            .Take(20)
            .Aggregate(", ====topname: ",
                (old, kv) => old + ", " + kv.name + "=<color=yellow>" + kv.count + "</color>");

        var toptype = typemap.OrderBy(t => t.count)
            .Reverse()
            //.Take(20)
            .Aggregate(", ====toptype: ",
                (old, kv) =>
                    old + ", " + (kv.type.Name == "AssetBundle" ? "<color=yellow>AssetBundle</color>" : kv.type.Name) +
                    "=<color=yellow>" + kv.count + "</color>");

        Log(prefix + " all=<color=yellow>" + objs.Count + "</color>, "
            + "name=<color=yellow>" + namemap.Count() + "</color>, " +
            "type=<color=yellow>" + typemap.Count() + "</color>" + topname + toptype);
    }

    private void testforeach()
    {
        var map = new Dictionary<int, string>();

        for (var i = 0; i < 10; i++)
        {
            map.Add(i, "aaaaaaa" + i);
        }

        var e = map.GetEnumerator();
        while (e.MoveNext())
        {
            //var i = e.Current.Key;
            //Debug.Log(e.Current);
        }
    }

    private void testgc()
    {
        var map = new Dictionary<int, string>();

        for (var i = 0; i < 10; i++)
        {
            map.Add(i, "aaaaaaa" + i);
        }

        make_garbage();
        Log("Memory used before collection {0}", GC.GetTotalMemory(false));

        GC.Collect();
        Log("Memory used after full collection {0}", GC.GetTotalMemory(false));
    }

    private void make_garbage()
    {
        Version vt = new Version();
        for (var i = 0; i < 10000; i++)
        {
            vt = new Version();
        }
        Log("maked garbage " + vt);
    }

    private void testnull()
    {
        var go = GameObject.Find("/Main Camera");
        var aa = go.AddComponent<EventSystem>();
        var dic = new Dictionary<EventSystem, EventSystem> {{aa, aa}};

        DestroyImmediate(aa);

        foreach (var pair in dic)
        {
            Log("==null:{0}, ReferenceEquals(null):{1}", pair.Key == null, ReferenceEquals(pair.Key, null));
        }
    }
    
    public static void Log(object obj, params object[] args)
    {
        var cur = Time.time;
        var mills = (int) ((cur - last_logtime)*1000);
        Debug.Log(Time.frameCount + " " + mills + " " + string.Format(color(obj.ToString(), "lightblue"), colors(args)));
        last_logtime = cur;
    }

    private static object[] colors(params object[] args)
    {
        return args.Select(a => (object)color( a != null ? a.ToString() : "null", "yellow", true)).ToArray();
    }

    private static string color(string word, string color, bool bold = false)
    {
        var str = string.Format("<color={0}>{1}</color>", color, word);
        return bold ? "<b>" + str + "</b>" : str;
    }
}