# Font Settings
This mod:
*	Provides a common font settings interface like many games & softwares do. You can:
	- Change font size.
	- Pick various fonts.
	- Align text spacing.
	- and so on...


## Installation

1. Install [SMAPI](https://smapi.io/).
2. Download this mod from [Nexusmods](https://www.nexusmods.com/stardewvalley/mods/12467) and install. Mod files are shown as follows:

<table>
	<thead>
		<tr>
			<th>Mod file(s)</th>
			<th>Is required</th>
			<th>Description</th>
			<th>Install location</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><em>Font Settings x.x.x.zip</em></td>
			<td>√</td>
			<td>Main file</td>
			<td>Unzip and put into <em>Mods</em> folder.</td>
		</tr>
		<tr>
			<td><em>vanilla font file for Chinese</em></td>
			<td></td>
			<td rowspan=3>These are font files game uses for each language. Download if you want to keep original font but with size/spacing/linespacing change.</td>
			<td rowspan=3>Unzip and put into <strong>this mod's</strong> <em>assets/fonts</em> folder.</td>
		</tr>
		<tr>
			<td><em>vanilla font file for Japanese</em></td>
			<td></td>
		</tr>
		<tr>
			<td><em>vanilla font file for Korean</em></td>
			<td></td>
		</tr>
	</tbody>
</table>

3. Done!


## Usage
Two ways to open font settings menu:
1. Click the font button in title menu (the one with an uppercase 'A').
2. HotKey (default `LeftAlt + F`, to edit see [Configuration](#configuration)).

In the menu, configure the fonts, then click OK, your font is set!

#### Some Notes
* All the fonts available are from your computer. Current supported types are TrueType (.ttf, .ttc), OpenType (.otf, .otc, .ttf, .ttc).
* In game there're mainly three font types: **Small**, **Medium**, **Dialogue** font. You need to configure them seperately.


## Configuration

Compatible with [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)!

| Field	| Description |
| --- | --- |
| ExampleText | Text for font samples. Keep it empty and mod will use built-in text. Otherwise set your own. |
| OpenFontSettingsMenu | Keybind to open font menu, default `LeftAlt + F`. |
| DisableTextShadow | Miscellaneous option. Whether to close text shadow, default `false`. |
| MinFontSize | Min reachable value of the font size option, default `5`. |
| MaxFontSize | Max reachable value of the font size option, default `75`. |
| MinSpacing | Min reachable value of the spacing option, default `-10`. |
| MaxSpacing | Max reachable value of the spacing option, default `10`. |
| MinLineSpacing | Min reachable value of the line spacing option, default `5`. |
| MaxLineSpacing | Max reachable value of the line spacing option, default `75`. |
| MinCharOffsetX | Min reachable value of the x-offset option, default `-10`. |
| MaxCharOffsetX | Max reachable value of the x-offset option, default `10`. |
| MinCharOffsetY | Min reachable value of the y-offset option, default `-10`. |
| MaxCharOffsetY | Max reachable value of the y-offset option, default `10`. |
| MinPixelZoom | Min reachable value of the pixel zoom option, default `0.5`. |
| MaxPixelZoom | Max reachable value of the pixel zoom option, default `5`. |
| SimplifiedDropDown | Set `true` if UI drop down options cause glitches, low-performance, default `true`. |

#### Edit Mod Assets
Textures for this mod are stored in the `FontSettings/assets` folder. To edit them, you may:
* For own use, draw your own version and replace the texture file. Your own texture must be same size and filename as original one.
* For shared use, use [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915). Each png file is an asset entry. Its name formats as `Mods/Becks723.FontSettings/<filename>`.

	e.g. to edit `delete.png`, add this to json.
	```js
	{
		"Action": "EditImage",
		"Target": "Mods/Becks723.FontSettings/delete",
		"FromFile": "path/to/yourtexturefile.png"
	}
	```


## Compatibility:
Lastest version works with Windows, MacOS, Linux.

#### Conflict with SpriteMaster mod
Condition: SpriteMaster version >0.14.0, FontSettings version unlimited.<br/>
Symptom: changes made to fonts do not apply, game becomes laggy, high cpu, etc.<br/>
**Solution**: In the configuration menu, go to `SpriteMaster > Advanced Settings > Extras.OpenGL`, uncheck `Enabled` and `OptimizeTexture2DSetData`, and restart the game. <small>Test environment: FontSettings 0.9.0, SpriteMaster 0.15.0-beta.16.0</small>

## Help & Feedback:

#### Where to feedback/ask for help
1. At [Nexus modpage POSTS tab](https://www.nexusmods.com/stardewvalley/mods/12467?tab=posts).
2. At [Stardew Valley Discord](https://discord.gg/stardewvalley). Ping me _@Becks723#7620_ anytime. I won't be always around but I'll check.

#### Report a bug
1. At [Nexus modpage BUGS tab](https://www.nexusmods.com/stardewvalley/mods/12467?tab=bugs).
2. At [Github issues](https://github.com/Becks723/StardewMods/issues).


## Release Notes
#### 0.9.1 - 2023-04-29
- Fix no response pressing "Save As New Preset" button.
- Fix you cannot delete a preset.

#### 0.9.0 - 2023-04-18
- Add support for [Toolbar Icons](https://www.nexusmods.com/stardewvalley/mods/11026).
- You can customize this mod's textures (See [edit mod assets](#edit-mod-assets) section).
- Add a "Reset Font" option into UI, for restoring vanilla font.
- Improve font change effects when you select "keep original" (all common characters done).
- Improve font settings menu UI slightly.

#### 0.8.1 - 2023-04-02
- Fix error when select font after refreshing fonts.
- Fix font not reload until ok is pressed the second time.

#### 0.8.0 - 2023-04-02
- Some performance ups.
	- Speed up menu opening.
	- Reduce game not resposing times.
	- Optimized UI drop down options. See `SimplifiedDropDown` option in [configure](#configuration) section.
- Finally remove font settings menu from main game menu.
- Bugfixes.

#### 0.7.3 - 2023-03-01
- Fix dialogue font's characters incomplete/clipped render.
- Fix in some case cannot click ok button.

#### 0.7.2 - 2023-02-20
- Adds a font button into title menu. You can find it at lb corner, appear as an uppercase 'A'.
- Now supports custom language.
- Bugfixes.

#### 0.7.1 - 2023-02-14
- Slim mod file. Move those vanilla font file (assets/fonts) to optional mod file.
- Improve font change effects when you select "keep original". (all languages now support, except hu, ru, tr)
- Bugfixes.

#### 0.7.0 - 2023-02-06
- Improvements:
	- Display UI sliders' value.
	- Improve font change effects when you select "keep original". (current cjk)

- Bugfixes:
	- Fix a major bug where all fonts get lost after returning to title (or anyone invalidating it).
	- Fix a bug where everytime game launches, 1. ExampleText gets cleared, 2. PixelZoom is set to 1.0.
	- Add back the refresh button.

- Compatibility:
	- Drop 0.2 migration.

#### 0.6.2 - 2023-01-05
- Fix a bug where Dialogue font failed to set when 'Enabled' is not checked.

#### 0.6.1 - 2023-01-03
- Hotfix: In English no effect changing fonts.