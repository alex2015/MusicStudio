using System.Collections.Generic;

namespace Player
{
    public static class Vars
    {
        /// <summary>
        /// Список полных имён файлов
        /// </summary>
        public static List<string> Files = new List<string>();

        public static string GetFileName(string file)
        {
            string[] tmp = file.Split('\\');
            return tmp[tmp.Length - 1];
        }
    }
}
