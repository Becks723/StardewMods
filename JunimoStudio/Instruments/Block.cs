//using System;
//using System.Collections.Generic;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Audio;
//using Microsoft.Xna.Framework.Graphics;
//using StardewValley;
//using StardewValley.Objects;
//using StardewValley.Tools;
//using Object = StardewValley.Object;
//using System.Linq;
//using PyTK.CustomElementHandler;
//using JunimoStudio.UI;

//namespace JunimoStudio.Instruments
//{
//    public class Block : Object, ISaveElement
//    {
//        internal static Texture2D Texture_Notes;

//        internal static Texture2D Texture_Keyboard;
//        internal static Texture2D Texture_Guitar;
//        internal static Texture2D Texture_Drumkit;
//        internal static Texture2D Texture_String;
//        internal static Texture2D Texture_Wind;
//        internal static Texture2D Texture_Custom;

//        internal static readonly string[] Labels_Keyboard = { ModEntry._i18n.Get("Keyboard_Labels_0") };
//        internal static readonly string[] Labels_Guitar = { ModEntry._i18n.Get("Guitar_Labels_0"), ModEntry._i18n.Get("Guitar_Labels_1") };
//        internal static readonly string[] Labels_Drumkit = { ModEntry._i18n.Get("Drumkit_Labels_0") };
//        internal static readonly string[] Labels_String = { ModEntry._i18n.Get("String_Labels_0") };
//        internal static readonly string[] Labels_Wind = { ModEntry._i18n.Get("Wind_Labels_0") };

//        internal static string lastScale = "C";
//        internal static string lastGroup = "5";
//        internal static int lastTimbre = 0;
//        internal static double lastDuration = 0.25;
//        internal static int lastVolume = 80;
//        internal static int lastVelocity = 4;
//        internal static int lastBpm = 100;
//        internal static bool lastStopAtNext = false;
//        internal static bool lastIsDelayed = false;
//        internal static double lastDelayTime = 0.25;
//        public Texture2D Texture { get; set; }
//        public SetSoundMenu Menu { get; set; }
//        private List<ICue> cues = new List<ICue>();
//        private bool startTimer;
//        private bool startAnimations;
//        private List<bool> startAnimationsForDelayed = new List<bool>();
//        private float yPositionOfRaisingNote;
//        private List<Rectangle> notesRects = new List<Rectangle>();
//        private int stopTime;
//        private int animatedNotesCount;
//        private int animatedDelayedNotesCount;

//        public Block()
//        {
//            Type = "Crafting";
//            CanBeGrabbed = true;
//            CanBeSetDown = true;
//            HasBeenPickedUpByFarmer = true;
//            HasBeenInInventory = true;
//            boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
//        }
//        public override bool actionWhenPurchased()
//        {
//            return false;
//        }
//        public override bool canBeTrashed()
//        {
//            return true;
//        }
//        public override bool isActionable(Farmer who)
//        {
//            return true;
//        }
//        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
//        {
//            if (justCheckingForActivity)
//                return true;
//            Game1.activeClickableMenu = Menu;
//            return true;
//        }
//        public override bool isPlaceable()
//        {
//            return true;
//        }
//        public override bool performToolAction(Tool t, GameLocation location)
//        {
//            if (t is Pickaxe)
//            {
//                location.playSound("hammer", 0);
//                Game1.createRadialDebris(location, 12, (int)this.tileLocation.X, (int)this.tileLocation.Y, Game1.random.Next(4, 10), false, -1, false, -1);
//                //t.getLastFarmerToUse().currentLocation.debris.Add(new Debris(new KeyboardBlock(), this.tileLocation.Value * 64f + new Vector2(32f, 32f)));
//                location.objects.Remove(this.tileLocation);
//                return false;
//            }
//            return false;
//        }
//        public override void farmerAdjacentAction(GameLocation location)
//        {
//            if (!startTimer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds - lastNoteBlockSoundTime >= 1000 && !Game1.dialogueUp)
//            {
//                if (Utilities.temporaryCues?.Count > 0)
//                {
//                    foreach (var cue in Utilities.temporaryCues)
//                        cue.Stop(AudioStopOptions.AsAuthored);
//                    Utilities.temporaryCues.Clear();
//                }
//                SetCues(ModEntry._i18n.Get("Keyboard_Name"), "a");
//                SetCues(ModEntry._i18n.Get("Guitar_Name"), "b");
//                SetCues(ModEntry._i18n.Get("Drumkit_Name"), "c");
//                SetCues(ModEntry._i18n.Get("String_Name"), "d");
//                SetCues(ModEntry._i18n.Get("Wind_Name"), "e");
//                if (!Utilities.ContainsStopAtNext(this) && !Utilities.ContainsDelayed(this))
//                {
//                    foreach (var cue in cues)
//                        cue.Play();
//                    //RegisterAnimations();
//                }
//                else if (Utilities.ContainsStopAtNext(this) && !Utilities.ContainsDelayed(this))
//                {
//                    Utilities.DoStopAtNextFunction(this, cues);
//                    //RegisterAnimations();
//                }
//                else if (!Utilities.ContainsStopAtNext(this) && Utilities.ContainsDelayed(this))
//                {
//                    Utilities.DoDelayedFunction(this, cues);
//                    //RegisterAnimationsWithDelayed();
//                }
//                else
//                {
//                    Utilities.DoStopAtNextFunction(this, cues);
//                    Utilities.DoDelayedFunction(this, cues);
//                    //RegisterAnimationsWithDelayed();
//                }
//                scale.Y = 1.3f;
//                if (!Utilities.AllDelayed(this))
//                    shakeTimer = 200;
//                startTimer = true;
//                //notesRects.Clear();
//                //notesRects.AddRange(new Rectangle[] {
//                //    Utilities.GetRandomSourceRectForNotes(),
//                //    Utilities.GetRandomSourceRectForNotes(),
//                //    Utilities.GetRandomSourceRectForNotes(),
//                //    Utilities.GetRandomSourceRectForNotes() });
//                lastNoteBlockSoundTime = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
//            }
//        }
//        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
//        {
//            base.updateWhenCurrentLocation(time, environment);
//            if (cues.Count >= 1)
//            {
//                if (startTimer)
//                {
//                    stopTime += time.ElapsedGameTime.Milliseconds;
//                    for (int i = 0; i < cues.Count; i++)
//                    {
//                        if (Menu.IsDelayed[i])
//                        {
//                            if (stopTime >= Utilities.WhenToPlayOrStop(Menu.Bpms[i], Menu.DelayTime[i]) && !cues[i].IsPlaying && !cues[i].IsStopping && !cues[i].IsStopped)
//                            {
//                                cues[i].Play();
//                                shakeTimer = 200;
//                                //startAnimationsForDelayed[i] = true;
//                            }
//                            if (stopTime >= Utilities.WhenToPlayOrStop(Menu.Bpms[i], Menu.DelayTime[i] + Menu.Durations[i] + 1) && cues[i].IsPlaying)
//                                cues[i].Stop(AudioStopOptions.AsAuthored);
//                        }
//                        else
//                        {
//                            if (stopTime >= Utilities.WhenToPlayOrStop(Menu.Bpms[i], Menu.Durations[i]) && cues[i].IsPlaying)
//                                cues[i].Stop(AudioStopOptions.AsAuthored);
//                        }
//                    }
//                }
//                if (Utilities.AllCuesStopped(cues))
//                {
//                    stopTime = 0;
//                    startTimer = false;
//                }
//            }
//            //if (startAnimations)
//            //{
//            //    //LightSource lightSource = this.netLightSource.Get();
//            //    //if (lightSource != null && !environment.hasLightSource(lightSource.Identifier))
//            //    //{
//            //    //    environment.sharedLights[lightSource.identifier] = lightSource.Clone();
//            //    //}
//            //    if (yPositionOfRaisingNote <= 450)
//            //        yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 3.5);
//            //    else if (yPositionOfRaisingNote <= 550)
//            //        yPositionOfRaisingNote += time.ElapsedGameTime.Milliseconds;
//            //    else if (yPositionOfRaisingNote <= 600)
//            //        yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 0.7);
//            //    else if (yPositionOfRaisingNote <= 650)
//            //        yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 0.3);
//            //    else if (yPositionOfRaisingNote <= 700)
//            //    {
//            //        yPositionOfRaisingNote = 0f;
//            //        startAnimations = false;
//            //        animatedNotesCount = 0;
//            //    }
//            //}
//            //for (int i = 0; i < startAnimationsForDelayed.Count; i++)
//            //{
//            //    if (startAnimationsForDelayed[i])
//            //    {
//            //        if (yPositionOfRaisingNote <= 450)
//            //            yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 3.5);
//            //        else if (yPositionOfRaisingNote <= 550)
//            //            yPositionOfRaisingNote += time.ElapsedGameTime.Milliseconds;
//            //        else if (yPositionOfRaisingNote <= 600)
//            //            yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 0.7);
//            //        else if (yPositionOfRaisingNote <= 650)
//            //            yPositionOfRaisingNote += (float)(time.ElapsedGameTime.Milliseconds * 0.3);
//            //        else if (yPositionOfRaisingNote <= 700)
//            //        {
//            //            yPositionOfRaisingNote = 0f;
//            //            startAnimationsForDelayed[i] = false;
//            //        }
//            //    }
//            //}

//            //LightSource lightSource = this.netLightSource.Get();
//            //if (lightSource != null && this.isOn && !environment.hasLightSource(lightSource.Identifier))
//            //{
//            //    environment.sharedLights[lightSource.identifier] = lightSource.Clone();
//            //}
//            //if (heldObject != null)
//            //{
//            //    lightSource = heldObject.netLightSource.Get();
//            //    if (lightSource != null && !environment.hasLightSource(lightSource.Identifier))
//            //    {
//            //        environment.sharedLights[lightSource.identifier] = lightSource.Clone();
//            //    }
//            //}
//        }
//        //public override void initializeLightSource(Vector2 tileLocation, bool mineShaft = false)
//        //{
//        //    int identifier = (int)(tileLocation.X * 2000f + tileLocation.Y);
//        //    this.lightSource = new LightSource(4, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), 0.3f, Color.DarkCyan, identifier, LightSource.LightContext.None, 0L);

//        //}
//        public override void draw(SpriteBatch b, int x, int y, float alpha = 1f)
//        {
//            b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32f, y * 64 + 53f)), new Rectangle?(Game1.shadowTexture.Bounds), Color.White, 0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.001f);
//            b.Draw(Texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), new Rectangle?(new Rectangle(0, 0, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
//            //if (startAnimations)
//            //    DrawRisingNotes(b, x, y, animatedNotesCount);
//            //for (int i = 0; i < startAnimationsForDelayed.Count; i++)
//            //{
//            //    if (startAnimationsForDelayed[i])
//            //        DrawRisingNotes(b, x, y, animatedDelayedNotesCount);
//            //}
//        }
//        public override void drawInMenu(SpriteBatch b, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
//        {
//            b.Draw(Texture, location + new Vector2(32f, 32f), new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(this.Texture, 16, 16, 0)), color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
//            if (maximumStackSize() > 1 && Stack > 1 && scaleSize > 0.3 && Stack != 2147483647)
//            {
//                Utility.drawTinyDigits(Stack, b, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(stack, 3f * scaleSize) + 3f * scaleSize, (float)(64.0 - 18.0 * scaleSize + 2.0)), 3f * scaleSize, 1f, color);
//            }
//        }
//        public override void drawWhenHeld(SpriteBatch b, Vector2 objectPosition, Farmer f)
//        {
//            b.Draw(Texture, objectPosition, new Rectangle?(Game1.getSquareSourceRectForNonStandardTileSheet(this.Texture, 16, 16, 0)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (f.getStandingY() + 3) / 10000f));
//        }
//        public override void drawPlacementBounds(SpriteBatch b, GameLocation location)
//        {
//            int x = (int)Game1.GetPlacementGrabTile().X * 64;
//            int y = (int)Game1.GetPlacementGrabTile().Y * 64;
//            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
//            bool isCheckingNonMousePlacement = Game1.isCheckingNonMousePlacement;
//            if (isCheckingNonMousePlacement)
//            {
//                Vector2 placementPosition = Utility.GetNearbyValidPlacementPosition(Game1.player, location, this, x, y);
//                x = (int)placementPosition.X;
//                y = (int)placementPosition.Y;
//            }
//            bool flag = Utility.playerCanPlaceItemHere(location, this, x, y, Game1.player) || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, this, x, y) && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player));
//            Game1.isCheckingNonMousePlacement = false;
//            b.Draw(Game1.mouseCursors, new Vector2(x / 64 * 64 - Game1.viewport.X, y / 64 * 64 - Game1.viewport.Y), new Rectangle?(new Rectangle(flag ? 194 : 210, 388, 16, 16)), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
//        }
//        private void AddCue(int i, string firstLetter)
//        {
//            Dictionary<string, string> currentTimbre = ModEntry.Index_GetIndex[$"{firstLetter}{ModEntry.Index_VelAndTimbre[Menu.LabelIndexes[i].ToString()]}"];
//            if (currentTimbre.ContainsKey(Menu.Scales[i] + Menu.Groups[i]))
//            {
//                cues.Add(ModEntry.soundBank.GetCue($"{firstLetter}{ModEntry.Index_VelAndTimbre[Menu.LabelIndexes[i].ToString()]}{ModEntry.Index_VelAndTimbre[Menu.Velocities[i].ToString()]}{currentTimbre[Menu.Scales[i] + Menu.Groups[i]]}"));
//                cues[i].SetVariable("Volume", Menu.Volumes[i]);
//            }
//            else
//            {
//                cues.Add(ModEntry.soundBank.GetCue("aaa1"));
//                cues[i].SetVariable("Volume", 0f);
//            }
//        }
//        private void SetCues(string whichName, string firstLetter)
//        {
//            if (this.Name.Equals(whichName))
//            {
//                cues.Clear();
//                AddCue(0, firstLetter);
//                if (Menu.TotalTabs > 1)
//                    AddCue(1, firstLetter);
//                if (Menu.TotalTabs > 2)
//                    AddCue(2, firstLetter);
//                if (Menu.TotalTabs > 3)
//                    AddCue(3, firstLetter);
//            }
//        }
//        private void RegisterAnimations()
//        {
//            animatedNotesCount = Menu.TotalTabs;
//            startAnimations = true;
//        }
//        private void RegisterAnimationsWithDelayed()
//        {
//            startAnimationsForDelayed.Clear();
//            startAnimationsForDelayed.AddRange(new bool[] { false, false, false, false });
//            animatedDelayedNotesCount = Utilities.GetSameDelayTimeCountForDelayed(this);
//            animatedNotesCount = Menu.TotalTabs - animatedDelayedNotesCount;
//            startAnimations = true;
//        }
//        private void DrawRisingNotes(SpriteBatch b, int x, int y, int notesCount)
//        {
//            switch (notesCount)
//            {
//                case 1:
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 20, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    break;
//                case 2:
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 8, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[1], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    break;
//                case 3:
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 4, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 20, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[1], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 44, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    break;
//                case 4:
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 - 16, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[0], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 8, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[1], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[2], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    b.Draw(Texture_Notes, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 56, y * 64 - 20 - yPositionOfRaisingNote / 10)), notesRects[3], Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
//                    break;
//            }
//        }

//        public virtual object getReplacement()
//        {
//            return new Chest(true);
//        }
//        public virtual Dictionary<string, string> getAdditionalSaveData()
//        {
//            Dictionary<string, string> saveData = new Dictionary<string, string>();
//            saveData.Add("tileLocation", TileLocation.ToString());
//            saveData.Add("stack", Stack.ToString());
//            if (Menu != null)
//            {
//                for (int i = 0; i < Menu.TotalTabs; i++)
//                {
//                    saveData.Add("scale" + i.ToString(), Menu.Scales[i]);
//                    saveData.Add("group" + i.ToString(), Menu.Groups[i]);
//                    saveData.Add("duration" + i.ToString(), Menu.Durations[i].ToString());
//                    saveData.Add("timbre" + i.ToString(), Menu.LabelIndexes[i].ToString());
//                    saveData.Add("volume" + i.ToString(), Menu.Volumes[i].ToString());
//                    saveData.Add("velocity" + i.ToString(), Menu.Velocities[i].ToString());
//                    saveData.Add("bpm" + i.ToString(), Menu.Bpms[i].ToString());
//                    saveData.Add("stopAtNext" + i.ToString(), Menu.StopAtNext[i].ToString());
//                    saveData.Add("isDelayed" + i.ToString(), Menu.IsDelayed[i].ToString());
//                    saveData.Add("delayTime" + i.ToString(), Menu.DelayTime[i].ToString());
//                }
//                saveData.Add("currentTab", Menu.CurrentTab.ToString());
//                saveData.Add("totalTabs", Menu.TotalTabs.ToString());
//            }
//            return saveData;
//        }
//        public virtual void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
//        {
//        }
//        public Block SetAdditionalDataToInstance(Dictionary<string, string> additionalSaveData, Block which, string[] whichLabel)
//        {
//            if (additionalSaveData.TryGetValue("tileLocation", out string data))
//                which.TileLocation = Utilities.StringToVector2(data);
//            else
//                which.TileLocation = Vector2.Zero;
//            if (additionalSaveData.TryGetValue("stack", out data))
//                which.Stack = Convert.ToInt32(data);
//            else
//                which.Stack = 1;
//            if (additionalSaveData.TryGetValue("totalTabs", out data))
//                which.Menu.TotalTabs = Convert.ToInt32(data);
//            else
//                which.Menu.TotalTabs = 1;
//            if (additionalSaveData.TryGetValue("currentTab", out data))
//                which.Menu.CurrentTab = Convert.ToInt32(data);
//            else
//                which.Menu.CurrentTab = 1;

//            which.Menu.Scales.Clear();
//            which.Menu.Groups.Clear();
//            which.Menu.Durations.Clear();
//            which.Menu.LabelIndexes.Clear();
//            which.Menu.Volumes.Clear();
//            which.Menu.Velocities.Clear();
//            which.Menu.Bpms.Clear();
//            which.Menu.StopAtNext.Clear();
//            which.Menu.IsDelayed.Clear();
//            which.Menu.DelayTime.Clear();
//            for (int i = 0; i < which.Menu.TotalTabs; i++)
//            {
//                if (additionalSaveData.TryGetValue("scale" + i.ToString(), out data))
//                    which.Menu.Scales.Add(data);
//                else
//                    which.Menu.Scales.Add("C");
//                if (additionalSaveData.TryGetValue("group" + i.ToString(), out data))
//                    which.Menu.Groups.Add(data);
//                else
//                    which.Menu.Groups.Add("5");
//                if (additionalSaveData.TryGetValue("duration" + i.ToString(), out data))
//                {
//                    if (Convert.ToDouble(data) > 2)
//                        data = "2.00";
//                    which.Menu.Durations.Add(Convert.ToDouble(data));
//                }
//                else
//                    which.Menu.Durations.Add(0.25);
//                if (additionalSaveData.TryGetValue("timbre" + i.ToString(), out data))
//                    which.Menu.LabelIndexes.Add(Convert.ToInt32(data));
//                else
//                    which.Menu.LabelIndexes.Add(0);
//                if (additionalSaveData.TryGetValue("volume" + i.ToString(), out data))
//                    which.Menu.Volumes.Add(Convert.ToInt32(data));
//                else
//                    which.Menu.Volumes.Add(80);
//                if (additionalSaveData.TryGetValue("velocity" + i.ToString(), out data))
//                    which.Menu.Velocities.Add(Convert.ToInt32(data));
//                else
//                    which.Menu.Velocities.Add(4);
//                if (additionalSaveData.TryGetValue("bpm" + i.ToString(), out data))
//                    which.Menu.Bpms.Add(Convert.ToInt32(data));
//                else
//                    which.Menu.Bpms.Add(100);
//                if (additionalSaveData.TryGetValue("stopAtNext" + i.ToString(), out data))
//                    which.Menu.StopAtNext.Add(Convert.ToBoolean(data));
//                else
//                    which.Menu.StopAtNext.Add(false);
//                if (additionalSaveData.TryGetValue("isDelayed" + i.ToString(), out data))
//                    which.Menu.IsDelayed.Add(Convert.ToBoolean(data));
//                else
//                    which.Menu.IsDelayed.Add(false);
//                if (additionalSaveData.TryGetValue("delayTime" + i.ToString(), out data))
//                {
//                    if (Convert.ToDouble(data) > 2)
//                        data = "2.00";
//                    which.Menu.DelayTime.Add(Convert.ToDouble(data));
//                }
//                else
//                    which.Menu.DelayTime.Add(0.25);
//            }
//            which.Menu = new SetSoundMenu(whichLabel, which.Menu.Scales, which.Menu.Groups, which.Menu.Durations, which.Menu.LabelIndexes, which.Menu.Volumes, which.Menu.Velocities, which.Menu.Bpms, which.Menu.StopAtNext, which.Menu.IsDelayed, which.Menu.DelayTime, which.Menu.TotalTabs, which.Menu.CurrentTab);
//            return which;
//        }



//        //public static ISaveIndex SaveIndex { get; set; }
//        //public override Item getOne()
//        //{
//        //    return GetNew();
//        //}
//        //public override bool CanLinkWith(object linkedObject)
//        //{
//        //    return linkedObject is Object obj && obj.netName.Get().Contains("Block");
//        //}
//        //public override NetString GetDataLink(object linkedObject)
//        //{
//        //    if (linkedObject is Item item)
//        //        return item.netName;
//        //    return null;
//        //}
//        //public static Object GetNew()
//        //{
//        //    //SaveIndex.ValidateIndex();
//        //    //var newBlock = new Object(SaveIndex.Index, 1);
//        //    //return newBlock;
//        //    Block b = new Block();
//        //    return b; 
//        //}
//        //public override void OnConstruction(IPlatoHelper helper, object linkedObject)
//        //{
//        //    base.OnConstruction(helper, linkedObject);
//        //    SaveIndex.ValidateIndex();
//        //    Data?.Set("Block", true);
//        //    //CheckParentSheetIndex();
//        //    checkTexture();
//        //    this.Build();

//        //    Data.Set("Name", Name, 0);
//        //    Data.Set("Scale", Menu.Scale,0);
//        //    Data.Set("Group", Menu.Group,0);
//        //}
//        //private void CheckParentSheetIndex()
//        //{
//        //    if (SaveIndex.Index != Base?.parentSheetIndex.Value)
//        //    {
//        //        SaveIndex.ValidateIndex(Base?.parentSheetIndex.Value ?? -1);
//        //        Base?.parentSheetIndex.Set(SaveIndex.Index);
//        //    }
//        //}
//        //private void checkTexture()
//        //{
//        //    CheckParentSheetIndex();
//        //    Texture = TextureKey;
//        //}
//    }
//}
