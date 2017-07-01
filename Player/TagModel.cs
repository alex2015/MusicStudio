using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Un4seen.Bass.AddOn.Tags;

namespace Player
{
    public class TagModel
    {
        public string PathFileName { get; set; }

        public int BitRate { get; set; }
        public int Freq { get; set; }
        public string Channels { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Title { get; set; }
        public string Year { get; set; }
        public TimeSpan Duration { get; set; }
        public string DurationText { get; set; }
        public string format { get; set; }

        public string TopDisplayText { get; set; }
        public string BottomDisplayText { get; set; }

        private Dictionary<int, string> ChannelsDict = new Dictionary<int, string>
        {
            {0, "Null"},
            {1, "Mono"},
            {2, "Stereo"}
        };


        public TagModel(string pathFileName)
        {
            PathFileName = pathFileName;

            var tagInfo = BassTags.BASS_TAG_GetFromFile(pathFileName);

            BitRate = tagInfo.bitrate;
            Freq = tagInfo.channelinfo.freq;
            Channels = ChannelsDict[tagInfo.channelinfo.chans];
            Artist = tagInfo.artist;
            Album = tagInfo.album;
            Title = tagInfo.title == string.Empty ? Path.GetFileName(pathFileName) : tagInfo.title;
            Year = tagInfo.year;

            Duration = TimeSpan.FromSeconds(Math.Round(tagInfo.duration));
            DurationText = setDurationText();

            format = Path.GetExtension(pathFileName).Substring(1).ToUpper();

            TopDisplayText = string.Format("{0} - {1}", Artist, Title);
            BottomDisplayText = string.Format("{0} :: {1} kHz, {2} kbps, {3} MB", format, Freq / 1000, BitRate, Math.Round(Convert.ToDouble(new FileInfo(pathFileName).Length) / 1000000, 2));
        }

        public override string ToString()
        {
            return TopDisplayText;
        }

        private string setDurationText()
        {
            var sb = new StringBuilder();

            if (Duration.Hours > 0)
            {
                sb.Append(Duration.Hours + ":");
            }

            sb.Append(Duration.Minutes + ":" + Duration.Seconds);

            return sb.ToString();
        }
    }
}
