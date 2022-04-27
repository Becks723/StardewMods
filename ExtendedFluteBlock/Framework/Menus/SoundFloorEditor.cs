using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FluteBlockExtension.Framework.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValleyUI.Controls;
using static FluteBlockExtension.Framework.Constants;

namespace FluteBlockExtension.Framework.Menus
{
    /// <summary>A menu for editing sound-floor mapping.</summary>
    internal class SoundFloorEditor : IClickableMenu, IDisposable
    {
        public static SoundFloorEditor ActiveInstance { get; private set; }

        private static readonly Lazy<Texture> _refresh = new(LoadRefreshTexture);

        private readonly RootElement _root;

        private readonly Func<SoundFloorMap> _map;

        private readonly Action _saveOnExit;

        private readonly List<SoundData> _soundList = new();

        private readonly List<FloorData> _floorList = new();

        private readonly SoundFloorDataGrid _dataGrid;

        private readonly ScrollViewer2 _propertyView;

        private readonly SoundTable _soundTable;

        private static readonly FloorData[] FloorDatabase = new FloorData[]
        {
            FloorData.NonFloor,
            FloorData.WoodFloor,
            FloorData.StoneFloor,
            FloorData.WeatheredFloor,
            FloorData.CrystalFloor,
            FloorData.StrawFloor,
            FloorData.GravelPath,
            FloorData.WoodPath,
            FloorData.CrystalPath,
            FloorData.CobblestonePath,
            FloorData.SteppingStonePath,
            FloorData.BrickFloor,
            FloorData.RusticPlankFloor,
            FloorData.StoneWalkwayFloor,
        };

        /// <summary>GMCM config menu, to which <see cref="Game1.activeClickableMenu"/> will be set after this <see cref="SoundFloorEditor"/> exits.</summary>
        public IClickableMenu PreviousConfigMenu;

        static SoundFloorEditor()
        {
            ModEntry.Harmony.Patch(
                original: HarmonyLib.AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.backButtonPressed)),
                prefix: new HarmonyLib.HarmonyMethod(typeof(SoundFloorEditor), nameof(TitleMenu_backButtonPressed_Prefix))
            );
        }

        public SoundFloorEditor(Func<SoundFloorMap> map, Action? saveOnExit = null)
        {
            ActiveInstance = this;

            this._map = map;
            this._saveOnExit = saveOnExit;

            this._root = new RootElement();

            // left: data grid.
            {
                this._dataGrid = new SoundFloorDataGrid
                {
                    MaxDisplayRows = 8,
                    _soundList = this._soundList,
                    _floorList = this._floorList,
                };
                this._dataGrid.SelectionChanged += this.DataGrid_SelectionChanged;

                // fill with data.
                var rawMap = map();
                FloorData[] missingFloors = FloorDatabase.Where(floor => !rawMap.Any(item => item.Floor == floor)).ToArray();
                var data = from item in rawMap.Concat(missingFloors.Select(floor => new SoundFloorMapItem() { Sound = SoundData.Empty, Floor = floor }))
                           where item?.Floor != null
                           where !item.Floor.IsEmptyFloor()
                           orderby item.Floor.WhichFloor
                           select item.Sound is null
                           ? new SoundFloorMapItem { Sound = SoundData.Empty, Floor = item.Floor }
                           : item;
                foreach (var item in data)
                {
                    this._soundList.Add(item.Sound);
                    this._floorList.Add(item.Floor);

                    this._dataGrid.AddRow(item);
                }

                this.width += this._dataGrid.Width + borderWidth;
            }

            // middle: 3 buttons: up, down, refresh.
            Button2 upButton, downButton, refreshButton;
            int buttonX = this._dataGrid.Width + borderWidth;
            {
                int buttonY = 0;
                upButton = new Button2
                {
                    LocalPosition = new Vector2(buttonX, buttonY),
                    SettableWidth = 80,
                    SettableHeight = 50,
                    Content = Game1.mouseCursors,
                    SourceRectangle = new Rectangle(76, 72, 40, 44),
                    Scale = 0.7f
                };
                upButton.Click += this.UpButton_Click;
                buttonY += upButton.Height + borderWidth;

                downButton = new Button2
                {
                    LocalPosition = new Vector2(buttonX, buttonY),
                    SettableWidth = 80,
                    SettableHeight = 50,
                    Content = Game1.mouseCursors,
                    SourceRectangle = new Rectangle(12, 76, 40, 44),
                    Scale = 0.7f
                };
                downButton.Click += this.DownButton_Click;
                buttonY += downButton.Height + borderWidth;

                refreshButton = new Button2
                {
                    LocalPosition = new Vector2(buttonX, buttonY),
                    SettableWidth = 80,
                    SettableHeight = 50,
                    Content = _refresh.Value,
                    Scale = 1.5f
                };
                refreshButton.Click += this.RefreshButton_Click;
                buttonY += refreshButton.Height + borderWidth;

                this.width += upButton.Width + borderWidth;
            }

            // right: property table.
            {
                this._propertyView = new ScrollViewer2()
                {
                    LocalPosition = new Vector2(buttonX + upButton.Width + borderWidth, 0),
                    SettableWidth = 600,
                    SettableHeight = this._dataGrid.Height,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                };

                this.width += this._propertyView.Width;
            }

            this._root.Add(this._dataGrid, upButton, downButton, refreshButton, this._propertyView);

            // update overall bounds.
            this.height = this._dataGrid.Height;
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            this._root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            // init sound, floor table.
            {
                this._soundTable = new SoundTable()
                {
                    SettableWidth = this._propertyView.Width - 32,
                    SettableHeight = this._propertyView.Height - 32,
                };
                this._soundTable.OkButtonClick += this.SoundTable_OkButtonClick;
            }

            // register "after exit" event.
            this.exitFunction = this.OnExited;
        }

        /// <summary>Callback before GMCM's save function called.</summary>
        public void OnSaving()
        {
            var map = this._map();

            map.Clear();
            for (int i = 0; i < this._soundList.Count; i++)
            {
                map.Add(new() { Sound = this._soundList[i], Floor = this._floorList[i] });
            }
        }

        public override bool readyToClose()
        {
            return !this._soundTable.AnyTextboxFocused;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this._propertyView.Content is OptionTable table)
            {
                table.ReceiveLeftClick(x, y, playSound);
            }

            this._dataGrid.ReceiveLeftClick(x, y, playSound);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            if (this._propertyView.Content is OptionTable table)
            {
                table.PerformHoverAction(x, y);
            }

            this._dataGrid.PerformHoverAction(x, y);
        }

        private void SoundTable_OkButtonClick(object sender, EventArgs e)
        {
            if (this._dataGrid.SelectedItem is SoundDataGridItem soundGridItem)
            {
                int index = this._dataGrid.SoundColumn.Rows.IndexOf(soundGridItem);

                this._soundList[index] = soundGridItem.Sound = this._soundTable.BuildData();
            }
        }

        private void DataGrid_SelectionChanged(object sender, EventArgs e)
        {
            switch (this._dataGrid.SelectedItem)
            {
                case SoundDataGridItem soundGrid:
                    this._soundTable.LoadData(soundGrid.Sound);
                    this._propertyView.Content = this._soundTable;
                    break;
            }
        }

        private void DownButton_Click(object sender, EventArgs e)
        {
            this._dataGrid.MoveDown();
        }

        private void UpButton_Click(object sender, EventArgs e)
        {
            this._dataGrid.MoveUp();
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            SoundTable.AllCues = CueUtilites.GetAllCues().ToArray();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            this._root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this.PreviousConfigMenu?.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this._root.Update(time);
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground)
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            base.draw(b);
            this._root.Draw(b);
            this.drawMouse(b);
        }

        private void OnExited()
        {
            // Title menu exit is not available here.
            // The title menu uses a back button to exit its submenu, so must edit TitleMenu.backButtonPressed logic.
            if (this.PreviousConfigMenu != null)
            {
                Game1.activeClickableMenu = this.PreviousConfigMenu;

                // reattach.
                this.exitFunction = this.OnExited;
            }

            // for individual case, do save when exiting.
            if (this._saveOnExit != null)
            {
                this.OnSaving();
                this._saveOnExit();
            }

            ActiveInstance = null;
        }

        private static bool TitleMenu_backButtonPressed_Prefix()
        {
            if (TitleMenu.subMenu is SoundFloorEditor editor && TitleMenu.subMenu.readyToClose())
            {
                Game1.playSound("bigDeSelect");
                TitleMenu.subMenu = editor.PreviousConfigMenu;
                return false;  // skip original.
            }

            return true;
        }

        private static Texture2D LoadRefreshTexture()
        {
            int width = 16;
            int height = 16;
            Texture2D result = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
            Dictionary<Color, (int x, int y)[]> colors = new()
            {
#pragma warning disable format
                { new Color(91, 43, 42, 255), new[] { (5, 0), (6, 0), (7, 0), (8, 0), (9, 0), (10, 0), (11, 0), (1, 1), (2, 1), (4, 1), (13, 1), (1, 2), (14, 2), (1, 3), (8, 3), (9, 3), (10, 3), (11, 3), (14, 3), (1, 4), (12, 4), (15, 4), (1, 5), (13, 5), (15, 5), (1, 6), (13, 6), (15, 6), (1, 7), (3, 7), (4, 7), (6, 7), (14, 7), (1, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (0, 9), (2, 9), (8, 9), (14, 9), (0, 10), (2, 10), (9, 10), (14, 10), (0, 11), (3, 11), (8, 11), (14, 11), (1, 12), (4, 12), (5, 12), (6, 12), (7, 12), (14, 12), (1, 13), (12, 13), (14, 13), (2, 14), (3, 14), (11, 14), (13, 14), (14, 14), (4, 15), (5, 15), (6, 15), (7, 15), (8, 15), (9, 15), (10, 15) } },
                { new Color(177, 78, 5, 255), new[] { (5, 1), (6, 1), (10, 1), (11, 1), (2, 2), (4, 2), (13, 2), (2, 3), (3, 3), (2, 4), (2, 5), (13, 10), (13, 11), (12, 12), (13, 12), (2, 13), (11, 13), (13, 13), (4, 14), (5, 14), (9, 14), (10, 14) } },
                { new Color(220, 123, 5, 255), new[] { (7, 1), (8, 1), (9, 1), (5, 2), (6, 2), (7, 2), (8, 2), (9, 2), (10, 2), (11, 2), (12, 2), (4, 3), (5, 3), (6, 3), (7, 3), (3, 4), (4, 4), (5, 4), (6, 4), (13, 4), (14, 4), (3, 5), (4, 5), (5, 5), (14, 5), (2, 6), (3, 6), (4, 6), (5, 6), (14, 6), (1, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (1, 10), (10, 10), (11, 10), (12, 10), (1, 11), (2, 11), (9, 11), (10, 11), (11, 11), (12, 11), (2, 12), (3, 12), (8, 12), (9, 12), (10, 12), (11, 12), (3, 13), (4, 13), (5, 13), (6, 13), (7, 13), (8, 13), (9, 13), (10, 13), (6, 14), (7, 14), (8, 14) } },
                { new Color(91, 43, 42, 254), new[] { (12, 1) } },
                { new Color(127, 0, 0, 2), new[] { (14, 1) } },
                { new Color(90, 42, 39, 252), new[] { (3, 2) } },
                { new Color(217, 121, 6, 255), new[] { (12, 3), (13, 3) } },
                { new Color(93, 46, 46, 11), new[] { (15, 3) } },
                { new Color(93, 44, 42, 255), new[] { (7, 4), (6, 5), (7, 6) } },
                { new Color(0, 0, 0, 2), new[] { (9, 5) } },
                { new Color(219, 122, 5, 255), new[] { (6, 6) } },
                { new Color(64, 64, 64, 4), new[] { (9, 6) } },
                { new Color(92, 44, 42, 255), new[] { (2, 7) } },
                { new Color(91, 44, 42, 255), new[] { (5, 7) } },
                { new Color(91, 42, 41, 253), new[] { (7, 7) } },
                { new Color(0, 0, 0, 1), new[] { (9, 7) } }
#pragma warning restore format
            };

            Color[] data = new Color[width * height];
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    Color c = Color.Transparent;
                    foreach (var kvp in colors)
                        if (kvp.Value.Any(xy => xy.x == width - 1 - i && xy.y == j))
                            c = kvp.Key;

                    data[i + j * width] = c;
                }
            }
            result.SetData(data);
            return result;
        }

        public void Dispose()
        {
            this._root.Dispose();
        }

        private class SoundFloorDataGrid : Element
        {
            private readonly ClickableTextureComponent _scrollUp, _scrollDown;

            private readonly SoundFloorDataGridColumn _soundColumn;

            private readonly SoundFloorDataGridColumn _floorColumn;

            private readonly int _rowHeight = SoundFloorDataGridItem.RowHeight;

            private readonly int _partitionThickness = 8;

            private int _topIndex = 0;

            private SoundFloorDataGridItem _selectedItem;

            internal List<SoundData> _soundList;

            internal List<FloorData> _floorList;

            public SoundFloorDataGridColumn SoundColumn => this._soundColumn;

            public SoundFloorDataGridColumn FloorColumn => this._floorColumn;

            public SoundFloorDataGridItem SelectedItem
            {
                get => this._selectedItem;
                set
                {
                    if (this._selectedItem != null)
                        this._selectedItem.IsSelected = false;

                    this._selectedItem = value;

                    if (value != null)
                        value.IsSelected = true;

                    SelectionChanged?.Invoke(this, EventArgs.Empty);
                }
            }

            public event EventHandler SelectionChanged;

            public override int Width => 12 + this._soundColumn.Width + this._partitionThickness + this._floorColumn.Width + 12;

            public override int Height => 12 + (this.MaxDisplayRows + 1) * this._rowHeight + this.MaxDisplayRows * this._partitionThickness + 12;

            public int MaxDisplayRows { get; set; } = 5;

            public SoundFloorDataGrid()
            {
                this._soundColumn = new SoundFloorDataGridColumn();
                this._floorColumn = new SoundFloorDataGridColumn();
                this._scrollUp = new(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new(421, 459, 11, 12), 4f);
                this._scrollDown = new(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new(421, 472, 11, 12), 4f);
            }

            public void ReceiveLeftClick(int x, int y, bool playSound = false)
            {
                if (this._scrollUp.containsPoint(x, y))
                {
                    this.ScrollUp();
                    this._scrollUp.scale -= 0.25f;
                    this._scrollUp.scale = Math.Max(0.75f, this._scrollUp.scale);
                }
                else if (this._scrollDown.containsPoint(x, y))
                {
                    this.ScrollDown();
                    this._scrollDown.scale -= 0.25f;
                    this._scrollDown.scale = Math.Max(0.75f, this._scrollDown.scale);
                }
            }

            public void PerformHoverAction(int x, int y)
            {
                this._scrollUp.tryHover(x, y);
                this._scrollDown.tryHover(x, y);
            }

            public void AddRow(SoundFloorMapItem item)
            {
                var soundRow = new SoundDataGridItem();
                soundRow.Sound = item.Sound;
                this._soundColumn.Rows.Add(soundRow);

                var floorRow = new FloorDataGridItem();
                floorRow.Floor = item.Floor;
                this._floorColumn.Rows.Add(floorRow);
            }

            public void MoveUp()
            {
                this.MoveBy(-1);
            }

            public void MoveDown()
            {
                this.MoveBy(1);
            }

            private void MoveBy(int deltaIndex)
            {
                var selected = this.SelectedItem;
                if (selected is null) return;

                bool isSound = selected is SoundDataGridItem;

                // update ui.
                var rowList = isSound ? this._soundColumn.Rows : this._floorColumn.Rows;
                int index = rowList.IndexOf(selected);
                bool neg = deltaIndex < 0;
                if (neg && index <= 0) return;
                if (!neg && index >= this._soundColumn.Rows.Count - 1) return;
                for (int i = 1; i <= Math.Abs(deltaIndex); i++)
                {
                    int curIndex = index + (neg ? -i : i);
                    rowList[curIndex].Position += new Vector2(0, this._rowHeight + this._partitionThickness);
                }
                selected.Position += new Vector2(0, (this._rowHeight + this._partitionThickness) * deltaIndex);
                rowList.RemoveAt(index);
                index += deltaIndex;
                rowList.Insert(index, selected);

                // bring selected row into view.
                if (index < this._topIndex)
                    this._topIndex = index;
                else if (index > this._topIndex + this.MaxDisplayRows - 1)
                    this._topIndex = index + 1 - this.MaxDisplayRows;

                // update data.
                System.Collections.IList dataList = isSound ? this._soundList : this._floorList;
                object data = isSound ? (selected as SoundDataGridItem).Sound : (selected as FloorDataGridItem).Floor;
                index = dataList.IndexOf(data);
                dataList.RemoveAt(index);
                index += deltaIndex;
                dataList.Insert(index, data);
            }

            public void ResetState()
            {
                this._topIndex = 0;
            }

            private MouseState _lastState;
            public override void Update(GameTime gameTime)
            {
                var mouseState = Game1.input.GetMouseState();

                int mouseX = StardewModdingAPI.Constants.TargetPlatform is GamePlatform.Android ? Game1.getMouseX() : Game1.getOldMouseX();
                int mouseY = StardewModdingAPI.Constants.TargetPlatform is GamePlatform.Android ? Game1.getMouseY() : Game1.getOldMouseY();

                bool leftClick = mouseState.LeftButton is ButtonState.Pressed && this._lastState.LeftButton is ButtonState.Released;

                // update selected row.
                var visibleRows = this._soundColumn.Rows.ToList().GetRange(this._topIndex, this.MaxDisplayRows)
                    .Union(this._floorColumn.Rows.ToList().GetRange(this._topIndex, this.MaxDisplayRows));
                foreach (var row in visibleRows)
                {
                    if (row.Bounds.Contains(mouseX, mouseY) && leftClick && row.CanSelect())
                    {
                        this.SelectedItem = row;
                        break;
                    }
                }

                // update scroll position.
                var scrollableBounds = new Rectangle((int)this.Position.X, (int)this.Position.Y + 12 + this._rowHeight, this.Width, this.Height + 12 - this._rowHeight);
                if (scrollableBounds.Contains(mouseX, mouseY))
                {
                    int deltaScrollValue = mouseState.ScrollWheelValue - this._lastState.ScrollWheelValue;
                    int delta = deltaScrollValue / 120;
                    switch (delta)
                    {
                        // scroll up.
                        case > 0:
                            this.ScrollUp(delta);
                            break;

                        // scroll down.
                        case < 0:
                            this.ScrollDown(-delta);
                            break;
                    }
                }

                this._lastState = mouseState;
            }

            public override void Draw(SpriteBatch b)
            {
                // 表格背景框。
                IClickableMenu.drawTextureBox(b, (int)this.Position.X, (int)this.Position.Y, this.Width, this.Height, Color.White);

                // 横分割线（含标题行）。
                // 见IClickableMenu.drawHorizontalPartition
                for (int i = 0; i < this.MaxDisplayRows; i++)
                {
                    b.Draw(Game1.menuTexture, new Rectangle((int)this.Position.X + 12, (int)this.Position.Y + 12 + this._rowHeight * (i + 1) + this._partitionThickness * i - 28, this.Width - 24, 64), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 25), Color.White);
                }

                // 竖分割线，两列，故画一条。
                // 见IClickableMenu.drawVerticalPartition
                b.Draw(Game1.menuTexture, new Rectangle((int)this.Position.X + 12 + this._soundColumn.Width - 28, (int)this.Position.Y + 12, 64, this.Height - 24), Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 26), Color.White);

                // 列标题。
                var font = Game1.dialogueFont;
                string title = I18n.Config_SoundFloor_Editor_SoundTitle();
                Vector2 titleSize = font.MeasureString(title);
                Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2(this.Position.X + 12 + this._soundColumn.Width / 2 - titleSize.X / 2, this.Position.Y + 12 + this._rowHeight / 2 - titleSize.Y / 2), Game1.textColor);

                title = I18n.Config_SoundFloor_Editor_FloorTitle();
                titleSize = font.MeasureString(title);
                Utility.drawTextWithShadow(b, title, Game1.dialogueFont, new Vector2(this.Bounds.Right - 12 - this._floorColumn.Width / 2 - titleSize.X / 2, this.Position.Y + 12 + this._rowHeight / 2 - titleSize.Y / 2), Game1.textColor);

                // 列。
                int startIndex = this._topIndex;
                int visibleRows = Math.Min(this.MaxDisplayRows, this._soundColumn.Rows.Count - startIndex);
                float rowY = this.Position.Y + 12 + this._rowHeight + this._partitionThickness;
                for (int i = startIndex; i < visibleRows + startIndex; i++)
                {
                    var soundRow = this._soundColumn.Rows[i];
                    var floorRow = this._floorColumn.Rows[i];
                    soundRow.Position = new Vector2(this.Position.X + 12, rowY);
                    floorRow.Position = new Vector2(this.Position.X + 12 + this._soundColumn.Width + this._partitionThickness, rowY);
                    soundRow.Draw(b);
                    floorRow.Draw(b);

                    rowY += this._rowHeight + this._partitionThickness;
                }

                // 上下移按钮。
                this._scrollUp.draw(b);
                this._scrollDown.draw(b);
            }

            protected override void OnPositionChanged(Vector2 oldPosition, Vector2 newPosition)
            {
                base.OnPositionChanged(oldPosition, newPosition);

                this._scrollUp.bounds.Location = new(this.Bounds.Center.X - 22, this.Bounds.Y - IClickableMenu.borderWidth / 2 - 48);
                this._scrollDown.bounds.Location = new(this.Bounds.Center.X - 22, this.Bounds.Bottom + IClickableMenu.borderWidth / 2);
            }

            private void ScrollUp(int by = 1)
            {
                this._topIndex -= by;

                if (this._topIndex < 0)
                    this._topIndex = 0;
                else if (this._topIndex > this._soundColumn.Rows.Count - this.MaxDisplayRows)
                    this._topIndex = this._soundColumn.Rows.Count - this.MaxDisplayRows;
                else
                    Game1.playSound("shwip");
            }

            private void ScrollDown(int by = 1)
            {
                this._topIndex += by;

                if (this._topIndex < 0)
                    this._topIndex = 0;
                else if (this._topIndex > this._soundColumn.Rows.Count - this.MaxDisplayRows)
                    this._topIndex = this._soundColumn.Rows.Count - this.MaxDisplayRows;
                else
                    Game1.playSound("shwip");
            }
        }

        private class SoundFloorDataGridColumn
        {
            public object Header { get; set; }

            public int Width => this.Rows.Count > 0 ? this.Rows.Max(r => r.Width) : 0;

            public List<SoundFloorDataGridItem> Rows { get; } = new();
        }

        private class SoundDataGridItem : SoundFloorDataGridItem
        {
            public SoundData Sound { get; set; }

            public override void Draw(SpriteBatch b)
            {
                base.Draw(b);

                Vector2 pos = this.Position;
                var font = Game1.smallFont;
                var sound = this.Sound;

                // draw sound.
                if (sound != null)
                {
                    string text = sound.IsEmptySound()
                        ? I18n.Sound_Empty()
                        : !string.IsNullOrWhiteSpace(sound.Name) ? sound.Name : sound.CueName;
                    float destX = pos.X + this.Width / 2 - font.MeasureString(text).X / 2;
                    float destY = pos.Y + this.Height / 2 - font.MeasureString(text).Y / 2;
                    b.DrawString(font, text, new Vector2(destX, destY), Game1.textColor * (sound.IsEnabled ? 1f : 0.33f));
                }
            }
        }

        private class FloorDataGridItem : SoundFloorDataGridItem
        {
            private static readonly Lazy<Texture2D> _floorTexture = new(() => Game1.content.Load<Texture2D>(@"TerrainFeatures\Flooring"));

            public FloorData Floor { get; set; }

            public override void Draw(SpriteBatch b)
            {
                base.Draw(b);

                Vector2 pos = this.Position;
                var font = Game1.smallFont;
                var floor = this.Floor;

                // draw floor.
                if (floor != null)
                {
                    // empty.
                    if (floor.IsEmptyFloor())
                    {
                        string emptyText = I18n.Floor_Empty();
                        Vector2 textSize = font.MeasureString(emptyText);

                        b.DrawString(font, emptyText, new Vector2(pos.X + this.Width / 2 - textSize.X / 2, pos.Y + this.Height / 2 - textSize.Y / 2), Game1.textColor);
                        return;
                    }

                    int? whichFloor = floor.WhichFloor;

                    // draw floor texture.
                    float scale = 3f;
                    float destX = pos.X + IClickableMenu.borderWidth / 2;
                    float destY = pos.Y + this.Height / 2 - 16 * scale / 2;
                    if (whichFloor != null)
                    {
                        int which = whichFloor.Value;
                        Rectangle srcRect = new(which % 4 * 64, which / 4 * 64, 16, 16);
                        b.Draw(_floorTexture.Value, new Vector2(destX, destY), srcRect, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
                    }

                    // draw floor info text.
                    string text = FloorInfo.GetDisplayName(floor) ?? string.Empty;
                    destX += 16 * scale + IClickableMenu.borderWidth / 2;
                    destY = pos.Y + this.Height / 2 - font.MeasureString(text).Y / 2;

                    b.DrawString(font, text, new Vector2(destX, destY), Game1.textColor);
                }
            }
        }

        private abstract class SoundFloorDataGridItem
        {
            public static readonly int RowHeight = 80;

            public bool IsSelected { get; set; } = false;

            public Vector2 Position { get; set; }

            public int Width { get; set; } = 200;

            public int Height { get; set; } = RowHeight;

            public Rectangle Bounds
            {
                get { return new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height); }
            }

            public virtual void Draw(SpriteBatch b)
            {
                // draw selection highlight.
                if (this.IsSelected)
                {
                    b.Draw(Game1.staminaRect, this.Bounds, Color.White * 0.25f);
                }
            }

            public virtual bool CanSelect()
            {
                return this is SoundDataGridItem;
            }
        }

        private class SoundTable : OptionTable
        {
            private readonly Label2 _nameLabel, _cueNameLabel, _pitchLabel, _notesLabel, _enableLabel;

            private readonly Label2 _pitchValueLabel;

            private readonly Textbox _nameBox, _notesBox;

            private readonly Slider<int> _rawPitchSlider;

            private readonly SortedComboBox _cueNameComboBox;

            private readonly Checkbox _enableCheckbox;

            /// <summary>An icon beside the cue name comboBox. When <see cref="_isCueValid"/>, shows as a clickable play button, otherwise an unclickable red cross.</summary>
            private readonly ClickableTextureComponent _stateIcon;

            private string _savedName, _savedCueName, _savedNotes;

            private int _savedRawPitch;

            private bool _savedEnabled;

            private bool _isCueValid;

            private bool _isCheckingCue;

            private string CueName => this._cueNameComboBox.Text;

            public static string[] AllCues = CueUtilites.GetAllCues().ToArray();

            public bool AnyTextboxFocused => this._nameBox.Focused || this._notesBox.Focused;

            public SoundTable()
            {
                this._nameBox = new();
                this._rawPitchSlider = new()
                {
                    Minimum = ToMidiNote(MIN_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    Maximum = ToMidiNote(MAX_PATCHED_PRESERVEDPARENTSHEETINDEX_VALUE),
                    Interval = 1,
                    Value = 60
                };
                this._notesBox = new();
                this._enableCheckbox = new();
                this._cueNameComboBox = new()
                {
                    MaxDisplayRows = 13,
                };
                this._cueNameComboBox.UpdateChoices(AllCues);
                this._cueNameComboBox.SelectionChanged += this.CueNameBox_SelectionChanged;

                this._stateIcon = new(Rectangle.Empty, Game1.mouseCursors, Rectangle.Empty, 0f);

                bool isEdited(Element elem)
                {
                    switch (elem)
                    {
                        case Textbox textbox:
                            if (textbox == this._nameBox)
                                return this._savedName != this._nameBox.String;
                            else if (textbox == this._notesBox)
                                return this._savedNotes != this._notesBox.String;
                            break;

                        case Slider slider:
                            if (slider == this._rawPitchSlider)
                                return this._savedRawPitch != this._rawPitchSlider.Value;
                            break;

                        case Checkbox checkbox:
                            if (checkbox == this._enableCheckbox)
                                return this._savedEnabled != checkbox.IsChecked;
                            break;

                        case ComboBox comboBox:
                            if (comboBox == this._cueNameComboBox)
                                return this._savedCueName != comboBox.Text && this._isCueValid;
                            break;
                    }

                    return false;
                }
                this._nameLabel = new AsteriskLabel<Textbox>(this._nameBox) { IsEditedObserver = isEdited };
                this._cueNameLabel = new AsteriskLabel<SortedComboBox>(this._cueNameComboBox) { IsEditedObserver = isEdited };
                this._pitchLabel = new AsteriskLabel<Slider<int>>(this._rawPitchSlider) { IsEditedObserver = isEdited };
                this._notesLabel = new AsteriskLabel<Textbox>(this._notesBox) { IsEditedObserver = isEdited };
                this._enableLabel = new AsteriskLabel<Checkbox>(this._enableCheckbox) { IsEditedObserver = isEdited };

                this.AddOption(this._nameLabel, this._nameBox);
                this.AddOption(this._cueNameLabel, this._cueNameComboBox, new ClickableTextureAdapter(this._stateIcon));
                this.AddOption(this._pitchLabel, this._rawPitchSlider, this._pitchValueLabel = new Label2());
                this.AddOption(this._notesLabel, this._notesBox);
                this.AddOption(this._enableLabel, this._enableCheckbox);
            }

            public void LoadData(SoundData data)
            {
                this._nameBox.String = this._savedName = data.Name ?? string.Empty;
                this._cueNameComboBox.SelectedItem = this._savedCueName = data.CueName;
                if (this._cueNameComboBox.SelectedIndex is -1)
                {
                    this._cueNameComboBox.Text = data.CueName;
                }
                this._rawPitchSlider.Value = this._savedRawPitch = data.RawPitch + 60;
                this._notesBox.String = this._savedNotes = data.Description ?? string.Empty;
                this._enableCheckbox.IsChecked = this._savedEnabled = data.IsEnabled;

                this.CheckCue(this.CueName);
            }

            public SoundData BuildData()
            {
                return new SoundData
                {
                    Name = this._nameBox.String,
                    CueName = this._cueNameComboBox.Text,
                    RawPitch = this._rawPitchSlider.Value - 60,
                    Description = this._notesBox.String,
                    IsEnabled = this._enableCheckbox.IsChecked
                };
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                // update cue name label color.
                this._cueNameLabel.IdleTextColor = !string.IsNullOrWhiteSpace(this._cueNameComboBox.Text) ? Game1.textColor : Color.Red;

                // update label text.
                this._nameLabel.Text = I18n.Config_Sound_EditorOptions_Name();
                this._cueNameLabel.Text = I18n.Config_Sound_EditorOptions_CueName();
                this._pitchLabel.Text = I18n.Config_Sound_EditorOptions_RawPitch();
                this._notesLabel.Text = I18n.Config_Sound_EditorOptions_Desc();
                this._enableLabel.Text = I18n.Config_Sound_EditorOptions_Enable();

                // update raw pitch slider width.
                this._rawPitchSlider.RequestWidth = this.Width / 3;

                // update raw pitch value label text.
                this._pitchValueLabel.Text = this._rawPitchSlider.Value.ToString();

                // update cue name comboBox width.
                this._cueNameComboBox.SettableWidth = this.Width / 3;
            }

            public override void ReceiveLeftClick(int x, int y, bool playSound = true)
            {
                base.ReceiveLeftClick(x, y, playSound);

                // play button.
                if (this._isCueValid && this._stateIcon.containsPoint(x, y))
                {
                    Game1.soundBank.PlayCue(this.CueName);
                    this._stateIcon.scale -= 0.25f;
                    this._stateIcon.scale = Math.Max(0.75f, this._stateIcon.scale);
                }

                // cueName combobox.
                this._cueNameComboBox.ReceiveLeftClick(x, y, playSound);
            }

            public override void PerformHoverAction(int x, int y)
            {
                base.PerformHoverAction(x, y);

                // play button.
                if (this._isCueValid)
                {
                    this._stateIcon.tryHover(x, y);
                }
            }

            protected override bool CanOk()
            {
                return !string.IsNullOrWhiteSpace(this._cueNameComboBox.Text);
            }

            protected override void RaiseOkButtonClicked()
            {
                this._savedName = this._nameBox.String;
                this._savedCueName = this._cueNameComboBox.Text;
                this._savedRawPitch = this._rawPitchSlider.Value;
                this._savedNotes = this._notesBox.String;
                this._savedEnabled = this._enableCheckbox.IsChecked;

                base.RaiseOkButtonClicked();
            }

            private void CueNameBox_SelectionChanged(object sender, EventArgs e)
            {
                this.CheckCue(this.CueName);
            }

            private void CheckCue(string cueName)  // TODO: 异步
            {
                if (this._isCheckingCue) return;

                this._isCheckingCue = true;

                string[] allCues = AllCues;
                if (!allCues.Contains(cueName))
                {
                    this._isCueValid = false;
                    this._stateIcon.sourceRect = new(269, 471, 14, 15);
                    this._stateIcon.bounds.Size = new(this._cueNameComboBox.RowHeight);
                }
                else
                {
                    this._isCueValid = true;
                    this._stateIcon.sourceRect = new(175, 379, 16, 16);
                    this._stateIcon.bounds.Size = new(this._cueNameComboBox.RowHeight);
                }
                this._stateIcon.scale = this._stateIcon.baseScale = (float)this._stateIcon.bounds.Height / this._stateIcon.sourceRect.Height;

                this._isCheckingCue = false;
            }
        }

        private class FloorTable : OptionTable
        {
            private readonly Label2 _selectFloorLabel;

            private readonly ComboBox _floorSelector;

            private object _savedSelectedFloor;

            private readonly Func<FloorData[]> _existingFloors;

            private readonly FloorData[] _floorDatabase = new FloorData[]
            {
                FloorData.NonFloor,
                FloorData.WoodFloor,
                FloorData.StoneFloor,
                FloorData.WeatheredFloor,
                FloorData.CrystalFloor,
                FloorData.StrawFloor,
                FloorData.GravelPath,
                FloorData.WoodPath,
                FloorData.CrystalPath,
                FloorData.CobblestonePath,
                FloorData.SteppingStonePath,
                FloorData.BrickFloor,
                FloorData.RusticPlankFloor,
                FloorData.StoneWalkwayFloor,
            };

            public FloorData SelectedFloor
            {
                get { return (FloorData)this._floorSelector.SelectedItem; }
            }

            public int ComboBoxWidth
            {
                get => this._floorSelector.RequestWidth;
                set => this._floorSelector.RequestWidth = value;
            }

            public FloorTable(Func<FloorData[]> existingFloors!!)
            {
                this._existingFloors = existingFloors;

                this._floorSelector = new ComboBox()
                {
                    Choices = this.LoadFloors(),
                    DisplayTextReslover = (_, floor) => this.GetDisplayText((FloorData)floor),
                    MaxDisplayRows = 5
                };

                bool IsEdited(ComboBox comboBox)
                {
                    if (object.ReferenceEquals(comboBox, this._floorSelector))
                    {
                        return this._savedSelectedFloor != this._floorSelector.SelectedItem;
                    }

                    return false;
                }
                this._selectFloorLabel = new AsteriskLabel<ComboBox>(this._floorSelector) { IsEditedObserver = IsEdited };

                this.AddOption(this._selectFloorLabel, this._floorSelector);
            }

            public void LoadData(FloorData data)
            {
                // reset combobox's state.
                this._floorSelector.SelectedIndex = 0;
                this._floorSelector.Choices = this.LoadFloors(data);

                // load selected item.
                this._floorSelector.SelectedItem = this._savedSelectedFloor = data;
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                // update label text.
                this._selectFloorLabel.Text = I18n.Config_Floor_EditorOptions_Select();
            }

            /// <summary>Update available floors in the combobox.</summary>
            private FloorData[] LoadFloors(FloorData selectedFloor = null)
            {
                FloorData[] existingFloors = this._existingFloors();

                List<FloorData> result = new();
                if (selectedFloor != null)
                    result.Add(selectedFloor);
                foreach (FloorData floor in this._floorDatabase)
                {
                    if (existingFloors.Contains(floor))
                        continue;

                    result.Add(floor);
                }

                return result.ToArray();
            }

            protected override void RaiseOkButtonClicked()
            {
                this._savedSelectedFloor = this._floorSelector.SelectedItem;

                base.RaiseOkButtonClicked();
            }

            private string GetDisplayText(FloorData floor)
            {
                return FloorInfo.GetDisplayName(floor);
            }
        }

        private class OptionTable : Container
        {
            protected readonly List<Row> _rows = new();

            private readonly ClickableTextureComponent _okButton;

            public event EventHandler OkButtonClick;

            protected OptionTable()
            {
                // init ok button.
                this._okButton = new ClickableTextureComponent("OK", new Rectangle(0, 0, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
            }

            public void AddOption(Label2 label!!, Element optionElement!!, params Element[] otherElements)
            {
                this._rows.Add(new Row
                {
                    Label = label,
                    OptionElement = optionElement,
                    OtherElements = otherElements
                });
                this.Add(label, optionElement);
                this.Add(otherElements);
            }

            public virtual void ReceiveLeftClick(int x, int y, bool playSound = true)
            {
                // ok button.
                if (this._okButton.containsPoint(x, y) && this.CanOk())
                {
                    Game1.playSound("coin");
                    this.RaiseOkButtonClicked();
                    this._okButton.scale -= 0.25f;
                    this._okButton.scale = Math.Max(0.75f, this._okButton.scale);
                }
            }

            public virtual void PerformHoverAction(int x, int y)
            {
                // ok button.
                if (this._okButton.containsPoint(x, y) && this.CanOk())
                {
                    this._okButton.scale = Math.Min(this._okButton.scale + 0.02f, this._okButton.baseScale + 0.1f);
                }
                else
                {
                    this._okButton.scale = Math.Max(this._okButton.scale - 0.02f, this._okButton.baseScale);
                }
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                // update options position.
                int borderWidth = IClickableMenu.borderWidth / 2;
                int y = borderWidth;
                foreach (Row row in this._rows)
                {
                    row.Label.LocalPosition = new Vector2(0, y);
                    row.OptionElement.LocalPosition = new Vector2(this.Width / 2, y);

                    int x = this.Width / 2 + row.OptionElement.Width + borderWidth;
                    foreach (Element elem in row.OtherElements)
                    {
                        elem.LocalPosition = new Vector2(x, y);
                        x += elem.Width + borderWidth;
                    }

                    y += Math.Max(row.Label.Height, row.OptionElement.Height) + borderWidth;
                }

                // update ok button position.
                this._okButton.setPosition((int)this.Position.X + this.Width - spaceToClearSideBorder - 64, (int)this.Position.Y + this.Height - spaceToClearSideBorder - 64);
            }

            public override void Draw(SpriteBatch b)
            {
                base.Draw(b);

                // ok button.
                if (this.CanOk())
                {
                    this._okButton.draw(b);
                }
                else
                {
                    this._okButton.draw(b, Color.White, 0.75f);
                    this._okButton.draw(b, Color.Black * 0.5f, 0.751f);
                }
            }

            protected virtual bool CanOk()
            {
                return true;
            }

            protected virtual void RaiseOkButtonClicked()
            {
                OkButtonClick?.Invoke(this, EventArgs.Empty);
            }

            protected class Row
            {
                public Label2 Label;
                public Element OptionElement;
                public Element[] OtherElements;
            }
        }
    }
}