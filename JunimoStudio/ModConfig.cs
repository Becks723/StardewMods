using JunimoStudio.Menus;
using StardewModdingAPI;

namespace JunimoStudio
{
    internal class ModConfig : ObservableObject
    {
        private EditMode _editMode;
        private bool _enableTracks;

        public PianoRollConfig PianoRoll { get; } = new();

        public ModConfigKeys Keys { get; } = new();

        public bool EnableTracks
        {
            get => this._enableTracks;
            set
            {
                if (this._enableTracks != value)
                {
                    this._enableTracks = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public EditMode EditMode
        {
            get => this._editMode;
            set
            {
                if (this._editMode != value)
                {
                    this._editMode = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public ModConfig()
        {
            this._enableTracks = true;
            this._editMode = EditMode.Simplified;
        }

        public SButton openSetTempoMenuKey { get; set; } = SButton.Divide;


        /**曲速（beat per minute）（以四分音符为一拍）*/
        public int musicTempo { get; set; } = 100;

        /**玩家绝对速度，默认（奔跑）为5 */
        public float playerAbsoluteTempo { get; set; } = 5;


        public float multiplier { get; set; } = 1;
    }
}