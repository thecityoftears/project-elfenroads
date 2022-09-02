namespace Elfencore.Shared.GameState
{

    /// <summary> Color class for representing RGB colors. Implemented to avoid using Unity classes that would make this core reliant on Unity </summary>
    public class Color
    {
        public readonly static Color WHITE = new Color(255, 255, 255);
        public readonly static Color BLACK = new Color(0, 0, 0);
        public readonly static Color RED = new Color(255, 0, 0);
        public readonly static Color GREEN = new Color(0, 255, 0);
        public readonly static Color BLUE = new Color(0, 0, 255);
        public readonly static Color ORANGE = new Color(255, 128, 0);
        public readonly static Color YELLOW = new Color(255, 255, 0);
        public readonly static Color LIME = new Color(128, 255, 0);
        public readonly static Color MINT = new Color(0, 255, 128);
        public readonly static Color CYAN = new Color(0, 255, 255);
        public readonly static Color PERIWINKLE = new Color(0, 128, 255);
        public readonly static Color PURPLE = new Color(128, 0, 255);
        public readonly static Color VIOLET = new Color(255, 0, 255);
        public readonly static Color MAGENTA = new Color(255, 0, 128);
        public readonly static Color GREY = new Color(128, 128, 128);

        public int r;
        public int g;
        public int b;

        public Color(int red, int green, int blue)
        {
            r = red;
            g = green;
            b = blue;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Color other = (Color)obj;
            return r == other.r && g == other.g && b == other.b;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + r;
            hash = (hash * 7) + g;
            hash = (hash * 7) + b;
            return hash;
        }
    }
};