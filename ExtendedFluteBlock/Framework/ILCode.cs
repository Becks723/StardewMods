using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using StardewModdingAPI;

namespace FluteBlockExtension.Framework
{
    internal struct RpcCurve
    {
        public int Variable;
    }

    internal struct RpcVariable
    {
        public float Value;

        public void SetValue(float value)
        {

        }
    }

    internal class LAalalSound
    {
        public RpcCurve[] RpcCurves;
    }

    internal class ILCode
    {
        private readonly XactSoundBankSound _currentXactSound;

        private LAalalSound _curSound;

        private float _rpcPitch;

        private RpcVariable[] _variables;

        private static IMonitor _monitor;

        public string Name { get; }

        public float Lalala()
        {
            if (_currentXactSound.trackIndex == 112)
            {
                RpcCurve rpcCurve = this._curSound.RpcCurves[3];
                float varValue = this._variables[rpcCurve.Variable].Value;

                float rate = (1200f - -1200) / (2400f - 0);
                float rpcPitch = varValue * rate - 1200;
                rpcPitch /= 1200f;
                _rpcPitch = rpcPitch;
                _UpdateSoundParameters();
                return 1f;
            }

            return 0f;
        }

        internal void _UpdateSoundParameters() { }

        public void SetVariable(string name, float value)
        {
            int num = 10101;
            if (this.Name is "flute" && name is "Pitch")
            {
                _monitor.Log("lalala", LogLevel.Trace);
                _variables[num].Value = value;
                return;
            }
            _variables[num].SetValue(value);
        }

        public static void Log(string message, LogLevel level = LogLevel.Trace)
        {
            _monitor.Log(message, level);
        }

        public static void Log()
        {
            _monitor.Log("lalala", LogLevel.Alert);
        }
    }
}
