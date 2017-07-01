using System;
using System.Collections.Generic;
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

        public string DisplayText { get; set; }

        private Dictionary<int, string> ChannelsDict = new Dictionary<int, string>
        {
            {0, "Null"},
            {1, "Mono"},
            {2, "Stereo"}
        };


        public TagModel(string pathFileName)
        {
            PathFileName = pathFileName;

            TAG_INFO tagInfo = BassTags.BASS_TAG_GetFromFile(pathFileName);

            Duration = TimeSpan.FromSeconds(Math.Round(tagInfo.duration));
            DurationText = setDurationText();

            BitRate = tagInfo.bitrate;
            Freq = tagInfo.channelinfo.freq;
            Channels = ChannelsDict[tagInfo.channelinfo.chans];
            Artist = tagInfo.artist;
            Album = tagInfo.album;

            Title = tagInfo.title == string.Empty ? System.IO.Path.GetFileName(pathFileName) : tagInfo.title;

            Year = tagInfo.year;

            DisplayText = string.Format("{0} - {1}", Artist, Title);
        }

        public override string ToString()
        {
            return DisplayText;
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
