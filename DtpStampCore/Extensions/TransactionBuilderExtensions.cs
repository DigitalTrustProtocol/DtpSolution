using NBitcoin;
using System.Linq;
using System.Text;

namespace DtpStampCore.Extensions
{
    public static class TransactionBuilderExtensions
    {
        public static TransactionBuilder SendOP_Return(this TransactionBuilder tb, byte[] data)
        {
            var message = Encoding.UTF8.GetBytes("dtp").Concat(data).ToArray();
            tb.Send(TxNullDataTemplate.Instance.GenerateScriptPubKey(message), Money.Zero);
            return tb;
        }


    }
}
