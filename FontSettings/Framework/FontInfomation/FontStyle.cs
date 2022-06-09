namespace FontSettings.Framework.FontInfomation
{
    [System.Flags]
    internal enum FontStyle
    {
        Regular = 0,   // 常规
        Bold = 1,      // 加粗
        Italic = 2,    // 斜体
        Strikeout = 4, // 删除线
        Underline = 8  // 下划线
    }
}
