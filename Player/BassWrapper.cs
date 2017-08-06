using System;
using System.Collections.Generic;
using Un4seen.Bass;

namespace Player
{
    public class BassWrapper : IDisposable
    {
        /// <summary>
        /// Частота дискретизации
        /// </summary>
        private int HZ = 44100;

        /// <summary>
        /// Состояние инициализации
        /// </summary>
        private bool InitDefaultDevice;

        /// <summary>
        /// Канал
        /// </summary>
        private int Stream;

        /// <summary>
        /// Громкость
        /// </summary>
        private int Volume = 100;

        private bool isStopped = true;

        public bool EndPlaylist;

        private readonly List<int> BassPluginsHandles = new List<int>();

        private string appPath = AppDomain.CurrentDomain.BaseDirectory;

        public readonly HashSet<string> SupportedAudioFileFormats;

        private volatile bool _disposed;

        public BassWrapper()
        {
            SupportedAudioFileFormats = new HashSet<string>(Resources.iWillPlayExtensions.Split('|'),
                StringComparer.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DeinitializeBassStream();
            _disposed = true;

            GC.SuppressFinalize(this);
        }

        private void DeinitializeBassStream()
        {
            if (Stream != 0)
            {
                Bass.BASS_StreamFree(Stream);
                Stream = 0;
            }
        }

        /// <summary>
        /// Инициализация Bass.dll
        /// </summary>
        /// <returns></returns>
        public bool InitBass()
        {
            if (!InitDefaultDevice)
            {
                InitDefaultDevice = Bass.BASS_Init(-1, HZ, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
                if (InitDefaultDevice)
                {
                    BassPluginsHandles.Add(Bass.BASS_PluginLoad(appPath + @"\plugins\bass_aac.dll"));
                    BassPluginsHandles.Add(Bass.BASS_PluginLoad(appPath + @"\plugins\bassflac.dll"));
                    BassPluginsHandles.Add(Bass.BASS_PluginLoad(appPath + @"\plugins\basswma.dll"));
                }
            }

            return InitDefaultDevice;
        }

        /// <summary>
        /// Воспроизведение
        /// </summary>
        /// <param name="fileName"></param>
        public void Play(string fileName)
        {
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED)
            {
                Stop();

                if (InitBass())
                {
                    Stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_DEFAULT);

                    if (Stream != 0)
                    {
                        Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
                        Bass.BASS_ChannelPlay(Stream, false);
                    }
                }
            }
            else
            {
                Bass.BASS_ChannelPlay(Stream, false);
            }

            isStopped = false;
        }

        /// <summary>
        /// Стоп
        /// </summary>
        public void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            isStopped = true;
        }

        /// <summary>
        /// Пауза
        /// </summary>
        public void Pause()
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_ChannelPause(Stream);
            }
        }

        /// <summary>
        /// Получение длительности канала в секундах
        /// </summary>
        /// <returns></returns>
        public int GetTimeOfStream()
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(Stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(Stream, TimeBytes);
            return (int) Time;
        }

        /// <summary>
        /// Получение текущей позиции в секундах
        /// </summary>
        /// <returns></returns>
        public int GetPosOfStream()
        {
            long pos = Bass.BASS_ChannelGetPosition(Stream);
            int posSec = (int) Bass.BASS_ChannelBytes2Seconds(Stream, pos);
            return posSec;
        }

        public void SetPosOfScroll(int pos)
        {
            Bass.BASS_ChannelSetPosition(Stream, (double) pos);
        }

        /// <summary>
        /// Установка громкости
        /// </summary>
        /// <param name="vol"></param>
        public void SetVolumeStream(int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }

        public bool ToNextTrack(bool notEndTrack)
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !isStopped)
            {
                // если текущий трек не последний
                if (notEndTrack)
                {
                    Play(PlayerInfo.filesInfo[++PlayerInfo.currentTrackNumber].PathFileName);

                    EndPlaylist = false;

                    return true;
                }

                EndPlaylist = true;
            }

            return false;
        }
    }
}
