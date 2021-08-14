using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JunimoStudio.Core
{
    public abstract class NoteBlockBase : INoteBlock, IDisposable
    {
        private readonly IChannelManager _channelManager;
        private bool disposedValue;

        public IChannelManager ChannelManager => _channelManager;

        public NoteBlockBase(IChannelManager channelManager)
        {
            _channelManager = channelManager ?? throw new ArgumentNullException(nameof(channelManager));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~NoteBlockBase()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void Play()
        {
        }
    }
}
