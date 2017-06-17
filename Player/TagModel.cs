using System;
using System.Collections.Generic;
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
            BitRate = tagInfo.bitrate;
            Freq = tagInfo.channelinfo.freq;
            Channels = ChannelsDict[tagInfo.channelinfo.chans];
            Artist = tagInfo.artist;
            Album = tagInfo.album;

            Title = tagInfo.title == string.Empty ? System.IO.Path.GetFileName(pathFileName) : tagInfo.title;

            Year = tagInfo.year;

        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", Artist, Title);
        }
    }
}
