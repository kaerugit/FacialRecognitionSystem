

namespace FacialRecognitionSystem.UtilityFunction;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

public static class PlayerDataStorage
{
    //プレーヤー名管理ファイル
    static private readonly string FILE_PLAYER_PATH = "PlayerData.json";
    //顔情報管理ファイル
    static private readonly string FILE_EMBEDDING_PATH = "PlayerEmbeddingData.json";
    
    static private object _lock = new object();

    /// <summary>
    /// プレーヤーのデータ
    /// </summary>
    static public PlayerDataList PlayerDataDB { get; private set; } = new PlayerDataList();

    /// <summary>
    /// 顔認証のデータ
    /// </summary>
    /// <remarks>
    /// 最初はPlayerDataDBと一緒だったが重たくなる可能性があるので別データにする
    /// </remarks>
    static public PlayerEmbeddingDataList PlayerEmbeddingDataDB { get; private set; } = new PlayerEmbeddingDataList();

    static PlayerDataStorage()
    {
        Load();
    }

    static public void Load()
    {

        if (true)
        {
            var filePath = Utility.GetDataFile(FILE_PLAYER_PATH);
            if (File.Exists(filePath))
            {
                //return new FaceDatabase();

                var json = File.ReadAllText(filePath);
                //return JsonSerializer.Deserialize<FaceDatabase>(json)
                //       ?? new FaceDatabase();
                var personDataDB = JsonSerializer.Deserialize<PlayerDataList>(json)
                       ?? new PlayerDataList();

                //var a = (PersonDataList)(personDataDB.AsQueryable().Where(w => !string.IsNullOrEmpty(w.Name)).ToList());

                //名前が登録されていないデータを除く
                personDataDB = new PlayerDataList(personDataDB.Where(w => !string.IsNullOrEmpty(w.PlayerName)));

                PlayerDataStorage.PlayerDataDB = personDataDB;
            }

        }


        if (true)
        {
            var filePath = Utility.GetDataFile(FILE_EMBEDDING_PATH);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);

                var personDataDB = JsonSerializer.Deserialize<PlayerEmbeddingDataList>(json)
                       ?? new PlayerEmbeddingDataList();

                //名前が登録されていないデータを除く
                personDataDB = new PlayerEmbeddingDataList(personDataDB.Where(w => PlayerDataStorage.PlayerDataDB.Any(a => w.PlayerId == a.PlayerId)));

                PlayerDataStorage.PlayerEmbeddingDataDB = personDataDB;
            }


        }
    }

    static public void SavePlayerData()
    {
        lock (_lock)
        {
            var filePath = Utility.GetDataFile(FILE_PLAYER_PATH);
            var json = JsonSerializer.Serialize(PlayerDataStorage.PlayerDataDB, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
            Utility.WriteAllText(filePath, json);
        }
    }


    static public void SaveEmbeddingData()
    {
        lock (_lock)
        {
            var filePath = Utility.GetDataFile(FILE_EMBEDDING_PATH);
            var json = JsonSerializer.Serialize(PlayerDataStorage.PlayerEmbeddingDataDB, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });
            Utility.WriteAllText(filePath, json);
        }
    }

    /// <summary>
    /// 削除(テストで使用)
    /// </summary>
    static public void Delete()
    {

        foreach (var filePath in new string[] { FILE_PLAYER_PATH , FILE_EMBEDDING_PATH })
        {
            try
            {
                File.Delete(filePath);
            }
            catch { }
        }
        

        Load();
    }


}