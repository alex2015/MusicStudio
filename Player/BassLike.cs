using System;
using System.Collections.Generic;
using Un4seen.Bass;

namespace Player
{
    public class BassLike
    {
        /// <summary>
        /// Частота дискретизации
        /// </summary>
        private static int HZ = 44100;

        /// <summary>
        /// Состояние инициализации
        /// </summary>
        private static bool InitDefaultDevice;

        /// <summary>
        /// Канал
        /// </summary>
        private static int Stream;

        /// <summary>
        /// Громкость
        /// </summary>
        private static int Volume = 100;

        private static bool isStopped = true;

        public static bool EndPlaylist;

        private static readonly List<int> BassPluginsHandles = new List<int>();

        private static string appPath = AppDomain.CurrentDomain.BaseDirectory;

        public static readonly HashSet<string> SupportedAudioFileFormats;

        static BassLike()
        {
            SupportedAudioFileFormats = new HashSet<string>(Resources.iWillPlayExtensions.Split('|'), StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Инициализация Bass.dll
        /// </summary>
        /// <returns></returns>
        public static bool InitBass()
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
        public static void Play(string fileName)
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
        public static void Stop()
        {
            Bass.BASS_ChannelStop(Stream);
            Bass.BASS_StreamFree(Stream);
            isStopped = true;
        }

        /// <summary>
        /// Пауза
        /// </summary>
        public static void Pause()
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
        public static int GetTimeOfStream()
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(Stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(Stream, TimeBytes);
            return (int) Time;
        }

        /// <summary>
        /// Получение текущей позиции в секундах
        /// </summary>
        /// <returns></returns>
        public static int GetPosOfStream()
        {
            long pos = Bass.BASS_ChannelGetPosition(Stream);
            int posSec = (int) Bass.BASS_ChannelBytes2Seconds(Stream, pos);
            return posSec;
        }

        public static void SetPosOfScroll(int pos)
        {
            Bass.BASS_ChannelSetPosition(Stream, (double) pos);
        }

        /// <summary>
        /// Установка громкости
        /// </summary>
        /// <param name="vol"></param>
        public static void SetVolumeStream(int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(Stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }

        public static bool ToNextTrack(bool notEndTrack)
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !isStopped)
            {
                // если текущий трек не последний
                if (notEndTrack)
                {
                    Play(Vars.filesInfo[++Vars.currentTrackNumber].PathFileName);

                    EndPlaylist = false;

                    return true;
                }

                EndPlaylist = true;
            }

            return false;
        }
    }
}
