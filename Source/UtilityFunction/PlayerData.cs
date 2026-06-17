using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using OpenCvSharp;

namespace FacialRecognitionSystem.UtilityFunction;

/// <summary>
/// プレイヤーのデータ
/// </summary>
public class PlayerData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";

    public double MaxScore { get; set; } = 0;
}

public class PlayerDataList : List<PlayerData>
{
    public PlayerDataList() { }

    public PlayerDataList(IEnumerable<PlayerData> collection) : base(collection)
    {
    }
}
