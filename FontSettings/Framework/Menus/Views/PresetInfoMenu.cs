using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontSettings.Framework.Preset;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus.Views
{
    internal class PresetInfoMenu : BaseMenu
    {
        private readonly IExtensible _preset;
        private readonly Action<PresetInfoMenu> _onClosed;

        public PresetInfoMenu(IExtensible preset, Action<PresetInfoMenu> onClosed)
        {
            this._preset = preset;
            this._onClosed = onClosed;

            this.ResetComponents();
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            var border = new TextureBoxBorder();
            border.SuggestedWidth = 500;
            border.MinHeight = 500;
            border.MaxHeight = 700;
            border.Box = TextureBoxes.ThickBorder;
            border.Padding += new Thickness(8);
            border.DrawShadow = false;
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                border.Child = grid;
                {
                    Label titleLabel = new Label();
                    titleLabel.Text = this._preset.TryGetInstance(out IPresetWithName withName) ? withName.Name : null;
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    grid.Children.Add(titleLabel);
                    grid.SetRow(titleLabel, 0);

                    var scrollViewer = new ScrollViewer();
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    scrollViewer.ShowsBackground = false;
                    grid.Children.Add(scrollViewer);
                    grid.SetRow(scrollViewer, 1);
                    {
                        var stack = new StackContainer();
                        stack.Orientation = Orientation.Vertical;
                        scrollViewer.Content = stack;
                        {
                            void AddKeyValueEntry(string key, string value)
                            {
                                Grid lRGrid = new Grid();
                                lRGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                                lRGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnit.Percent) });
                                if (stack.Children.Any())
                                {
                                    float interval = Game1.smallFont.MeasureString("A").X;
                                    lRGrid.Margin = new Thickness(0, interval, 0, 0);
                                }
                                stack.Children.Add(lRGrid);
                                {
                                    Label keyLabel = new Label();
                                    keyLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                    keyLabel.VerticalAlignment = VerticalAlignment.Top;
                                    keyLabel.Wrapping = TextWrapping.Enable;
                                    keyLabel.Text = key;
                                    lRGrid.Children.Add(keyLabel);
                                    lRGrid.SetColumn(keyLabel, 0);

                                    Label valueLabel = new Label();
                                    valueLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                    valueLabel.VerticalAlignment = VerticalAlignment.Top;
                                    valueLabel.Wrapping = TextWrapping.Enable;
                                    valueLabel.Font = FontType.SmallFont;
                                    valueLabel.Text = value;
                                    lRGrid.Children.Add(valueLabel);
                                    lRGrid.SetColumn(valueLabel, 1);
                                }
                            }

                            if (this._preset.TryGetInstance(out IPresetWithName withName_))
                                AddKeyValueEntry(I18n.Ui_PresetInfoMenu_Name(), withName_.Name);

                            if (this._preset.TryGetInstance(out IPresetWithDescription withNotes))
                                AddKeyValueEntry(I18n.Ui_PresetInfoMenu_Notes(), withNotes.Description);

                            if (this._preset.TryGetInstance(out IPresetFromContentPack fromContentPack))
                            {
                                IManifest manifest = fromContentPack.SContentPack.Manifest;

                                AddKeyValueEntry(I18n.Ui_PresetInfoMenu_CpName(), manifest.Name);
                                AddKeyValueEntry(I18n.Ui_PresetInfoMenu_CpAuthor(), manifest.Author);
                                AddKeyValueEntry(I18n.Ui_PresetInfoMenu_CpVer(), manifest.Version.ToString());
                            }
                        }
                    }

                    var okButton = new TextureButton(
                        Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
                    okButton.HorizontalAlignment = HorizontalAlignment.Right;
                    okButton.VerticalAlignment = VerticalAlignment.Bottom;
                    okButton.ClickSound = "bigDeSelect";
                    okButton.Click += this.OkButton_Click;
                    grid.Children.Add(okButton);
                    grid.SetRow(okButton, 2);
                }
            }
            context.SetRootElement(border);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Close()
        {
            this._onClosed(this);
        }
    }
}
