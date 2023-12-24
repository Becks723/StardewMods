using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValleyUI.Controls;
using StardewValleyUI;
using StardewValleyUI.Menus;
using StardewValley;
using Microsoft.Xna.Framework;
using FontSettings.Framework.Menus.ViewModels;
using StardewValleyUI.Data;
using Microsoft.Xna.Framework.Graphics;

namespace FontSettings.Framework.Menus.Views
{
    internal class ExportMenu : BaseMenu
    {
        private readonly Action _onClosed;
        private readonly ExportMenuModel _viewModel;

        public ExportMenu(Action onClosed, ExportMenuModel viewModel)
        {
            this._onClosed = onClosed;

            this.ResetComponents();

            this.DataContext = this._viewModel = viewModel;
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            var border = new TextureBoxBorder();
            border.MinWidth = 500;
            border.MaxWidth = 800;
            border.MinHeight = 500;
            border.MaxHeight = 700;
            border.Box = TextureBoxes.ThickBorder;
            border.Padding += new Thickness(8);
            {
                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                border.Child = grid;
                {
                    Label titleLabel = new Label();
                    titleLabel.Text = I18n.Ui_ExportMenu_Title();
                    titleLabel.Font = FontType.SpriteText;
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    grid.Children.Add(titleLabel);
                    grid.SetRow(titleLabel, 0);

                    ScrollViewer viewer = new ScrollViewer();
                    viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
                    viewer.ShowsBackground = false;
                    grid.Children.Add(viewer);
                    grid.SetRow(viewer, 1);
                    {
                        StackContainer stack = new StackContainer();
                        stack.Orientation = Orientation.Vertical;
                        viewer.Content = stack;
                        {
                            AddLabelOption(I18n.Ui_ExportMenu_OutInXnb(), () =>
                            {
                                CheckBox checkBox = new CheckBox();
                                context.TwoWayBinds(() => this._viewModel.InXnb, () => checkBox.IsChecked);
                                return checkBox;
                            });

                            AddLabelOption(I18n.Ui_ExportMenu_OutDir(), () =>
                            {
                                Label label = new Label();
                                label.Font = FontType.SmallFont;
                                label.Wrapping = TextWrapping.Enable;
                                context.OneWayBinds(() => this._viewModel.OutputDirectory, () => label.Text);
                                return label;
                            });

                            AddLabelOption(I18n.Ui_ExportMenu_OutName(), customHorizAlignment: true, option: () =>
                            {
                                Grid g = new Grid();
                                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                                g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                {
                                    Textbox nameBox = new Textbox();
                                    context.TwoWayBinds(() => this._viewModel.OutputName, () => nameBox.Text);
                                    g.Children.Add(nameBox);
                                    g.SetColumn(nameBox, 0);

                                    Label extLabel = new Label();
                                    extLabel.Font = FontType.SmallFont;
                                    context.OneWayBinds(() => this._viewModel.OutputExtensions, () => extLabel.Text, new OutputExtensionsConverter());
                                    g.Children.Add(extLabel);
                                    g.SetColumn(extLabel, 1);
                                }
                                return g;
                            });

                            // XNB settings section
                            Label xnbSection = new Label();
                            xnbSection.Text = I18n.Ui_ExportMenu_Section_Xnb();
                            xnbSection.Font = FontType.SpriteText;
                            xnbSection.HorizontalAlignment = HorizontalAlignment.Left;
                            xnbSection.Margin = new Thickness(0, 16, 0, 0);
                            stack.Children.Add(xnbSection);

                            AddLabelOption(I18n.Ui_ExportMenu_Profile(), () =>
                            {
                                ComboBox combo = new ComboBox();
                                combo.SuggestedWidth = 250f;
                                combo.ItemsSource = Enum.GetValues<GraphicsProfile>();
                                combo.ItemAppearance = Appearance.ForData(new GraphicsProfileAppearance());
                                context.TwoWayBinds(() => this._viewModel.GraphicsProfile, () => combo.SelectedItem);
                                return combo;
                            });

                            AddLabelOption(I18n.Ui_ExportMenu_Compress(), () =>
                            {
                                CheckBox checkBox = new CheckBox();
                                context.TwoWayBinds(() => this._viewModel.IsCompressed, () => checkBox.IsChecked);
                                return checkBox;
                            });

                            void AddLabelOption(string labelText, Func<Element> option,
                                bool customHorizAlignment = false,
                                bool customVertiAlignment = false)
                                => stack.Children.Add(LabelOption(labelText, option, customHorizAlignment, customVertiAlignment));

                            Element LabelOption(string labelText, Func<Element> option,
                                bool customHorizAlignment = false,
                                bool customVertiAlignment = false)
                            {
                                var g = new Grid();
                                g.Margin = new Thickness(0, 8, 0, 0);
                                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnit.Percent) });
                                g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnit.Percent) });
                                {
                                    Label l = new Label();
                                    l.Text = labelText;
                                    l.Font = FontType.DialogueFont;
                                    l.HorizontalAlignment = HorizontalAlignment.Left;
                                    l.VerticalAlignment = VerticalAlignment.Center;
                                    g.Children.Add(l);
                                    g.SetColumn(l, 0);

                                    Element opt = option();
                                    if (!customHorizAlignment)
                                        opt.HorizontalAlignment = HorizontalAlignment.Left;
                                    if (!customVertiAlignment)
                                        opt.VerticalAlignment = VerticalAlignment.Center;
                                    g.Children.Add(opt);
                                    g.SetColumn(opt, 1);
                                }
                                return g;
                            }
                        }
                    }

                    StackContainer buttonStack = new StackContainer();
                    buttonStack.Orientation = Orientation.Horizontal;
                    buttonStack.HorizontalAlignment = HorizontalAlignment.Right;
                    grid.Children.Add(buttonStack);
                    grid.SetRow(buttonStack, 2);
                    {
                        var exportButton = new TextureButton(Textures.Export, null, 4f);
                        exportButton.ClickSound = "coin";
                        exportButton.ToolTip = I18n.Ui_ExportMenu_Export();
                        context.OneWayBinds(() => this._viewModel.IsExporting, () => exportButton.GreyedOut);
                        context.OneWayBinds(() => this._viewModel.ExportCommand, () => exportButton.Command);
                        buttonStack.Children.Add(exportButton);

                        var cancelButton = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
                        cancelButton.ClickSound = "bigDeSelect";
                        cancelButton.ToolTip = I18n.Ui_ExportMenu_Cancel();
                        cancelButton.Click += this.OnCanelButtonClicked;
                        buttonStack.Children.Add(cancelButton);
                    }
                }
            }
            context.SetRootElement(border);
        }

        private void OnCanelButtonClicked(object sender, EventArgs e)
        {
            this._onClosed();
        }

        private class OutputExtensionsConverter : BindingConverter<string[], string>
        {
            protected override string Convert(string[] extensions, object? parameter)
            {
                if (extensions == null)
                    return ".";

                return new StringBuilder()
                    .Append('.')
                    .AppendJoin(" + ", extensions)
                    .ToString();
            }

            protected override string[] ConvertBack(string targetValue, object? parameter)
            {
                throw new NotSupportedException();
            }
        }

        private class GraphicsProfileAppearance : DataAppearanceBuilder<GraphicsProfile>
        {
            protected override Element Build(AppearanceBuildContext<GraphicsProfile> context)
            {
                GraphicsProfile value = context.Target;

                Label label = new Label();
                label.Text = value.ToString();
                label.Font = FontType.SmallFont;
                label.Margin = new Thickness(4, 0, 0, 0);
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Center;
                return label;
            }
        }
    }
}
