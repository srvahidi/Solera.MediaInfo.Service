using Amazon.S3;
using Amazon.S3.Transfer;

namespace Solera.MediaInfo.Service.Helpers
{
    public interface ITransferUtilitySimpleFactory
    {
        ITransferUtility Create(IAmazonS3 s3Client);
    }

    public class TransferUtilitySimpleFactory : ITransferUtilitySimpleFactory
    {
        public ITransferUtility Create(IAmazonS3 s3Client)
        {
            return new TransferUtility(s3Client);
        }
    }
}
