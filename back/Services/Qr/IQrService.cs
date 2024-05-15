using FluentResults;

namespace Quizer.Services.Qr
{
    public interface IQrService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Result with QR code png image byte array or error</returns>
        public Result<byte[]> GetQrByName(string name);
    }
}
