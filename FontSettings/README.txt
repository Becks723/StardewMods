Font Settings:
	A mod to change Stardew Valley in-game fonts.


Installation:
	1. Install SMAPI (website: smapi.io)
	2. Put unzipped mod file into Mods folder.
	3. Done!


Compatibility:
	Lastest version works with Windows, MacOS, Linux.


Usage:
	Press LeftAlt + F to call out the Font Settings menu. Configure the fonts, then click OK, your font is set!
	Settings:
		General
			- Enabled: Whether to enable custom font. Check to enable, otherwise keep vanilla.
			- Font: Select the font you like. Those fonts are all from your computer.
			- Font Size: Configure the font size.

		Advanced
			- Spacing: Configure the horizontal spacing between two adjacent characters.
			- Line Spacing: Configure the vertical spacing between two adjacent lines.
			- X-offset: Configure the horizontal offset of each character.
			- Y-offset: Configure the vertical offset of each character.
			- Pixel Zoom (Dialogue Font only): Configure the factor by which to multiply the font size.

		Preset
			You can optionally store your configs as a preset, for convenivence. 
			To create a new preset, press Save current as...
			To edit a preset, switch to it and press save.
			To remove a preset, switch to it and press delete.

	About Font File:
		All the fonts available are from your computer. Supported types are TrueType (.ttf, .ttc), OpenType (.otf, .otc, .ttf, .ttc).

	About in-game Font Types:
		In game there're mainly three font types: Small, Medium, Dialogue fonts. You need to configure them seperately.

	About Latin Language:
		For those languages only contains latin characters, 
		they just use game's default font as Dialogue Font, which is hardcoded in a spritesheet, 
		so you might not set Dialogue Font for now.
		If you have a solution, let me know!


Configure:
	These are mod configs, not font configs above. All of these are supported in GenericModConfigMenu.

	- ExampleText               Text for font samples. Keep it empty and mod will use built-in text. Otherwise set your own.
    - OpenFontSettingsMenu      Keybind to open font menu, default LeftAlt + F.
    - DisableTextShadow         Miscellaneous option. Whether to close text shadow, default false.
	- MinFontSize				Min value of the font size option, default 5.
	- MaxFontSize				Max value of the font size option, default 75.
    - MinSpacing				Min value of the spacing option, default -10.
    - MaxSpacing				Max value of the spacing option, default 10.
    - MinLineSpacing			Min value of the line spacing option, default 5.
    - MaxLineSpacing			Max value of the line spacing option, default 75.
    - MinCharOffsetX			Min value of the x-offset option, default -10.
    - MaxCharOffsetX			Max value of the x-offset option, default 10.
    - MinCharOffsetY			Min value of the y-offset option, default -10.
    - MaxCharOffsetY			Max value of the y-offset option, default 10.
	- MinPixelZoom			    Min value of the pixel zoom option, default 0.5.
    - MaxPixelZoom			    Max value of the pixel zoom option, default 5.
    - FontSettingsInGameMenu    Legacy option, don't touch it.


Release Notes:
0.6.3 - 2023-01-
- Fix a bug where everytime game launches, 1. ExampleText gets cleared, 2. PixelZoom is set to 1.0.

0.6.2 - 2023-01-05
- Fix a bug where Dialogue font failed to set when 'Enabled' is not checked.

0.6.1 - 2023-01-03
- Hotfix: In English no effect changing fonts.