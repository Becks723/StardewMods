namespace FontSettings.Framework
{
    internal enum InvalidPresetNameTypes
    {
        EmptyName,
        ContainsInvalidChar,
        DuplicatedName
    }

    internal static class InvalidPresetNameTypesExtensions
    {
        internal static string GetMessage(this InvalidPresetNameTypes type)
        {
            return type switch
            {
                InvalidPresetNameTypes.EmptyName => I18n.Ui_InvalidPresetName_Empty(),
                InvalidPresetNameTypes.ContainsInvalidChar => I18n.Ui_InvalidPresetName_InvalidChar(),
                InvalidPresetNameTypes.DuplicatedName => I18n.Ui_InvalidPresetName_Duplicated(),
                _ => throw new System.NotSupportedException(),
            };
        }
    }
}
