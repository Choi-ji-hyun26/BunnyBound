using System.Collections.Generic;

[System.Serializable]
public class GameProgressData
{
    public PlayerProgressData player = new();
    public List<StageData> stages = new();
}
