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
using StardewValley.Controls;
using StardewValley.Menus;

namespace FluteBlockExtension.Framework.Menus
{
    /// <summary>A menu for editing sound-floor mapping.</summary>
    internal class SoundFloorEditor : IClickableMenu, IDisposable
    {
        private readonly RootElement _root;

        private readonly Func<SoundFloorMap> _map;

        private readonly Lazy<Texture> _addIcon = new(LoadAddIcon);

        private readonly List<SoundData> _soundList = new();

        private readonly List<FloorData> _floorList = new();

        private readonly SoundFloorDataGrid _dataGrid;

        private readonly ScrollViewer2 _propertyView;

        private readonly SoundTable _soundTable;

        private readonly FloorTable _floorTable;

        /// <summary>GMCM config menu, to which <see cref="Game1.activeClickableMenu"/> will be set after this <see cref="SoundFloorEditor"/> exits.</summary>
        public IClickableMenu ConfigMenu;

        static SoundFloorEditor()
        {
            ModEntry.Harmony.Patch(
                original: HarmonyLib.AccessTools.Method(typeof(TitleMenu), nameof(TitleMenu.backButtonPressed)),
                prefix: new HarmonyLib.HarmonyMethod(typeof(SoundFloorEditor), nameof(TitleMenu_backButtonPressed_Prefix))
            );
        }

        public SoundFloorEditor(Func<SoundFloorMap> map)
        {
            this._map = map;

            this.width = 1000;
            this.height = 700;
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;

            this._root = new RootElement();
            this._root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this._dataGrid = new SoundFloorDataGrid
            {
                MaxDisplayRows = 8
            };
            this._dataGrid.SelectionChanged += this.DataGrid_SelectionChanged;
            this._dataGrid._soundList = this._soundList;
            this._dataGrid._floorList = this._floorList;
            foreach (var item in map())
            {
                this._soundList.Add(item.Sound);
                this._floorList.Add(item.Floor);

                this._dataGrid.AddRow(item);
            }

            int buttonX = this._dataGrid.Width + borderWidth;
            int buttonY = 0;
            Button2 upButton = new Button2
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

            Button2 downButton = new Button2
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

            Button2 addRowButton = new Button2
            {
                LocalPosition = new Vector2(buttonX, buttonY),
                SettableWidth = 80,
                SettableHeight = 50,
                Content = this._addIcon.Value,
                Scale = 1f
            };
            addRowButton.Click += this.AddRowButton_Click;
            buttonY += addRowButton.Height + borderWidth;

            Button2 deleteButton = new Button2
            {
                LocalPosition = new Vector2(buttonX, buttonY),
                SettableWidth = 80,
                SettableHeight = 50,
                Content = Game1.mouseCursors,
                SourceRectangle = new Rectangle(322, 498, 12, 12),
                Scale = 2.5f
            };
            deleteButton.Click += this.DeleteButton_Click;
            this._propertyView = new ScrollViewer2()
            {
                LocalPosition = new Vector2(buttonX + 80 + borderWidth, 0),
                SettableWidth = 600,
                SettableHeight = 600,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            };

            this._root.Add(this._dataGrid, upButton, downButton, addRowButton, deleteButton, this._propertyView);

            this._soundTable = new SoundTable()
            {
                SettableWidth = this._propertyView.Width - 32,
                SettableHeight = this._propertyView.Height - 32,
            };
            this._soundTable.OkButtonClick += this.SoundTable_OkButtonClick;

            this._floorTable = new FloorTable(() => this._floorList.ToArray())
            {
                SettableWidth = this._propertyView.Width - 32,
                SettableHeight = this._propertyView.Height - 32,
                ComboBoxWidth = (this._propertyView.Width - 32) / 3
            };
            this._floorTable.OkButtonClick += this.FloorTable_OkButtonClick;

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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (this._propertyView.Content is OptionTable table)
            {
                table.ReceiveLeftClick(x, y, playSound);
            }
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            if (this._propertyView.Content is OptionTable table)
            {
                table.PerformHoverAction(x, y);
            }
        }

        private void SoundTable_OkButtonClick(object sender, EventArgs e)
        {
            if (this._dataGrid.SelectedItem is SoundDataGridItem soundGridItem)
            {
                int index = this._dataGrid.SoundColumn.Rows.IndexOf(soundGridItem);

                SoundData editedSound = new SoundData
                {
                    Name = this._soundTable.Name,
                    CueName = this._soundTable.CueName,
                    RawPitch = this._soundTable.RawPitch,
                    FilePaths = new(this._soundTable.FilePaths),
                    Description = this._soundTable.Description
                };

                this._soundList[index] = soundGridItem.Sound = editedSound;
            }
        }

        private void FloorTable_OkButtonClick(object sender, EventArgs e)
        {
            if (this._dataGrid.SelectedItem is FloorDataGridItem floorGridItem)
            {
                int index = this._dataGrid.FloorColumn.Rows.IndexOf(floorGridItem);

                this._floorList[index] = floorGridItem.Floor = this._floorTable.SelectedFloor;
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

                case FloorDataGridItem floorGrid:
                    this._floorTable.LoadData(floorGrid.Floor);
                    this._propertyView.Content = this._floorTable;
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

        private void AddRowButton_Click(object sender, EventArgs e)
        {
            var emptySoundFloor = new SoundFloorMapItem() { Sound = SoundData.Empty, Floor = FloorData.Empty };
            this._dataGrid.AddRow(emptySoundFloor);
            this._soundList.Add(emptySoundFloor.Sound);
            this._floorList.Add(emptySoundFloor.Floor);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            var selected = this._dataGrid.SelectedItem;

            var column = selected is SoundDataGridItem ? this._dataGrid.SoundColumn : this._dataGrid.FloorColumn;
            System.Collections.IList dataList = selected is SoundDataGridItem ? this._soundList : this._floorList;
            int index = column.Rows.IndexOf(selected);

            dataList.RemoveAt(index);
            column.Rows.RemoveAt(index);
            this._dataGrid.SelectedItem = null;
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            this.xPositionOnScreen = Game1.uiViewport.Width / 2 - this.width / 2;
            this.yPositionOnScreen = Game1.uiViewport.Height / 2 - this.height / 2;
            this._root.LocalPosition = new Vector2(this.xPositionOnScreen, this.yPositionOnScreen);

            this.ConfigMenu?.gameWindowSizeChanged(oldBounds, newBounds);
        }

        public override void update(GameTime time)
        {
            base.update(time);
            this._root.Update(time);
            this._floorTable.ComboBoxWidth = 300;
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
            Game1.activeClickableMenu = this.ConfigMenu;
        }

        private static bool TitleMenu_backButtonPressed_Prefix()
        {
            if (TitleMenu.subMenu is SoundFloorEditor editor && TitleMenu.subMenu.readyToClose())
            {
                Game1.playSound("bigDeSelect");
                TitleMenu.subMenu = editor.ConfigMenu;
                return false;  // skip original.
            }

            return true;
        }

        private static Texture2D LoadAddIcon()
        {
            Texture2D texture = new Texture2D(Game1.graphics.GraphicsDevice, 30, 30);
            Color foreground = new Color(255, 254, 128);
            Color background = Color.Transparent;

            Color[] data = new Color[30 * 30];
            for (int j = 0; j < 30; j++)
                for (int i = 0; i < 30; i++)
                    data[i + j * 30] = (i >= 9 && i <= 19) || (j >= 9 && j <= 19)
                        ? foreground
                        : background;
            texture.SetData(data);
            return texture;
        }

        public void Dispose()
        {
            this._root.Dispose();
        }

        private class SoundFloorDataGrid : Element
        {
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
                foreach (var row in this._soundColumn.Rows.Union(this._floorColumn.Rows))
                {
                    if (row.Bounds.Contains(mouseX, mouseY) && leftClick)
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
                        case > 0 when this._topIndex > 0:
                            this._topIndex -= delta;
                            break;

                        // scroll down.
                        case < 0 when this._topIndex + this.MaxDisplayRows < this._soundColumn.Rows.Count:
                            this._topIndex -= delta;
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
                    // empty.
                    if (sound.IsEmptySound())
                    {
                        string emptyText = I18n.Sound_Empty();
                        Vector2 textSize = font.MeasureString(emptyText);

                        b.DrawString(font, emptyText, new Vector2(pos.X + this.Width / 2 - textSize.X / 2, pos.Y + this.Height / 2 - textSize.Y / 2), Game1.textColor);
                        return;
                    }

                    string text = sound.Name ?? string.Empty;
                    float destX = pos.X + IClickableMenu.borderWidth / 2;
                    float destY = pos.Y + this.Height / 2 - font.MeasureString(text).Y / 2;
                    b.DrawString(font, text, new Vector2(destX, destY), Game1.textColor);
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
        }

        private class SoundTable : OptionTable
        {
            private readonly Label2 _nameLabel, _cueNameLabel, _pitchLabel, _pathsLabel, _notesLabel;

            private string _savedName, _savedCueName, _savedRawPitch, _savedPaths, _savedNotes;

            public Textbox NameBox { get; }

            public Textbox CueNameBox { get; }

            public Textbox PitchBox { get; }

            public Textbox PathsBox { get; }

            public Textbox NotesBox { get; }

            public string Name => this.NameBox.String;

            public string CueName => this.CueNameBox.String;

            public int RawPitch => int.TryParse(this.PitchBox.String, out int rawPitch) ? rawPitch : 0;

            public FilePath[] FilePaths => this.ResolvePaths(this.PathsBox.String);

            public string Description => this.NotesBox.String;

            public SoundTable()
            {
                this.NameBox = new Textbox();
                this.CueNameBox = new Textbox();
                this.PitchBox = new Textbox();
                this.PathsBox = new Textbox();
                this.NotesBox = new Textbox();

                bool isEdited(Textbox textbox)
                {
                    if (object.ReferenceEquals(textbox, this.NameBox))
                    {
                        return this._savedName != this.NameBox.String;
                    }
                    else if (object.ReferenceEquals(textbox, this.CueNameBox))
                    {
                        return this._savedCueName != this.CueNameBox.String;
                    }
                    else if (object.ReferenceEquals(textbox, this.PitchBox))
                    {
                        return this._savedRawPitch != this.PitchBox.String;
                    }
                    else if (object.ReferenceEquals(textbox, this.PathsBox))
                    {
                        return this._savedPaths != this.PathsBox.String;
                    }
                    else if (object.ReferenceEquals(textbox, this.NotesBox))
                    {
                        return this._savedNotes != this.NotesBox.String;
                    }

                    return false;
                }
                this._nameLabel = new AsteriskLabel<Textbox>(this.NameBox) { IsEditedObserver = isEdited };
                this._cueNameLabel = new AsteriskLabel<Textbox>(this.CueNameBox) { IsEditedObserver = isEdited };
                this._pitchLabel = new AsteriskLabel<Textbox>(this.PitchBox) { IsEditedObserver = isEdited };
                this._pathsLabel = new AsteriskLabel<Textbox>(this.PathsBox) { IsEditedObserver = isEdited };
                this._notesLabel = new AsteriskLabel<Textbox>(this.NotesBox) { IsEditedObserver = isEdited };

                this.AddOption(this._nameLabel, this.NameBox);
                this.AddOption(this._cueNameLabel, this.CueNameBox);
                this.AddOption(this._pitchLabel, this.PitchBox);
                this.AddOption(this._pathsLabel, this.PathsBox);
                this.AddOption(this._notesLabel, this.NotesBox);
            }

            public void LoadData(SoundData data)
            {
                bool readOnly = SoundsConfig.BuiltInSoundFloorPairs.Select(p => p.Sound).Contains(data);

                this.NameBox.String = this._savedName = data.Name ?? string.Empty;
                this.NameBox.IsReadOnly = readOnly;

                this.CueNameBox.String = this._savedCueName = data.CueName ?? string.Empty;
                this.CueNameBox.IsReadOnly = readOnly;

                this.PitchBox.String = this._savedRawPitch = data.RawPitch.ToString();
                this.PitchBox.IsReadOnly = readOnly;

                this.PathsBox.String = this._savedPaths = string.Join(' ', data.FilePaths.Select(p => "\"" + p.Path + "\""));
                this.PathsBox.IsReadOnly = readOnly;

                this.NotesBox.String = this._savedNotes = data.Description ?? string.Empty;
                this.NotesBox.IsReadOnly = readOnly;
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);

                // update cue name label color.
                this._cueNameLabel.IdleTextColor = string.IsNullOrEmpty(this.CueName) ? Color.Red : Game1.textColor;

                // update label text.
                this._nameLabel.Text = I18n.Config_Sound_EditorOptions_Name();
                this._cueNameLabel.Text = I18n.Config_Sound_EditorOptions_CueName();
                this._pitchLabel.Text = I18n.Config_Sound_EditorOptions_RawPitch();
                this._pathsLabel.Text = I18n.Config_Sound_EditorOptions_FilePaths();
                this._notesLabel.Text = I18n.Config_Sound_EditorOptions_Desc();
            }

            protected override bool CanOk()
            {
                return !string.IsNullOrEmpty(this.CueName);
            }

            protected override void RaiseOkButtonClicked()
            {
                this._savedName = this.NameBox.String;
                this._savedCueName = this.CueNameBox.String;
                this._savedRawPitch = this.PitchBox.String;
                this._savedPaths = this.PathsBox.String;
                this._savedNotes = this.NotesBox.String;

                base.RaiseOkButtonClicked();
            }

            private FilePath[] ResolvePaths(string str)
            {
                if (string.IsNullOrWhiteSpace(str))
                    return Array.Empty<FilePath>();

                using var reader = new StringReader(str);
                bool quote = false;
                char c;
                int i;
                StringBuilder sb = new StringBuilder();
                List<FilePath> paths = new List<FilePath>();
                while ((i = reader.Read()) != -1)
                {
                    c = (char)i;
                    if (c is '"')
                    {
                        quote = !quote;
                        if (quote)
                            continue;
                    }

                    if (quote)
                    {
                        sb.Append(c);
                    }
                    else if (sb.Length > 0)
                    {
                        string path = sb.ToString();  // must be absolute.
                        FilePath filePath = FilePath.With(path, true); // TODO: file dialog
                        paths.Add(filePath);
                        sb.Clear();
                    }
                }

                return paths.ToArray();
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
                this._floorSelector.SelectedItem = _savedSelectedFloor = data;
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
                _savedSelectedFloor = _floorSelector.SelectedItem;

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

            public void AddOption(Label2 label!!, Element optionElement!!, Element[] otherElements = null)
            {
                this._rows.Add(new Row
                {
                    Label = label,
                    OptionElement = optionElement,
                    OtherElements = otherElements
                });
                this.Add(label, optionElement);
                if (otherElements != null)
                    this.Add(otherElements);
            }

            public void ReceiveLeftClick(int x, int y, bool playSound = true)
            {
                // ok button.
                if (this._okButton.containsPoint(x, y) && this.CanOk())
                {
                    Game1.playSound("yoba");
                    this.RaiseOkButtonClicked();
                    this._okButton.scale -= 0.25f;
                    this._okButton.scale = Math.Max(0.75f, this._okButton.scale);
                }
            }

            public void PerformHoverAction(int x, int y)
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