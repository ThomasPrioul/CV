namespace CV
{
    using System.Windows.Media;

    public static class ColorExtensions
    {
        public static SolidColorBrush ToBrush(this Color color) => new SolidColorBrush(color);
    }
}