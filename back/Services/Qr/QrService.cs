using FluentResults;
using QRCoder;
using System.Drawing.Imaging;
using System.Drawing;
using Quizer.Exceptions.Services;
using QRCoder.Exceptions;
using System.IO;

namespace Quizer.Services.Qr
{
    public class QrService : IQrService, IQrGenerationService
    {
        private Dictionary<string, byte[]> _qrCodes = [];

        public Result<byte[]> GetQrByName(string name)
        {
            _qrCodes.TryGetValue(name, out var result);

            if (result == null)
            {
                return Result.Fail(new QrNotFoundError($"QR code {name} not found."));
            }

            return result;
        }

        public void GenerateQrCode(string name, string str)
        {
            using QRCodeGenerator qrGenerator = new QRCodeGenerator();
            try
            {
                using QRCodeData qrCodeData = qrGenerator.CreateQrCode(str, QRCodeGenerator.ECCLevel.Q);
                using PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
                byte[] qrCodeImage = qrCode.GetGraphic(20);
                _qrCodes[name] = qrCodeImage;
                File.WriteAllBytes(@"C:\Users\User\Desktop\yourfile.png", qrCodeImage);
            }
            catch (DataTooLongException e)
            {
                throw new QrGenerationFailException($"QR {name} generation failed ", e);

            }
        }
    }
}
