using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Player
{
    public static class Vars
    {
        public static int currentTrackNumber;

        public static ObservableCollection<TagModel> filesInfo = new ObservableCollection<TagModel>();
    }
}
