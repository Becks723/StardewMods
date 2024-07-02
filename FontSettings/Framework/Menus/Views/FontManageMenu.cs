using System;
using System.Diagnostics;
using FontSettings.Framework.Menus.ViewModels;
using FontSettings.Framework.Menus.Views.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Controls.Primitives;
using StardewValleyUI.Data.Converters;
using StardewValleyUI.Menus;

namespace FontSettings.Framework.Menus.Views
{
    internal class FontManageMenu : BaseMenu, IOverlayMenu
    {
        private readonly FontManageMenuModel _viewModel;
        private readonly Action<FontManageMenu> _onOpened;
        private readonly Action<FontManageMenu> _onClosed;

        public event EventHandler<OverlayMenuClosedEventArgs> Closed;

        private readonly Texture2D _sectionBox = Textures.SectionBox;
        private readonly Texture2D _emoteMenu = Game1.temporaryContent.Load<Texture2D>("LooseSprites/EmoteMenu");
        private readonly TextureBox _borderlessBox = TextureBox.From(
            texture: Game1.menuTexture,
            sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1),
            scale: 1f,
            borderThickness: new Thickness(8));

        public FontManageMenu(SearchManager searchManager, Action<FontManageMenu> onOpened, Action<FontManageMenu> onClosed)
        {
            this._onOpened = onOpened;
            this._onClosed = onClosed;

            this.ResetComponents();

            this._viewModel = new FontManageMenuModel(searchManager);
            this.DataContext = this._viewModel;
        }

        protected override void ResetComponents(MenuInitializationContext context)
        {
            var border = new TextureBoxBorder();
            border.SuggestedWidth = 600;
            border.SuggestedHeight = 800;
            border.Box = TextureBoxes.Default;
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
                    titleLabel.Text = I18n.Ui_FontManageMenu_Title();
                    titleLabel.VerticalAlignment = VerticalAlignment.Center;
                    titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
                    titleLabel.Font = FontType.SpriteText;
                    grid.Children.Add(titleLabel);
                    grid.SetRow(titleLabel, 0);

                    ScrollViewer view = new ScrollViewer();
                    view.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                    view.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                    view.ShowsBackground = false;
                    grid.Children.Add(view);
                    grid.SetRow(view, 1);
                    {
                        StackContainer stack = new StackContainer();
                        stack.Orientation = Orientation.Vertical;
                        view.Content = stack;
                        {
                            float sectionPadding = borderWidth / 3;
                            float frameSpacing = borderWidth / 2;
                            float optionSpacing = borderWidth / 2;

                            Border SectionBorder()
                            {
                                var border = new TextureBoxBorder();
                                border.Box = TextureBox.From(this._sectionBox, new Rectangle(0, 0, 3, 3), 1f, Thickness.One);
                                border.Padding += new Thickness(sectionPadding);
                                return border;
                            }

                            // search settings
                            var searchBorder = SectionBorder();
                            searchBorder.Margin = new Thickness(0, frameSpacing, 0, 0);
                            stack.Children.Add(searchBorder);
                            {
                                var searchStack = new StackContainer();
                                searchStack.Orientation = Orientation.Vertical;
                                searchBorder.Child = searchStack;
                                {
                                    Label searchLabel = new Label();
                                    searchLabel.Font = FontType.SpriteText;
                                    searchLabel.Text = I18n.Ui_FontManageMenu_Section_Search();
                                    searchLabel.HorizontalAlignment = HorizontalAlignment.Left;
                                    searchStack.Children.Add(searchLabel);

                                    SearchFolderBox folderBox = new SearchFolderBox();
                                    context.OneWayBinds(() => this._viewModel.SearchFolders, () => folderBox.ItemsSource);
                                    context.TwoWayBinds(() => this._viewModel.SelectedFolderIndex, () => folderBox.SelectedIndex);
                                    context.OneWayBinds(() => this._viewModel.NewFolderCommand, () => folderBox.NewFolderCommand);
                                    context.OneWayBinds(() => this._viewModel.NewFolderCommand, () => folderBox.NewFolderCommand);
                                    context.OneWayBinds(() => this._viewModel.EditFolderCommand, () => folderBox.EditFolderCommand);
                                    folderBox.Appearance = Appearance.ForControl<SearchFolderBox>(ctx =>
                                    {
                                        SearchFolderBox box = ctx.Target;

                                        Grid grid = new Grid();
                                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                        {
                                            ScrollViewer viewer = new ScrollViewer();
                                            viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                                            viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                                            viewer.SuggestedHeight = 256;
                                            viewer.CanContentScroll = true;
                                            viewer.BackgroundBox = this._borderlessBox;
                                            viewer.Appearance = Appearance.ForControl<ScrollViewer>(context =>
                                            {
                                                var scrollViewer = context.Target;

                                                TextureBoxBorder background = new TextureBoxBorder();
                                                background.DrawShadow = false;
                                                context.OneWayBinds(() => scrollViewer.BackgroundBox, () => background.Box);
                                                context.OneWayBinds(() => scrollViewer.ShowsBackground, () => background.Box, new DelegateConverter<bool, TextureBox>(on => on ? scrollViewer.BackgroundBox : null));
                                                {
                                                    Grid grid = new Grid();
                                                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                                                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.FillRemaningSpace });
                                                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                                    background.Child = grid;
                                                    {
                                                        var contentPresenter = new ScrollContentPresenter();
                                                        context.DefinePart(ScrollViewer.CONTENT_PART, contentPresenter);
                                                        context.DefinePart(ContentControl.CONTENT_PRESENTER_PART, contentPresenter);
                                                        context.OneWayBinds(() => scrollViewer.CanContentScroll, () => contentPresenter.CanContentScroll);
                                                        grid.Children.Add(contentPresenter);
                                                        grid.SetColumn(contentPresenter, 0);
                                                        grid.SetRow(contentPresenter, 0);

                                                        ScrollBar vertiScrollBar = new ScrollBar();
                                                        vertiScrollBar.Orientation = Orientation.Vertical;
                                                        context.OneWayBinds(() => scrollViewer.ActualVerticalScrollBarVisibility, () => vertiScrollBar.Visibility);
                                                        context.OneWayBinds(() => scrollViewer.VerticalOffset, () => vertiScrollBar.Value);
                                                        context.OneWayBinds(() => scrollViewer.ScrollableHeight, () => vertiScrollBar.Maximum);
                                                        context.OneWayBinds(() => scrollViewer.ViewportHeight, () => vertiScrollBar.Viewport);
                                                        context.DefinePart(ScrollViewer.VSCROLLBAR_PART, vertiScrollBar);
                                                        grid.Children.Add(vertiScrollBar);
                                                        grid.SetColumn(vertiScrollBar, 1);
                                                        grid.SetRow(vertiScrollBar, 0);

                                                        ScrollBar horizScrollBar = new ScrollBar();
                                                        horizScrollBar.Orientation = Orientation.Horizontal;
                                                        context.OneWayBinds(() => scrollViewer.ActualHorizontalScrollBarVisibility, () => horizScrollBar.Visibility);
                                                        context.OneWayBinds(() => scrollViewer.HorizontalOffset, () => horizScrollBar.Value);
                                                        context.OneWayBinds(() => scrollViewer.ScrollableWidth, () => horizScrollBar.Maximum);
                                                        context.OneWayBinds(() => scrollViewer.ViewportWidth, () => horizScrollBar.Viewport);
                                                        context.DefinePart(ScrollViewer.HSCROLLBAR_PART, horizScrollBar);
                                                        grid.Children.Add(horizScrollBar);
                                                        grid.SetColumn(horizScrollBar, 0);
                                                        grid.SetRow(horizScrollBar, 1);
                                                    }
                                                }
                                                return background;
                                            });
                                            grid.Children.Add(viewer);
                                            grid.SetRow(viewer, 0);
                                            {
                                                ItemsPresenter presenter = new ItemsPresenter();
                                                ctx.DefinePart(ItemsControl.ITEMS_PRESENTER_PART, presenter);
                                                viewer.Content = presenter;
                                            }

                                            var border = new TextureBoxBorder();
                                            border.Box = this._borderlessBox;
                                            border.DrawShadow = false;
                                            border.HorizontalAlignment = HorizontalAlignment.Right;
                                            grid.Children.Add(border);
                                            grid.SetRow(border, 1);
                                            {
                                                StackContainer stack = new StackContainer();
                                                stack.Orientation = Orientation.Horizontal;
                                                border.Child = stack;
                                                {
                                                    var newButton = new TextureButton(Game1.mouseCursors, new Rectangle(0, 411, 16, 16), 3f);
                                                    newButton.VerticalAlignment = VerticalAlignment.Center;
                                                    newButton.ClickSound = "coin";
                                                    newButton.ToolTip = I18n.Ui_FontManageMenu_New();
                                                    newButton.Click += (sender, e) => viewer.ScrollToBottom();  // always scroll to btm when new folder is added. Maybe a temp solution? Any better solution?
                                                    context.OneWayBinds(() => this._viewModel.NewFolderCommand, () => newButton.Command);
                                                    context.OneWayBinds(() => this._viewModel.CanNewFolder, () => newButton.GreyedOut, new TrueFalseConverter());
                                                    stack.Children.Add(newButton);

                                                    var delButton = new TextureButton(Game1.mouseCursors, new Rectangle(268, 470, 16, 16), 3f);
                                                    delButton.VerticalAlignment = VerticalAlignment.Center;
                                                    delButton.Margin = new Thickness(8, 0, 0, 0);
                                                    delButton.ClickSound = "trashcan";
                                                    delButton.ToolTip = I18n.Ui_FontManageMenu_Delete();
                                                    context.OneWayBinds(() => this._viewModel.DeleteFolderCommand, () => delButton.Command);
                                                    context.OneWayBinds(() => this._viewModel.CanDeleteFolder, () => delButton.GreyedOut, new TrueFalseConverter());
                                                    stack.Children.Add(delButton);
                                                }
                                            }
                                        }
                                        return grid;
                                    });
                                    folderBox.ItemAppearance = Appearance.ForData(context =>
                                    {
                                        var folder = context.Target as SearchFolderViewModel;

                                        Grid grid = new Grid();
                                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.FillRemaningSpace });
                                        grid.MousePressed += this.OnFolderItemClicked;
                                        {
                                            CheckBox checkBox = new CheckBox();
                                            checkBox.VerticalAlignment = VerticalAlignment.Center;
                                            context.TwoWayBinds(() => folder.Enabled, () => checkBox.IsChecked);
                                            grid.Children.Add(checkBox);
                                            grid.SetColumn(checkBox, 0);

                                            Label label = new Label();
                                            label.VerticalAlignment = VerticalAlignment.Center;
                                            label.HorizontalAlignment = HorizontalAlignment.Left;
                                            label.Font = FontType.SmallFont;
                                            label.Wrapping = TextWrapping.Enable;
                                            label.Margin = new Thickness(24, 0, 0, 0);
                                            context.OneWayBinds(() => folder.Path, () => label.Text);
                                            context.OneWayBinds(() => folder.IsEditing, () => label.Visibility,
                                                new DelegateConverter<bool, Visibility>(isEditing => isEditing ? Visibility.Hidden : Visibility.Visible));
                                            grid.Children.Add(label);
                                            grid.SetColumn(label, 1);

                                            Textbox2 textbox = new Textbox2();
                                            textbox.VerticalAlignment = VerticalAlignment.Center;
                                            textbox.Margin = new Thickness(24 - 14, 4, 0, 4);
                                            textbox.EnterPressed += this.OnPathBoxEnterPressed;
                                            context.TwoWayBinds(() => folder.Path, () => textbox.Text);
                                            context.OneWayBinds(() => folder.IsEditing, () => textbox.Visibility, new BooleanVisibilityConverter(Visibility.Hidden));
                                            grid.Children.Add(textbox);
                                            grid.SetColumn(textbox, 1);
                                        }
                                        return grid;
                                    });
                                    searchStack.Children.Add(folderBox);

                                    var logOption = new StackContainer();
                                    logOption.Orientation = Orientation.Horizontal;
                                    logOption.HorizontalAlignment = HorizontalAlignment.Left;
                                    logOption.Margin = new Thickness(0, optionSpacing, 0, 0);
                                    searchStack.Children.Add(logOption);
                                    {
                                        var checkbox = new CheckBox();
                                        context.TwoWayBinds(() => this._viewModel.LogDetails, () => checkbox.IsChecked);

                                        var label = new Label();
                                        label.Font = FontType.SmallFont;
                                        label.Text = I18n.Ui_FontManageMenu_LogDetails();
                                        label.Margin = new Thickness(borderWidth / 3, 0, 0, 0);

                                        logOption.Children.Add(checkbox);
                                        logOption.Children.Add(label);
                                    }
                                }
                            }
                        }
                    }

                    var cancelButton = new TextureButton(Game1.mouseCursors, new Rectangle(192, 256, 64, 64));
                    cancelButton.HorizontalAlignment = HorizontalAlignment.Right;
                    cancelButton.VerticalAlignment = VerticalAlignment.Bottom;
                    cancelButton.ClickSound = "bigDeSelect";
                    cancelButton.ToolTip = I18n.Ui_FontManageMenu_Cancel();
                    cancelButton.CommandParameter = this;
                    context.OneWayBinds(() => this._viewModel.CancelCommand, () => cancelButton.Command);
                    grid.Children.Add(cancelButton);
                    grid.SetRow(cancelButton, 2);

                    var okButton = new TextureButton(
                        Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46));
                    okButton.HorizontalAlignment = HorizontalAlignment.Right;
                    okButton.VerticalAlignment = VerticalAlignment.Bottom;
                    okButton.Margin = new Thickness(0, 0, 64 + borderWidth / 5, 0);
                    okButton.ClickSound = "coin";
                    okButton.ToolTip = I18n.Ui_FontManageMenu_Ok();
                    okButton.CommandParameter = this;
                    context.OneWayBinds(() => this._viewModel.OkCommand, () => okButton.Command);
                    grid.Children.Add(okButton);
                    grid.SetRow(okButton, 2);
                }
            }
            context.SetRootElement(border);
        }

        private void OnFolderItemClicked(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButtons.ContainsKey(MouseButtons.LeftButton))
            {
                // should not handle event here!

                // Find the item wrapper of this element.
                SearchFolderBoxItem item = default;
                {
                    Element elem = (Element)sender;
                    Element? parent = elem.GetVisualParent();  // TODO: GetVisualParent() is internal, find an alternative or make it public?
                    while (parent != null)
                    {
                        if (parent is SearchFolderBoxItem it)
                        {
                            item = it;
                            break;
                        }

                        parent = parent.GetVisualParent();
                    }

                    if (item == null)
                        throw new InvalidOperationException("???");
                }

                SearchFolderViewModel folder = item.Content as SearchFolderViewModel;
                Debug.Assert(folder != null);

                // item must be selected before edit/confirm.
                if (this._viewModel.SelectedFolderIndex == this._viewModel.SearchFolders.IndexOf(folder))
                {
                    if (!this._viewModel.IsEditingFolder)
                        this._viewModel.EditFolderCommand.Execute(null);
                    else
                        this._viewModel.ConfirmEditFolderCommand.Execute(null);
                }
            }
        }

        private void OnPathBoxEnterPressed(object sender, EventArgs e)
        {
            this._viewModel.ConfirmEditFolderCommand.Execute(null);
        }

        void IOverlayMenu.Open()
        {
            this._onOpened(this);
        }

        void IOverlayMenu.Close(object parameter)
        {
            Closed?.Invoke(this, new OverlayMenuClosedEventArgs(parameter));

            this._onClosed(this);
        }
    }
}
