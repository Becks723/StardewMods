#if EXTERN_FONT_LIBS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.Fonts;

namespace FontSettings.Framework.FontInfomation
{
    internal class FontInfoFromOtherLibs : IFontInfoSource
    {
        public FontModel[] GetFontInfo(string path)
        {
            List<FontModel> result = new List<FontModel>();
            try
            {
                FontDescription[] descriptions;
                if (path.ToLowerInvariant().EndsWith(".ttc"))
                    descriptions = FontDescription.LoadFontCollectionDescriptions(path);
                else
                    descriptions = new[] { FontDescription.LoadDescription(path) };

                for (int i = 0; i < descriptions.Length; i++)
                {
                    FontDescription desc = descriptions[i];
                    FontModel fontModel = new()
                    {
                        FullPath = path,
                        Name = desc.FontName(CultureInfo.InstalledUICulture),
                        FamilyName = desc.FontFamily(CultureInfo.InstalledUICulture),
                        SubfamilyName = desc.FontSubFamilyName(CultureInfo.InstalledUICulture),
                        FontIndex = i  // TODO: 找到更好的方法，现在这个是因为按顺序的
                    };
                    result.Add(fontModel);
                }
            }
            catch (InvalidFontFileException)  // 上面这个库不能加载PostScript Outlines格式的字体
            {
                var reader = new Typography.OpenFont.OpenFontReader();
                var previewFont = reader.ReadPreview(File.OpenRead(path));

                Dictionary<int, Typography.OpenFont.PreviewFontInfo> previewFonts = new();
                if (previewFont.IsFontCollection)
                    for (int i = 0; i < previewFont.MemberCount; i++)
                        previewFonts.Add(i, previewFont.GetMember(i));
                else
                    previewFonts.Add(0, previewFont);

                foreach (var pair in previewFonts)
                {
                    int index = pair.Key;
                    var font = pair.Value;
                    FontModel fontModel = new()
                    {
                        FullPath = path,
                        Name = font.NameEntry.FullFontName,
                        FamilyName = font.NameEntry.FontName,
                        SubfamilyName = font.NameEntry.FontSubFamily,
                        FontIndex = index
                    };
                    result.Add(fontModel);
                }
            }
            return result.ToArray();
        }
    }
}
#endif