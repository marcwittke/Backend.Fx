namespace Backend.Fx.RandomData
{
    public static class TestPhoneGenerator
    {
        public static string Mobile()
        {
            var phone = Numbers.MobileNetworks.Random();
            while (phone.Length < TestRandom.Instance.Next(11, 12))
            {
                phone += Numbers.Ciphers.Random();
            }
            return phone;
        }

        public static string LandLine()
        {
            var phone = Numbers.LandLineNetworks.Random();
            while (phone.Length < TestRandom.Instance.Next(8, 11))
            {
                phone += Numbers.Ciphers.Random();
            }
            return phone;
        }
    }
}