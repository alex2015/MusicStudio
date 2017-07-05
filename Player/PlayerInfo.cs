using System.Collections.ObjectModel;

namespace Player
{
    public static class PlayerInfo
    {
        public static int currentTrackNumber;

        public static ObservableCollection<TrackModel> filesInfo = new ObservableCollection<TrackModel>();
    }
}
