#### English | [中文](./字体包制作教程.md)

This guide helps mod authors create a Font Settings pack.

## Table of Contents
- [Overview](#overview)
- [Create a Font Settings pack](#create-a-font-settings-pack)
  - [Folder structure](#folder-structure)
  - [`content.json`](#contentjson)
- [Create your first Font Settings pack](#create-your-first-pack)
- [See Also](#see-also)

## Overview
A Font Settings pack is the content pack for [Font Settings mod](https://www.nexusmods.com/stardewvalley/mods/12467). It pre-defines one or more font settings for users to choose in game. In game it's converted into one or more **presets** (Figure 1).

| <img src="pack-in-game.gif" width="75%" /> |
| :-: |
| Figure 1: Font pack becomes preset(s) in game. |

## Create a Font Settings pack
### Folder structure

<pre>
Root Folder
  /*content.json<a href="#con"><sup>1</sup></a>
  /*manifest.json<a href="#man"><sup>2</sup></a>
  /i18n
    /default.json
    /zh.json
    /...
  /somefontfile.ttf
  /somecharfile.txt
  /...
</pre>
<small>
<p>File with * is necessary.</p>
<p id="con">1. content.json stores all settings infomation. It's the main one.</p>
<p id="man">2. manifest.json is the manifest of this pack (mod). This is a SMAPI stuff.</p>
</small>

### `content.json`
Main file. You will spend most of time editing this. It consists of an array of [settings object](#settings).

#### Settings
A group of parameters of font.

| Parameter | Type | Description | Preserved
| -- | -- | -- | -- |
| Name | `string` | Name of this settings. Supports [i18n](#translation-i18n). | |
| Notes | `string` | Some notes of this settings. Can be short or long. Supports [i18n](#translation-i18n). | |
| FontFile | `string` | Font to use. Should be a font name or relative path. The mod scans the best fit font, whose path **ends with** this  value. If your font's not installed in system, needs to include a real font file into mod folder.  | |
| Index | `int` | A zero-based index of font in a collection file. Often used when given file is a font collection, common extensions are .ttc, .otc. | |
| Type | `string` | In-game font type. Supported values are `small`, `medium`, `dialogue`. If multiple, seperate them with comma (`,`).  | |
| Language | `string` | Game language(s) supported. Should be one or more [language codes](#language-code). If multiple, seperate them with comma (`,`). | |
| Size | `float` | Font size **in pixel**. | |
| Spacing | `float` | Distance between two characters. Default `0`. | |
| LineSpacing | `float` | Distance between two baselines. Default `0`. | |
| OffsetX | `float` | Characters offset in X coord. Default `0`. | |
| OffsetY | `float` | Characters offset in Y coord. Default `0`. | |
| PixelZoom | `float` | Zoom level of **dialogue font**. A value `2` means twice the size when drawing. It's only applied to dialogue font. | |
| Character | `string` | Custom character range. This will override the vanilla range. See [character range](#character-range) section. | |
| CharacterAdd | `string` | An addtional range based on vanilla. See [character range](#character-range) section. | |
| CharacterRemove | `string` | An reduced range based on vanilla. See [character range](#character-range) section. | |

#### Translation (i18n)
Use `{{i18n: <key>}}` format. Replace `<key>` with certain key in i18n file.

#### Language Code
**Vanilla languages**
| Language | Code |
| --- | --- |
| English | `en` |
| German | `de` |
| Spanish | `es` |
| French | `fr` |
| Hungarian | `hu` |
| Italian | `it` |
| Japanese | `ja` |
| Korean | `ko` |
| Portuguese | `pt` |
| Russian | `ru` |
| Turkish | `tr` |
| Chinese | `zh` |

**Custom languages**

Defined in certain content pack, inside `content.json` file. Like:
```json
{
    "Format": "1.28.0",
    "Changes": [
        {
            "Action": "EditData",
            "Target": "Data/AdditionalLanguages",
            "Entries": {
                "Pathoschild.Esperanto": {
                    "ID": "Pathoschild.Esperanto",
                    "LanguageCode": "eo",  // >>> HERE IT IS <<<
                    "ButtonTexture": "Mods/Pathoschild.Esperanto/Button",
                    "UseLatinFont": true,
                    "TimeFormat": "[HOURS_24_00]:[MINUTES]",
                    "ClockTimeFormat": "[HOURS_24_00]:[MINUTES]",
                    "ClockDateFormat": "[DAY_OF_WEEK] [DAY_OF_MONTH]"
                }
            }
        }
    ]
}
```
Format see [Custom Language](https://stardewvalleywiki.com/Modding:Custom_languages) on wiki.

#### Character Range
To define a character range, you need to create a character file, then fill filename into certain parameter.

Supported character file formats:
- Simple text file. Enumerate all characters one by one. Should be a .txt file.

## Create your first pack
The following steps gives you a basic font pack.

1. Create a folder named `MyFontPack`. This is your mod root folder. 
2. In root folder, create `content.json` file. Copy following json into content file, then save.
```json
[
  // a settings with andy; 30px; small font and english in game.
  {
    "Name": "andy",
    "FontFile": "andyb.ttf",
    "Size": 30,
    "Spacing": 0,
    "LineSpacing": 30,
    "Type": "small",
    "Language": "en"
  },

  // a settings with Microsoft Sans Serif; 26px; small font and English, German, French in game; use custom character range.
  {
    "Name": "{{i18n: microsoft-sans-serif}}",
    "FontFile": "micross.ttf",
    "Size": 26,
    "Spacing": 0,
    "LineSpacing": 30,
    "Type": "small",
    "Language": "en,de,fr",
    "Character": "custom-range.txt"
  }
]
```

3. In root folder, create a `custom-range.txt` file. Copy following letters in it, then save in UTF-8. This is character file of the second settings in last step.
```
abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789¹²³ªº%$€¥£¢&*@#|áâàäåãæçéêèëíîìïıñóôòöõøœšßúûùüýÿžÂÀÄÅÃÆÇÉÊÈËÍÎÌÏÑÓÔÒÖÕØŒŠÛÙÜÝŸ,:;-–—•.…“‘’‘ ‚ “”„‹›«»/\?!¿¡()[]{}©®§+×=_°
```

4. In root folder, create a folder named `i18n`, then in it create a file named `default.json`. Copy following json in it, then save in UTF-8. This is i18n for the second settings.
```json
{
  "microsoft-sans-serif": "Microsoft Sans Serif"
}
```

5. In root folder, create `manifest.json` file with following json.
```json
{
  "Name": "MyFontPack",
  "Author": "your name",
  "Version": "1.0.0",
  "Description": "My first font pack.",
  "UniqueID": "YourName.MyFontPack",
  "UpdateKeys": [],
  "ContentPackFor": {
    "UniqueID": "Becks723.FontSettings"
  }
}
```

6. Put the pack into `Mods` folder and run the game. You should see it in Font Settings presets.

## See Also
- [Nexusmods modpage](https://www.nexusmods.com/stardewvalley/mods/12467)
- [README](README.md)