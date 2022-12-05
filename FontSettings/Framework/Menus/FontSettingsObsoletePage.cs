using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Data;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus
{
    internal class FontSettingsObsoletePage : BaseMenu
    {
        private readonly FontSettingsObsoletePageModel _viewModel;

        public FontSettingsObsoletePage(ModConfig config, Action<ModConfig> saveConfig, int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            this.ResetComponents();

            this._viewModel = new FontSettingsObsoletePageModel(config, saveConfig);
            this.DataContext = this._viewModel;
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            Border border = new Border();
            border.SuggestedWidth = this.width;
            border.SuggestedHeight = this.height;
            border.Padding = new Thickness(48, 0);
            {
                StackContainer stack = new StackContainer();
                stack.Orientation = Orientation.Vertical;
                stack.VerticalAlignment = VerticalAlignment.Center;
                border.Child = stack;
                {
                    Label label = new Label();
                    label.Font = FontType.DialogueFont;
                    label.Wrapping = TextWrapping.Enable;
                    context.OneWayBinds(() => this._viewModel.HotkeyString, () => label.Text, new StringFormatter(this.GetText));
                    stack.Children.Add(label);

                    CheckBox checkBox = new CheckBox();
                    checkBox.HorizontalAlignment = HorizontalAlignment.Left;
                    checkBox.Margin = new Thickness(0, borderWidth / 2, 0, 0);
                    context.TwoWayBinds(() => this._viewModel.HideThisTab, () => checkBox.IsChecked);
                    stack.Children.Add(checkBox);
                    {
                        Label boxLabel = new Label();
                        boxLabel.Font = FontType.DialogueFont;
                        boxLabel.Text = I18n.Ui_ObsoletePage_HideThisTab();
                        checkBox.Content = boxLabel;
                    }
                }
            }

            context.SetRootElement(border);
        }

        private string GetText(string hotkey)
        {
            return I18n.Ui_ObsoletePage_Paragraph(hotkey);
        }

        private class StringFormatter : BindingConverter<string, string>
        {
            private readonly Func<string, string> _formatter;

            public StringFormatter(Func<string, string> formatter)
            {
                this._formatter = formatter;
            }

            public override string Convert(string sourceValue)
            {
                return this._formatter(sourceValue);
            }

            public override string ConvertBack(string targetValue)
            {
                throw new NotSupportedException();
            }
        }
    }
}
