
namespace FacialRecognitionSystem.UtilityFunction;

using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

public class AppConfig : INotifyPropertyChanged
{
    public static readonly string FILE_PATH = "config.json";
    private object _lock = new object();

    
    //カメラ1台目
    
    private int _videoCaptureID1 = -1;
    public int VideoCaptureID1
    {
        get => _videoCaptureID1;
        set { _videoCaptureID1 = value; OnChanged(); }
    }


    private string _fileSaveFullPath1 = "";

    public string FileSaveFullPath1
    {
        get => _fileSaveFullPath1;
        set { _fileSaveFullPath1 = value; OnChanged(); }
    }

    //カメラ2台目

    private int _videoCaptureID2 = -1;
    public int VideoCaptureID2
    {
        get => _videoCaptureID2;
        set { _videoCaptureID2 = value; OnChanged(); }
    }


    private string _fileSaveFullPath2 = "";

    public string FileSaveFullPath2
    {
        get => _fileSaveFullPath2;
        set { _fileSaveFullPath2 = value; OnChanged(); }
    }


    //以下高度な設定

    private int _timerInterval = 20;

    /// <summary>
    /// 確認するインターバル（タイマーの間隔）
    /// </summary>
    public int TimerInterval
    {
        get => _timerInterval;
        set { _timerInterval = value; OnChanged(); }
    }

    private decimal _faceScore = (decimal)0.9;

    /// <summary>
    /// 顔認識のスコア
    /// </summary>
    public decimal FaceScore
    {
        get => _faceScore;
        set { _faceScore = value; OnChanged(); }
    }


    private decimal _judgementScore = (decimal)0.5;

    /// <summary>
    /// 顔類似判定割合
    /// </summary>
    public decimal JudgementScore
    {
        get => _judgementScore;
        set { _judgementScore = value; OnChanged(); }
    }

   

    private decimal _exclusionRate = (decimal)0.1;

    /// <summary>
    /// 画像全体の大きさよりこちらの設定値未満の値は除外する
    /// </summary>
    /// <remarks>
    /// 後ろにいるギャラリーなどは無視
    /// </remarks>
    public decimal ExclusionRate
    {
        get => _exclusionRate;
        set { _exclusionRate = value; OnChanged(); }
    }

    private string _newPlayerName = "new challenger";

    /// <summary>
    /// 新規時のプレーヤー名
    /// </summary>
    public string NewPlayerName
    {
        get => _newPlayerName;
        set { _newPlayerName = value; OnChanged(); }
    }


    private string _joinPlayerFileFullPath = "";

    /// <summary>
    /// 参加プレーヤのファイルを作成
    /// </summary>
    public string JoinPlayerFileFullPath
    {
        get => _joinPlayerFileFullPath;
        set { _joinPlayerFileFullPath = value; OnChanged(); }
    }


    private string _joinPlayerSplit = "";

    /// <summary>
    /// 参加プレーヤの区切り文字(空欄の場合は改行)
    /// </summary>
    public string JoinPlayerSplitChar
    {
        get => _joinPlayerSplit;
        set { _joinPlayerSplit = value; OnChanged(); }
    }

    private int _frameWidth = 0;

    /// <summary>
    /// 画像幅
    /// </summary>
    /// <remarks>0で自動</remarks>
    public int FrameWidth
    {
        get => _frameWidth;
        set { _frameWidth = value; OnChanged(); }
    }

    private int _frameHeight = 0;

    /// <summary>
    /// 画像高さ
    /// </summary>
    /// <remarks>0で自動</remarks>
    public int FrameHeight
    {
        get => _frameHeight;
        set { _frameHeight = value; OnChanged(); }
    }

    private bool _developMode = false;

    /// <summary>
    /// 開発者用モード
    /// </summary>
    public bool DevelopMode
    {
        get => _developMode;
        set { _developMode = value; OnChanged(); }
    }


    public event PropertyChangedEventHandler? PropertyChanged;

    public const string DEFAULT_FILE_SAVE_FULLPATH1 = "user1.txt";
    public const string DEFAULT_FILE_SAVE_FULLPATH2 = "user2.txt";

    public AppConfig()
    {
        var dataFolder = Utility.GetDataFolder();

        if (this.FileSaveFullPath1.Length == 0)
        {
            this.FileSaveFullPath1 = Utility.GetRelativePath( Path.Combine(dataFolder, DEFAULT_FILE_SAVE_FULLPATH1));
        }
        if (this.FileSaveFullPath2.Length == 0)
        {
            this.FileSaveFullPath2 = Utility.GetRelativePath(Path.Combine(dataFolder, DEFAULT_FILE_SAVE_FULLPATH2));
        }
    }
    private void OnChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        Save(); // 値変更時に即保存
    }

    public void Save()
    {
        lock (_lock)
        {
            var configPath = Utility.GetDataFile(FILE_PATH);

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            Utility.WriteAllText(configPath, json);
        }
    }

    public static AppConfig Load()
    {
        var configPath = Utility.GetDataFile(FILE_PATH);

        if (!File.Exists(configPath))
        {
            return new AppConfig();
        }

        var json = File.ReadAllText(configPath);
        return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
    }

  

}
