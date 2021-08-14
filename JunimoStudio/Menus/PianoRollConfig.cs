using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Menus
{
    internal class PianoRollConfig : ObservableObject
    {
        private GridResolution _grid = GridResolution.Beat;

        public GridResolution Grid
        {
            get => this._grid;
            set
            {
                if (this._grid != value)
                {
                    this._grid = value;
                    this.RaisePropertyChanged();
                }
            }
        }
    }

    internal enum GridResolution
    {
        Bar,
        Beat,
        HalfBeat,
        OneThirdBeat,
        QuarterBeat,
        OneSixthBeat,
        //Step,
        //HalfStep,
        //OneThirdStep,
        //QuarterStep,
        //OneSixthStep,
    }
}
