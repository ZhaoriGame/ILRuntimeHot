using ILRuntime.Runtime.Intepreter;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ILRuntime.Runtime.Stack;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using System;
using System.Linq;
using Debug = MyDebug.Debug;



public static class Const
{
    public static bool IsShowDownloadFileName = true;
    public static string ConfigURL = "http://148.70.2.142/HotFile/Config/Config.txt";
    public static bool UseAB = false;
}

public class Enter : MonoBehaviour
{
	
	private void Start()
	{
		ConfigDownloader.Instance.StartToDownload(Const.ConfigURL, Success, Failed );
	}

    /// <summary>
    /// Config 成功下载后
    /// </summary>
    private void Success()
    {
            ParseConfigs();
           
        //下载manifest文件先，完成后下载Dll
            UAssetBundleDownloader.Instance.DownloadResources(OnDownLoadComplite, null, null, true, 
                UStaticFuncs.GetPlatformFolder(Application.platform), UStaticFuncs.GetPlatformFolder(Application.platform) + ".manifest");
    }

    private void OnDownLoadComplite(List<string> ls)
    {
        UAssetBundleDownloader.Instance.DownloadResources(_1 =>
        {
            var dll = "Dll/AHotGames";
            byte[] pdbBytes = null;
#if UNITY_EDITOR
            if (!UConfigManager.bUsingAb)
            {
                dll += ".bytes";
            }
            //.pdb用于调试，pdb文件包含了编译后程序指向源代码的位置信息Editor
            pdbBytes = System.IO.File.ReadAllBytes("Assets/RemoteResources/Dll/AHotGames.pdb");
#endif
            ILRuntimeHandler.Instance.DoLoadDll("ahotmages"
                , UAssetBundleDownloader.Instance.OnLoadAsset<TextAsset>(dll).bytes, pdbBytes);

            //设置消息发送？？消息接收Load||LoadPrefab||UnLoadAll
            ILRuntimeHandler.Instance.SetUnityMessageReceiver(MonoInstancePool.GetInstance<UEmitMessage>(true).gameObject);

            ILRuntimeHandler.Instance.OnLoadClass("AEntrance", new GameObject("AEntrance"));
        }, null, PreLoad);

    }
    /// <summary>
    /// 预加载
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    private bool PreLoad(string arg)
    {
        var preloads = ConfigDownloader.Instance.OnGetValue("preloads");
        var lPreloads = preloads.Split(',').ToList();
        foreach (var p in lPreloads)
        {
            if (arg.ToLower().StartsWith(p.ToLower()))
            {
                return true;
            }
        }
        return false;
    }

    private void Failed()
    {
        Debug.LogError("下载配置文件失败。");
    }

    //转换Config文件
    //useab = 1为返回使用AB包加载
    private void ParseConfigs()
	{
		UConfigManager.bUsingAb = ConfigDownloader.Instance.OnGetIntValue("useab") == 1;
        Debug.LogError(UConfigManager.bUsingAb);

#if UNITY_EDITOR
		UConfigManager.bUsingAb = Const.UseAB;
#endif
	}
}
