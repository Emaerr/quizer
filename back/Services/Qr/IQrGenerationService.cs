using FluentResults;

namespace Quizer.Services.Qr
{
    public interface IQrGenerationService
    {
        /// <summary>
        /// Generates the QR code of given string.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <exception cref="QrGenerationFailException"></exception>
        public void GenerateQrCode(string name, string data);
    }
}