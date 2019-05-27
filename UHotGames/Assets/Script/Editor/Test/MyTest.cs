using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Debug = MyDebug.Debug;

public static class  MyTest {

    [MenuItem("MyTools/Test1")]
	static void Test1()
    {
        Debug.Log(Application.persistentDataPath);
        Debug.LogWarning("nihao");
        
    }
}
