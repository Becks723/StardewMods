//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace JunimoStudio
//{
//    public interface ISounder
//    {
//        void Register(string key);

//        void Register(string[] keys);

//        void Unregister(string key);

//        void Unregister(string[] keys);

//        void Play();

//        /// <summary>
//        /// Stop all sounds.
//        /// </summary>
//        void StopAll();

//        /// <summary>
//        /// Stop the sound specified by a key.
//        /// </summary>
//        /// <param name="key">The key specified the sound to stop.</param>
//        void Stop(string key);

//        /// <summary>
//        /// Stop several sounds specified by their keys.
//        /// </summary>
//        /// <param name="keys">An array of keys specified the sounds to stop.</param>
//        void Stop(string[] keys);

//        void SetVolume(string key, float value);

//        void SetVelocity(string key, float value);

//        void SetPan(string key, float value);
//    }
//}
