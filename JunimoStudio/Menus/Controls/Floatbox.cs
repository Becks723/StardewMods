using System.Linq;

namespace JunimoStudio.Menus.Controls
{
    internal class Floatbox : Textbox
    {
        public float Value
        {
            get => String == "" || String == "-" ? 0 : float.Parse(String);
            set => String = value.ToString();
        }

        protected override void ReceiveInput(string str)
        {
            bool hasDot = String.Contains('.');
            bool valid = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (!char.IsDigit(c) && !(c == '.' && !hasDot) && !(c == '-' && String == "" && i == 0))
                {
                    valid = false;
                    break;
                }
                if (c == '.')
                    hasDot = true;
            }
            if (!valid)
                return;

            String += str;
            Callback?.Invoke(this);
        }
    }
}
