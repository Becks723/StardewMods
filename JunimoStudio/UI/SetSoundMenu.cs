//using System;
//using System.Collections;
//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using JunimoStudio.Instruments;
//using JunimoStudio.UI;
//using StardewModdingAPI;
//using StardewValley;
//using StardewValley.Menus;

//namespace JunimoStudio.UI
//{
//    public class SetSoundMenu : IClickableMenu
//    {
//        private List<ClickableTextureComponent> musicElements = new List<ClickableTextureComponent>();
//        private List<ClickableTextureComponent> arrows = new List<ClickableTextureComponent>();
//        public string[] Labels;
//        private List<Element> components = new List<Element>();
//        private List<ClickableComponent> tabs = new List<ClickableComponent>();
//        private List<InputBox> inputBoxes = new List<InputBox>();
//        private string text;
//        private TextBoxEvent e;
//        private ClickableTextureComponent deleteButton;
//        private ClickableTextureComponent add;
//        private Rectangle leftArrow = new Rectangle(8, 268, 44, 40);
//        private Rectangle rightArrow = new Rectangle(12, 204, 44, 40);
//        public static Texture2D Texture_Noticeboard;
//        public static bool isMouseVisible;
//        public int TotalTabs;
//        public int CurrentTab;
//        private enum CursorState
//        {
//            Normal,
//            Adjust,
//            HorizontalSizing
//        }

//        public List<int> Volumes = new List<int>();
//        public List<int> Bpms = new List<int>();
//        public List<double> Durations = new List<double>();
//        public List<int> Velocities = new List<int>();
//        public List<int> LabelIndexes = new List<int>();
//        public List<string> Scales = new List<string>();
//        public List<string> Groups = new List<string>();
//        public List<bool> StopAtNext = new List<bool>();
//        public List<bool> IsDelayed = new List<bool>();
//        public List<double> DelayTime = new List<double>();

//        /// <summary>
//        /// Constructor used for creating a brand new block menu.
//        /// </summary>
//        /// <param name="labels"></param>
//        public SetSoundMenu(string[] labels)
//        {
//            Labels = labels;
//            CleanUpForNew();
//            SetUpPositions();
//        }

//        /// <summary>
//        /// Constructor for recreating block when reloading a save.
//        /// </summary>
//        /// <param name="labels"></param>
//        /// <param name="scales"></param>
//        /// <param name="groups"></param>
//        /// <param name="durations"></param>
//        /// <param name="timbres"></param>
//        /// <param name="volumes"></param>
//        /// <param name="bpms"></param>
//        /// <param name="totalTabs"></param>
//        /// <param name="currentTab"></param>
//        public SetSoundMenu(string[] labels, List<string> scales, List<string> groups, List<double> durations, List<int> timbres, List<int> volumes, List<int> velocities, List<int> bpms, List<bool> stopAtNext, List<bool> isDelayed, List<double> delayTime, int totalTabs, int currentTab)
//        {
//            this.Scales = scales;
//            this.Groups = groups;
//            this.Durations = durations;
//            this.LabelIndexes = timbres;
//            this.Volumes = volumes;
//            this.Velocities = velocities;
//            this.Bpms = bpms;
//            this.StopAtNext = stopAtNext;
//            this.IsDelayed = isDelayed;
//            this.DelayTime = delayTime;
//            this.TotalTabs = totalTabs;
//            this.CurrentTab = currentTab;
//            this.Labels = labels;
//            this.SetUpPositions();
//        }
//        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
//        {
//            base.gameWindowSizeChanged(oldBounds, newBounds);
//            SetUpPositions();
//        }
//        public void ClearUp()
//        {
//            xPositionOnScreen = Game1.viewport.Width / 2 - 425;
//            yPositionOnScreen = Game1.viewport.Height / 2 - 250;
//            width = 850;
//            height = 500;
//            borderWidth = 35;
//            isMouseVisible = true;
//            musicElements.Clear();
//            arrows.Clear();
//            components.Clear();
//            tabs.Clear();
//            inputBoxes.Clear();
//        }
//        public void SetUpPositions()
//        {
//            ClearUp();
//            musicElements.Add(new ClickableTextureComponent("1", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 2, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(8, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("2", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 6, this.yPositionOnScreen + IClickableMenu.borderWidth * 2, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(16, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("3", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 2, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("4", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 2, this.yPositionOnScreen + IClickableMenu.borderWidth * 2, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(32, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("5", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 8, this.yPositionOnScreen + IClickableMenu.borderWidth * 4, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(40, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("6", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 6, this.yPositionOnScreen + IClickableMenu.borderWidth * 4, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(48, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("7", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 4, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(56, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("8", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 2, this.yPositionOnScreen + IClickableMenu.borderWidth * 4, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(64, 16, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("C", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 14, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#C", new Rectangle((int)(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 11.7), this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 64, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 1, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#C", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 11, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("D", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 9, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(32, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#D", new Rectangle((int)(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 6.7), this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 64, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 1, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#D", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 6, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(32, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("E", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(40, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("F", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 2, this.yPositionOnScreen + IClickableMenu.borderWidth * 8, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(48, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#F", new Rectangle((int)(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 14.7), this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 64, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 1, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#F", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 14, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(48, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("G", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 12, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(56, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#G", new Rectangle((int)(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 9.7), this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 64, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 1, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#G", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 9, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(56, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("A", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 7, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(8, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#A", new Rectangle((int)(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 4.7), this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 64, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(24, 1, 8, 12), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("#A", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 4, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(8, 34, 8, 14), 4f, false));
//            musicElements.Add(new ClickableTextureComponent("B", new Rectangle(Game1.viewport.Width - this.xPositionOnScreen - IClickableMenu.borderWidth * 2, this.yPositionOnScreen + IClickableMenu.borderWidth * 11, 32, 64), "", "", Game1.content.Load<Texture2D>("LooseSprites\\font_bold"), new Rectangle(16, 34, 8, 14), 4f, false));
//            add = new ClickableTextureComponent("+", new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth * 13, this.yPositionOnScreen + IClickableMenu.borderWidth, 40, 44), "", "", Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f, false);
//            arrows.Add(new ClickableTextureComponent("left arrow", new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth, this.yPositionOnScreen + IClickableMenu.borderWidth, 64, 64), "", "", Game1.mouseCursors, this.leftArrow, 1f, false));
//            arrows.Add(new ClickableTextureComponent("right arrow", new Rectangle(this.xPositionOnScreen + IClickableMenu.borderWidth * 11, this.yPositionOnScreen + IClickableMenu.borderWidth, 64, 64), "", "", Game1.mouseCursors, this.rightArrow, 1f, false));
//            components.Add(new SmallScroll(ModEntry._i18n.Get("Menu_BPM"), this.xPositionOnScreen + IClickableMenu.borderWidth, Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 3, 10, 310, Bpms[CurrentTab - 1], val => { Bpms[CurrentTab - 1] = val; Block.lastBpm = val; }, null, true));
//            components.Add(new Slider(ModEntry._i18n.Get("Menu_Volume"), this.xPositionOnScreen + IClickableMenu.borderWidth, Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 5, 0, 100, Volumes[CurrentTab - 1], val => { Volumes[CurrentTab - 1] = val; Block.lastVolume = val; }, true, null, false));
//            components.Add(new Slider(ModEntry._i18n.Get("Menu_Velocity"), this.xPositionOnScreen + IClickableMenu.borderWidth, Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 7, 0, 5, Velocities[CurrentTab - 1], val => { Velocities[CurrentTab - 1] = val; Block.lastVelocity = val; }, true, val => { return (val + 1).ToString(); }, false));
//            //components.Add(new Slider(ModEntry._i18n.Get("Menu_Duration"), this.xPositionOnScreen + IClickableMenu.borderWidth, Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 9, 0, 127, Durations[CurrentTab - 1], val => { Durations[CurrentTab - 1] = val; Block.lastDuration = val; }, true, val =>
//            //         {
//            //             foreach (var key in ModEntry.Index_Duration.Keys)
//            //             {
//            //                 if (key == val.ToString())
//            //                     return (Convert.ToDouble(ModEntry.Index_Duration[key]) / 4).ToString();
//            //             }
//            //             return "null";
//            //         }, StopAtNext[CurrentTab - 1]));
//            //components.Add(new Slider(ModEntry._i18n.Get("Menu_DelayTime"), this.xPositionOnScreen + IClickableMenu.borderWidth, Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 11, 0, 127, DelayTime[CurrentTab - 1], val => { DelayTime[CurrentTab - 1] = val; Block.lastDelayTime = val; }, true, val =>
//            //         {
//            //             foreach (var key in ModEntry.Index_Duration.Keys)
//            //             {
//            //                 if (key == val.ToString())
//            //                     return (Convert.ToDouble(ModEntry.Index_Duration[key]) / 4).ToString();
//            //             }
//            //             return "null";
//            //         }, !IsDelayed[CurrentTab - 1]));
//            components.Add(new CheckBox(ModEntry._i18n.Get("Menu_IsDelayed"), (int)(this.xPositionOnScreen + IClickableMenu.borderWidth * 10.5), Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 11, IsDelayed[CurrentTab - 1], val =>
//                         {
//                             IsDelayed[CurrentTab - 1] = val;
//                             Block.lastIsDelayed = val;
//                             inputBoxes[1].greyedOut = val ? false : true;
//                             if (val && StopAtNext[CurrentTab - 1])
//                             {
//                                 StopAtNext[CurrentTab - 1] = false;
//                                 Block.lastStopAtNext = false;
//                                 SetUpPositions();
//                             }
//                         }, true));
//            components.Add(new CheckBox(ModEntry._i18n.Get("Menu_StopAtNext"), (int)(this.xPositionOnScreen + IClickableMenu.borderWidth * 10.5), Game1.viewport.Height - this.yPositionOnScreen - IClickableMenu.borderWidth * 9, StopAtNext[CurrentTab - 1], val =>
//                         {
//                             StopAtNext[CurrentTab - 1] = val;
//                             Block.lastStopAtNext = val;
//                             inputBoxes[0].greyedOut = val ? true : false;
//                             if (val && IsDelayed[CurrentTab - 1])
//                             {
//                                 IsDelayed[CurrentTab - 1] = false;
//                                 Block.lastIsDelayed = false;
//                                 SetUpPositions();
//                             }
//                         }, true));
//            inputBoxes.Add(new InputBox(ModEntry._i18n.Get("Menu_Duration"), Durations[CurrentTab - 1], xPositionOnScreen + borderWidth, Game1.viewport.Height - yPositionOnScreen - borderWidth * 9, 192, 48, StopAtNext[CurrentTab - 1]));
//            inputBoxes.Add(new InputBox(ModEntry._i18n.Get("Menu_DelayTime"), DelayTime[CurrentTab - 1], xPositionOnScreen + borderWidth, Game1.viewport.Height - yPositionOnScreen - borderWidth * 11, 192, 48, !IsDelayed[CurrentTab - 1]));
//            e = new TextBoxEvent(OnEnter);
//            foreach (var inputBox in inputBoxes)
//                inputBox.OnEnterPressed += e;
//            tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen - (int)(borderWidth * 2.9f), yPositionOnScreen + (int)(borderWidth * 0.5f), 100, 50), "1"));
//            if (TotalTabs > 1)
//                tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen - (int)(borderWidth * 2.9f), yPositionOnScreen + (int)(borderWidth * 0.5f) + (int)(borderWidth * 1.5f), 100, 50), "2"));
//            if (TotalTabs > 2)
//                tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen - (int)(borderWidth * 2.9f), yPositionOnScreen + (int)(borderWidth * 0.5f) + (int)(borderWidth * 1.5f) * 2, 100, 50), "3"));
//            if (TotalTabs > 3)
//                tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen - (int)(borderWidth * 2.9f), yPositionOnScreen + (int)(borderWidth * 0.5f) + (int)(borderWidth * 1.5f) * 3, 100, 50), "4"));
//        }
//        public void HandleButtonClick(string name)
//        {
//            if (name == null)
//                return;
//            if (name == "left arrow")
//            {
//                LabelIndexes[CurrentTab - 1]--;
//                if (LabelIndexes[CurrentTab - 1] < 0)
//                    LabelIndexes[CurrentTab - 1] = Labels.Length - 1;
//                Block.lastTimbre = LabelIndexes[CurrentTab - 1];
//                goto IL_101;
//            }
//            if (name == "right arrow")
//            {
//                LabelIndexes[CurrentTab - 1]++;
//                if (LabelIndexes[CurrentTab - 1] >= Labels.Length)
//                    LabelIndexes[CurrentTab - 1] = 0;
//                Block.lastTimbre = LabelIndexes[CurrentTab - 1];
//                goto IL_101;
//            }
//            if (name == "+")
//            {
//                TotalTabs++;
//                AddSound();
//                CurrentTab = TotalTabs;
//                SetUpPositions();
//                goto IL_101;
//            }
//            if (name == "1/" || name == "2/" || name == "3/" || name == "4/")
//            {
//                int index = Convert.ToInt32(name.Substring(0, 1)) - 1;
//                TotalTabs--;
//                CurrentTab = TotalTabs;
//                RemoveSound(index);
//                SetUpPositions();
//                goto IL_101;
//            }
//            if (name == "C" || name == "#C" || name == "D" || name == "#D" || name == "E" || name == "F" || name == "#F" || name == "G" || name == "#G" || name == "A" || name == "#A" || name == "B")
//            {
//                Scales[CurrentTab - 1] = name;
//                Block.lastScale = name;
//                goto IL_101;
//            }
//            if (name == "1" || name == "2" || name == "3" || name == "4" || name == "5" || name == "6" || name == "7" || name == "8")
//            {
//                Groups[CurrentTab - 1] = name;
//                Block.lastGroup = name;
//                goto IL_101;
//            }
//        IL_101:
//            Game1.playSound("bigDeSelect");
//        }
//        public override void leftClickHeld(int x, int y)
//        {
//            for (int i = 0; i < components.Count; i++)
//                components[i].LeftClickHeld(x, y);
//        }
//        public override void receiveLeftClick(int x, int y, bool playsound = true)
//        {
//            foreach (var button in musicElements)
//            {
//                if (button.containsPoint(x, y))
//                {
//                    HandleButtonClick(button.name);
//                    button.scale -= 0.5f;
//                    button.scale = Math.Max(3.5f, button.scale);
//                }
//            }
//            foreach (var arrow in arrows)
//            {
//                if (arrow.containsPoint(x, y))
//                {
//                    HandleButtonClick(arrow.name);
//                    arrow.scale -= 0.2f;
//                    arrow.scale = Math.Max(1.2f, arrow.scale);
//                }
//            }
//            if (add.containsPoint(x, y) && TotalTabs < 4)
//            {
//                HandleButtonClick(add.name);
//                add.scale -= 0.5f;
//                add.scale = Math.Max(3.5f, add.scale);
//            }
//            for (int i = 0; i < components.Count; i++)
//            {
//                if (components[i].bounds.Contains(x, y))
//                    components[i].ReceiveLeftClick(x, y);
//            }
//            foreach (var inputBox in inputBoxes)
//            {
//                inputBox.Selected = (inputBox.Bounds.Contains(x, y) && !inputBox.greyedOut) ? true : false;
//            }
//            foreach (var tab in tabs)
//            {
//                if (tab.containsPoint(x, y) && deleteButton != null && !deleteButton.containsPoint(x, y))
//                {
//                    CurrentTab = Convert.ToInt32(tab.name);
//                    SetUpPositions();
//                    break;
//                }
//            }
//            if (deleteButton != null && deleteButton.containsPoint(x, y))
//            {
//                HandleButtonClick(deleteButton.name + "/");
//                deleteButton.scale -= 0.5f;
//                deleteButton.scale = Math.Max(3.5f, add.scale);
//            }
//        }
//        public override void releaseLeftClick(int x, int y)
//        {
//            for (int i = 0; i < components.Count; i++)
//                components[i].LeftClickReleased(x, y);
//        }
//        public override void receiveScrollWheelAction(int direction)
//        {
//            for (int i = 0; i < components.Count; i++)
//                components[i].ReceiveScrollWheelAction(direction);
//        }
//        public override void performHoverAction(int x, int y)
//        {
//            foreach (var button in musicElements)
//                button.scale = button.containsPoint(x, y) ? Math.Min(button.scale + 0.5f, button.baseScale + 0.3f) : Math.Max(button.scale - 0.02f, button.baseScale);
//            foreach (var arrow in arrows)
//                arrow.scale = arrow.containsPoint(x, y) ? Math.Min(arrow.scale + 0.2f, arrow.baseScale + 0.1f) : Math.Max(arrow.scale - 0.02f, arrow.baseScale);
//            for (int i = 0; i < components.Count; i++)
//                components[i].performHoverAction = components[i].bounds.Contains(x, y) ? true : false;
//            foreach (var box in inputBoxes)
//            {
//                box.hoverAction = (box.Bounds.Contains(x, y) && !box.greyedOut) ? true : false;
//            }
//            add.scale = add.containsPoint(x, y) && TotalTabs < 4 ? Math.Min(add.scale + 0.5f, add.baseScale + 0.3f) : Math.Max(add.scale - 0.02f, add.baseScale);
//            foreach (var tab in tabs)
//            {
//                if (tab.containsPoint(x, y) && TotalTabs > 1)
//                {
//                    deleteButton = new ClickableTextureComponent(tab.name, new Rectangle(tab.bounds.X, tab.bounds.Y + 9, 36, 36), "", "", Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 3f, false);
//                    deleteButton.scale = deleteButton.containsPoint(x, y) ? Math.Min(deleteButton.scale + 0.5f, deleteButton.baseScale + 0.3f) : Math.Max(deleteButton.scale - 0.02f, deleteButton.baseScale);
//                    break;
//                }
//                else
//                    deleteButton = null;
//            }
//        }
//        public override void update(GameTime time)
//        {
//            if (inputBoxes.Count == 2)
//            {
//                if (Durations[CurrentTab - 1] != inputBoxes[0].Target)
//                    Durations[CurrentTab - 1] = inputBoxes[0].Target;
//                if (Block.lastDuration != inputBoxes[0].Target)
//                    Block.lastDuration = inputBoxes[0].Target;
//                if (DelayTime[CurrentTab - 1] != inputBoxes[1].Target)
//                    DelayTime[CurrentTab - 1] = inputBoxes[1].Target;
//                if (Block.lastDelayTime != inputBoxes[1].Target)
//                    Block.lastDelayTime = inputBoxes[1].Target;
//            }
//            base.update(time);
//        }
//        public override void draw(SpriteBatch b)
//        {
//            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
//            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White, 1f, true);
//            foreach (ClickableTextureComponent button in musicElements)
//                button.draw(b);
//            foreach (var arrow in arrows)
//                arrow.draw(b);
//            if (TotalTabs < 4)
//                add.draw(b);
//            else
//            {
//                add.draw(b);
//                add.draw(b, Color.Black * 0.5f, 0.9f);
//            }
//            foreach (var tab in tabs)
//            {
//                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), tab.bounds.X + (int)(borderWidth * 1.1f), tab.bounds.Y, 60, tab.bounds.Height, Color.White * (CurrentTab.ToString() == tab.name ? 1f : 0.7f), 1f, true);
//                Utility.drawTextWithShadow(b, tab.name, Game1.smallFont, new Vector2(tab.bounds.X + (int)(borderWidth * 1.1f) + 20, tab.bounds.Y + 10), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            }
//            Utility.drawTextWithShadow(b, Labels[LabelIndexes[CurrentTab - 1]], Game1.smallFont, new Vector2(this.xPositionOnScreen + IClickableMenu.borderWidth * 3, this.yPositionOnScreen + IClickableMenu.borderWidth), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            b.Draw(Texture_Noticeboard, new Vector2(Game1.viewport.Width - xPositionOnScreen - borderWidth * 2, yPositionOnScreen - borderWidth * 3), new Rectangle?(), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
//            Utility.drawTextWithShadow(b, Scales[CurrentTab - 1], Game1.smallFont, new Vector2(Game1.viewport.Width - xPositionOnScreen - borderWidth * 1.5f, yPositionOnScreen - borderWidth * 2.5f), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            Utility.drawTextWithShadow(b, Groups[CurrentTab - 1], Game1.smallFont, new Vector2(Game1.viewport.Width - xPositionOnScreen - borderWidth * 0.8f, yPositionOnScreen - borderWidth * 2.5f), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
//            for (int i = 0; i < components.Count; i++)
//            {
//                components[i].Draw(b);
//                if (!components[i].greyedOut && (components[i].performHoverAction || components[i].isHeld))
//                    components[i].Draw(b, Game1.viewport.Width - xPositionOnScreen - borderWidth * 0.3f, yPositionOnScreen - borderWidth * 1.5f);
//            }
//            foreach (var inputBox in inputBoxes)
//                inputBox.Draw(b, true);
//            if (deleteButton != null)
//                deleteButton.draw(b);
//            DrawMouse(b, components, inputBoxes);
//        }
//        public static void DrawMouse(SpriteBatch b, List<Element> elements, List<InputBox> inputs = null)
//        {
//            if (!isMouseVisible)
//                return;
//            for (int i = 0; i < elements.Count; i++)
//            {
//                if (!elements[i].greyedOut && (elements[i].performHoverAction || elements[i].isHeld))
//                {
//                    b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), new Rectangle(0, 16, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
//                    return;
//                }
//            }
//            if (inputs != null)
//            {
//                foreach (var box in inputs)
//                {
//                    if (box.hoverAction)
//                    {
//                        b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), new Rectangle(0, 16, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
//                        return;
//                    }
//                }
//            }
//            if (!Game1.options.hardwareCursor)
//                b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), new Rectangle(0, 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
//        }
//        //public override void receiveKeyPress(Keys key)
//        //{
//        //    base.receiveKeyPress(key);
//        //    if (key.Equals(Keys.A))
//        //    {
//        //        List<int> d = Durations;
//        //        List<int> v = Volumes;
//        //        List<int> b = Bpms;
//        //    }
//        //}
//        private void CleanUpForNew()
//        {
//            Scales.Clear();
//            Groups.Clear();
//            Durations.Clear();
//            LabelIndexes.Clear();
//            Bpms.Clear();
//            Volumes.Clear();
//            Velocities.Clear();
//            StopAtNext.Clear();
//            IsDelayed.Clear();
//            DelayTime.Clear();
//            CurrentTab = 1;
//            TotalTabs = 1;
//            AddSound();
//        }
//        private void AddSound()
//        {
//            Scales.Add(Block.lastScale);
//            Groups.Add(Block.lastGroup);
//            Durations.Add(Block.lastDuration);
//            LabelIndexes.Add(Block.lastTimbre < Labels.Length ? Block.lastTimbre : 0);
//            Volumes.Add(Block.lastVolume);
//            Velocities.Add(Block.lastVelocity);
//            Bpms.Add(Block.lastBpm);
//            StopAtNext.Add(Block.lastStopAtNext);
//            IsDelayed.Add(Block.lastIsDelayed);
//            DelayTime.Add(Block.lastDelayTime);
//        }
//        private void RemoveSound(int index)
//        {
//            Scales.RemoveAt(index);
//            Groups.RemoveAt(index);
//            Durations.RemoveAt(index);
//            LabelIndexes.RemoveAt(index);
//            Volumes.RemoveAt(index);
//            Velocities.RemoveAt(index);
//            Bpms.RemoveAt(index);
//            StopAtNext.RemoveAt(index);
//            IsDelayed.RemoveAt(index);
//            DelayTime.RemoveAt(index);
//        }
//        private void OnEnter(TextBox sender)
//        {
//            if (sender is InputBox)
//            {
//                InputBox box = sender as InputBox;
//                if (box.Text.Length > 0)
//                    box.Target = Utilities.CheckDoubleForamt(box.Text);
//            }
//            sender.Text = "";
//            sender.Selected = false;
//        }
//    }
//}