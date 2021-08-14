using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JunimoStudio.Core.Plugins.Instruments;

namespace JunimoStudio.Core.Framework
{
    internal class Channel : IChannel
    {
        private IInstrumentPlugin _generator;

        public string Name { get; set; }

        public INoteCollection Notes { get; }

        public bool Mute { get; set; }

        public IInstrumentPlugin Generator
        {
            get => _generator;
            set
            {
                _generator = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public int EffectTrack { get; set; }

        public Channel(IInstrumentPlugin generator, int existingChannelNum)
            : this(new NoteCollection(), generator, existingChannelNum)
        {
        }

        public Channel(INoteCollection notes, IInstrumentPlugin generator, int existingChannelNum)
        {
            Notes = notes ?? throw new ArgumentNullException(nameof(notes));
            if (generator != null)
            {
                Generator = generator;
                Name = generator.Name;
            }
            Mute = false;
            EffectTrack = existingChannelNum;
        }

        public void Dispose()
        {
        }

    }
}
