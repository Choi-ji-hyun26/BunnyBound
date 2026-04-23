using System;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static bool isDirty = false;

    public static void MarkDirty()
    {
        isDirty = true;
    }

    public static void Flush<T>(string path, T data, int version)
    {
        if(!isDirty)
            return;

        var wrapper = new SaveFile<T>(version, data);
        string json = JsonUtility.ToJson(wrapper, true);

        string tempPath = path + ".tmp";

        try
        {
            // 1. temp 저장
            File.WriteAllText(tempPath, json);
            // 2. 기존 파일 교체
            if(File.Exists(path))
                File.Replace(tempPath, path, null);
            // 3. 최초 저장 시 
            else
                File.Move(tempPath, path);

            isDirty = false;
        }
        catch(Exception e)
        {
            Debug.LogError($"Save Failed : {e}");
        }
    }

    public static SaveFile<T> Load<T>(string path)
    {
        if(!File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveFile<T>>(json);
        }
        catch(Exception e)
        {
            Debug.LogWarning($"Load Failed: {e}");
            return null;
        }
    }
}
