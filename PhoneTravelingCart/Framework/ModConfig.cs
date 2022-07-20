namespace PhoneTravelingCart.Framework
{
    internal class ModConfig
    {
        public bool RemotePurchase { get; set; } = false;

        public void ResetToDefault()
        {
            this.RemotePurchase = false;
        }
    }
}
