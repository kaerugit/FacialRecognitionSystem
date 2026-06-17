using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms.Design;


namespace FacialRecognitionSystem.UtilityFunction;

public static class Utility
{
    const  string  DATA_FOLDER = "Data";
    const string TEMP_FOLDER = "Temp";

    /// <summary>
    /// Dataフォルダの取得
    /// </summary>
    /// <returns></returns>
    public static string GetDataFolder()
    {
        return createDirectory(DATA_FOLDER);
    }

    /// <summary>
    /// Tempフォルダの取得
    /// </summary>
    /// <returns></returns>
    public static string GetTempFolder()
    {
        return createDirectory(TEMP_FOLDER);
    }

    public static void  DeleteTempFolder()
    {
        foreach (var file in Utility.GetTempFiles())
        {
            try
            {
                File.Delete(file);
            }
            catch { }
        }
    }

    public static string[] GetTempFiles()
    {
        string folder = GetTempFolder();

         var files = Directory.GetFiles(folder, "*.jpg")
                .Concat(Directory.GetFiles(folder, "*.json"))
                .ToArray();

        return files;

    }

    /// <summary>
    /// TEMPフォルダのファイル名リスト（拡張子なし）を作成
    /// </summary>
    /// <returns></returns>
    public static List<string> GetTempIdList()
    {
        return  Utility.GetTempFiles().AsQueryable().Select(s => Path.GetFileNameWithoutExtension(s)).ToList();
    }

    private static string createDirectory(string folderName)
    {
        string exeDir = AppContext.BaseDirectory;
        var folder = Path.Combine(exeDir, folderName) ?? "";

        // フォルダが無ければ作成
        if (!Directory.Exists(folder))
        {
            try
            {
                Directory.CreateDirectory(folder);
            }
            catch { }
        }

        return folder;


    }


    public static string GetDataFile(string fileName)
    {
        return Path.Combine(Utility.GetDataFolder(), fileName);
    }


    /// <summary>
    /// ファイルの書き込み(utf-8)
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="text"></param>
    /// <param name="isSameCheck">同じ内容の場合書き込まないかチェックする場合：true</param>
    public static void WriteAllText(string fileName ,string text,bool isSameCheck = false)
    {

        var enc = new UTF8Encoding(false);


        fileName = Utility.GetFullPath(fileName);

        //テキストが同じ内容の場合書き込まない
        if (isSameCheck == true)
        {
            if (File.Exists(fileName) == true)
            {
                var beforeText = File.ReadAllText(fileName, enc);

                if (beforeText == text)
                {
                    return;
                }
            }
        }

        try
        {
            File.WriteAllText(fileName, text, enc);
        }
        catch { }
        
    }


    const string RELATIVE_PATH_CHAR = ".\\";

    /// <summary>
    /// 実行中のフォルダ配下の場合相対パスの取得
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string  GetRelativePath(string fileName)
    {
        return fileName.Replace(AppContext.BaseDirectory, RELATIVE_PATH_CHAR);
    }

    /// <summary>
    /// 実行中のフォルダ配下の場合相対パスから絶対パスに変更
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetFullPath(string fileName)
    {
        if (fileName.StartsWith(RELATIVE_PATH_CHAR))
        {
            fileName = AppContext.BaseDirectory + fileName.Substring(RELATIVE_PATH_CHAR.Length);
        }
        return fileName;
    }


    public static T DeepCopy<T>(T item) where T : new()
    {
        var json = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = true });
        return JsonSerializer.Deserialize<T>(json) ?? new T();
    }
}
