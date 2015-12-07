using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

public class Tester : MonoBehaviour
{
    public enum LogColor
    {
        blue,
        magenta,
        maroon,
        orange,
        red,
        purple,
        yellow,
        white,
        cyan,
        lightblue
    }

    private static Dictionary<string, int> oldObjectStrs;
    private static Dictionary<string, int> oldResourceStrs;
    //unity会在内部资源释放时，回朔所有引用，置为null，所以不能用oldResources来检测。靠，我服, 只能用这个了。

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
        if (GUI.Button(new Rect(20, 40, 180, 20), "load prefab"))
        {
            loadprefab();
        }

        if (GUI.Button(new Rect(20, 70, 180, 20), "load prefab instantiate"))
        {
            loadprefab_instantiate();
        }

        if (GUI.Button(new Rect(20, 100, 180, 20), "object destory"))
        {
            object_destory();
        }

        if (GUI.Button(new Rect(20, 130, 180, 20), "load prefab unload"))
        {
            loadprefab_unload();
        }


        if (GUI.Button(new Rect(240, 40, 180, 20), "load texture"))
        {
            loadpart_texture();
        }

        if (GUI.Button(new Rect(240, 70, 180, 20), "load texture instantiate"))
        {
            loadpart_texture_instantiate();
        }

        if (GUI.Button(new Rect(240, 100, 180, 20), "testure destory"))
        {
            texture_destory();
        }

        if (GUI.Button(new Rect(240, 130, 180, 20), "load texture unload"))
        {
            loadpart_texture_unload();
        }



        if (GUI.Button(new Rect(20, 190, 180, 20), "dump MyTest"))
        {
            diff(1, "MyTest");
        }

        if (GUI.Button(new Rect(20, 220, 180, 20), "unload"))
        {
            StartCoroutine(unload());
        }
        
        if (GUI.Button(new Rect(20, 250, 180, 20), "dump"))
        {
            diff(1);
        }

        if (GUI.Button(new Rect(20, 280, 180, 20), "switch scene"))
        {
            switchscene();
        }

        if (GUI.Button(new Rect(20, 310, 180, 20), "test magic null"))
        {
            testImpossibleNull();
        }
    }

    private void loadprefab()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
    }

    private void loadprefab_instantiate()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
        Instantiate(go);
    }

    private void object_destory()
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

    private void loadprefab_unload()
    {
        var go = Resources.Load<GameObject>("MyTestPrefab");
        Log("load prefab {0}", go);
        Resources.UnloadAsset(go); //会报异常，然后失败
    }

    private void loadpart_texture()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
    }

    private void loadpart_texture_instantiate()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
        Object.Instantiate(tex); //会报异常，但会成功，不要这么用
    }

    private void texture_destory()
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

    private void loadpart_texture_unload()
    {
        var tex = Resources.Load<Texture>("MyTestTexture");
        Log("load texture {0}", tex);
        Resources.UnloadAsset(tex); //这个会卸载掉loadprefab加载进来的gameobject的贴图。
    }

    private void switchscene()
    {
        Log("switch scene START ==================================");
        Application.LoadLevel("unitytest2");
        Log("switch scene DONE");
    }


    private void instantiate()
    {
        var go = GameObject.Find("Archer_Emiya");
        var anim = go.GetComponent<Animation>();
        Instantiate(anim);
    }

    private IEnumerator unload()
    {
        Log("UnloadUnusedAssets START");
        yield return Resources.UnloadUnusedAssets();
        Log("UnloadUnusedAssets DONE, GC.Collect START");
        GC.Collect();
        Log("UnloadUnusedAssets -> GC.Collect DONE");
    }

    private void diff(int mode, string filter = null)
    {
        var objects = new Dictionary<Object, Object>();
        var objectStrs = new Dictionary<string, int>();

        foreach (var o in FindObjectsOfType<Object>())
        {
            objects[o] = o;
            var str = o.ToString();
            var oldcount = 0;
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
                    if (o.Key.StartsWith(filter) || o.Key.StartsWith("yizi01"))
                    {
                        Log("dump obj {0} = {1}", o.Key, o.Value);
                    }
                }
                foreach (var o in resourceStrs)
                {
                    if (o.Key.StartsWith(filter) || o.Key.StartsWith("yizi01"))
                    {
                        Log("dump res {0} = {1}", o.Key, o.Value);
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
            var oldCount = 0;
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
        Log("-------------------");
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
                        (old, kv) => old + ", " + kv.Key + "=<color=yellow>" + kv.Value + "</color>");
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
            .Take(20)
            .Aggregate(", ====toptype: ",
                (old, kv) => old + ", " + kv.type.Name + "=<color=yellow>" + kv.count + "</color>");

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
            var i = e.Current.Key;
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

        MakeSomeGarbage();
        Log("Memory used before collection {0}", GC.GetTotalMemory(false));

        GC.Collect();
        Log("Memory used after full collection {0}", GC.GetTotalMemory(false));
    }

    private void MakeSomeGarbage()
    {
        Version vt;

        // Create objects and release them to fill up memory with unused objects.
        for (var i = 0; i < 10000; i++)
        {
            vt = new Version();
        }
    }

    private void testImpossibleNull()
    {
        var go = GameObject.Find("/Main Camera");
        var aa = go.AddComponent<EventSystem>();
        var bb = go.AddComponent<EventSystem>();
        var dic = new Dictionary<EventSystem, EventSystem> {{aa, aa}, {bb, bb}};

        DestroyImmediate(aa);
        DestroyImmediate(bb);

        foreach (var pair in dic)
        {
            Debug.Log(pair.Key == null);
        }
    }

    private static string AddColor(string word, LogColor color, bool bold = false)
    {
        var formatStr = string.Format("<color={0}>{1}</color>", color, word);
        if (bold)
        {
            formatStr = "<b>" + formatStr + "</b>";
        }
        return formatStr;
    }

    private static object[] ReplaceWordList(object[] replaceWords)
    {
        for (var i = 0; i < replaceWords.Length; i++)
        {
            replaceWords[i] = AddColor(replaceWords[i] == null ? "null" : replaceWords[i].ToString(), LogColor.yellow,
                true);
        }
        return replaceWords;
    }


    public static void Log(string str)
    {
        Debug.Log(Time.frameCount + " " + AddColor(str, LogColor.lightblue));
    }

    public static void Log(object obj, params object[] replaceWords)
    {
        Debug.Log(Time.frameCount + " " +
                  string.Format(AddColor(obj.ToString(), LogColor.lightblue), ReplaceWordList(replaceWords)));
    }
}