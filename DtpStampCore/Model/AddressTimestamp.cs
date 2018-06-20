namespace DtpStampCore.Model
{
    public class AddressTimestamp
    {
        public byte[] Address { get; set; }
        public long Time { get; set; } 

        /// <summary>
        /// -1 = no Timestamps, 0 = unconfirmed tx, above 0 is the number of confimations
        /// </summary>
        public int Confirmations { get; set; }

        public AddressTimestamp()
        {
            Address = new byte[0];
            Time = 0;
            Confirmations = -1;
        }
    }
}
