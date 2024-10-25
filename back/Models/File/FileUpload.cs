using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;
using System.Configuration;

namespace Quizer.Models.File
{
    public class FileUpload(string filePath, bool determineExtension = false)
    {
        [BindProperty]
        public IFormFile Upload { get; set; }
        public async Task OnPostAsync()
        {
            if (determineExtension)
            {
                string? extenstion = Path.GetExtension(Upload.FileName);
                if (extenstion == null)
                {
                    throw new ArgumentException();
                }

                filePath = filePath + "." + extenstion;
            }
            
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await Upload.CopyToAsync(fileStream);
            }
        }
    }
}
