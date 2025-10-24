using System.Collections.Generic;

[System.Serializable] // 【关键】这个特性让Unity的JsonUtility可以处理这个类
public class PlanetData
{
    public string noteText = "";
    public List<string> screenshotPaths = new List<string>();
}