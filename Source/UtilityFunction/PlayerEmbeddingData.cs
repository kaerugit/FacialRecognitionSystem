namespace FacialRecognitionSystem.UtilityFunction;

/// <summary>
///  顔認証のデータ
/// </summary>
public class PlayerEmbeddingData
{
    public int PlayerId { get; set; }
    public float[] Embedding { get; set; } = default!;
}

public class PlayerEmbeddingDataList : List<PlayerEmbeddingData>
{

    public PlayerEmbeddingDataList() { }

    public PlayerEmbeddingDataList(IEnumerable<PlayerEmbeddingData> collection) : base(collection)
    {
    }
}
