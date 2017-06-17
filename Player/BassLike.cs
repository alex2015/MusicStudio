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
        public static int HZ = 44100;

        /// <summary>
        /// Состояние инициализации
        /// </summary>
        private static bool InitDefaultDevice;

        /// <summary>
        /// Канал
        /// </summary>
        public static int Stream;

        /// <summary>
        /// Громкость
        /// </summary>
        public static int Volume = 100;

        private static bool isStopped = true;

        public static bool EndPlaylist;

        private static readonly List<int> BassPluginsHandles = new List<int>();

        private static string appPath = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Инициализация Bass.dll
        /// </summary>
        /// <param name="hz"></param>
        /// <returns></returns>
        public static bool InitBass(int hz)
        {
            if (!InitDefaultDevice)
            {
                InitDefaultDevice = Bass.BASS_Init(-1, hz, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
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
        /// <param name="vol"></param>
        public static void Play(string fileName, int vol)
        {
            if (Bass.BASS_ChannelIsActive(Stream) != BASSActive.BASS_ACTIVE_PAUSED)
            {
                Stop();

                if (InitBass(HZ))
                {
                    Stream = Bass.BASS_StreamCreateFile(fileName, 0, 0, BASSFlag.BASS_DEFAULT);

                    if (Stream != 0)
                    {
                        Volume = vol;
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
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetTimeOfStream(int stream)
        {
            long TimeBytes = Bass.BASS_ChannelGetLength(stream);
            double Time = Bass.BASS_ChannelBytes2Seconds(stream, TimeBytes);
            return (int) Time;
        }

        /// <summary>
        /// Получение текущей позиции в секундах
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static int GetPosOfStream(int stream)
        {
            long pos = Bass.BASS_ChannelGetPosition(stream);
            int posSec = (int) Bass.BASS_ChannelBytes2Seconds(stream, pos);
            return posSec;
        }

        public static void SetPosOfScroll(int stream, int pos)
        {
            Bass.BASS_ChannelSetPosition(stream, (double) pos);
        }

        /// <summary>
        /// Установка громкости
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vol"></param>
        public static void SetVolumeStream(int stream, int vol)
        {
            Volume = vol;
            Bass.BASS_ChannelSetAttribute(stream, BASSAttribute.BASS_ATTRIB_VOL, Volume / 100F);
        }

        public static bool ToNextTrack(bool notEndTrack)
        {
            if (Bass.BASS_ChannelIsActive(Stream) == BASSActive.BASS_ACTIVE_STOPPED && !isStopped)
            {
                // если текущий трек не последний
                if (notEndTrack)
                {
                    Play(Vars.filesInfo[++Vars.currentTrackNumber].PathFileName, Volume);

                    EndPlaylist = false;

                    return true;
                }

                EndPlaylist = true;
            }

            return false;
        }
    }
}
