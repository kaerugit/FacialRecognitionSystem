using FacialRecognitionSystem.UtilityFunction;

namespace FacialRecognitionSystem.FacialRecognitionFunction;

using System.Numerics;
using System.Text.Json;

/// <summary>
/// 顔情報の類似判定
/// </summary>
public class FaceRecognitionLogic
{

    // コサイン類似度
    public double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, na = 0, nb = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            na += a[i] * a[i];
            nb += b[i] * b[i];
        }

        return dot / (Math.Sqrt(na) * Math.Sqrt(nb));
    }

    // 類似人物検索
    public (PlayerData?, double)  FindPlayer(float[] embedding, double targetScore)
    {
        var db = PlayerDataStorage.PlayerEmbeddingDataDB;

        double bestScore = 0.0;     //ベストの類似スコアの管理
        double maxScore = 0.0;      //デバッグ用（判定とは関係なし）
        PlayerEmbeddingData? bestPerson = null;

        //PlayerData? playerData = null;

        ////連続の判定は少し緩くする
        //if (lastPlayerID != 0)
        //{
        //    var lastPlayerData = PlayerDataStorage.PlayerEmbeddingDataDB.FirstOrDefault(w => w.PlayerId == lastPlayerID);
        //    if (lastPlayerData != null)
        //    {
        //        var score = CosineSimilarity(embedding, lastPlayerData.Embedding);

        //        //連続の場合は半分でOKとする
        //        //if (score > (targetScore/2 ))
        //        if (score > 0.2)
        //        {
        //            bestPerson = lastPlayerData;
        //            maxScore = score;
        //        }
        //    }
        //}

        var namePlayerList = PlayerDataStorage.PlayerDataDB.Where(w => string.IsNullOrEmpty(w.PlayerName)).Select(s => s.PlayerId).ToList();

        //名前の入っているプレイヤーを優先
        foreach (var player in db.Where(w => namePlayerList.Contains(w.PlayerId)))
        {
            var score = CosineSimilarity(embedding, player.Embedding);
    
            if (score > targetScore)
            {
                maxScore = score;
                bestPerson = player;
                break;
            }
        }

        if (bestPerson == null)
        {
            foreach (var player in db.Where(w => !namePlayerList.Contains(w.PlayerId)))
            {
                var score = CosineSimilarity(embedding, player.Embedding);

                if (score > maxScore)
                {
                    maxScore = score;
                }

                //ベストの更新
                if (score > targetScore && score > bestScore)
                {
                    bestScore = score;
                    bestPerson = player;
                }
            }
        }

        return (
            PlayerDataStorage.PlayerDataDB.FirstOrDefault(w => w.PlayerId == (bestPerson?.PlayerId　?? int.MinValue)), 
            maxScore);
   
    }

    // 新規登録
    public PlayerData InsertPlayer(float[] embedding, double maxScore)
    {
        int newId = 1;

        //顔認証データ
        if (true)
        {
            var db = PlayerDataStorage.PlayerEmbeddingDataDB;

            newId = db.Count == 0
                ? 1
                : db.Max(p => p.PlayerId) + 1;

            var p = new PlayerEmbeddingData
            {
                PlayerId = newId,
                Embedding = embedding
            };

            db.Add(p);

            PlayerDataStorage.SaveEmbeddingData();

        }

        //プレイヤーデータ
        if (true)
        {
            var db = PlayerDataStorage.PlayerDataDB;

            var p = new PlayerData
            {
                PlayerId = newId,
                PlayerName = "",
                MaxScore = maxScore
            };

            db.Add(p);

            PlayerDataStorage.SavePlayerData();

            return p;
        }


    }
}
