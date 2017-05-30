using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Tools;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Aac;
using Un4seen.Bass.AddOn.Flac;
using Un4seen.Bass.AddOn.Wma;

namespace Player
{
    public class PlayerWrapper : IDisposable
    {
        private const int ProgressTimeout = 100;
        private const double LastSecondsThatCanBeIgnoredWhileInspectingDisconnectCondition = 1.5;
        public static readonly HashSet<string> SupportedAudioFileFormats;
        private string _uri;
        private UriType _uriType;
        private int _streamHandle;
        private long _streamLength;
        private long _contentLength;
        private int _downloadedContentLength;
        private double _streamDivContentFactor;
        private Timer _progressTimer;
        private DOWNLOADPROC _downloadBassProcedure;
        private SYNCPROC _bassSyncEnd;
        private TimeSpan _playPosition;
        private TimeSpan _downloadPosition;
        private volatile bool _wasDisposed;
        public event EventHandler PlayPositionChanged;
        public event EventHandler DownloadPositionChanged;
        public event EventHandler PlayFinished;
        public event EventHandler<ErrorEventArgs> ErrorOccurred;


        public State PlayerState { get; private set; }

        public TimeSpan Duration { get; private set; }

        public TimeSpan PlayPosition
        {
            get
            {
                return _playPosition;
            }
            private set
            {
                if (_playPosition.Ticks == value.Ticks)
                    return;
                _playPosition = value;
                EventsHelper.Raise(PlayPositionChanged, this, new EventArgs());
            }
        }

        public TimeSpan DownloadPosition
        {
            get
            {
                return _downloadPosition;
            }
            private set
            {
                if (_downloadPosition.Ticks == value.Ticks)
                    return;
                _downloadPosition = value;
                EventsHelper.Raise(DownloadPositionChanged, this, new EventArgs());
            }
        }

        public float Volume
        {
            get
            {
                if (_streamHandle == 0)
                    return 0.0f;
                float num = 0.0f;
                if (!Bass.BASS_ChannelGetAttribute(_streamHandle, (BASSAttribute)2, ref num))
                    return 0.0f;
                return num;
            }
            set
            {
                if (_streamHandle == 0)
                    return;
                Bass.BASS_ChannelSetAttribute(_streamHandle, (BASSAttribute)2, value);
            }
        }



        static PlayerWrapper()
        {
            SupportedAudioFileFormats = new HashSet<string>(Resources.iWillPlayExtensions.Split('|'), StringComparer.OrdinalIgnoreCase);
        }

        public PlayerWrapper(string uri, UriType uriType = UriType.Unknown)
        {
            _uri = uri;
            _uriType = uriType == UriType.Unknown ? DetectUriType(uri) : uriType;
            // ISSUE: method pointer
            //this._downloadBassProcedure = new DOWNLOADPROC((object)this, __methodptr(DownloadBassProcedure));
            _downloadBassProcedure = DownloadBassProcedure;
            // ISSUE: method pointer
            _bassSyncEnd = BassSyncEnd;
            // ISSUE: method pointer
            _progressTimer = new Timer(ProgressTimerCallback, null, -1, -1);
            _streamHandle = 0;
            InitializeBassStream(0);
        }

        ~PlayerWrapper()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_wasDisposed)
                return;
            if (isDisposing && _progressTimer != null)
            {
                _progressTimer.Dispose();
                _progressTimer = null;
            }
            DeinitializeBassStream();
            _wasDisposed = true;
        }

        public static UriType DetectUriType(string uri)
        {
            UriType uriType1 = UriType.Unknown;
            Uri result;
            if (Uri.TryCreate(uri, UriKind.Absolute, out result) && result.Scheme == Uri.UriSchemeFile)
                uriType1 |= UriType.LocalFile;
            int startIndex = uri.LastIndexOf('.');
            UriType uriType2;
            if (startIndex < 0)
            {
                uriType2 = uriType1 | UriType.Common;
            }
            else
            {
                switch (uri.Substring(startIndex).ToLower())
                {
                    case ".aac":
                    case ".mp4":
                    case ".m4a":
                    case ".m4v":
                    case ".m4b":
                    case ".m4p":
                    case ".m4r":
                        uriType2 = uriType1 | UriType.Mpeg4Part14;
                        break;
                    case ".flac":
                        uriType2 = uriType1 | UriType.Flac;
                        break;
                    case ".wma":
                        uriType2 = uriType1 | UriType.Wma;
                        break;
                    default:
                        uriType2 = uriType1 | UriType.Common;
                        break;
                }
            }
            return uriType2;
        }

        public static int CreateBassStreamHandle(string uri, UriType uriType, int offset = 0, DOWNLOADPROC downloadProc = null)
        {
            int num;
            switch (uriType)
            {
                case UriType.Unknown:
                case UriType.LocalFile:
                    throw new Exception("UriType was not specified");
                case UriType.Common:
                    num = Bass.BASS_StreamCreateURL(uri, offset, (BASSFlag)131072, downloadProc, IntPtr.Zero);
                    break;
                case UriType.Mpeg4Part14:
                    num = BassAac.BASS_AAC_StreamCreateURL(uri, offset, (BASSFlag)131072, downloadProc, IntPtr.Zero);
                    break;
                case UriType.Flac:
                    num = BassFlac.BASS_FLAC_StreamCreateURL(uri, offset, (BASSFlag)131072, downloadProc, IntPtr.Zero);
                    break;
                case UriType.Wma:
                    num = BassWma.BASS_WMA_StreamCreateURL(uri, offset, 0L, (BASSFlag)131072);
                    break;
                case UriType.Common | UriType.LocalFile:
                    num = Bass.BASS_StreamCreateFile(uri, offset, 0L, (BASSFlag)131072);
                    break;
                case UriType.Mpeg4Part14 | UriType.LocalFile:
                    num = BassAac.BASS_AAC_StreamCreateFile(uri, offset, 0L, (BASSFlag)131072);
                    break;
                case UriType.Flac | UriType.LocalFile:
                    num = BassFlac.BASS_FLAC_StreamCreateFile(uri, offset, 0L, (BASSFlag)131072);
                    break;
                case UriType.Wma | UriType.LocalFile:
                    num = BassWma.BASS_WMA_StreamCreateFile(uri, offset, 0L, (BASSFlag)131072);
                    break;
                default:
                    throw new NotSupportedException("Not supported UriType: " + uriType);
            }
            if (num == 0)
                throw new Exception("BASS stream creating finished with error: " + Bass.BASS_ErrorGetCode());
            return num;
        }

        public static TimeSpan SecondsInDoubleToTimeSpan(double seconds)
        {
            double num1 = Math.Floor(seconds);
            double num2 = seconds - num1;
            return new TimeSpan((long)num1 * 10000000L + (long)Math.Round(num2 * 10000000.0));
        }

        public void Play()
        {
            CheckIfInValidForPlayingState();
            if (PlayerState == State.InPlay)
                return;
            Bass.BASS_ChannelPlay(_streamHandle, PlayerState == State.InStop);
            _progressTimer.Change(100, 100);
            PlayerState = State.InPlay;
        }

        public void Pause()
        {
            CheckIfInValidForPlayingState();
            if (PlayerState == State.InPause || PlayerState == State.InStop)
                return;
            Bass.BASS_ChannelPause(_streamHandle);
            _progressTimer.Change(-1, -1);
            PlayerState = State.InPause;
        }

        public void Stop()
        {
            CheckIfInValidForPlayingState();
            if (PlayerState == State.InStop)
                return;
            Bass.BASS_ChannelStop(_streamHandle);
            _progressTimer.Change(-1, -1);
            PlayerState = State.InStop;
        }

        public void Seek(TimeSpan position)
        {
            CheckIfInValidForPlayingState();
            long num1 = Bass.BASS_ChannelSeconds2Bytes(_streamHandle, position.TotalSeconds);
            long position1 = Bass.BASS_ChannelGetPosition(_streamHandle, (BASSMode)268435456);
            if ((_uriType & UriType.LocalFile) != UriType.LocalFile)
            {
                long num2 = Bass.BASS_ChannelSeconds2Bytes(_streamHandle, DownloadPosition.TotalSeconds);
                if (num1 > num2)
                    num1 = num2;
            }
            if (Bass.BASS_ChannelSetPosition(_streamHandle, num1) || Bass.BASS_ErrorGetCode() != BASSError.BASS_ERROR_POSITION || Bass.BASS_ChannelSetPosition(_streamHandle, position1))
                return;
            PlayPosition = SecondsInDoubleToTimeSpan(Bass.BASS_ChannelBytes2Seconds(_streamHandle, Bass.BASS_ChannelGetPosition(_streamHandle)));
        }

        private void CheckIfDisposed()
        {
            if (_wasDisposed)
                throw new ObjectDisposedException("SleeperPlayer");
        }

        private void CheckIfInValidForPlayingState()
        {
            CheckIfDisposed();
            if (_streamHandle == 0)
                throw new InvalidOperationException("BASS stream was not created");
        }

        private void InitializeBassStream(int offset)
        {
            _downloadedContentLength = 0;
            DeinitializeBassStream();
            _streamHandle = CreateBassStreamHandle(_uri, _uriType, offset, _downloadBassProcedure);
            _streamLength = Bass.BASS_ChannelGetLength(_streamHandle);
            _contentLength = Bass.BASS_StreamGetFilePosition(_streamHandle, (BASSStreamFilePosition)2);
            _streamDivContentFactor = _streamLength / (double)_contentLength;
            Duration = SecondsInDoubleToTimeSpan(Bass.BASS_ChannelBytes2Seconds(_streamHandle, _streamLength));
            Bass.BASS_ChannelSetSync(_streamHandle, (BASSSync)2, 0L, _bassSyncEnd, IntPtr.Zero);
        }

        private void DeinitializeBassStream()
        {
            if (_streamHandle != 0)
            {
                Bass.BASS_StreamFree(_streamHandle);
                _streamHandle = 0;
            }
            Duration = TimeSpan.Zero;
            _playPosition = TimeSpan.Zero;
            _downloadPosition = TimeSpan.Zero;
            PlayerState = State.InStop;
            _streamLength = 0L;
            _contentLength = 0L;
            _downloadedContentLength = 0;
            _streamDivContentFactor = 0.0;
        }

        private void DownloadBassProcedure(IntPtr buffer, int length, IntPtr user)
        {
            if (length == 0)
                return;
            _downloadedContentLength += length;
            DownloadPosition = SecondsInDoubleToTimeSpan(Bass.BASS_ChannelBytes2Seconds(_streamHandle, (long)(_downloadedContentLength * _streamDivContentFactor)));
        }

        private void BassSyncEnd(int handle, int channel, int data, IntPtr user)
        {
            PlayerState = State.InStop;
            _progressTimer.Change(-1, -1);
            if (Bass.BASS_ChannelGetPosition(_streamHandle) < Bass.BASS_ChannelSeconds2Bytes(_streamHandle, Bass.BASS_ChannelBytes2Seconds(_streamHandle, _streamLength) - 1.5))
                EventsHelper.Raise(ErrorOccurred, this, new ErrorEventArgs(new Exception("BASS was not reached end of stream")));
            else
                EventsHelper.Raise(PlayFinished, this, new EventArgs());
        }

        private void ProgressTimerCallback(object stateInfo)
        {
            if (_wasDisposed)
                return;
            long position = Bass.BASS_ChannelGetPosition(_streamHandle);
            if (position == 0L && PlayerState == State.InPlay && Bass.BASS_ChannelIsActive(_streamHandle) == null)
                Bass.BASS_ChannelPlay(_streamHandle, false);
            PlayPosition = SecondsInDoubleToTimeSpan(Bass.BASS_ChannelBytes2Seconds(_streamHandle, position));
        }

        [Flags]
        public enum UriType : uint
        {
            Unknown = 0,
            Common = 1,
            Mpeg4Part14 = 2,
            Flac = 4,
            Wma = 8,
            LocalFile = 2147483648
        }

        public enum State
        {
            InPlay,
            InPause,
            InStop
        }
    }
}
