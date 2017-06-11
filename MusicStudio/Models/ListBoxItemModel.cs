namespace MusicStudio.Models
{
    internal class ListBoxItemModel
    {
        public string Value { get; set; }
        public string DisplayValue { get; set; }

        public override string ToString()
        {
            return DisplayValue;
        }
    }
}
