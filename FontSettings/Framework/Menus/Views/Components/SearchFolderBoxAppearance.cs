using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValleyUI;
using StardewValleyUI.Controls;
using StardewValleyUI.Controls.Primitives;
using StardewValleyUI.Data.Converters;

namespace FontSettings.Framework.Menus.Views.Components
{
    internal class SearchFolderBoxAppearance : ControlAppearanceBuilder<SearchFolderBox>
    {
        private readonly Texture2D _emoteMenu = Game1.temporaryContent.Load<Texture2D>("LooseSprites/EmoteMenu");
        private readonly TextureBox _borderlessBox = TextureBox.From(
            texture: Game1.menuTexture,
            sourceRect: Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1),
            scale: 1f,
            borderThickness: new Thickness(8));

        protected override Element Build(AppearanceBuildContext<SearchFolderBox> context)
        {
            SearchFolderBox box = context.Target;

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            {
                ScrollViewer viewer = new ScrollViewer();
                viewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                viewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                viewer.MaxHeight = 256;
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
                    context.DefinePart(ItemsControl.ITEMS_PRESENTER_PART, presenter);
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
                        context.OneWayBinds(() => box.NewFolderCommand, () => newButton.Command);
                        stack.Children.Add(newButton);

                        var delButton = new TextureButton(Game1.mouseCursors, new Rectangle(268, 470, 16, 16), 3f);
                        delButton.VerticalAlignment = VerticalAlignment.Center;
                        delButton.Margin = new Thickness(8, 0, 0, 0);
                        context.OneWayBinds(() => box.DeleteFolderCommand, () => delButton.Command);
                        stack.Children.Add(delButton);

                        var editButton = new TextureButton(this._emoteMenu, new Rectangle(64, 16, 16, 16), 3f);
                        editButton.VerticalAlignment = VerticalAlignment.Center;
                        editButton.Margin = new Thickness(8, 0, 0, 0);
                        context.OneWayBinds(() => box.EditFolderCommand, () => editButton.Command);
                        stack.Children.Add(editButton);
                    }
                }
            }
            return grid;
        }
    }
}
