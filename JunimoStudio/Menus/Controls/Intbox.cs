namespace JunimoStudio.Menus.Controls
{
    internal class Intbox : Textbox
    {
        public int Value
        {
            get => String == "" || String == "-" ? 0 : int.Parse(String);
            set => String = value.ToString();
        }

        protected override void ReceiveInput(string str)
        {
            bool valid = true;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                if (!char.IsDigit(c) && !(c == '-' && String == "" && i == 0))
                {
                    valid = false;
                    break;
                }
            }
            if (!valid)
                return;

            String += str;
            Callback?.Invoke(this);
        }
    }
}
