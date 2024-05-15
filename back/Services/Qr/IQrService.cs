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

        /// <summary>
        /// Generates the QR code of given string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <exception cref="QrGenerationFailException"></exception>
        public void GenerateQrCode(string name, string data);
    }
}
