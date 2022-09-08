namespace TextColor
{
    public static class Colors
    {
        public static Color Error      => new Color(255,   0,   0);
        public static Color FatalError => new Color(128,   0,   0);
        public static Color Warning    => new Color(255, 255,   0);
        public static Color Ignorable  => new Color(128, 128, 128);
        public static Color Selected   => new Color(  0, 128, 128);
        public static Color Text       => new Color(255, 255, 255);
        public static Color Background => new Color(  0,   0,   0);
        public static Color Important  => new Color(  0, 255, 255);
    }
    public class Color
    {
        public byte R;
        public byte G;
        public byte B;

        public string AnsiFGCode => $"\u001b[38;2;{R};{G};{B}m";
        public string AnsiBGCode => $"\u001b[48;2;{R};{G};{B}m";
        public static string Reset = "\u001b[m";
        public Color(){}
        public Color(byte r, byte g, byte b) { R = r; G = g; B = b; }
    }
}