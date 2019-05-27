using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using Debug = MyDebug.Debug;

public class ConfigDownloader : MonoBehaviour
{
    private static ConfigDownloader s_instance;
    public static ConfigDownloader Instance
    {
        get
        {
            if (s_instance == null)
            {
                s_instance = MonoInstancePool.GetInstance<ConfigDownloader>(true);
            }
            return s_instance;
        }
    }
    void Start()
    {
        s_instance = this;
    }

    private static Action delDownloadComplete;
    private static Action delDownloadFailed;
    public void StartToDownload(string configUrl, Action del, Action failedDel)
    {
        delDownloadComplete = del;
        delDownloadFailed = failedDel;

        StartCoroutine(DownloadConfig(configUrl));
        //UEventListener.Instance.RegistEventListener(UEvents.DownloadCompleteEvent, eventBase => { Debug.LogError("加载完成"); });
    }
    //Config 文件里的键值对
    Dictionary<string, string> dKeyValue = new Dictionary<string, string>();

    public static int retryTime = 10;
    private static int retried = 0;
    /// <summary>
    /// 读取config文件
    /// </summary>
    /// <param name="configUrl"></param>
    /// <returns></returns>
    IEnumerator DownloadConfig(string configUrl)
    {
        //
        var url = configUrl;
            //+ "?" + ApiDateTime.SecondsFromBegin();
        var www = new WWW(url);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            if (retried >= retryTime)
            {
                delDownloadFailed?.Invoke();
            }
            else
            {
                //重新尝试
                retried++;
                yield return new WaitForSeconds(0.1f);
                Debug.Log(url + " error:" + www.error);
                StartCoroutine(DownloadConfig(configUrl));
            }
        }
        //下载成功
        else
        {
            var text = www.text;
            //根据换行切割
            string[] alines = text.Split(new string[] { "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < alines.Length; i++)
            {
                //注释忽略
                if (alines[i].StartsWith("#") || alines[i].StartsWith("//"))
                {
                    continue;
                }
                //根据“=”号切割
                string[] akeyvalue = alines[i].Split(new char[] { '=' }, 2);
                dKeyValue.Add(akeyvalue[0], akeyvalue[1]);
                
            }

            delDownloadComplete?.Invoke();
            Debug.Log("Config 文件下载完成");
            //UEventListener.Instance.DispatchEvent(UEvents.DownloadCompleteEvent, true);
        }
        www.Dispose();
    }

    private string sBundleVersion;

    private string GetBundleVersion()
    {
        if (string.IsNullOrEmpty(sBundleVersion))
        {
            sBundleVersion = Utils_Plugins.Util_GetBundleVersion();
        }
        return sBundleVersion;
    }

    public string OnGetValue(string skey)
    {
        if (dKeyValue.ContainsKey(skey + GetBundleVersion()))
        {
            return dKeyValue[skey + GetBundleVersion()];
        }
        if (dKeyValue.ContainsKey(skey))
        {
            return dKeyValue[skey];
        }
        return "";
    }
    public int OnGetIntValue(string skey)
    {
        return typeParser.intParse(OnGetValue(skey));
    }
    public float OnGetFloatValue(string skey)
    {
        return typeParser.floatParse(OnGetValue(skey));
    }
}
